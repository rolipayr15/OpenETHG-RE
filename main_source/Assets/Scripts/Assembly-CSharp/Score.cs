using Prime31.MessageKit;
using Firebase.Crashlytics;
using EyesAnalytics;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[Obfuscation(Exclude = false)]
public class Score : MonoBehaviour
{
	private Dictionary<Difficulty, int> difficultyLevelToInt = new Dictionary<Difficulty, int> { { (Difficulty)0, 1 }, { (Difficulty)1, 2 }, { (Difficulty)2, 3 }, { (Difficulty)3, 4 }, { (Difficulty)4, 5 } };

	public static readonly IReadOnlyDictionary<StoreItems, string> PrefsKeys = new Dictionary<StoreItems, string> { { (StoreItems)8, "DIFFICULTY_LEVEL_WON_KRASUE_MANSION" }, { (StoreItems)9, "DIFFICULTY_LEVEL_WON_CHARLIE_HOSPITAL" }, { (StoreItems)10, "DIFFICULTY_LEVEL_WON_GOOD_BOY_SCHOOL" }, { (StoreItems)4, "DIFFICULTY_LEVEL_WON_TRICK_OR_TREAT" }, { (StoreItems)2, "DIFFICULTY_LEVEL_WON_DOUBLE_TROUBLE" }, { (StoreItems)11, "DIFFICULTY_LEVEL_WON_PIXEL" }, { (StoreItems)12, "DIFFICULTY_LEVEL_WON_WITCH" }, { (StoreItems)3, "DIFFICULTY_LEVEL_WON_ENDLESS" }, { (StoreItems)26, "DIFFICULTY_LEVEL_WON_GOOSE_MANSION" } };

	private bool leaving;

	private static Action<Score> leave = delegate(Score score) { score.Leave(); };

	private void Start()
	{
		Screens.View((ScreenId)17);
		if (FirebaseState.IsInitialized)
		{
			Crashlytics.Log("Win scene");
		}
		if (UserGameSettings.currentPreset.HasValue)
		{
			StoreItems preset = UserGameSettings.currentPreset.Value;
			if (!PrefsKeys.TryGetValue(preset, out string key))
			{
				Debug.LogWarning("Score: no progress key for preset " + preset);
			}
			else if (PlayerPrefs.GetInt(key, 0) < difficultyLevelToInt[UserGameSettings.currentDifficulty])
			{
				PlayerPrefs.SetInt(key, difficultyLevelToInt[UserGameSettings.currentDifficulty]);
				MessageKit.post(134);
			}
		}
		UserGameSettings.isOneTimeUnlocked = false;
		ReviewPromptPresenter.Schedule(this);
	}

	public void BackToMenu()
	{
		if (!leaving)
		{
			leaving = true;
			leave(this);
		}
	}

	private void Leave()
	{
		Ads.SetNextInterstitialTrigger((AdTrigger)3);
		SceneManager.LoadScene("PostGameAd");
	}
}
