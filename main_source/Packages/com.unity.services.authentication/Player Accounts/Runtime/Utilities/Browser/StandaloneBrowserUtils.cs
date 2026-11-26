#if UNITY_EDITOR || UNITY_STANDALONE
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Services.Authentication.PlayerAccounts
{
    // Eyes recovery: HttpListener never accepts connections on "localhost" in this Mono player, so the browser
    // could not return after sign-in. The loopback redirect is served by plain sockets on 127.0.0.1 and ::1.
    class StandaloneBrowserUtils : IBrowserUtils
    {
        const int k_MinPort = 49215;
        const int k_MaxPort = 65535;
        const int k_MaxRequestSize = 16384;

        public event Action<string> AuthCodeReceivedEvent;

        readonly object m_Lock = new object();
        readonly List<TcpListener> m_Listeners = new List<TcpListener>();
        int? m_BoundPort = null;
        int m_Generation;

        public string GetRedirectUri()
        {
            return $"http://localhost:{m_BoundPort}/callback";
        }

        public bool Bind()
        {
            if (m_BoundPort.HasValue)
            {
                return true;
            }

            for (var port = k_MinPort; port < k_MaxPort; port++)
            {
                if (TryOpen(port))
                {
                    m_BoundPort = port;
                    return true;
                }
            }

            return false;
        }

        bool TryOpen(int port)
        {
            lock (m_Lock)
            {
                CloseListeners();
                try
                {
                    var v4 = new TcpListener(IPAddress.Loopback, port);
                    v4.Start();
                    m_Listeners.Add(v4);
                }
                catch (SocketException)
                {
                    CloseListeners();
                    return false;
                }

                try
                {
                    var v6 = new TcpListener(IPAddress.IPv6Loopback, port);
                    v6.Start();
                    m_Listeners.Add(v6);
                }
                catch (SocketException)
                {
                    // IPv6 is optional: browsers fall back to 127.0.0.1.
                }

                return true;
            }
        }

        void CloseListeners()
        {
            foreach (var listener in m_Listeners)
            {
                listener.Stop();
            }

            m_Listeners.Clear();
        }

        public async Task LaunchUrlAsync(string url)
        {
            int generation;
            lock (m_Lock)
            {
                generation = ++m_Generation;
                if (m_Listeners.Count == 0 && !TryOpen(m_BoundPort.Value))
                {
                    throw PlayerAccountsExceptionHandler.HandleError("Could not open the local sign-in port", nameof(url));
                }
            }

            var state = UriHelper.ParseQueryString(new Uri(url).Query).TryGetValue("state", out var expected) ? expected : null;
            if (state == null)
            {
                throw PlayerAccountsExceptionHandler.HandleError("State parameter not found in URL", nameof(state));
            }

            Application.OpenURL(url);

            while (true)
            {
                TcpClient client;
                try
                {
                    client = await AcceptAny();
                }
                catch (Exception ex) when (ex is ObjectDisposedException || ex is SocketException || ex is InvalidOperationException)
                {
                    Logger.Log("Sign-in listener closed. " + ex.Message);
                    return;
                }

                string target;
                using (client)
                {
                    var stream = client.GetStream();
                    target = await ReadRequestTarget(stream);
                    await SendBrowserResponse(stream);
                }

                if (generation != m_Generation)
                {
                    return;
                }

                // Browsers also ask for /favicon.ico: only the callback finishes the sign-in.
                if (target == null || !target.StartsWith("/callback", StringComparison.Ordinal))
                {
                    continue;
                }

                lock (m_Lock)
                {
                    CloseListeners();
                }

                AuthCodeReceivedEvent?.Invoke(GetAuthCode(target, state));
                return;
            }
        }

        async Task<TcpClient> AcceptAny()
        {
            var pending = new List<Task<TcpClient>>();
            lock (m_Lock)
            {
                foreach (var listener in m_Listeners)
                {
                    pending.Add(listener.AcceptTcpClientAsync());
                }
            }

            if (pending.Count == 0)
            {
                throw new ObjectDisposedException("listener");
            }

            return await await Task.WhenAny(pending);
        }

        static async Task<string> ReadRequestTarget(NetworkStream stream)
        {
            var request = new StringBuilder();
            var buffer = new byte[1024];
            while (request.Length < k_MaxRequestSize)
            {
                var read = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (read <= 0)
                {
                    break;
                }

                request.Append(Encoding.ASCII.GetString(buffer, 0, read));
                if (request.ToString().Contains("\r\n\r\n"))
                {
                    break;
                }
            }

            var parts = request.ToString().Split('\r')[0].Split(' ');
            return parts.Length >= 2 ? parts[1] : null;
        }

        static async Task SendBrowserResponse(NetworkStream stream)
        {
            var body = Encoding.UTF8.GetBytes("<html><body><b>DONE!</b><br>(You can return to your app and close this tab/window)</body></html>");
            var head = Encoding.ASCII.GetBytes($"HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=utf-8\r\nContent-Length: {body.Length}\r\nConnection: close\r\n\r\n");
            await stream.WriteAsync(head, 0, head.Length);
            await stream.WriteAsync(body, 0, body.Length);
            await stream.FlushAsync();
        }

        static string GetAuthCode(string target, string state)
        {
            var mark = target.IndexOf('?');
            var query = mark >= 0 ? target.Substring(mark) : "";
            var values = UriHelper.ParseQueryString(query);
            values.TryGetValue("code", out var code);
            values.TryGetValue("error", out var error);
            values.TryGetValue("state", out var incomingState);

            if (!string.IsNullOrEmpty(error))
            {
                throw PlayerAccountsExceptionHandler.HandleError(error);
            }

            if (incomingState != state)
            {
                throw PlayerAccountsException.Create(PlayerAccountsErrorCodes.InvalidState, "Received request with an invalid state");
            }

            return code;
        }
    }
}
#endif
