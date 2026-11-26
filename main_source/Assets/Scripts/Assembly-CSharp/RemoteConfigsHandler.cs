using EyesAnalytics;
using UnityEngine;
public static class RemoteConfigsHandler
{
	private static bool configsApplied;

	public static void ApplyConfigs()
	{
		if (!RemoteConfigWrapper.IsReady || configsApplied)
		{
			return;
		}
		configsApplied = true;
		UserGameSettings.interstitialMinIntervalSeconds = RemoteConfigWrapper.GetInt("InterstitialMinIntervalSeconds");
		UserGameSettings.preLevelInterstitial = RemoteConfigWrapper.GetInt("PreLevelInter");
		UserProperties.SetExperiment("pre_level_inter", PreLevelInterstitialPolicy.Variant(UserGameSettings.preLevelInterstitial));
		Difficulty difficulty = FirstPlayDifficulty.Parse(RemoteConfigWrapper.GetString("FirstPlayingDiff"));
		UserGameSettings.firstPlayDifficulty = difficulty;
		UserProperties.SetExperiment("first_diff", FirstPlayDifficulty.Variant(difficulty));
		LightingMode lighting = LightingDefaultMode.Parse(RemoteConfigWrapper.GetString("LightingDefaultMode"));
		UserProperties.SetExperiment("lighting_default", LightingDefaultMode.Variant(lighting));
		LightingSettings.ApplyRemoteDefault(lighting);
		UserGameSettings.freeCoinsReward = RemoteConfigWrapper.GetInt("FreeCoinsRWValue");
		UserGameSettings.reviveNumberPerGame = RemoteConfigWrapper.GetInt("ReviveNumber");
		UserGameSettings.UnlockPrices_Charlie_Hospital = RemoteConfigWrapper.GetInt("UP_Charlie_Hos");
		UserGameSettings.UnlockPrices_GoodBoy_School = RemoteConfigWrapper.GetInt("UP_GoodBoy_Sch");
		UserGameSettings.UnlockPrices_TrickOrTreat = RemoteConfigWrapper.GetInt("UP_TrickOrTreat");
		UserGameSettings.UnlockPrices_DoubleTrouble = RemoteConfigWrapper.GetInt("UP_DoubleTrouble");
		UserGameSettings.UnlockPrices_Pixel = RemoteConfigWrapper.GetInt("UP_Pixel");
		UserGameSettings.UnlockPrices_Endless = RemoteConfigWrapper.GetInt("UP_Endless");
		UserGameSettings.UnlockPrices_Witch = RemoteConfigWrapper.GetInt("UP_Witch");
		UserGameSettings.UnlockPrices_Immersive = RemoteConfigWrapper.GetInt("UP_Immersive");
		UserGameSettings.gameplayRewardsEndless = RemoteConfigWrapper.GetInt("RewardsEndless");
		UserGameSettings.gameplayRewardsTrickOrTreat = RemoteConfigWrapper.GetInt("RewardsTrickOrTr");
		UserGameSettings.kickCountdownInitialValue = RemoteConfigWrapper.GetInt("KickCountdownInS");
		OnlineConfig.RemoteReviewerFlag = Application.version.Equals(RemoteConfigWrapper.GetString("DisableAds"));
		UserGameSettings.adsDisabledViaRemoteConfig = OnlineConfig.UseDeviceAccount || OnlineConfig.RemoteReviewerFlag;
		UserGameSettings.multiplayer_devUrl = RemoteConfigWrapper.GetString("devUrl");
		UserGameSettings.multiplayer_mainUrl = RemoteConfigWrapper.GetString("mainUrl");
		UserGameSettings.showIntroWhenOnboarding = RemoteConfigWrapper.GetBool("ShowIntroWhenOnb");
		UserGameSettings.multiplayerLastCompatibleVersion = RemoteConfigWrapper.GetString("MultiLastVersion");
		UserGameSettings.latestVersion = RemoteConfigWrapper.GetString("LatestVersion");
		UserGameSettings.ExpPoints.pointsPerBag = RemoteConfigWrapper.GetInt("pointsPerBag");
		UserGameSettings.ExpPoints.pointsPerEye = RemoteConfigWrapper.GetInt("pointsPerEye");
		UserGameSettings.ExpPoints.pointsForSaw = RemoteConfigWrapper.GetInt("pointsForSaw");
		UserGameSettings.ExpPoints.pointsForKey = RemoteConfigWrapper.GetInt("pointsForKey");
		UserGameSettings.ExpPoints.pointsForSafe = RemoteConfigWrapper.GetInt("pointsForSafe");
		UserGameSettings.ExpPoints.pointsForPortraitsMinigame = RemoteConfigWrapper.GetInt("pointsForPortrai");
		UserGameSettings.ExpPoints.winPercent = RemoteConfigWrapper.GetInt("winPercent");
		UserGameSettings.ExpPoints.mapMansionPercent = RemoteConfigWrapper.GetInt("mapMansionPerc");
		UserGameSettings.ExpPoints.mapHospitalPercent = RemoteConfigWrapper.GetInt("mapHospitalPerc");
		UserGameSettings.ExpPoints.mapSchoolPercent = RemoteConfigWrapper.GetInt("mapSchoolPerc");
		UserGameSettings.ExpPoints.multiplayerPercent = RemoteConfigWrapper.GetInt("multiplayerPerc");
		UserGameSettings.ExpPoints.bonusEventPercent = RemoteConfigWrapper.GetInt("bonusEventPerc");
		UserGameSettings.ExpPoints.difficultyNewbiePercent = RemoteConfigWrapper.GetInt("diffNewbiePerc");
		UserGameSettings.ExpPoints.difficultyEasyPercent = RemoteConfigWrapper.GetInt("diffEasyPerc");
		UserGameSettings.ExpPoints.difficultyNormalPercent = RemoteConfigWrapper.GetInt("diffNormalPerc");
		UserGameSettings.ExpPoints.difficultyHardPercent = RemoteConfigWrapper.GetInt("diffHardPerc");
		UserGameSettings.ExpPoints.difficultyNightmarePercent = RemoteConfigWrapper.GetInt("diffNightPerc");
		UserGameSettings.levelUpReward = RemoteConfigWrapper.GetInt("levelUpReward");
		UserGameSettings.experimentStoreButton = RemoteConfigWrapper.GetString("store_button");
		UserGameSettings.maxStamina = RemoteConfigWrapper.GetInt("maxStamina");
		UserGameSettings.maxStaminaAfterBoost = RemoteConfigWrapper.GetInt("maxStaminaBoost");
		UserGameSettings.staminaDrainPerSecond = RemoteConfigWrapper.GetInt("staminaDrain");
		UserGameSettings.staminaRegenPerSecond = (float)RemoteConfigWrapper.GetInt("staminaRegen") / 100f;
		UserGameSettings.staminaRegenPerSecondAfterBoost = (float)RemoteConfigWrapper.GetInt("staminaRegenBost") / 100f;
		UserGameSettings.discordUrl = RemoteConfigWrapper.GetString("discordUrl");
		ApplyGameNews();
		Debug.Log("[RemoteConfig] Settings applied for this session.");
	}

	private static void ApplyGameNews()
	{
		GameNewsItem[] items = GameNewsRemoteConfigReader.Read();
		Debug.Log(string.Format("[GameNews] Received {0} valid entries.", items.Length));
		if (!OnlineConfig.IsCustomServer)
		{
			GameNewsService.Synchronize(items);
		}
	}
}
