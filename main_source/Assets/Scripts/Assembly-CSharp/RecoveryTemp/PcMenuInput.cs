#if RECOVERY_PC_MENU
using System.Collections;
using System.IO;
using Rewired;
using UnityEngine;
using UnityEngine.SceneManagement;

// TEMPORARY, test builds only (RECOVERY_PC_MENU is set by RecoveryBuild, never by the shipped game).
// Input now goes through the real path (own Rewired_Core and RewiredStandaloneInputModule).
// This only opens a window, reports scenes that are not in the build, and helps automated checks.
public sealed class PcMenuInput : MonoBehaviour
{
	private const string NotInBuildMessage = "Эта карта появится позже. This map is coming soon.";

	private static PcMenuInput instance;

	private bool autoStarted;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Boot()
	{
		GameObject holder = new GameObject("PcMenuInput");
		DontDestroyOnLoad(holder);
		instance = holder.AddComponent<PcMenuInput>();
#if !UNITY_ANDROID
		Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
#endif
		Application.runInBackground = true;
	}

	// Returns true when the scene is missing from this build, after telling the player why nothing starts.
	public static bool BlockMissingScene(string sceneName)
	{
		string level = UserGameSettings.currentLevel.ToString();
		if (Application.CanStreamedLevelBeLoaded(sceneName) && Application.CanStreamedLevelBeLoaded(level))
		{
			return false;
		}
		Debug.Log("[PcMenuInput] Scene not in this build: " + sceneName + " / " + level);
		MessageBox.Show(NotInBuildMessage);
		return true;
	}

