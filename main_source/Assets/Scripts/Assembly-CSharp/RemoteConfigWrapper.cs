using Debug = UnityEngine.Debug;
using System.Globalization;
using Firebase.Crashlytics;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Firebase.RemoteConfig;

public static class RemoteConfigWrapper
{
	private static readonly Dictionary<string, int> defaultIntValues = new Dictionary<string, int> { { "FreeCoinsRWValue", 50 }, { "InterstitialMinIntervalSeconds", 90 }, { "ReviveNumber", 99 }, { "RewardsEndless", 100 }, { "RewardsTrickOrTr", 100 }, { "UP_Charlie_Hos", 500 }, { "UP_GoodBoy_Sch", 500 }, { "UP_TrickOrTreat", 500 }, { "UP_DoubleTrouble", 600 }, { "UP_Pixel", 100 }, { "UP_Endless", 300 }, { "UP_Witch", 500 }, { "UP_Immersive", 100 }, { "KickCountdownInS", 45 }, { "PreLevelInter", -1 }, { "pointsPerBag", 2 }, { "pointsPerEye", 3 }, { "pointsForSaw", 5 }, { "pointsForKey", 5 }, { "pointsForSafe", 15 }, { "pointsForPortrai", 20 }, { "winPercent", 150 }, { "mapMansionPerc", 100 }, { "mapHospitalPerc", 120 }, { "mapSchoolPerc", 150 }, { "multiplayerPerc", 110 }, { "bonusEventPerc", 100 }, { "diffNewbiePerc", 100 }, { "diffEasyPerc", 105 }, { "diffNormalPerc", 120 }, { "diffHardPerc", 130 }, { "diffNightPerc", 150 }, { "levelUpReward", 50 }, { "maxStamina", 5 }, { "maxStaminaBoost", 15 }, { "staminaDrain", 1 }, { "staminaRegen", 10 }, { "staminaRegenBost", 15 } };

	private static readonly Dictionary<string, bool> defaultBoolValues = new Dictionary<string, bool> { { "ShowIntroWhenOnb", true }, { "CalibratorPrompt", true } };

	private static readonly Dictionary<string, string> defaultStringValues = new Dictionary<string, string> { { "devUrl", "api.ethg-dev.online" }, { "DisableAds", "" }, { "mainUrl", "backend.ethg-api.online" }, { "MultiLastVersion", "7.0.3" }, { "FirstPlayingDiff", "Normal" }, { "LightingDefaultMode", "flashlight" }, { "CalibStartStops", "saved" }, { "LatestVersion", "7.0.120" }, { "store_button", "old_store_button" }, { "discordUrl", "https://discord.gg/y5MNSekGAJ" }, { "gameNewsRich", "{\"items\":[]}" } };

	private static readonly Dictionary<string, float> defaultFloatValues = new Dictionary<string, float> { { "CalibMinStops", -1f }, { "CalibMaxStops", 1f } };

	private static Dictionary<string, string> values = new Dictionary<string, string>();

	private static Task initializationTask;

	public static string StartupConfigSource { get; private set; } = "default";

	public static bool IsReady { get; private set; }

	public static bool IsFetchComplete => initializationTask != null && initializationTask.IsCompleted;

	public static string ClassifySource(bool fetchedThisLaunch, bool anyRemoteValue)
	{
		if (fetchedThisLaunch)
		{
			return "remote";
		}
		return anyRemoteValue ? "cached" : "default";
	}

	public static bool IsNeverFetchedInfoError(Exception exception)
	{
		if (exception is AggregateException)
		{
			exception = exception.InnerException;
		}
		return exception is ArgumentOutOfRangeException outOfRange && outOfRange.ParamName == "value";
	}

	public static Task InitializeAsync()
	{
		if (initializationTask == null)
		{
			initializationTask = FetchConfigAsync();
		}
		return initializationTask;
	}

