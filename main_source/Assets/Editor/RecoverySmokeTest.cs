using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Plays the game from the first scene in batch mode and writes every error and the scenes reached to a report.
/// Unity -batchmode -projectPath . -executeMethod RecoverySmokeTest.Run
/// </summary>
public static class RecoverySmokeTest
{
	private const string ActiveKey = "RecoverySmokeTest.Active";

	private const string StartedKey = "RecoverySmokeTest.Started";

	private const double MenuSeconds = 45.0;

	private const double SceneSeconds = 60.0;

	private static string[] queue;

	private static int monstersWoken;

	private static readonly Dictionary<Characters.States.EnemyStateController, Vector3> MonsterStart = new Dictionary<Characters.States.EnemyStateController, Vector3>();

	private static readonly List<string> MonsterReport = new List<string>();

	private static int shotTaken = -1;

	private static string lastStageScene = "Menu";

	private static int stage;

	private static double stageEnd;

	private static readonly Dictionary<string, int> Errors = new Dictionary<string, int>();

	private static readonly List<string> ScenesSeen = new List<string>();

	public static void Run()
	{
		// RECOVERY_SMOKE_SCENES=Mansion,Hospital plays those scenes one after another after the menu.
		string scenes = System.Environment.GetEnvironmentVariable("RECOVERY_SMOKE_SCENES");
		queue = string.IsNullOrEmpty(scenes) ? new string[0] : scenes.Split(',');
		stage = 0;
		stageEnd = EditorApplication.timeSinceStartup + MenuSeconds;
		Directory.CreateDirectory("E:/eyes/logs/shots");
		EditorSceneManager.OpenScene("Assets/scenes/Init.unity");
		SessionState.SetBool(ActiveKey, true);
		SessionState.SetFloat(StartedKey, (float)EditorApplication.timeSinceStartup);
		// The project enters play mode without a domain reload, so the static constructor will not run again.
		Application.logMessageReceived += OnLog;
		UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
		EditorApplication.update += OnUpdate;
		EditorApplication.isPlaying = true;
	}

