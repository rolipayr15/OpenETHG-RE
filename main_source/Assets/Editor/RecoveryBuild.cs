using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Named parts of the game, so one part can be built without the unfinished ones.
// Project Settings > Scenes In Build stays the full list of the real game; these builds pass their
// scenes explicitly and never edit it.
public static class RecoveryBuild
{
	private const string PcMenuDefine = "RECOVERY_PC_MENU";

	private const string OutputRoot = "E:/eyes/builds/";

	// Everything the menu opens. Scene order matters: Init loads the next build index.
	private static readonly string[] Menu = new string[]
	{
		"Assets/scenes/Init.unity",
		"Assets/scenes/MenuLoader.unity",
		"Assets/scenes/Menu.unity",
		"Assets/Bibloteka/QuitApp/Quit.unity",
		"Assets/scenes/MessageBox/MessageBox.unity",
		"Assets/scenes/MessageBox/CustomToast.unity",
		"Assets/scenes/MessageBox/RateMe.unity",
		"Assets/scenes/MessageBox/QuestionBox.unity",
		"Assets/scenes/MessageBox/Immersive.unity",
		"Assets/scenes/MessageBox/JournalEntryBox.unity",
		"Assets/scenes/MessageBox/ATTpreConsent.unity",
		"Assets/Multiplayer/MultiplayerLobby.unity",
		"Assets/scenes/AssetBundleManager/AssetBundleManager.unity",
		"Assets/scenes/MessageBox/birthday/Store.unity",
		"Assets/scenes/MessageBox/RemoveAdsBox.unity",
		"Assets/scenes/MessageBox/UpdateAvailable.unity",
		"Assets/Multiplayer/Profile.unity",
		"Assets/Multiplayer/Authentication.unity",
		"Assets/Multiplayer/ChangeUsername.unity",
		"Assets/scenes/MessageBox/ChatSettings.unity",
		"Assets/scenes/MessageBox/MenuPurchase.unity",
		"Assets/Multiplayer/MultiplayerKeys.unity",
		"Assets/scenes/MessageBox/HowToPlay.unity",
		"Assets/scenes/MessageBox/NetworkIssues.unity",
		"Assets/scenes/MessageBox/ServerTest.unity",
		"Assets/scenes/MessageBox/GameNews.unity",
		"Assets/scenes/MessageBox/BrightnessCalibration.unity"
	};

	// Shared by every level: the road into a level and the screens after it.
	private static readonly string[] LevelFlow = new string[]
	{
		"Assets/scenes/Loading.unity",
		"Assets/scenes/Intro.unity",
		"Assets/scenes/Over.unity",
		"Assets/scenes/Outro.unity",
		"Assets/scenes/Win.unity",
		"Assets/scenes/PostGameAd.unity",
		"Assets/scenes/PreGameAd.unity"
	};

	private static readonly string[] Mansion = new string[] { "Assets/scenes/Mansion.unity" };

	private static readonly string[] Hospital = new string[] { "Assets/scenes/Hospital.unity" };

	private static readonly string[] School = new string[] { "Assets/scenes/School.unity" };

	[MenuItem("Recovery/Build for PC/Menu only")]
	public static void BuildMenuWindows()
	{
		Build("menu_pc", Menu);
	}

	[MenuItem("Recovery/Build for PC/Menu + Mansion")]
	public static void BuildMansionWindows()
	{
		Build("mansion_pc", Menu.Concat(LevelFlow).Concat(Mansion).ToArray());
	}

	[MenuItem("Recovery/Build alpha/Windows")]
	public static void BuildAlphaWindows()
	{
		Build("eyes_alpha_pc", Menu.Concat(LevelFlow).Concat(Mansion).Concat(Hospital).Concat(School).ToArray());
	}

	[MenuItem("Recovery/Build alpha/Android APK")]
	public static void BuildAlphaAndroid()
	{
		// ARM64 exists only on IL2CPP. Minimal stripping keeps reflection-based game code working.
		PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
		PlayerSettings.SetManagedStrippingLevel(UnityEditor.Build.NamedBuildTarget.Android, ManagedStrippingLevel.Minimal);
		PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
		BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
		{
			scenes = Menu.Concat(LevelFlow).Concat(Mansion).Concat(Hospital).Concat(School).ToArray(),
			locationPathName = OutputRoot + "eyes_alpha_android/Eyes_alpha.apk",
			target = BuildTarget.Android,
			targetGroup = BuildTargetGroup.Android,
			extraScriptingDefines = new string[] { PcMenuDefine },
			options = BuildOptions.None
		});
		Debug.Log("[RecoveryBuild] android: " + report.summary.result + ", " + report.summary.totalErrors + " errors, " + (report.summary.totalSize >> 20) + " MB");
	}

	[MenuItem("Recovery/Build for PC/Menu + Hospital")]
	public static void BuildHospitalWindows()
	{
		Build("hospital_pc", Menu.Concat(LevelFlow).Concat(Hospital).ToArray());
	}

	[MenuItem("Recovery/Build for PC/Menu + School")]
	public static void BuildSchoolWindows()
	{
		Build("school_pc", Menu.Concat(LevelFlow).Concat(School).ToArray());
	}

	private static void Build(string name, string[] scenes)
	{
		List<string> missing = scenes.Where(scene => !System.IO.File.Exists(scene)).ToList();
		if (missing.Count > 0)
		{
			throw new System.IO.FileNotFoundException("Missing scenes: " + string.Join(", ", missing));
		}
		BuildPlayerOptions options = new BuildPlayerOptions
		{
			scenes = scenes,
			locationPathName = OutputRoot + name + "/Eyes.exe",
			target = BuildTarget.StandaloneWindows64,
			targetGroup = BuildTargetGroup.Standalone,
			extraScriptingDefines = new string[] { PcMenuDefine },
			options = BuildOptions.None
		};
		BuildReport report = BuildPipeline.BuildPlayer(options);
		BuildSummary summary = report.summary;
		if (name == "eyes_alpha_pc")
		{
			string serverOut = OutputRoot + name + "/Server";
			System.IO.Directory.CreateDirectory(serverOut);
			foreach (string file in new string[] { "eyes_server.py", "admin.html", "start_server.bat", "README.md" })
			{
				System.IO.File.Copy("Tools~/server/" + file, serverOut + "/" + file, true);
			}
		}
		Debug.Log("[RecoveryBuild] " + name + ": " + summary.result + ", " + summary.totalErrors + " errors, " + (summary.totalSize / 1048576UL) + " MB, " + summary.totalTime);
		foreach (BuildStep step in report.steps)
		{
			foreach (BuildStepMessage message in step.messages)
			{
				if (message.type == LogType.Error || message.type == LogType.Exception)
				{
					Debug.Log("[RecoveryBuild] error: " + message.content);
				}
			}
		}
		if (Application.isBatchMode)
		{
			EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
		}
	}
}