	private void Start()
	{
		string shotDirectory = System.Environment.GetEnvironmentVariable("RECOVERY_MENU_SHOTS");
		if (!string.IsNullOrEmpty(shotDirectory))
		{
			StartCoroutine(TakeShots(shotDirectory));
		}
		string autoLevel = System.Environment.GetEnvironmentVariable("RECOVERY_AUTO_LEVEL");
		if (!string.IsNullOrEmpty(autoLevel))
		{
			StartCoroutine(AutoStart(autoLevel));
		}
		string serverCheck = System.Environment.GetEnvironmentVariable("RECOVERY_SERVER_CHECK");
		if (!string.IsNullOrEmpty(serverCheck))
		{
			StartCoroutine(ServerSettingsCheck(serverCheck));
		}
		if (!string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("RECOVERY_UPA_CHECK")))
		{
			StartCoroutine(PlayerAccountsCheck());
		}
		string mpAuto = System.Environment.GetEnvironmentVariable("RECOVERY_MP_AUTO");
		if (!string.IsNullOrEmpty(mpAuto))
		{
			StartCoroutine(MultiplayerAuto(mpAuto));
		}
		string lookAt = System.Environment.GetEnvironmentVariable("RECOVERY_LOOK_AT");
		if (!string.IsNullOrEmpty(lookAt))
		{
			StartCoroutine(LookAt(lookAt));
		}
	}

	// RECOVERY_LOOK_AT=Hospital:first,second loads the level and screenshots each named object
	// from 1 m in front (E:/eyes/logs/shots/player_<name>.png), then quits.
	private static IEnumerator LookAt(string spec)
	{
		string[] parts = spec.Split(':');
		while (SceneManager.GetActiveScene().name != "Menu")
		{
			yield return null;
		}
		yield return new WaitForSecondsRealtime(2f);
		UserGameSettings.currentLevel = (Level)System.Enum.Parse(typeof(Level), parts[0]);
		UserGameSettings.currentGameMode = GameMode.SINGLE;
		SceneManager.LoadScene(parts[0]);
		yield return new WaitForSecondsRealtime(10f);
		Camera cam = null;
		foreach (Camera c in Camera.allCameras)
		{
			if (c.name == "FPSCamera")
			{
				cam = c;
			}
		}
		foreach (string name in parts[1].Split(','))
		{
			GameObject target = GameObject.Find(name);
			foreach (Renderer r in FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
			{
				if (r.name == name)
				{
					target = r.gameObject;
					break;
				}
			}
			if (target == null || cam == null)
			{
				Debug.Log("[Look] missing " + name + " cam=" + (cam != null));
				continue;
			}
			Renderer rend = target.GetComponent<Renderer>();
			Vector3 center = rend != null ? rend.bounds.center : target.transform.position;
			Transform rig = cam.transform.root;
			foreach (MonoBehaviour mb in rig.GetComponentsInChildren<MonoBehaviour>())
			{
				if (mb is vp_FPCamera || mb is vp_FPController || mb is vp_FPInput)
				{
					mb.enabled = false;
				}
			}
			for (int i = 0; i < 30; i++)
			{
				rig.position = center - target.transform.forward * 1.2f - Vector3.up * 1.6f;
				cam.transform.position = center - target.transform.forward * 1.2f;
				cam.transform.rotation = Quaternion.LookRotation(center - cam.transform.position);
				yield return new WaitForEndOfFrame();
			}
			Debug.Log("[Look] " + name + " material=" + (rend != null && rend.sharedMaterial != null ? rend.sharedMaterial.name + "/" + rend.sharedMaterial.shader.name + " supported=" + rend.sharedMaterial.shader.isSupported : "-") + " lightmap=" + (rend != null ? rend.lightmapIndex : -2) + " lmST=" + (rend != null ? rend.lightmapScaleOffset.ToString() : "-") + " batched=" + (rend != null && rend.isPartOfStaticBatch));
			ScreenCapture.CaptureScreenshot("E:/eyes/logs/shots/player_" + name + ".png");
			yield return new WaitForSecondsRealtime(1f);
		}
		Application.Quit();
	}

	// Reproduces the browser return: a request to the local listener, after one start and after two.
	private static IEnumerator PlayerAccountsCheck()
	{
		string port = null;
		Application.logMessageReceived += delegate(string message, string stack, LogType type)
		{
			System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(message, "localhost%3A([0-9]+)");
			if (match.Success)
			{
				port = match.Groups[1].Value;
			}
		};
		while (SceneManager.GetActiveScene().name != "Menu")
		{
			yield return null;
		}
		yield return new WaitForSecondsRealtime(3f);
		System.Threading.Tasks.Task init = Unity.Services.Core.UnityServices.InitializeAsync();
		while (!init.IsCompleted)
		{
			yield return null;
		}
		Unity.Services.Authentication.PlayerAccounts.IPlayerAccountService accounts = Unity.Services.Authentication.PlayerAccounts.PlayerAccountService.Instance;
		_ = accounts.StartSignInAsync();
		yield return new WaitForSecondsRealtime(3f);
		yield return ProbeListener("one start", port);
		yield return new WaitForSecondsRealtime(1f);
		_ = accounts.StartSignInAsync();
		yield return new WaitForSecondsRealtime(1f);
		_ = accounts.StartSignInAsync();
		yield return new WaitForSecondsRealtime(3f);
		yield return ProbeListener("two starts", port);
		yield return new WaitForSecondsRealtime(1f);
		Application.Quit();
	}

	private static IEnumerator ProbeListener(string label, string port)
	{
		using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Get("http://localhost:" + port + "/callback?error=recovery_probe&state=probe"))
		{
			request.timeout = 5;
			yield return request.SendWebRequest();
			Debug.Log("[Upa] " + label + ": port=" + port + " result=" + request.result + " code=" + request.responseCode + " error=" + request.error);
		}
	}

	// Shows the server window and tries the official backend, then restores the saved choice.
	private static IEnumerator ServerSettingsCheck(string directory)
	{
		while (SceneManager.GetActiveScene().name != "Menu")
		{
			yield return null;
		}
		yield return new WaitForSecondsRealtime(3f);
		bool wasCustom = OnlineConfig.IsCustomServer;
		string host = OnlineConfig.CustomHost;
		int port = OnlineConfig.CustomPort;
		Debug.Log("[Srv] start custom=" + wasCustom + " host=" + host + ":" + port);
		FindAnyObjectByType<MenuContentWindow>().ShowOptions();
		yield return new WaitForSecondsRealtime(1f);
		ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(directory, "settings.png"));
		yield return new WaitForSecondsRealtime(0.5f);
		Debug.Log("[Srv] invalid address accepted=" + OnlineConfig.Select(true, "", 0));
		OnlineConfig.Select(false, "", 0);
		Debug.Log("[Srv] official news=" + GameNewsService.GetAll().Count + " name=" + UserGameSettings.multiplayerUsername + " basic=" + UserGameSettings.IsSignedInWithBasicAuth);
		Debug.Log("[Srv] official url=" + ApiConfiguration.GetBackendHttpUrl("/api/v1/find") + " ws=" + ApiConfiguration.GetBackendWebSocketUrl("g", 0));
		MultiplayerLobby.Show(true);
		yield return new WaitForSecondsRealtime(12f);
		Debug.Log("[Srv] official after fetch news=" + GameNewsService.GetAll().Count + " daily=" + Eyes.DailyChallenges.DailyChallengeRuntime.Refresh().QuestId);
		Debug.Log("[Srv] official canPlay=" + MultiplayerNetworkTest.CanPlay + " signedIn=" + UserGameSettings.isSignedIn + " price=" + UserGameSettings.gamePriceInKeys + " unity=" + Unity.Services.Core.UnityServices.State);
		ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(directory, "official_lobby.png"));
		yield return new WaitForSecondsRealtime(1f);
		MyAuthentication.Show();
		yield return new WaitForSecondsRealtime(2f);
		ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(directory, "login_window.png"));
		yield return new WaitForSecondsRealtime(0.5f);
		FindAnyObjectByType<MyAuthentication>()?.CloseWindow();
		yield return new WaitForSecondsRealtime(1f);
		yield return RunServerTestWindow(directory, "official");
		OnlineConfig.Select(wasCustom, host, port);
		Debug.Log("[Srv] restored custom=" + OnlineConfig.IsCustomServer);
		yield return ServerNews.Fetch();
		Debug.Log("[Srv] news count=" + GameNewsService.GetAll().Count + " unread=" + GameNewsService.HasUnread);
		yield return new WaitForSecondsRealtime(6f);
		Debug.Log("[Srv] daily " + Eyes.DailyChallenges.DailyChallengeRuntime.Refresh().QuestId + " until " + Eyes.DailyChallenges.DailyChallengeRuntime.Refresh().ExpiresAtUtc);
		Debug.Log("[Srv] custom canPlay=" + MultiplayerNetworkTest.CanPlay + " price=" + UserGameSettings.gamePriceInKeys);
		yield return RunServerTestWindow(directory, "custom");
		Debug.Log("[Srv] price after test=" + UserGameSettings.gamePriceInKeys);
		FindAnyObjectByType<MultiplayerLobby>()?.Close();
		yield return new WaitForSecondsRealtime(1f);
		ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(directory, "menu_daily.png"));
		yield return new WaitForSecondsRealtime(0.5f);
		GameNewsWindow.Show();
		yield return new WaitForSecondsRealtime(1.5f);
		ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(directory, "news.png"));
		yield return new WaitForSecondsRealtime(0.5f);
		FindAnyObjectByType<GameNewsWindow>().Close();
		yield return new WaitForSecondsRealtime(0.5f);
		Profile.Show();
		yield return new WaitForSecondsRealtime(3f);
		ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(directory, "profile.png"));
		yield return new WaitForSecondsRealtime(0.5f);
		FindAnyObjectByType<Profile>().SetPanel(1);
		yield return new WaitForSecondsRealtime(3f);
		ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(directory, "profile_stats.png"));
		yield return new WaitForSecondsRealtime(1f);
		Application.Quit();
	}

	private static IEnumerator RunServerTestWindow(string directory, string label)
	{
		ServerTest.Show();
		yield return new WaitForSecondsRealtime(1f);
		ServerTest window = FindAnyObjectByType<ServerTest>();
		window.StartServerTest();
		yield return new WaitForSecondsRealtime(12f);
		UnityEngine.UI.Text info = (UnityEngine.UI.Text)typeof(ServerTest).GetField("infoLabel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(window);
		Debug.Log("[Srv] server test " + label + ": " + info.text.Replace("\n", " | "));
		ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(directory, "servertest_" + label + ".png"));
		yield return new WaitForSecondsRealtime(0.5f);
		if (window != null)
		{
			window.Close();
		}
		yield return new WaitForSecondsRealtime(0.5f);
	}

	// Plays one online round without a person: "host" and "guest" meet through random matchmaking.
	private static IEnumerator MultiplayerAuto(string role)
	{
		string shots = System.Environment.GetEnvironmentVariable("RECOVERY_MP_SHOTS");
		while (SceneManager.GetActiveScene().name != "Menu")
		{
			yield return null;
		}
		yield return new WaitForSecondsRealtime(3f);
		KeysManager.OverwriteMultiplayerKeys(100);
		Debug.Log("[MP] opening lobby as " + role);
		MultiplayerLobby.Show(true);
		MultiplayerLobby lobby = null;
		while (lobby == null)
		{
			yield return null;
			lobby = FindAnyObjectByType<MultiplayerLobby>();
		}
		float deadline = Time.realtimeSinceStartup + 30f;
		while ((!MultiplayerNetworkTest.CanPlay || !UserGameSettings.IsSignedInWithBasicAuth) && Time.realtimeSinceStartup < deadline)
		{
			yield return new WaitForSecondsRealtime(0.5f);
		}
		Debug.Log("[MP] canPlay=" + MultiplayerNetworkTest.CanPlay + " signedIn=" + UserGameSettings.IsSignedInWithBasicAuth + " user=" + UserGameSettings.multiplayerUsername);
		Debug.Log("[MP] daily=" + Eyes.DailyChallenges.DailyChallengeRuntime.Refresh().QuestId + " news=" + GameNewsService.GetAll().Count + " price=" + UserGameSettings.gamePriceInKeys);
		if (!string.IsNullOrEmpty(shots))
		{
			ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(shots, role + "_lobby.png"));
		}
		yield return new WaitForSecondsRealtime(role == "guest" ? 3f : 0.5f);
		string codeFile = System.Environment.GetEnvironmentVariable("RECOVERY_MP_CODE");
		if (string.IsNullOrEmpty(codeFile))
		{
			lobby.CreateCoopGameWithRandom();
		}
		else if (role == "host")
		{
			string enemy = System.Environment.GetEnvironmentVariable("RECOVERY_MP_ENEMY");
			string level = System.Environment.GetEnvironmentVariable("RECOVERY_MP_LEVEL");
			if (!string.IsNullOrEmpty(enemy))
			{
				UserGameSettings.currentEnemy = (Enemy)System.Enum.Parse(typeof(Enemy), enemy);
			}
			if (!string.IsNullOrEmpty(level))
			{
				UserGameSettings.currentLevel = (Level)System.Enum.Parse(typeof(Level), level);
			}
			lobby.CreateVsGameFriend();
			string code = "";
			while (string.IsNullOrEmpty(code))
			{
				yield return new WaitForSecondsRealtime(0.2f);
				UnityEngine.UI.Text label = (UnityEngine.UI.Text)typeof(MultiplayerLobby).GetField("codeLabel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(lobby);
				code = label.text;
			}
			System.IO.File.WriteAllText(codeFile, code);
			Debug.Log("[MP] code " + code);
		}
		else
		{
			while (!System.IO.File.Exists(codeFile) || System.IO.File.ReadAllText(codeFile).Length != 5)
			{
				yield return new WaitForSecondsRealtime(0.2f);
			}
			UnityEngine.UI.InputField join = (UnityEngine.UI.InputField)typeof(MultiplayerLobby).GetField("joinInput", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(lobby);
			join.text = System.IO.File.ReadAllText(codeFile);
			yield return null;
			yield return null;
			lobby.JoinAGame();
		}
		Debug.Log("[MP] searching");
		while (SceneManager.GetActiveScene().name != "Mansion" && SceneManager.GetActiveScene().name != "Hospital" && SceneManager.GetActiveScene().name != "School")
		{
			yield return null;
		}
		Debug.Log("[MP] in " + SceneManager.GetActiveScene().name + " enemy=" + UserGameSettings.currentEnemy + " host=" + UserGameSettings.isMultiplayerHost + " other=" + UserGameSettings.multiplayerSecondPlayerUsername);
		yield return new WaitForSecondsRealtime(6f);
		MultiplayerManager manager = FindAnyObjectByType<MultiplayerManager>();
		System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
		bool ready = (bool)typeof(MultiplayerManager).GetField("arePlayersReady", flags).GetValue(manager);
		CustomSingleton<PauseManager>.Instance.PauseOff();
		yield return new WaitForSecondsRealtime(1f);
		Debug.Log("[MP] started=" + ready + " bags=" + UserGameSettings.bagsCollected + "/" + UserGameSettings.bagsToCollect + " mode=" + UserGameSettings.currentGameMode);
		if (!string.IsNullOrEmpty(shots))
		{
			ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(shots, role + "_game.png"));
		}
		foreach (Camera cam in Camera.allCameras)
		{
			Debug.Log("[MP] camera " + cam.name + " parent=" + (cam.transform.parent != null ? cam.transform.parent.name : "-") + " pos=" + cam.transform.position + " depth=" + cam.depth + " far=" + cam.farClipPlane + " mask=" + cam.cullingMask + " rect=" + cam.rect + " target=" + (cam.targetTexture != null ? cam.targetTexture.name : "-"));
		}
		foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
		{
			if (canvas.isRootCanvas)
			{
				Debug.Log("[MP] canvas " + canvas.name + " enabled=" + canvas.enabled + " active=" + canvas.gameObject.activeInHierarchy + " order=" + canvas.sortingOrder);
			}
		}
		GameObject[] sharedBags = GameObject.FindGameObjectsWithTag("MoneyBag");
		Debug.Log("[MP] active bags=" + sharedBags.Length);
		if (role == "host")
		{
			for (int i = 0; i < 3 && i < sharedBags.Length; i++)
			{
				sharedBags[i].GetComponent<MoneyBag>().Clicked(false);
				yield return new WaitForSecondsRealtime(1f);
			}
		}
		else
		{
			yield return new WaitForSecondsRealtime(3f);
		}
		yield return new WaitForSecondsRealtime(1f);
		Debug.Log("[MP] after host bags: collected=" + UserGameSettings.bagsCollected + " mine=" + UserGameSettings.myBagsCollected + " active=" + GameObject.FindGameObjectsWithTag("MoneyBag").Length);
		Transform player = CustomSingleton<PlayerManager>.Instance.Player.transform;
		Vector3 start = player.position;
		for (int i = 0; i < 8; i++)
		{
			player.position = start + player.forward * (i * 0.5f);
			yield return new WaitForSecondsRealtime(1f);
			Debug.Log("[MP] me=" + player.position + " avatar=" + manager.avatar.transform.position + " active=" + manager.avatar.activeInHierarchy);
		}
		CustomSingleton<PauseManager>.Instance.PauseOff();
		yield return new WaitForSecondsRealtime(0.5f);
		if (!string.IsNullOrEmpty(shots))
		{
			ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(shots, role + "_moved.png"));
		}
		if (role == "host")
		{
			Debug.Log("[MP] host sends win");
			CustomSingleton<MultiplayerSettings>.Instance.SendFinishGame(GameFinishedReason.HOST_WON);
		}
		yield return new WaitForSecondsRealtime(6f);
		bool finished = (bool)typeof(MultiplayerManager).GetField("isGameFinished", flags).GetValue(manager);
		Debug.Log("[MP] finished=" + finished);
		if (!string.IsNullOrEmpty(shots))
		{
			ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(shots, role + "_summary.png"));
		}
		yield return new WaitForSecondsRealtime(2f);
		if (!string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("RECOVERY_MP_AGAIN")))
		{
			Debug.Log("[MP] play again");
			FindAnyObjectByType<MultiplayerGui>().SendPlayAgainReady();
			float until = Time.realtimeSinceStartup + 40f;
			Scene first = SceneManager.GetActiveScene();
			while ((SceneManager.GetActiveScene() == first || SceneManager.GetActiveScene().name != "Mansion") && Time.realtimeSinceStartup < until)
			{
				yield return new WaitForSecondsRealtime(0.5f);
			}
			Debug.Log("[MP] second round scene=" + SceneManager.GetActiveScene().name);
			yield return new WaitForSecondsRealtime(8f);
			MultiplayerManager second = FindAnyObjectByType<MultiplayerManager>();
			Debug.Log("[MP] second round started=" + (second != null && (bool)typeof(MultiplayerManager).GetField("arePlayersReady", flags).GetValue(second)));
			if (!string.IsNullOrEmpty(shots))
			{
				ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(shots, role + "_again.png"));
			}
			yield return new WaitForSecondsRealtime(1f);
		}
		Debug.Log("[MP] done");
		Application.Quit();
	}

	private IEnumerator AutoStart(string level)
	{
		while (SceneManager.GetActiveScene().name != "Menu")
		{
			yield return null;
		}
		yield return new WaitForSecondsRealtime(3f);
		Debug.Log("[PcMenuInput] Auto start " + level);
		SceneManager.LoadScene(level);
		yield return new WaitForSecondsRealtime(4f);
		yield return new WaitForEndOfFrame();
		LogRenderState();
		if (!string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("RECOVERY_AUTO_TEST")))
		{
			yield return RunGameplayChecks();
		}
		while (true)
		{
			yield return new WaitForSecondsRealtime(5f);
			LogInputState();
		}
	}

	// Presses things the way the input code would, so bags, journal and flashlight are checked without a person.
	private static IEnumerator RunGameplayChecks()
	{
		LightingSettings.SetMode(LightingMode.Flashlight, false);
		LightingSettings.SetBrightness(1f, false);
		GameObject[] bags = GameObject.FindGameObjectsWithTag("MoneyBag");
		Debug.Log("[Check] active bags=" + bags.Length + " target=" + UserGameSettings.bagsToCollect + " tutorial=" + TutorialManager.ShouldPlayTutorial());
		JournalWindow journal = CustomSingleton<JournalWindow>.Instance;
		Debug.Log("[Check] journal before bags: canOpen=" + journal.CanOpenJournal() + " anyUnlocked=" + PlayerPrefz.GetBool("WAS_ANY_JOURNAL_ENTRY_UNLOCKED", false));
		for (int i = 0; i < 4 && i < bags.Length; i++)
		{
			bags[i].GetComponent<MoneyBag>().Clicked(false);
			yield return new WaitForSecondsRealtime(0.3f);
		}
		Debug.Log("[Check] bagsCollected=" + UserGameSettings.bagsCollected + " activeNow=" + GameObject.FindGameObjectsWithTag("MoneyBag").Length);
		Debug.Log("[Check] journal after bags: canOpen=" + journal.CanOpenJournal() + " anyUnlocked=" + PlayerPrefz.GetBool("WAS_ANY_JOURNAL_ENTRY_UNLOCKED", false) + " windowActive=" + journal.isActiveAndEnabled + " startActions=" + GameplayInputContext.Shared.Current.StartActions + " ctx=" + GameplayInputContext.Shared.Current.Context + " journalTutorial=" + PlayerPrefz.GetBool("JOURNAL_TUTORIAL_COMPLETED", false) + " manager=" + (CustomSingleton<JournalManager>.Instance != null ? CustomSingleton<JournalManager>.Instance.isActiveAndEnabled.ToString() : "none") + " unlockedIds=" + JournalProgressStore.GetUnlockedIds().Count + " listeners19=" + (FindFirstObjectByType<JournalManager>(FindObjectsInactive.Include) != null));
		Debug.Log("[Check] flashlight color=" + Shader.GetGlobalColor("_LightColor0X") + " pos=" + Shader.GetGlobalVector("_WorldSpaceLightPos0X") + " cookie=" + Shader.GetGlobalTexture("_LightTexture0X") + " mix=" + Shader.GetGlobalVector("_FlashlightMixWeights"));
		yield return new WaitForSecondsRealtime(4f);
		ScreenCapture.CaptureScreenshot("E:/eyes/logs/check_flashlight.png");
		yield return new WaitForSecondsRealtime(1f);
		journal.ShowJournal();
		Debug.Log("[Check] journal open=" + journal.IsOpen());
		yield return new WaitForSecondsRealtime(2f);
		ScreenCapture.CaptureScreenshot("E:/eyes/logs/check_journal.png");
		yield return new WaitForSecondsRealtime(1f);
		journal.CloseJournal();
	}

	private static void LogRenderState()
	{
		Debug.Log("[Diag] gpu=" + SystemInfo.graphicsDeviceType + " astc=" + SystemInfo.SupportsTextureFormat(TextureFormat.ASTC_4x4) + " lightmaps=" + LightmapSettings.lightmaps.Length + " mode=" + LightmapSettings.lightmapsMode + " ambient=" + RenderSettings.ambientLight + "/" + RenderSettings.ambientMode + " colorSpace=" + QualitySettings.activeColorSpace);
		for (int i = 0; i < LightmapSettings.lightmaps.Length; i++)
		{
			Texture2D map = LightmapSettings.lightmaps[i].lightmapColor;
			if (map == null)
			{
				Debug.Log("[Diag] lightmap " + i + " null");
				continue;
			}
			RenderTexture rt = RenderTexture.GetTemporary(64, 64, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			Graphics.Blit(map, rt);
			RenderTexture old = RenderTexture.active;
			RenderTexture.active = rt;
			Texture2D read = new Texture2D(64, 64, TextureFormat.RGBA32, false, true);
			read.ReadPixels(new Rect(0f, 0f, 64f, 64f), 0, 0);
			RenderTexture.active = old;
			RenderTexture.ReleaseTemporary(rt);
			float sum = 0f;
			foreach (Color c in read.GetPixels())
			{
				sum += c.r + c.g + c.b;
			}
			Debug.Log("[Diag] lightmap " + i + " " + map.name + " fmt=" + map.format + " size=" + map.width + " avg=" + sum / (3f * 64f * 64f));
		}
		int total = 0;
		int mapped = 0;
		System.Collections.Generic.Dictionary<string, int> shaders = new System.Collections.Generic.Dictionary<string, int>();
		foreach (Renderer renderer in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
		{
			total++;
			if (renderer.lightmapIndex >= 0 && renderer.lightmapIndex < 65534)
			{
				mapped++;
			}
			Material material = renderer.sharedMaterial;
			string key = (material != null && material.shader != null) ? (material.shader.name + (material.shader.isSupported ? "" : "!UNSUPPORTED")) : "null";
			shaders.TryGetValue(key, out int count);
			shaders[key] = count + 1;
		}
		string list = "";
		foreach (System.Collections.Generic.KeyValuePair<string, int> pair in shaders)
		{
			list += pair.Key + "=" + pair.Value + "; ";
		}
		Debug.Log("[Diag] renderers=" + total + " lightmapped=" + mapped + " shaders: " + list);
		ResolvedLighting r = EyesLightingComposer.Current;
		Debug.Log("[Diag] lighting mode=" + LightingSettings.Mode + " brightness=" + LightingSettings.BrightnessMultiplier + " lmScale=" + r.lmScale + " flash=" + r.flashlightIntensity + " floor=" + r.flashlightFloor + " reveal=" + r.revealNormalize + " mix=" + r.flashlightMixWeights + " ambient=" + r.ambientColor + " falloff=" + r.falloffStart + ".." + r.falloffEnd + " color=" + r.falloffColor + " desat=" + r.desaturationLevel + " profile=" + (EyesLightingComposer.Inputs.profile != null ? EyesLightingComposer.Inputs.profile.name : "none") + " g_LM=" + Shader.GetGlobalFloat("_LMScale") + " g_Bright=" + Shader.GetGlobalFloat("_ShaderBrightness"));
		foreach (Camera camera in Camera.allCameras)
		{
			Debug.Log("[Diag] camera " + camera.name + " rect=" + camera.rect + " depth=" + camera.depth + " clear=" + camera.clearFlags + " bg=" + camera.backgroundColor + " target=" + (camera.targetTexture != null ? camera.targetTexture.name : "screen") + " mask=" + camera.cullingMask);
		}
		foreach (Renderer renderer in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
		{
			Material material = renderer.sharedMaterial;
			if (renderer.isVisible && (material == null || material.shader.name.StartsWith("Standard") || material.shader.name.StartsWith("Unlit") || material.shader.name.StartsWith("Particles")))
			{
				Debug.Log("[Diag] plain renderer " + renderer.name + " parent=" + (renderer.transform.parent != null ? renderer.transform.parent.name : "-") + " material=" + (material != null ? material.name : "null") + " shader=" + (material != null ? material.shader.name : "-"));
			}
		}
		Vector2 probe = new Vector2(Screen.width * 0.8f, Screen.height * 0.2f);
		foreach (UnityEngine.UI.Graphic graphic in FindObjectsByType<UnityEngine.UI.Graphic>(FindObjectsSortMode.None))
		{
			bool drawn = graphic.isActiveAndEnabled;
			for (Canvas c = graphic.canvas; drawn && c != null; c = (c.transform.parent != null) ? c.transform.parent.GetComponentInParent<Canvas>() : null)
			{
				drawn = c.enabled;
			}
			if (drawn && RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, probe, graphic.canvas != null && graphic.canvas.renderMode != RenderMode.ScreenSpaceOverlay ? graphic.canvas.worldCamera : null))
			{
				string path = graphic.name;
				for (Transform t = graphic.transform.parent; t != null; t = t.parent)
				{
					path = t.name + "/" + path;
				}
				Debug.Log("[Diag] graphic over probe: " + path + " type=" + graphic.GetType().Name + " color=" + graphic.color + " crAlpha=" + graphic.canvasRenderer.GetAlpha() + " raycast=" + graphic.raycastTarget);
			}
		}
	}

	private static void LogInputState()
	{
		GameplayInputContext.Permissions state = GameplayInputContext.Shared.Current;
		ControlModeManager manager = ControlModeManager.Instance;
		Player player = ReInput.isReady ? ReInput.players.GetPlayer(0) : null;
		int enabledMaps = 0;
		if (player != null)
		{
			foreach (ControllerMap map in player.controllers.maps.GetAllMaps())
			{
				if (map.enabled)
				{
					enabledMaps++;
				}
			}
		}
		Debug.Log("[PcMenuInput] scene=" + SceneManager.GetActiveScene().name + " ctx=" + state.Context + " mapCat=" + state.MapCategory + " move=" + state.Move + " look=" + state.Look + " mode=" + ((manager != null) ? manager.Mode.ToString() : "none") + " focus=" + (manager != null && manager.HasFocus) + " recovery=" + (manager != null && manager.RequiresRecoveryUI) + " enabledMaps=" + enabledMaps + " cursor=" + Cursor.lockState + " timeScale=" + Time.timeScale);
	}

	private IEnumerator TakeShots(string directory)
	{
		Directory.CreateDirectory(directory);
		for (int i = 0; i < 40; i++)
		{
			yield return new WaitForSecondsRealtime(5f);
			ScreenCapture.CaptureScreenshot(Path.Combine(directory, "shot_" + i.ToString("00") + ".png"));
		}
	}
}
#endif
