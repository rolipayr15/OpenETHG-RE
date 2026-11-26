using System.Collections.Generic;
using Eyes.DailyChallenges;
using EyesAnalytics;
using I2.Loc;
using Prime31.MessageKit;
using UnityEngine;

public class MenuContentWindow : CustomSingleton<MenuContentWindow>
{
	[SerializeField]
	private Canvas options;

	[SerializeField]
	private GameObject credits;

	[SerializeField]
	private GameObject chooseLevelWindow;

	[SerializeField]
	private Canvas quitWindow;

	[SerializeField]
	private Canvas streakWindow;

	[SerializeField]
	private Canvas customKrasueWindow;

	[SerializeField]
	private GameObject privacyPolicyWindow;

	[SerializeField]
	private SocialGUI socialWindow;

	[SerializeField]
	private DailyChallengeMenuView dailyChallengeWindow;

	private bool isWindowOpen;

	private bool isPrivacyPolicyWindowOpen;

	private bool isCreditsWindowOpen;

	private Dictionary<string, object> parameters;

	public bool IsWindowOpen => isWindowOpen;

	private void Update()
	{
		if (!TextEntryInputGuard.BlocksShortcuts && Input.GetKeyDown(KeyCode.Escape))
		{
			BackButton();
		}
	}

	// Back closes the topmost overlay (privacy policy, credits), then any window, then offers to quit.
	private void BackButton()
	{
		if (!isWindowOpen)
		{
			ShowQuitWindow();
		}
		else if (isPrivacyPolicyWindowOpen)
		{
			ClosePrivacyPolicy();
		}
		else if (isCreditsWindowOpen)
		{
			CloseCreditsWindow();
		}
		else
		{
			HideAllWindows();
			Screens.View(ScreenId.MainMenu);
		}
	}

	public void ClosePrivacyPolicy()
	{
		isPrivacyPolicyWindowOpen = false;
		privacyPolicyWindow.SetActive(false);
		Screens.CloseOverlay(ScreenId.PrivacyPolicy);
	}

	public void CloseCreditsWindow()
	{
		isCreditsWindowOpen = false;
		credits.SetActive(false);
		Screens.CloseOverlay(ScreenId.Credits);
	}

	private void ShowQuitWindow()
	{
		MessageKit.post(72);
		quitWindow.enabled = true;
		isWindowOpen = true;
	}

	// Android back-to-home instead of killing the process.
	public void QuitGame()
	{
		AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
		activity.Call<bool>("moveTaskToBack", true);
	}

	public void ShowOptions()
	{
		HideAllWindows();
		options.enabled = true;
		Breadcrumbs.Log("menu_button_settings");
		isWindowOpen = true;
		Screens.View(ScreenId.Settings);
	}

	public void ShowDailyChallenge()
	{
		if (dailyChallengeWindow == null)
		{
			return;
		}
		HideAllWindows();
		dailyChallengeWindow.Show();
		isWindowOpen = true;
		Screens.View(ScreenId.DailyChallenge);
	}

	public void ShowCredits()
	{
		isCreditsWindowOpen = true;
		credits.SetActive(true);
		Breadcrumbs.Log("Menu - Credits");
		Screens.ViewOverlay(ScreenId.Credits);
	}

	public void ShowChooseLevel()
	{
		ChooseLevelManager.Instance.ShowChooseLevel();
		Breadcrumbs.Log("expert_mode_screen");
		isWindowOpen = true;
		Screens.View(ScreenId.LevelSelect);
	}

	public void ShowSocial()
	{
		socialWindow.Show();
		Breadcrumbs.Log("menu_button_social");
		isWindowOpen = true;
		Screens.View(ScreenId.Social);
	}

	public void ShowStreak()
	{
		streakWindow.enabled = true;
		StreakManager.Instance.OpenWindow();
		Breadcrumbs.Log("Menu - Streak");
		isWindowOpen = true;
		Screens.View(ScreenId.Streak);
	}

	public void ShowCustomKrasue()
	{
		customKrasueWindow.enabled = true;
		Breadcrumbs.Log("Menu - Custom Krasue");
		isWindowOpen = true;
		Screens.View(ScreenId.CustomKrasue);
	}

	public void ShowPrivacyPolicy()
	{
		isPrivacyPolicyWindowOpen = true;
		privacyPolicyWindow.SetActive(true);
		Breadcrumbs.Log("Menu - Privacy Policy");
		Screens.ViewOverlay(ScreenId.PrivacyPolicy);
	}

	public void ShowNotEnoughCoins()
	{
		MessageBox.Show(ScriptLocalization.NotEnoughCoins);
	}

	public void CloseAllWindows()
	{
		HideAllWindows();
		Screens.View(ScreenId.MainMenu);
	}

	public void HideAllWindows()
	{
		if (dailyChallengeWindow != null)
		{
			dailyChallengeWindow.Hide();
		}
		chooseLevelWindow.SetActive(false);
		streakWindow.enabled = false;
		credits.SetActive(false);
		options.enabled = false;
		socialWindow.Hide();
		customKrasueWindow.enabled = false;
		quitWindow.enabled = false;
		CustomKrasue.Instance.CloseWindow();
		privacyPolicyWindow.SetActive(false);
		isWindowOpen = false;
		isPrivacyPolicyWindowOpen = false;
		isCreditsWindowOpen = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}
