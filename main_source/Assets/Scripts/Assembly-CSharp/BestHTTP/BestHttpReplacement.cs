using System.Collections.Concurrent;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
// Own replacement of the paid BestHTTP package: the HTTP and WebSocket surface the game uses, on Unity and .NET primitives.
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace BestHTTP
{
	public enum HTTPMethods
	{
		Get = 0,
		Head = 1,
		Post = 2,
		Put = 3,
		Delete = 4,
		Patch = 5
	}

	public enum HTTPRequestStates
	{
		Initial = 0,
		Queued = 1,
		Processing = 2,
		Finished = 3,
		Error = 4,
		Aborted = 5,
		ConnectionTimedOut = 6,
		TimedOut = 7
	}

	public delegate void OnRequestFinishedDelegate(HTTPRequest originalRequest, HTTPResponse response);

	public sealed class HTTPResponse
	{
		public int StatusCode { get; internal set; }

		public string Message { get; internal set; }

		public byte[] Data { get; internal set; }

		public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;

		public string DataAsText => (Data != null) ? Encoding.UTF8.GetString(Data) : null;

		public Dictionary<string, string> Headers { get; internal set; }

		public string GetFirstHeaderValue(string name)
		{
			if (Headers != null)
			{
				foreach (KeyValuePair<string, string> header in Headers)
				{
					if (string.Equals(header.Key, name, StringComparison.OrdinalIgnoreCase))
					{
						return header.Value;
					}
				}
			}
			return null;
		}
	}

	public sealed class HTTPRequest
	{
		private readonly Dictionary<string, string> headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		private readonly TaskCompletionSource<HTTPResponse> completion = new TaskCompletionSource<HTTPResponse>();

		private UnityWebRequest active;

		private bool started;

		private bool aborted;

		public Uri Uri { get; }

		public Uri CurrentUri => Uri;

		public HTTPMethods MethodType { get; set; }

		public byte[] RawData { get; set; }

		public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60.0);

		public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(20.0);

		public HTTPRequestStates State { get; private set; }

		public OnRequestFinishedDelegate Callback { get; set; }

		public Exception Exception { get; private set; }

		public HTTPResponse Response { get; private set; }

		public object Tag { get; set; }

		internal IEnumerable<KeyValuePair<string, string>> Headers => headers;

		public HTTPRequest(Uri uri)
			: this(uri, HTTPMethods.Get, null)
		{
		}

		public HTTPRequest(Uri uri, OnRequestFinishedDelegate callback)
			: this(uri, HTTPMethods.Get, callback)
		{
		}

		public HTTPRequest(Uri uri, HTTPMethods method)
			: this(uri, method, null)
		{
		}

		public HTTPRequest(Uri uri, HTTPMethods method, OnRequestFinishedDelegate callback)
		{
			Uri = uri;
			MethodType = method;
			Callback = callback;
		}

		public void AddHeader(string name, string value)
		{
			headers[name] = value;
		}

		public void SetHeader(string name, string value)
		{
			headers[name] = value;
		}

		public string GetFirstHeaderValue(string name)
		{
			return headers.TryGetValue(name, out string value) ? value : null;
		}

		public HTTPRequest Send()
		{
			if (started)
			{
				return this;
			}
			started = true;
			MainThreadPump.Run(Begin);
			return this;
		}

		public Task<HTTPResponse> GetHTTPResponseAsync(CancellationToken token = default(CancellationToken))
		{
			if (token.CanBeCanceled)
			{
				token.Register(Abort);
			}
			Send();
			return completion.Task;
		}

		public void Abort()
		{
			aborted = true;
			MainThreadPump.Run(delegate
			{
				if (active != null)
				{
					active.Abort();
				}
			});
		}

		private void Begin()
		{
			if (aborted)
			{
				Finish(HTTPRequestStates.Aborted, null);
				return;
			}
			State = HTTPRequestStates.Processing;
			UnityWebRequest web = new UnityWebRequest(Uri, MethodType.ToString().ToUpperInvariant());
			web.downloadHandler = new DownloadHandlerBuffer();
			if (RawData != null)
			{
				web.uploadHandler = new UploadHandlerRaw(RawData);
			}
			if (OfficialServerPin.Applies(Uri))
			{
				web.certificateHandler = new OfficialServerPinHandler();
			}
			web.timeout = Mathf.Max(1, (int)Timeout.TotalSeconds);
			foreach (KeyValuePair<string, string> header in headers)
			{
				web.SetRequestHeader(header.Key, header.Value);
			}
			active = web;
			web.SendWebRequest().completed += delegate
			{
				Complete(web);
			};
		}

		private void Complete(UnityWebRequest web)
		{
			active = null;
			if (aborted)
			{
				web.Dispose();
				Finish(HTTPRequestStates.Aborted, null);
				return;
			}
			HTTPResponse response = null;
			HTTPRequestStates state = HTTPRequestStates.Finished;
			if (web.result == UnityWebRequest.Result.ConnectionError)
			{
				state = (web.error != null && web.error.IndexOf("timeout", StringComparison.OrdinalIgnoreCase) >= 0) ? HTTPRequestStates.TimedOut : HTTPRequestStates.Error;
				Exception = new Exception(web.error);
			}
			else
			{
				response = new HTTPResponse
				{
					StatusCode = (int)web.responseCode,
					Message = web.error ?? "OK",
					Data = (web.downloadHandler != null) ? web.downloadHandler.data : null,
					Headers = web.GetResponseHeaders()
				};
			}
			web.Dispose();
			Finish(state, response);
		}

		private void Finish(HTTPRequestStates state, HTTPResponse response)
		{
			State = state;
			Response = response;
			try
			{
				Callback?.Invoke(this, response);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			completion.TrySetResult(response);
		}
	}

	public static class HTTPManager
	{
		public static string UserAgent = "Eyes";

		public static TimeSpan ConnectTimeout = TimeSpan.FromSeconds(20.0);

		public static TimeSpan RequestTimeout = TimeSpan.FromSeconds(60.0);
	}

	// Runs socket and request callbacks on Unity's main thread.
	public sealed class MainThreadPump : MonoBehaviour
	{
		private static readonly ConcurrentQueue<Action> Queue = new ConcurrentQueue<Action>();

		private static MainThreadPump instance;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Boot()
		{
			GameObject holder = new GameObject("HttpMainThreadPump");
			holder.hideFlags = HideFlags.HideAndDontSave;
			DontDestroyOnLoad(holder);
			instance = holder.AddComponent<MainThreadPump>();
		}

		public static void Run(Action action)
		{
			Queue.Enqueue(action);
		}

		private void Update()
		{
			while (Queue.TryDequeue(out Action action))
			{
				try
				{
					action();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}
	}
}

namespace BestHTTP.Connections.HTTP2
{
}

namespace BestHTTP.WebSocket
{
	public enum WebSocketStates : byte
	{
		Connecting = 0,
		Open = 1,
		Closing = 2,
		Closed = 3,
		Unknown = 4
	}

	public interface IExtension
	{
	}

	public delegate void OnWebSocketOpenDelegate(WebSocket webSocket);

	public delegate void OnWebSocketMessageDelegate(WebSocket webSocket, string message);

	public delegate void OnWebSocketBinaryDelegate(WebSocket webSocket, byte[] data);

	public delegate void OnWebSocketClosedDelegate(WebSocket webSocket, ushort code, string message);

	public delegate void OnWebSocketErrorDelegate(WebSocket webSocket, string reason);

	// RFC 6455 client over TcpClient and SslStream. System.Net.WebSockets on Unity's Mono cannot accept the pinned
	// official certificate and has no way to bypass a system proxy reliably, so the transport is implemented here.
	public sealed class WebSocket
	{
		private const string AcceptGuid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

		private const int MaxMessage = 4194304;

		private readonly Uri uri;

		private readonly string subProtocol;

		private readonly CancellationTokenSource lifetime = new CancellationTokenSource();

		private readonly SemaphoreSlim sendLock = new SemaphoreSlim(1, 1);

		private TcpClient client;

		private Stream stream;

		private bool closedRaised;

		public OnWebSocketOpenDelegate OnOpen;

		public OnWebSocketMessageDelegate OnMessage;

		public OnWebSocketBinaryDelegate OnBinary;

		public OnWebSocketClosedDelegate OnClosed;

		public OnWebSocketErrorDelegate OnError;

		public WebSocketStates State { get; private set; } = WebSocketStates.Unknown;

		public bool IsOpen => State == WebSocketStates.Open;

		public HTTPRequest InternalRequest { get; }

		public TimeSpan PingFrequency { get; set; } = TimeSpan.FromSeconds(10.0);

		public bool StartPingThread { get; set; }

		public WebSocket(Uri uri)
			: this(uri, null, null, null)
		{
		}

		public WebSocket(Uri uri, string origin, string protocol, params IExtension[] extensions)
		{
			this.uri = uri;
			subProtocol = protocol;
			InternalRequest = new HTTPRequest(uri);
		}

		public void Open()
		{
			if (State == WebSocketStates.Unknown)
			{
				State = WebSocketStates.Connecting;
				Task.Run(ConnectAndRead);
			}
		}

		public void Send(byte[] data)
		{
			Queue(2, data);
		}

		public void Send(string text)
		{
			Queue(1, Encoding.UTF8.GetBytes(text));
		}

		public void Close()
		{
			Close(1000, "Bye");
		}

		public void Close(ushort code, string message)
		{
			if (State == WebSocketStates.Closed || State == WebSocketStates.Closing)
			{
				return;
			}
			bool wasOpen = State == WebSocketStates.Open;
			State = WebSocketStates.Closing;
			Task.Run(async delegate
			{
				try
				{
					if (wasOpen)
					{
						byte[] text = Encoding.UTF8.GetBytes(message ?? "");
						byte[] payload = new byte[2 + Math.Min(text.Length, 120)];
						payload[0] = (byte)(code >> 8);
						payload[1] = (byte)code;
						Array.Copy(text, 0, payload, 2, payload.Length - 2);
						await WriteFrame(8, payload);
					}
				}
				catch (Exception)
				{
				}
				RaiseClosed(code, message);
				await Task.Delay(500);
				Shutdown();
			});
		}

		private void Queue(byte opcode, byte[] data)
		{
			if (State != WebSocketStates.Open)
			{
				return;
			}
			Task.Run(async delegate
			{
				try
				{
					await WriteFrame(opcode, data);
				}
				catch (Exception exception)
				{
					if (State == WebSocketStates.Open)
					{
						RaiseError(exception.Message);
						Shutdown();
					}
				}
			});
		}

		private async Task WriteFrame(byte opcode, byte[] payload)
		{
			byte[] mask = new byte[4];
			lock (MaskRandom)
			{
				MaskRandom.NextBytes(mask);
			}
			int headerSize = payload.Length < 126 ? 2 : (payload.Length < 65536 ? 4 : 10);
			byte[] frame = new byte[headerSize + 4 + payload.Length];
			frame[0] = (byte)(0x80 | opcode);
			if (payload.Length < 126)
			{
				frame[1] = (byte)(0x80 | payload.Length);
			}
			else if (payload.Length < 65536)
			{
				frame[1] = 0xFE;
				frame[2] = (byte)(payload.Length >> 8);
				frame[3] = (byte)payload.Length;
			}
			else
			{
				frame[1] = 0xFF;
				frame[6] = (byte)(payload.Length >> 24);
				frame[7] = (byte)(payload.Length >> 16);
				frame[8] = (byte)(payload.Length >> 8);
				frame[9] = (byte)payload.Length;
			}
			Array.Copy(mask, 0, frame, headerSize, 4);
			for (int i = 0; i < payload.Length; i++)
			{
				frame[headerSize + 4 + i] = (byte)(payload[i] ^ mask[i & 3]);
			}
			await sendLock.WaitAsync(lifetime.Token);
			try
			{
				await stream.WriteAsync(frame, 0, frame.Length, lifetime.Token);
				await stream.FlushAsync(lifetime.Token);
			}
			finally
			{
				sendLock.Release();
			}
		}

		private static readonly System.Random MaskRandom = new System.Random();

		private async Task Handshake()
		{
			client = new TcpClient();
			client.NoDelay = true;
			bool secure = uri.Scheme == "wss" || uri.Scheme == "https";
			int port = uri.IsDefaultPort ? (secure ? 443 : 80) : uri.Port;
			Task connect = client.ConnectAsync(uri.Host, port);
			if (await Task.WhenAny(connect, Task.Delay(10000)) != connect)
			{
				throw new TimeoutException("Connection timed out");
			}
			await connect;
			Stream network = client.GetStream();
			if (secure)
			{
				SslStream ssl = new SslStream(network, false, OfficialServerPin.ValidateSocketCertificate);
				await ssl.AuthenticateAsClientAsync(uri.Host, null, SslProtocols.Tls12, false);
				network = ssl;
			}
			stream = network;
			byte[] nonce = new byte[16];
			lock (MaskRandom)
			{
				MaskRandom.NextBytes(nonce);
			}
			string key = Convert.ToBase64String(nonce);
			StringBuilder request = new StringBuilder();
			request.Append("GET ").Append(uri.PathAndQuery).Append(" HTTP/1.1\r\n");
			request.Append("Host: ").Append(uri.IsDefaultPort ? uri.Host : uri.Host + ":" + uri.Port).Append("\r\n");
			request.Append("Upgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Version: 13\r\n");
			request.Append("Sec-WebSocket-Key: ").Append(key).Append("\r\n");
			if (!string.IsNullOrEmpty(subProtocol))
			{
				request.Append("Sec-WebSocket-Protocol: ").Append(subProtocol).Append("\r\n");
			}
			foreach (KeyValuePair<string, string> header in InternalRequest.Headers)
			{
				request.Append(header.Key).Append(": ").Append(header.Value).Append("\r\n");
			}
			request.Append("\r\n");
			byte[] bytes = Encoding.UTF8.GetBytes(request.ToString());
			await stream.WriteAsync(bytes, 0, bytes.Length);
			await stream.FlushAsync();
			StringBuilder head = new StringBuilder();
			byte[] one = new byte[1];
			while (head.Length < 16384)
			{
				if (await stream.ReadAsync(one, 0, 1) != 1)
				{
					throw new IOException("Connection closed during handshake");
				}
				head.Append((char)one[0]);
				if (head.Length >= 4 && head[head.Length - 1] == '\n' && head[head.Length - 2] == '\r' && head[head.Length - 3] == '\n')
				{
					break;
				}
			}
			string response = head.ToString();
			string statusLine = response.Split('\r')[0];
			if (!statusLine.Contains(" 101"))
			{
				throw new WebSocketHandshakeException(statusLine);
			}
			using (System.Security.Cryptography.SHA1 sha = System.Security.Cryptography.SHA1.Create())
			{
				string expected = Convert.ToBase64String(sha.ComputeHash(Encoding.ASCII.GetBytes(key + AcceptGuid)));
				if (response.IndexOf(expected, StringComparison.Ordinal) < 0)
				{
					throw new IOException("Invalid handshake response");
				}
			}
		}

		private async Task ReadExactly(byte[] buffer, int count)
		{
			int done = 0;
			while (done < count)
			{
				int read = await stream.ReadAsync(buffer, done, count - done, lifetime.Token);
				if (read <= 0)
				{
					throw new IOException("Connection lost");
				}
				done += read;
			}
		}

		private async Task PingLoop()
		{
			while (StartPingThread && State == WebSocketStates.Open && !lifetime.IsCancellationRequested)
			{
				await Task.Delay(PingFrequency, lifetime.Token);
				if (State == WebSocketStates.Open)
				{
					await WriteFrame(9, new byte[0]);
				}
			}
		}

		private async Task ConnectAndRead()
		{
			try
			{
				await Handshake();
				MainThreadPump.Run(delegate
				{
					if (State == WebSocketStates.Connecting)
					{
						State = WebSocketStates.Open;
					}
					OnOpen?.Invoke(this);
					_ = Task.Run(async delegate
					{
						try
						{
							await PingLoop();
						}
						catch (Exception)
						{
						}
					});
				});
				MemoryStream assembled = new MemoryStream();
				int messageOpcode = 0;
				byte[] head = new byte[2];
				byte[] extended = new byte[8];
				while (true)
				{
					await ReadExactly(head, 2);
					int opcode = head[0] & 0x0F;
					bool final = (head[0] & 0x80) != 0;
					long size = head[1] & 0x7F;
					if (size == 126)
					{
						await ReadExactly(extended, 2);
						size = (extended[0] << 8) | extended[1];
					}
					else if (size == 127)
					{
						await ReadExactly(extended, 8);
						size = 0L;
						for (int i = 0; i < 8; i++)
						{
							size = (size << 8) | extended[i];
						}
					}
					if (size < 0 || size > MaxMessage || assembled.Length + size > MaxMessage)
					{
						throw new IOException("Message too large");
					}
					byte[] payload = new byte[size];
					await ReadExactly(payload, (int)size);
					if (opcode == 8)
					{
						ushort code = size >= 2 ? (ushort)((payload[0] << 8) | payload[1]) : (ushort)1000;
						string reason = size > 2 ? Encoding.UTF8.GetString(payload, 2, (int)size - 2) : "";
						if (code == 1000 || code >= 3000 || State == WebSocketStates.Closing)
						{
							RaiseClosed(code, reason);
						}
						else
						{
							RaiseError("Closed by server: " + code + " " + reason);
						}
						Shutdown();
						return;
					}
					if (opcode == 9)
					{
						await WriteFrame(10, payload);
						continue;
					}
					if (opcode == 10)
					{
						continue;
					}
					if (opcode != 0)
					{
						messageOpcode = opcode;
					}
					assembled.Write(payload, 0, payload.Length);
					if (final)
					{
						byte[] message = assembled.ToArray();
						assembled.SetLength(0L);
						int type = messageOpcode;
						MainThreadPump.Run(delegate
						{
							if (type == 1)
							{
								OnMessage?.Invoke(this, Encoding.UTF8.GetString(message));
							}
							else
							{
								OnBinary?.Invoke(this, message);
							}
						});
					}
				}
			}
			catch (Exception exception)
			{
				if (State == WebSocketStates.Closing || State == WebSocketStates.Closed)
				{
					RaiseClosed(1000, "Closed");
				}
				else
				{
					UnityEngine.Debug.LogWarning("[Online] socket failed: " + exception.GetType().Name + ": " + exception.Message + (exception.InnerException != null ? " / " + exception.InnerException.Message : ""));
					RaiseError(exception.Message);
				}
				Shutdown();
			}
		}

		private void Shutdown()
		{
			lifetime.Cancel();
			try
			{
				stream?.Dispose();
				client?.Close();
			}
			catch (Exception)
			{
			}
		}

		private void RaiseError(string reason)
		{
			MainThreadPump.Run(delegate
			{
				if (!closedRaised)
				{
					closedRaised = true;
					State = WebSocketStates.Closed;
					OnError?.Invoke(this, reason);
				}
			});
		}

		private void RaiseClosed(ushort code, string message)
		{
			MainThreadPump.Run(delegate
			{
				if (!closedRaised)
				{
					closedRaised = true;
					State = WebSocketStates.Closed;
					OnClosed?.Invoke(this, code, message);
				}
			});
		}
	}

	public sealed class WebSocketHandshakeException : Exception
	{
		public WebSocketHandshakeException(string statusLine)
			: base(statusLine)
		{
		}
	}
}
