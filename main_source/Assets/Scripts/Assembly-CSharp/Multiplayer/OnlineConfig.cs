using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Prime31.MessageKit;
using UnityEngine;
using UnityEngine.SceneManagement;

// Recovery addition: chooses between the official backend and a custom server instance.
// A forced address (EYES_SERVER, server.txt beside the player) wins over the setting.
// Resources/online_server is the address a test build starts with.
public static class OnlineConfig
{
	private const string ModeKey = "MP_SERVER_CUSTOM";

	private const string HostKey = "MP_SERVER_HOST";

	private const string PortKey = "MP_SERVER_PORT";

	public const int DefaultPort = 8080;

	private const int NormalGamePrice = 10;

	private static bool applied;

	private static string forcedServer;

	public static bool RemoteReviewerFlag;

	// Raised after the active server changes; everything bound to a server refreshes from this.
	public static event Action ServerChanged;

	private const string CustomNameKey = "MP_CUSTOM_NAME";

	private static string NameKey => IsCustomServer ? CustomNameKey : "Username";

	// News files: the official archive keeps its original name.
	public static string NewsArchivePrefix => IsCustomServer ? "custom_" + Hash(CustomHost + ":" + CustomPort).Substring(0, 8) + "_" : "";

	public static bool IsCustomServer { get; private set; }

	public static string CustomHost { get; private set; } = "";

	public static int CustomPort { get; private set; } = DefaultPort;

	public static bool IsForced => !string.IsNullOrEmpty(forcedServer);

	public static bool UseDeviceAccount => IsCustomServer;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void Apply()
	{
		if (applied)
		{
			UserGameSettings.adsDisabledViaRemoteConfig = IsCustomServer || RemoteReviewerFlag;
			return;
		}
		applied = true;
		forcedServer = ReadForcedServer();
		TextAsset asset = Resources.Load<TextAsset>("online_server");
		string buildDefault = asset != null ? asset.text.Trim() : "";
		string initial = IsForced ? forcedServer : buildDefault;
		if (Uri.TryCreate(initial, UriKind.Absolute, out Uri uri))
		{
			CustomHost = uri.Host;
			CustomPort = uri.Port;
		}
		bool custom = IsForced || !string.IsNullOrEmpty(buildDefault);
		if (!IsForced)
		{
			custom = PlayerPrefs.GetInt(ModeKey, custom ? 1 : 0) == 1;
			CustomHost = PlayerPrefs.GetString(HostKey, CustomHost);
			CustomPort = PlayerPrefs.GetInt(PortKey, CustomPort);
		}
		Activate(custom);
	}

	// Saves a choice made in the settings window. Returns false when the address cannot be used.
	public static bool Select(bool custom, string host, int port)
	{
		host = (host ?? "").Trim();
		if (custom && (host.Length == 0 || port < 1 || port > 65535 || Uri.CheckHostName(host) == UriHostNameType.Unknown))
		{
			return false;
		}
		PlayerPrefs.SetInt(ModeKey, custom ? 1 : 0);
		if (custom)
		{
			CustomHost = host;
			CustomPort = port;
			PlayerPrefs.SetString(HostKey, host);
			PlayerPrefs.SetInt(PortKey, port);
		}
		PlayerPrefs.Save();
		forcedServer = null;
		Activate(custom);
		RefreshServerState();
		return true;
	}

	// Drops everything that belonged to the previous server and reconnects to the current one.
	private static void RefreshServerState()
	{
		UserGameSettings.stats = null;
		GameNewsService.ReloadForCurrentServer();
		if (CustomSingleton<MultiplayerSettings>.HasInstance)
		{
			MultiplayerSettings net = CustomSingleton<MultiplayerSettings>.Instance;
			net.Clear();
			net.CloseWebsocket();
			if (IsCustomServer)
			{
				int attempts = 3;
				net.StartCoroutine(AutoSignIn(() => attempts-- > 0));
			}
			net.StartCoroutine(ServerNews.Fetch());
		}
		MessageKit.post(128);
		ServerChanged?.Invoke();
		Eyes.DailyChallenges.DailyChallengeRuntime.Refresh();
	}

