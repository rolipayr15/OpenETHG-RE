using System;
using System.Collections.Generic;
using System.Reflection;
using Google.Play.AssetDelivery;
using Multiplayer;
using UnityEngine;

[Obfuscation(Exclude = false)]
public static class UserGameSettings
{
	public static class ExpPoints
	{
		public static int sum;

		public static int pointsPerBag = 2;

		public static int pointsPerEye = 3;

		public static int pointsForSaw = 5;

		public static int pointsForSafe = 15;

		public static int pointsForKey = 5;

		public static int pointsForPortraitsMinigame = 20;

		public static int winPercent = 150;

		public static int mapMansionPercent = 100;

		public static int mapHospitalPercent = 120;

		public static int mapSchoolPercent = 150;

		public static int multiplayerPercent = 110;

		public static int bonusEventPercent = 100;

		public static int difficultyNewbiePercent = 100;

		public static int difficultyEasyPercent = 105;

		public static int difficultyNormalPercent = 120;

		public static int difficultyHardPercent = 130;

		public static int difficultyNightmarePercent = 150;
	}

	public static Mode currentMode;

	public static Difficulty currentDifficulty = Difficulty.Normal;

	public static Enemy currentEnemy;

	public static Level currentLevel;

	public static GameMode currentGameMode;

	public static Platform platform;

	public static string currentSecondPlayerUsername;

	public static int currentSecondPlayerLevel;

	public static int bagsCollected;

	public static int bagsToCollect;

	public static int halloweenMultiplier = 1;

	public static int? lastWinDay;

	public static float? volume;

	public static float? sensitivity;

	public const float MinMouseSensitivity = 0.1f;

	public const float DefaultMouseSensitivity = 1.2f;

	public const float MaxMouseSensitivity = 8f;

	public static float? fov;

	public static bool? socialEnabled;

	private static bool? _dynamicJoystickEnabled;

	public static float lastGameDuration;

	public static bool wasLastGameWon;

	public static bool eyesUsed;

	public static string language = "";

	public static bool unlockedCharlie;

	public static bool unlockedHospital;

	public static bool unlockedSchool;

	public static bool unlockedNoAds;

	public static bool unlockedEndlessMode;

	public static bool unlockedTrickOrTreatMode;

	public static bool unlockedDoubleEnemyMode;

	public static bool unlockedDog;

	public static bool unlockedPixelMode;

	public static bool unlockedCharlieHospital;

	public static bool unlockedGoodBoySchool;

	public static bool unlockedWitch;

	public static bool unlockedImmersive;

	public static bool unlockedImmersiveOneTime;

	public static bool? UnlockedWelcomeBundle = false;

	public static IDictionary<string, PlayAssetBundleRequest> bundleRequests = new Dictionary<string, PlayAssetBundleRequest>();

	public static string lastVersion = "";

	public static bool? showWhatsNew;

	public static bool openStore;

	public static bool playingForTheFirstTime;

	public static int playCounter;

	public static bool wasPlayerSeenByMonster;

	public static bool userQuitToMenu;

	public static int eyesCollected;

	private static bool _googleNBO;

	public static bool firebaseAvailable;

	public static DateTime freeCoinsAdAvailableIn = DateTime.Now - TimeSpan.FromSeconds(10.0);

	public static int freeCoinsReward = 100;

	public static int freeCoinsDesignVersion = 2;

	public static int reviveNumberPerGame = 99;

	public static int interstitialMinIntervalSeconds = 90;

	public static int preLevelInterstitial = -1;

	public static Difficulty firstPlayDifficulty = Difficulty.Normal;

	public static bool isFirebaseDataFetched;

	public static int streakDailyReward = 50;

	public static int streakWeeklyReward = 300;

	public static StoreItems? currentPreset;

	public static bool isOneTimeUnlocked;

	public static int UnlockPrices_Charlie_Hospital = 300;

	public static int UnlockPrices_GoodBoy_School = 300;

