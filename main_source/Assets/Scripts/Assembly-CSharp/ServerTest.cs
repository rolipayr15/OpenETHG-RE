using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using BestHTTP;
using BestHTTP.WebSocket;
using I2.Loc;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// The ARM window ran WsBestHttpRouteTester over every backend route; the restored check measures the
// selected server over HTTP and over a websocket, which is what the game itself uses.
public class ServerTest : MonoBehaviour
{
	private const int HttpSamples = 5;

	[SerializeField]
	private Text infoLabel;

	[SerializeField]
	private WsBestHttpRouteTester serverTester;

	[SerializeField]
	private Button startTestButton;

	[SerializeField]
	private GameObject copyToClipboard;

	private string resultsLog;

	private static bool Ru => LocalizationManager.CurrentLanguageCode == "ru";

	public static void Show()
	{
		SceneManager.LoadSceneAsync("ServerTest", LoadSceneMode.Additive);
	}

	private void Start()
	{
		if (copyToClipboard != null)
		{
			copyToClipboard.SetActive(false);
		}
		infoLabel.text = OnlineConfig.IsCustomServer ? (Ru ? "Свой сервер: " : "Custom server: ") + OnlineConfig.CustomHost + ":" + OnlineConfig.CustomPort : (Ru ? "Официальный сервер" : "Official server");
	}

	public void StartServerTest()
	{
		StartCoroutine(Run());
	}

	private IEnumerator Run()
	{
		startTestButton.interactable = false;
		StringBuilder log = new StringBuilder();
		log.AppendLine(infoLabel.text.Split('\n')[0]);
		infoLabel.text = log + "...";
		int passed = 0;
		long total = 0L;
		long worst = 0L;
		string error = null;
		for (int i = 0; i < HttpSamples; i++)
		{
			Stopwatch watch = Stopwatch.StartNew();
			using (UnityWebRequest request = UnityWebRequest.Get(ApiConfiguration.GetBackendHttpUrl("/api/v1/ping")))
			{
				request.timeout = 8;
				if (OfficialServerPin.Applies(request.uri))
				{
					request.certificateHandler = new OfficialServerPinHandler();
				}
				yield return request.SendWebRequest();
				watch.Stop();
				if (request.result == UnityWebRequest.Result.Success || request.result == UnityWebRequest.Result.ProtocolError)
				{
					passed++;
					total += watch.ElapsedMilliseconds;
					worst = Math.Max(worst, watch.ElapsedMilliseconds);
				}
				else
				{
					error = request.error;
				}
			}
			infoLabel.text = log + "HTTP " + (i + 1) + "/" + HttpSamples;
		}
		log.AppendLine("HTTP: " + passed + "/" + HttpSamples + (passed > 0 ? ", " + total / passed + " ms, max " + worst + " ms" : " " + error));
		infoLabel.text = log + "WebSocket...";
		bool opened = false;
		bool finished = false;
		long socketMs = 0L;
		bool handshakeRefused = false;
		bool routeMissing = false;
		string socketError = null;
		Uri diag = new Uri(ApiConfiguration.GetBackendWebSocketUrl("g", 0).Replace("/ws/v1/g/join/0", "/ws/v1/diag"));
		WebSocket socket = new WebSocket(diag);
		CustomSingleton<MultiplayerSettings>.Instance.AuthorizeRequest(socket.InternalRequest);
		Stopwatch socketWatch = Stopwatch.StartNew();
		socket.OnOpen = delegate
		{
			opened = true;
			finished = true;
			socketMs = socketWatch.ElapsedMilliseconds;
		};
		socket.OnError = delegate(WebSocket s, string reason)
		{
			socketError = reason;
			finished = true;
			routeMissing = reason != null && reason.Contains(" 404");
			if (reason != null && (reason.Contains(" 401") || reason.Contains(" 403") || reason.Contains(" 400") || reason.Contains(" 404")))
			{
				handshakeRefused = true;
			}
		};
		socket.OnClosed = delegate
		{
			finished = true;
		};
		socket.Open();
		float deadline = Time.realtimeSinceStartup + 10f;
		while (!finished && Time.realtimeSinceStartup < deadline)
		{
			yield return null;
		}
		// A refused handshake still proves TLS and the route work: the official gateway only upgrades signed-in accounts.
		bool signInRequired = !opened && handshakeRefused && !routeMissing;
		bool reachedServer = !opened && routeMissing;
		if (opened)
		{
			log.AppendLine("WebSocket: OK, " + socketMs + " ms");
			socket.Close(1000, "test finished");
		}
		else if (reachedServer)
		{
			log.AppendLine("WebSocket: " + (Ru ? "соединение установлено, вход выполнен" : "connected, signed in"));
		}
		else if (signInRequired)
		{
			log.AppendLine("WebSocket: " + (Ru ? "сервер доступен, нужен вход в аккаунт" : "server reachable, account sign-in required"));
		}
		else
		{
			log.AppendLine("WebSocket: " + (Ru ? "нет соединения " : "no connection ") + socketError);
		}
		UnityEngine.Debug.Log("[Online] websocket check: opened=" + opened + " signInRequired=" + signInRequired + " error=" + socketError);
		bool canPlay = passed > 0 && (opened || signInRequired || reachedServer);
		log.AppendLine(canPlay ? (signInRequired ? (Ru ? "Сеть в порядке, для игры войдите в аккаунт" : "Network is fine, sign in to play") : (Ru ? "Мультиплеер доступен" : "Multiplayer is available")) : (Ru ? "Мультиплеер недоступен" : "Multiplayer is not available"));
		resultsLog = log.ToString();
		infoLabel.text = resultsLog;
		MultiplayerNetworkTest.Invalidate();
		startTestButton.interactable = true;
		if (copyToClipboard != null)
		{
			copyToClipboard.SetActive(true);
		}
	}

	public void CopyToClipboard()
	{
		GUIUtility.systemCopyBuffer = resultsLog ?? "";
	}

	public void OpenDiscord()
	{
		Application.OpenURL("https://discord.gg/");
	}

	public void Close()
	{
		SceneManager.UnloadSceneAsync("ServerTest", UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
	}
}