	private static void Activate(bool custom)
	{
		bool changed = custom != IsCustomServer;
		IsCustomServer = custom;
		LoadPlayerName();
		if (custom)
		{
			string server = IsForced ? forcedServer : "http://" + CustomHost + ":" + CustomPort;
			ApiConfiguration.SetServer(server);
			Debug.Log("[Online] custom server " + server);
		}
		else
		{
			ApiConfiguration.UseOfficialServers();
			Debug.Log("[Online] official server");
		}
		UserGameSettings.adsDisabledViaRemoteConfig = custom || RemoteReviewerFlag;
		UserGameSettings.gamePriceInKeys = RecoveryTestBuild.FreeEverything ? 0 : NormalGamePrice;
		MultiplayerNetworkTest.Invalidate();
		if (UserGameSettings.IsSignedInWithBasicAuth || changed)
		{
			MultiplayerSettings.ClearReviewerBasicAuth();
		}
	}

	private static void LoadPlayerName()
	{
		string name = PlayerPrefs.GetString(NameKey, "");
		string suffix = Environment.GetEnvironmentVariable("EYES_PLAYER_SUFFIX");
		if (IsCustomServer && (string.IsNullOrEmpty(name) || !string.IsNullOrEmpty(suffix)))
		{
			name = "Player" + Hash(SystemInfo.deviceUniqueIdentifier + suffix).Substring(0, 4).ToUpperInvariant();
		}
		UserGameSettings.multiplayerUsername = name;
	}

	public static void SavePlayerName(string name)
	{
		UserGameSettings.multiplayerUsername = name;
		PlayerPrefs.SetString(NameKey, name);
		PlayerPrefs.Save();
	}

	// The server's test mode: multiplayer does not spend keys while it is on.
	public static void ApplyServerRules(string pingJson)
	{
		bool free = pingJson != null && pingJson.Replace(" ", "").Contains("\"free_keys\":true");
		UserGameSettings.gamePriceInKeys = free || RecoveryTestBuild.FreeEverything ? 0 : NormalGamePrice;
	}

	private static string ReadForcedServer()
	{
		string value = Environment.GetEnvironmentVariable("EYES_SERVER");
		if (!string.IsNullOrEmpty(value))
		{
			return value.Trim();
		}
		try
		{
			string path = Path.Combine(Path.GetDirectoryName(Application.dataPath) ?? "", "server.txt");
			if (File.Exists(path))
			{
				return File.ReadAllText(path).Trim();
			}
		}
		catch (Exception exception)
		{
			Debug.LogWarning(exception.Message);
		}
		return null;
	}

	public static string DeviceCredential()
	{
		string seed = SystemInfo.deviceUniqueIdentifier + Environment.GetEnvironmentVariable("EYES_PLAYER_SUFFIX");
		string hash = Hash("eyes-device-account:" + seed);
		return Convert.ToBase64String(Encoding.UTF8.GetBytes("d" + hash.Substring(0, 16) + ":" + hash.Substring(16)));
	}

	// The device account on a custom server; retried until the server answers.
	public static System.Collections.IEnumerator AutoSignIn(Func<bool> keepTrying)
	{
		while (IsCustomServer && !UserGameSettings.IsSignedInWithBasicAuth && keepTrying())
		{
			string credential = DeviceCredential();
			BestHTTP.HTTPRequest request = CustomSingleton<MultiplayerSettings>.Instance.SendReviewerBasicAuthLogin(credential);
			while (request.State < BestHTTP.HTTPRequestStates.Finished)
			{
				yield return null;
			}
			if (request.State == BestHTTP.HTTPRequestStates.Finished && request.Response != null && request.Response.IsSuccess)
			{
				MultiplayerSettings.ActivateReviewerBasicAuth(credential);
				yield break;
			}
			yield return new WaitForSecondsRealtime(5f);
		}
	}

	public static bool IsSceneInBuild(string scene)
	{
		for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
		{
			if (Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i)) == scene)
			{
				return true;
			}
		}
		return false;
	}

	private static string Hash(string value)
	{
		using (SHA256 sha = SHA256.Create())
		{
			StringBuilder builder = new StringBuilder();
			foreach (byte b in sha.ComputeHash(Encoding.UTF8.GetBytes(value)))
			{
				builder.Append(b.ToString("x2"));
			}
			return builder.ToString();
		}
	}
}