	// Batch mode has no game view, so cameras and overlay canvases are rendered into a texture by hand.
	private static void Capture(string path)
	{
		RenderTexture target = RenderTexture.GetTemporary(1280, 720, 24);
		GameObject helper = new GameObject("SmokeUiCamera");
		Camera uiCamera = helper.AddComponent<Camera>();
		uiCamera.clearFlags = CameraClearFlags.Nothing;
		uiCamera.cullingMask = ~0;
		uiCamera.targetTexture = target;
		List<Canvas> overlays = new List<Canvas>();
		foreach (Canvas canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
		{
			if (canvas.isRootCanvas && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
			{
				overlays.Add(canvas);
				canvas.renderMode = RenderMode.ScreenSpaceCamera;
				canvas.worldCamera = uiCamera;
				canvas.planeDistance = 1f;
			}
		}
		Canvas.ForceUpdateCanvases();
		RenderTexture previous = RenderTexture.active;
		RenderTexture.active = target;
		GL.Clear(true, true, Color.black);
		RenderTexture.active = previous;
		List<Camera> cameras = new List<Camera>(Camera.allCameras);
		cameras.Remove(uiCamera);
		cameras.Sort((Camera a, Camera b) => a.depth.CompareTo(b.depth));
		foreach (Camera camera in cameras)
		{
			RenderTexture original = camera.targetTexture;
			camera.targetTexture = target;
			camera.Render();
			camera.targetTexture = original;
		}
		uiCamera.Render();
		RenderTexture.active = target;
		Texture2D image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
		image.ReadPixels(new Rect(0f, 0f, 1280f, 720f), 0, 0);
		image.Apply();
		RenderTexture.active = previous;
		File.WriteAllBytes(path, image.EncodeToJPG(80));
		foreach (Canvas canvas in overlays)
		{
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		}
		Object.DestroyImmediate(image);
		Object.DestroyImmediate(helper);
		RenderTexture.ReleaseTemporary(target);
	}

	private static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
	{
		ScenesSeen.Add(((int)(EditorApplication.timeSinceStartup - SessionState.GetFloat(StartedKey, 0f))) + "s " + scene.name + " (" + mode + ")");
	}

	private static void OnLog(string message, string stackTrace, LogType type)
	{
		if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
		{
			string[] lines = stackTrace.Split('\n');
			string key = "[" + lastStageScene + "] " + type + ": " + message + " @ " + (lines.Length > 0 ? lines[0] : string.Empty);
			Errors.TryGetValue(key, out int count);
			Errors[key] = count + 1;
		}
	}

	// Wakes the monster halfway through a level scene, then reports whether it actually moved.
	private static void ProbeMonsters()
	{
		if (stage == 0 || monstersWoken == stage || EditorApplication.timeSinceStartup < stageEnd - SceneSeconds / 2.0)
		{
			return;
		}
		monstersWoken = stage;
		MonsterStart.Clear();
		foreach (Characters.States.EnemyStateController controller in Object.FindObjectsByType<Characters.States.EnemyStateController>(FindObjectsSortMode.None))
		{
			MonsterStart[controller] = controller.transform.position;
		}
		if (MonsterStart.Count > 0)
		{
			Prime31.MessageKit.MessageKit.post(25);
		}
	}

	private static void ReportAudio()
	{
		DarkTonic.MasterAudio.MasterAudioGroup group = DarkTonic.MasterAudio.MasterAudio.GrabGroup("fx", false);
		DarkTonic.MasterAudio.PlaySoundResult result = DarkTonic.MasterAudio.MasterAudio.PlaySound("fx");
		DarkTonic.MasterAudio.SoundGroupVariation voice = (result != null) ? result.ActingVariation : null;
		MonsterReport.Add("Monster audio in " + queue[stage - 1] + ": ready=" + DarkTonic.MasterAudio.MasterAudio.SoundsReady + " fxVoices=" + ((group != null) ? group.TotalVoices : -1) + " played=" + (result != null && result.SoundPlayed) + " voice=" + ((voice != null) ? voice.name : "none") + " clip=" + ((voice != null && voice.VarAudio.clip != null) ? voice.VarAudio.clip.name : "none") + " playing=" + (voice != null && voice.VarAudio.isPlaying) + " active=" + ((group != null) ? group.ActiveVoices : -1));
	}

	private static void ReportInput()
	{
		Rewired.Player player = Rewired.ReInput.isReady ? Rewired.ReInput.players.GetPlayer(0) : null;
		int maps = 0;
		int enabledMaps = 0;
		string controllers = "";
		if (player != null)
		{
			foreach (Rewired.ControllerMap map in player.controllers.maps.GetAllMaps())
			{
				maps++;
				if (map.enabled)
				{
					enabledMaps++;
				}
			}
			foreach (Rewired.Controller controller in player.controllers.Controllers)
			{
				controllers += controller.name + ",";
			}
		}
		MonsterReport.Add("Monster input in " + queue[stage - 1] + ": ready=" + Rewired.ReInput.isReady + " player=" + (player != null ? player.name : "none") + " maps=" + maps + " enabled=" + enabledMaps + " controllers=" + controllers + " touch=" + (Object.FindAnyObjectByType<Rewired.ComponentControls.TouchJoystick>(FindObjectsInactive.Include) != null) + " mode=" + (ControlModeManager.Instance != null ? ControlModeManager.Instance.Mode.ToString() : "none") + " move=" + GameplayInputContext.Shared.Current.Move + " look=" + GameplayInputContext.Shared.Current.Look + " ctx=" + GameplayInputContext.Shared.Current.Context + " mapCat=" + GameplayInputContext.Shared.Current.MapCategory + " top=" + (GameplayInputContext.Shared.Current.TopOwner != null ? GameplayInputContext.Shared.Current.TopOwner.ToString() : "none") + " focus=" + (ControlModeManager.Instance != null && ControlModeManager.Instance.HasFocus) + " recovery=" + (ControlModeManager.Instance != null && ControlModeManager.Instance.RequiresRecoveryUI) + " explicit=" + (ControlModeManager.Instance != null && ControlModeManager.Instance.RequiresExplicitResume));
	}

	private static string MeshInfo(Renderer r)
	{
		MeshFilter mf = r.GetComponent<MeshFilter>();
		if (mf == null || mf.sharedMesh == null) return "-";
		return mf.sharedMesh.name + "|" + AssetDatabase.GetAssetPath(mf.sharedMesh) + "|" + mf.sharedMesh.bounds.extents + "|sub=" + mf.sharedMesh.subMeshCount;
	}

	private static void ReportMonsters()
	{
		ReportAudio();
		ReportInput();
		HashSet<Shader> checkedShaders = new HashSet<Shader>();
		foreach (Renderer r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
		{
			foreach (Material m in r.sharedMaterials)
			{
				if (m == null || m.shader == null || !checkedShaders.Add(m.shader))
				{
					continue;
				}
				if (!m.shader.isSupported || ShaderUtil.ShaderHasError(m.shader) || m.shader.name == "Hidden/InternalErrorShader")
				{
					string msg = "";
					foreach (ShaderMessage sm in ShaderUtil.GetShaderMessages(m.shader))
					{
						msg += " | " + sm.message + " @" + sm.line;
						if (msg.Length > 300) break;
					}
					MonsterReport.Add("Bad shader in " + queue[stage - 1] + ": " + m.shader.name + " (material " + m.name + " on " + r.name + ") supported=" + m.shader.isSupported + msg);
				}
			}
		}
		foreach (Renderer r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
		{
			Vector3 c = r.transform.position;
			Bounds bb = r.bounds;
			if (!(float.IsFinite(c.x) && float.IsFinite(c.y) && float.IsFinite(c.z) && float.IsFinite(bb.extents.x) && float.IsFinite(bb.center.x) && bb.extents.magnitude < 1e6f))
			{
				Transform t = r.transform;
				string path = t.name;
				while (t.parent != null) { t = t.parent; path = t.name + "/" + path; }
				MonsterReport.Add("Bad bounds in " + queue[stage - 1] + ": " + path + " (" + r.GetType().Name + ") pos=" + c + " ext=" + bb.extents + " mesh=" + MeshInfo(r) + " static=" + r.isPartOfStaticBatch);
			}
		}
		foreach (KeyValuePair<Characters.States.EnemyStateController, Vector3> pair in MonsterStart)
		{
			if (pair.Key == null)
			{
				continue;
			}
			UnityEngine.AI.NavMeshAgent agent = pair.Key.GetComponent<UnityEngine.AI.NavMeshAgent>();
			MonsterReport.Add("Monster " + pair.Key.name + " in " + queue[stage - 1] + ": active=" + pair.Key.IsActive + " state=" + pair.Key.DiagnosticCurrentStateName + " moved=" + Vector3.Distance(pair.Value, pair.Key.transform.position).ToString("0.00") + " onNavMesh=" + (agent != null && agent.isOnNavMesh) + " speed=" + (agent != null ? agent.speed.ToString("0.00") : "-"));
		}
		MonsterStart.Clear();
	}

	private static void OnUpdate()
	{
		ProbeMonsters();
		if (EditorApplication.timeSinceStartup < stageEnd)
		{
			return;
		}
		string shot = stage == 0 ? "Menu" : queue[stage - 1];
		if (shotTaken != stage)
		{
			// Give the capture a moment before moving on.
			shotTaken = stage;
			if (stage > 0)
			{
				ReportMonsters();
			}
			Capture("E:/eyes/logs/shots/" + shot + ".jpg");
			if (stage > 0)
			{
				// What the monster sees (the eye ability).
				Prime31.MessageKit.MessageKit<bool>.post(7, true);
				Capture("E:/eyes/logs/shots/" + shot + "_vision.jpg");
				Prime31.MessageKit.MessageKit<bool>.post(7, false);
			}
			stageEnd = EditorApplication.timeSinceStartup + 1.0;
			return;
		}
		if (stage < queue.Length)
		{
			stage++;
			lastStageScene = queue[stage - 1];
			string enemy = System.Environment.GetEnvironmentVariable("RECOVERY_SMOKE_ENEMY");
			if (!string.IsNullOrEmpty(enemy))
			{
				UserGameSettings.currentEnemy = (Enemy)System.Enum.Parse(typeof(Enemy), enemy);
			}
			UnityEngine.SceneManagement.SceneManager.LoadScene(lastStageScene);
			stageEnd = EditorApplication.timeSinceStartup + SceneSeconds;
			return;
		}
		EditorApplication.update -= OnUpdate;
		SessionState.SetBool(ActiveKey, false);
		List<string> report = new List<string>();
		report.Add("Scenes:");
		report.AddRange(ScenesSeen);
		report.AddRange(MonsterReport);
		report.Add("Errors: " + Errors.Count);
		foreach (KeyValuePair<string, int> pair in Errors)
		{
			report.Add(pair.Value + "x " + pair.Key);
		}
		File.WriteAllLines("E:/eyes/logs/smoke_report.txt", report);
		EditorApplication.Exit(0);
	}
}