	public static int UnlockPrices_TrickOrTreat = 300;

	public static int UnlockPrices_DoubleTrouble = 300;

	public static int UnlockPrices_Pixel = 300;

	public static int UnlockPrices_Endless = 300;

	public static int UnlockPrices_Witch = 300;

	public static int UnlockPrices_Immersive = 300;

	public static int UnlockPrices_Hospital = 350;

	public static int UnlockPrices_School = 350;

	public static bool expertMode;

	public static int gameplayRewardsEndless = 100;

	public static int gameplayRewardsTrickOrTreat = 100;

	public static bool adsDisabledViaRemoteConfig;

	public static bool showNewContentButton;

	public static bool isImmersiveMode;

	public static bool ATTPreConsentDisabledViaRemoteConfig = true;

	public static bool ATTConsentDisabledViaRemoteConfig;

	public static int ATTPreConsentVariant = 2;

	public static int GDPRconsentVariant = 1;

	private static bool nboDisabledViaRemoteConfig;

	public static bool firebaseRemoteConfigFetchedAndActivated;

	public static int initialEyesNumber;

	public static int welcomeBundleTitleVersion;

	public static int kickCountdownInitialValue = 40;

	public static bool isMultiplayerHost = true;

	public static bool isInvitedPlayer;

	public static int myBagsCollected;

	public static int secondPlayerBagsCollected;

	public static int freeTickets = 30;

	public static int rewardedAdTickets = 30;

	public static int purchaseTickets = 1000;

	public static int gamePriceInKeys = 10;

	public static bool hostLeftGame;

	public static bool isEnemyOnTheSameFloor;

	public static bool multiplayerPlayAgain;

	public static bool showIntroWhenOnboarding = true;

	public static string multiplayerGameId = "";

	public static string singleGameId = "";

	public static bool dynamicDelivery = true;

	public static bool isPlayingWithRandom;

	public static bool unlockedBundle1;

	public static bool unlockedBundle2;

	public static bool unlockedBundle3;

	public static bool unlockedMultiplayerSub;

	public static bool unlockedMultiplayerUnlimited;

	public static string multiplayer_mainUrl = "backend.ethg-api.online";

	public static string multiplayer_devUrl = "api.ethg-dev.online";

	public static AllStats stats;

	public static bool IsSignedInWithBasicAuth;

	public static string multiplayerLastCompatibleVersion = "7.0.0";

	public static string multiplayerUsername = "<<your username>>";

	public static string multiplayerSecondPlayerUsername = "<<player's username>>";

	public static AudioClip customKrasueSound;

	public static int roomNumber;

	public static bool unlockedBirthdayGold;

	public static bool unlockedBirthdayPlatinium;

	public static bool unlockedBirthdayDiamond;

	public static string latestVersion = "7.0.116";

	public static bool skipThisTime;

	public static CalcResult calcResult;

	public static string temporaryExpCalculator = "";

	public static string temporaryExpCalculatorBonus = "";

	public static int temporaryExpCalculatorFinal;

	public static bool hasPlayerLeveledUp;

	public static int levelUpReward = 50;

	public static int totalDistance;

	public static int currentDistance;

	public static int currentGameSteps;

	public static string experimentStoreButton = "old_store_button";

	public static float maxStamina = 5f;

	public static float maxStaminaAfterBoost = 15f;

	public static float staminaDrainPerSecond = 1f;

	public static float staminaRegenPerSecond = 0.1f;

	public static float staminaRegenPerSecondAfterBoost = 0.15f;

	public static string discordUrl = "https://discord.gg/y5MNSekGAJ";

	public static ControlMode controlMode
	{
		get
		{
			ControlMode fallback = ControlModeManager.DefaultMode(Application.isMobilePlatform);
			ControlMode saved = (ControlMode)PlayerPrefs.GetInt("controlMode", (int)fallback);
			return ControlModeManager.IsValidMode(saved) ? saved : fallback;
		}
		set
		{
			if (!ControlModeManager.IsValidMode(value))
			{
				throw new ArgumentOutOfRangeException("value");
			}
			PlayerPrefs.SetInt("controlMode", (int)value);
			PlayerPrefs.Save();
		}
	}

