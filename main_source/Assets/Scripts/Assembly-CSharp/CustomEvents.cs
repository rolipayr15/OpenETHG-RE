using System.Collections.Generic;

public static class CustomEvents
{
	public enum ResultShareAction
	{
		Clicked = 0,
		Done = 1,
		Cancelled = 2,
		Unknown = 3,
		Error = 4,
		Saved = 5
	}

	public const string EYE_COLLECTED = "eye_collected";

	public const string EYE_USED = "eye_used";

	public const string TUTORIAL_STEP_MOVING = "tutorial_step_moving";

	public const string TUTORIAL_STEP_COLLECTING = "tutorial_step_collecting";

	public const string TUTORIAL_STEP_INTERACTION = "tutorial_step_interaction";

	public const string TUTORIAL_STEP_EYE = "tutorial_step_eye";

	public const string TUTORIAL_STEP_JOURNAL = "tutorial_step_journal";

	public const string MENU_BUTTON_FREE_COINS = "menu_button_free_coins";

	public const string BAG_COLLECTED = "bag_collected";

	public const string MENU_BUTTON_SETTINGS = "menu_button_settings";

	public const string MENU_BUTTON_SOCIAL = "menu_button_social";

	public const string MENU_BUTTON_FB = "menu_button_fb";

	public const string MENU_BUTTON_VK = "menu_button_vk";

	public const string MENU_BUTTON_REMOVE_ADS = "menu_button_remove_ads";

	public const string GHOST_IS_CHASING = "Ghost is chasing";

	public const string FIRST_EYE_USED = "First eye used";

	public const string MENU_BUTTON_CREDITS = "Menu - Credits";

	public const string MENU_BUTTON_STREAK = "Menu - Streak";

	public const string MENU_BUTTON_CUSTOM_KRASUE = "Menu - Custom Krasue";

	public const string MENU_BUTTON_PRIVACY_POLICY = "Menu - Privacy Policy";

	public const string INIT_STARTED = "init_started";

	public const string HOUSE_ADS_VK = "House ads - VK";

	public const string REVIVE_NO_AD = "Revive - no ad available";

	public const string REVIVE_SUCCESS = "Revive - successful";

	public const string REVIVE_CANCEL = "Revive - cancel";

	public const string DIFFICULTY_CHANGED = "Difficulty changed";

	public const string ALL_BAGS_COLLECTED = "all_bags_collected";

	public const string POTION_CLOAK = "potion_cloak";

	public const string POTION_DRUNK = "potion_drunk";

	public const string POTION_EXTRA_EYES = "potion_extra_eyes";

	public const string POTION_LIMITED_VISION = "potion_limited_vision";

	public const string POTION_MONEY_BAGS_VISION = "potion_money_bags_vision";

	public const string POTION_SPRINT = "potion_sprint";

	public const string IMMERSIVE_UNLOCKED_COINS = "immersive_unlocked_coins";

	public const string IMMERSIVE_UNLOCKED_AD = "immersive_unlocked_ad";

	public const string IMMERSIVE_UNLOCKED_IAP = "immersive_unlocked_iap";

	public const string EXPERT_MODE_SCREEN = "expert_mode_screen";

	public const string UNLOCKED_NBO = "unlocked_nbo";

	public const string MULTIPLAYER_APP_QUIT = "multiplayer_app_quit";

	private static Dictionary<string, object> parameters;

	public static void LevelLoaded()
	{
		string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
		if (FirebaseState.IsInitialized)
		{
			Firebase.Crashlytics.Crashlytics.Log("Scene: " + scene);
		}
	}

	public static void FirebaseEventLevelFinished(Level location, Enemy opponent, Mode mode, Difficulty difficulty, GameFinishedResult result, bool expertMode, int bagsCollected, int eyesCollected, string endReason = null)
	{
		EyesAnalytics.Game.End(result, bagsCollected, eyesCollected, (int)UnityEngine.Time.timeSinceLevelLoad, endReason);
	}

	public static void FirebaseEventResultShare(bool isGameOver, ResultShareAction action)
	{
		if (UnityEngine.Application.isEditor)
		{
			return;
		}
		if (action > ResultShareAction.Saved)
		{
			action = ResultShareAction.Unknown;
		}
		EyesAnalytics.Social.Share(isGameOver, (EyesAnalytics.ShareStage)action);
	}

	public static void SendEventRateMeError(string error)
	{
		EyesAnalytics.Social.ReviewError(error);
	}
}