	private static async Task FetchConfigAsync()
	{
		try
		{
			FirebaseRemoteConfig remoteConfig = FirebaseRemoteConfig.DefaultInstance;
			await remoteConfig.EnsureInitializedAsync();
			await remoteConfig.SetDefaultsAsync(CreateDefaults());
			await remoteConfig.ActivateAsync();
			CaptureValues(remoteConfig, false);
			await remoteConfig.FetchAsync((Debug.isDebugBuild || Application.isEditor) ? TimeSpan.Zero : TimeSpan.FromHours(24.0));
			LastFetchStatus status = ReadLastFetchStatus(remoteConfig);
			if (status != LastFetchStatus.Success)
			{
				Debug.LogWarning(string.Format("[RemoteConfig] Fetch failed: {0}. Using cached values or defaults.", status));
			}
			else if (!IsReady)
			{
				await remoteConfig.ActivateAsync();
				CaptureValues(remoteConfig, true);
			}
		}
		catch (Exception exception)
		{
			if (!IsNeverFetchedInfoError(exception))
			{
				ReportNonFatal(exception);
			}
		}
	}

	private static void ReportNonFatal(Exception exception)
	{
		if (FirebaseState.IsInitialized)
		{
			Crashlytics.LogException(exception);
		}
	}

	private static LastFetchStatus ReadLastFetchStatus(FirebaseRemoteConfig remoteConfig)
	{
		return remoteConfig.Info.LastFetchStatus;
	}

	private static Dictionary<string, object> CreateDefaults()
	{
		Dictionary<string, object> defaults = new Dictionary<string, object>();
		foreach (KeyValuePair<string, int> pair in defaultIntValues)
		{
			defaults.Add(pair.Key, pair.Value);
		}
		foreach (KeyValuePair<string, bool> pair in defaultBoolValues)
		{
			defaults.Add(pair.Key, pair.Value);
		}
		foreach (KeyValuePair<string, string> pair in defaultStringValues)
		{
			defaults.Add(pair.Key, pair.Value);
		}
		foreach (KeyValuePair<string, float> pair in defaultFloatValues)
		{
			defaults.Add(pair.Key, pair.Value);
		}
		return defaults;
	}

	private static void CaptureValues(FirebaseRemoteConfig remoteConfig, bool fetchedNow)
	{
		if (IsReady)
		{
			return;
		}
		Dictionary<string, string> captured = new Dictionary<string, string>();
		bool anyRemoteValue = false;
		foreach (KeyValuePair<string, ConfigValue> pair in remoteConfig.AllValues)
		{
			captured.Add(pair.Key, pair.Value.StringValue);
			anyRemoteValue |= pair.Value.Source == ValueSource.RemoteValue;
		}
		values = captured;
		StartupConfigSource = ClassifySource(fetchedNow, anyRemoteValue);
	}

	public static void CompleteStartup()
	{
		if (!IsReady)
		{
			LogDiagnostic(IsFetchComplete ? "[RemoteConfig] Startup finished with the fetch complete." : "[RemoteConfig] Startup finished before the fetch completed.");
			IsReady = true;
		}
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("DEVELOPMENT_BUILD")]
	private static void LogDiagnostic(string message)
	{
		UnityEngine.Debug.Log(message);
	}

	public static int GetInt(string key)
	{
		if (values.TryGetValue(key, out string text) && int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
		{
			return parsed;
		}
		if (defaultIntValues.TryGetValue(key, out int fallback))
		{
			return fallback;
		}
		Debug.LogWarning("[RemoteConfig] Missing integer value for key: " + key);
		return 0;
	}

	public static float GetFloat(string key)
	{
		if (values.TryGetValue(key, out string text) && float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed))
		{
			return parsed;
		}
		if (defaultFloatValues.TryGetValue(key, out float fallback))
		{
			return fallback;
		}
		Debug.LogWarning("[RemoteConfig] Missing float value for key: " + key);
		return 0f;
	}

	public static bool GetBool(string key)
	{
		if (values.TryGetValue(key, out string text))
		{
			if (text == "1")
			{
				return true;
			}
			if (text == "0")
			{
				return false;
			}
			if (bool.TryParse(text, out bool parsed))
			{
				return parsed;
			}
		}
		if (defaultBoolValues.TryGetValue(key, out bool fallback))
		{
			return fallback;
		}
		Debug.LogWarning("[RemoteConfig] Missing boolean value for key: " + key);
		return false;
	}

	public static string GetString(string key)
	{
		if (values.TryGetValue(key, out string text) && !string.IsNullOrWhiteSpace(text))
		{
			return text;
		}
		if (defaultStringValues.TryGetValue(key, out string fallback))
		{
			return fallback;
		}
		Debug.LogWarning("[RemoteConfig] Missing string value for key: " + key);
		return string.Empty;
	}
}