	public static float mouseSensitivity
	{
		get
		{
			return SanitizeMouseSensitivity(PlayerPrefs.GetFloat("mouseSensitivity", DefaultMouseSensitivity));
		}
		set
		{
			PlayerPrefs.SetFloat("mouseSensitivity", SanitizeMouseSensitivity(value));
			PlayerPrefs.Save();
		}
	}

	public static bool mouseInvertY
	{
		get
		{
			return PlayerPrefz.GetBool("mouseInvertY", false);
		}
		set
		{
			PlayerPrefz.SetBool("mouseInvertY", value);
			PlayerPrefs.Save();
		}
	}

	public static bool showKeyboardHints
	{
		get
		{
			return PlayerPrefz.GetBool("showKeyboardHints", true);
		}
		set
		{
			PlayerPrefz.SetBool("showKeyboardHints", value);
			PlayerPrefs.Save();
		}
	}

	public static bool showControllerHints
	{
		get
		{
			return PlayerPrefz.GetBool("showControllerHints", true);
		}
		set
		{
			PlayerPrefz.SetBool("showControllerHints", value);
			PlayerPrefs.Save();
		}
	}

	public static bool showStepCounter
	{
		get
		{
			return PlayerPrefz.GetBool("showStepCounter", false);
		}
		set
		{
			PlayerPrefz.SetBool("showStepCounter", value);
			PlayerPrefs.Save();
		}
	}

	// Cached after the first read; the setter keeps the cache and PlayerPrefs in sync.
	public static bool dynamicJoystickEnabled
	{
		get
		{
			if (!_dynamicJoystickEnabled.HasValue)
			{
				_dynamicJoystickEnabled = PlayerPrefz.GetBool("dynamicJoystickEnabled", false);
			}
			return _dynamicJoystickEnabled.Value;
		}
		set
		{
			PlayerPrefz.SetBool("dynamicJoystickEnabled", value);
			_dynamicJoystickEnabled = value;
		}
	}

	// Google "new buyer offer" eligibility, unless remote config switched it off.
	public static bool googleNBO
	{
		get
		{
			return !nboDisabledViaRemoteConfig && _googleNBO;
		}
		set
		{
			_googleNBO = value;
		}
	}

	public static bool NBODisabledViaRemoteConfig
	{
		get
		{
			return nboDisabledViaRemoteConfig;
		}
		set
		{
			nboDisabledViaRemoteConfig = value;
		}
	}

	// The ARM build tests the static at offset 0x10C (adsDisabledViaRemoteConfig in the field layout) to choose
	// between the basic-auth flag and Unity Authentication; kept as compiled.
	public static bool isSignedIn
	{
		get
		{
			if (adsDisabledViaRemoteConfig)
			{
				return IsSignedInWithBasicAuth;
			}
			if (Unity.Services.Core.UnityServices.State != Unity.Services.Core.ServicesInitializationState.Initialized)
			{
				return false;
			}
			Unity.Services.Authentication.IAuthenticationService auth = Unity.Services.Authentication.AuthenticationService.Instance;
			return auth.IsSignedIn && !auth.IsExpired;
		}
	}

	private static float SanitizeMouseSensitivity(float value)
	{
		if (float.IsNaN(value) || float.IsInfinity(value))
		{
			return DefaultMouseSensitivity;
		}
		return Mathf.Clamp(value, MinMouseSensitivity, MaxMouseSensitivity);
	}

	public static bool IsMultiplayer()
	{
		return currentGameMode == GameMode.COOP || currentGameMode == GameMode.VS;
	}

	public static bool IsMultiplayerCoop()
	{
		return currentGameMode == GameMode.COOP;
	}

	public static bool IsMultiplayerVs()
	{
		return currentGameMode == GameMode.VS;
	}
}
