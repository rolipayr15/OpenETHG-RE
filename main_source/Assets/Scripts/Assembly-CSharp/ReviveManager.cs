using UnityEngine.SceneManagement;
using Eyes.DailyChallenges;
using Firebase.Crashlytics;
using I2.Loc;
using EyesAnalytics;
using Bibloteka;
using DarkTonic.MasterAudio;
using Prime31.MessageKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class ReviveManager : CustomSingleton<ReviveManager>
{



	private Canvas canvas;

	private bool isOpen;

	private bool isDeathDelayPending;

	private uint windowVersion;

	private GameplayInputWindow inputWindow;

	private GameplayInputContext.Lease visibleDialogLease;

	[SerializeField]
	private GameObject reviveWithCoins;

	[SerializeField]
	private GameObject reviveWithAds;

	[SerializeField]
	private Text watchAdLabel;

	private int reviveLimit;

	private int reviveCounter;

	private bool isWaitingForRewardedAd;

	private const float RewardedAdWaitTimeout = 3f;

	private const float RewardedAdCheckInterval = 0.25f;

	[SerializeField]
	private GameObject backToMenuButton;

	[SerializeField]
	private Text revivePriceWithCoinsLabel;

	private bool areRewardedAdsEnabled;

	[SerializeField]
	private GameObject toggleAds;

	[SerializeField]
	private GameObject coinsTotal;

	[SerializeField]
	private GameObject descCoins;

	[SerializeField]
	private GameObject descAds;

	[SerializeField]
	private Button reviveWithCoinsButton;

	private void OnEnable()
	{
		if (!UserGameSettings.IsMultiplayer())
		{
			MessageKit.addObserver(17, WaitAndShowMenu);
		}
	}

	private void Start()
	{
		if (UserGameSettings.IsMultiplayer())
		{
			return;
		}
		canvas = GetComponent<Canvas>();
		areRewardedAdsEnabled = PlayerPrefz.GetBool("SHOW_REWARDED_ADS", true);
		if (UserGameSettings.googleNBO)
		{
			toggleAds.SetActive(false);
			areRewardedAdsEnabled = false;
		}
		else if (!UserGameSettings.unlockedNoAds)
		{
			toggleAds.SetActive(false);
			areRewardedAdsEnabled = true;
		}
		else
		{
			toggleAds.SetActive(true);
		}
		Toggle toggle = toggleAds.GetComponent<Toggle>();
		if (toggle != null)
		{
			toggle.isOn = areRewardedAdsEnabled;
		}
		SetBuyWithAds(areRewardedAdsEnabled);
		watchAdLabel.text = ScriptLocalization.ReviveButtonVer2;
		SetValuesFromFirebaseConfig();
		revivePriceWithCoinsLabel.text = CustomSingleton<CoinsManager>.Instance.GetRevivePriceWithCoins().ToString();
	}

	private void SetValuesFromFirebaseConfig()
	{
		reviveLimit = UserGameSettings.reviveNumberPerGame;
	}

	private void WaitAndShowMenu()
	{
		if (!isActiveAndEnabled || isDeathDelayPending || isWaitingForRewardedAd || isOpen)
		{
			return;
		}
		OpenWindowInput();
		isDeathDelayPending = true;
		StartCoroutine(ShowReviveMenuCoroutine());
	}

	private void OpenWindowInput()
	{
		inputWindow = GameplayInputWindow.GetOrAdd(this);
		inputWindow.Open();
		if (visibleDialogLease != null && !visibleDialogLease.IsDisposed)
		{
			return;
		}
		visibleDialogLease = GameplayInputContext.Shared.Register(this, GameplayInputContext.Kind.DeathWindow, IsDialogVisible, GameplayInputContext.WindowInput.Navigate | GameplayInputContext.WindowInput.Submit);
		GamepadSelectionScope.Begin(gameObject, visibleDialogLease, PreferredOption);
	}

	private Selectable PreferredOption()
	{
		Selectable option;
		if (!areRewardedAdsEnabled)
		{
			option = reviveWithCoinsButton;
		}
		else
		{
			option = reviveWithAds != null ? reviveWithAds.GetComponentInChildren<Selectable>() : null;
		}
		if (option == null || !GamepadSelectionScope.IsValid(option))
		{
			option = backToMenuButton != null ? backToMenuButton.GetComponent<Selectable>() : null;
		}
		return option;
	}

	private bool IsDialogVisible()
	{
		return canvas != null && canvas.enabled && !FullscreenAdSessions.IsShowing;
	}

	private void CloseWindowInput()
	{
		isOpen = false;
		isWaitingForRewardedAd = false;
		windowVersion++;
		if (canvas != null)
		{
			canvas.enabled = false;
		}
		if (inputWindow != null)
		{
			inputWindow.Close();
		}
		GamepadSelectionScope.End(gameObject);
		visibleDialogLease?.Dispose();
		visibleDialogLease = null;
	}

	private IEnumerator ShowReviveMenuCoroutine()
	{
		yield return Utils.GetDelay(3f);
		isDeathDelayPending = false;
		ShowReviveMenu();
	}

	public void ShowReviveMenu()
	{
		if (!isActiveAndEnabled || UserGameSettings.IsMultiplayer() || isWaitingForRewardedAd || isOpen)
		{
			return;
		}
		if (canvas == null)
		{
			canvas = GetComponent<Canvas>();
			if (canvas == null)
			{
				return;
			}
		}
		OpenWindowInput();
		RefreshPaymentMethod();
		if (!UserGameSettings.googleNBO && !UserGameSettings.unlockedNoAds)
		{
			if (reviveLimit <= reviveCounter)
			{
				Game.ReviveFlow((Game.ReviveStage)5, "ad", "revive_limit");
				Continue("revive_limit");
				return;
			}
			if (CustomSingleton<AdsManager>.Instance.IsRewardedAdAvailable())
			{
				OpenRewardedReviveMenu();
				return;
			}
			CustomSingleton<AdsManager>.Instance.EnsureRewardedAdsLoading();
			StartCoroutine(WaitForRewardedAd());
			return;
		}
		reviveWithCoinsButton.interactable = CustomSingleton<CoinsManager>.Instance.CanReviveWithCoins();
		canvas.enabled = true;
		isOpen = true;
		Game.ReviveFlow((Game.ReviveStage)1, areRewardedAdsEnabled ? "ad" : "coins");
	}

	private void OpenRewardedReviveMenu()
	{
		reviveCounter++;
		canvas.enabled = true;
		isOpen = true;
		Game.ReviveFlow((Game.ReviveStage)1, "ad");
	}

	private IEnumerator WaitForRewardedAd()
	{
		isWaitingForRewardedAd = true;
		Game.ReviveFlow((Game.ReviveStage)0, "ad");
		float elapsed = 0f;
		WaitForSecondsRealtime checkDelay = new WaitForSecondsRealtime(RewardedAdCheckInterval);
		while (elapsed < RewardedAdWaitTimeout)
		{
			yield return checkDelay;
			elapsed += RewardedAdCheckInterval;
			if (CustomSingleton<AdsManager>.Instance.IsRewardedAdAvailable())
			{
				isWaitingForRewardedAd = false;
				OpenRewardedReviveMenu();
				yield break;
			}
		}
		isWaitingForRewardedAd = false;
		CustomSingleton<AdsManager>.Instance.LogRewardedAdAvailability("revive_timeout");
		Breadcrumbs.Log("Revive - no ad available");
		Game.ReviveFlow((Game.ReviveStage)5, "ad", "ad_wait_timeout");
		Continue("revive_unavailable");
	}

	public void ShowAd()
	{
		Game.ReviveFlow((Game.ReviveStage)2, "ad");
		Game.ReviveToken reviveToken = Game.CaptureReviveToken();
		uint expectedWindowVersion = windowVersion;
		CustomSingleton<AdsManager>.Instance.ShowRewardedAd((RewardedAdPlacement)1, delegate(bool completed)
		{
			if (this == null || !isActiveAndEnabled || expectedWindowVersion != windowVersion || !isOpen)
			{
				return;
			}
			if (completed)
			{
				if (canvas != null)
				{
					canvas.enabled = false;
				}
				RevivePlayer();
			}
			else
			{
				Game.ReviveFlow((Game.ReviveStage)6, "ad", "ad_not_completed", reviveToken);
			}
		});
	}

	public void RevivePlayer()
	{
		StopAllCoroutines();
		CloseWindowInput();
		Breadcrumbs.Log("Revive - successful");
		MessageKit.post(53);
		MasterAudio.FadeOutAllOfSound("Enemy", 1f);
		if (FirebaseState.IsInitialized)
		{
			Crashlytics.Log("Revive");
		}
	}

	public void Cancel()
	{
		Breadcrumbs.Log("Revive - cancel");
		Game.ReviveFlow((Game.ReviveStage)4, areRewardedAdsEnabled ? "ad" : "coins");
		Continue("revive_declined");
	}

	private void Continue(string endReason)
	{
		PushNotificationPermissionPresenter.FinishGame(this);
		DailyChallengeRuntime.Finish(this, endReason);
		MessageKit.post(90);
		CustomEvents.FirebaseEventLevelFinished(UserGameSettings.currentLevel, UserGameSettings.currentEnemy, UserGameSettings.currentMode, UserGameSettings.currentDifficulty, (GameFinishedResult)1, UserGameSettings.expertMode, UserGameSettings.bagsCollected, UserGameSettings.eyesCollected, endReason);
		if (CustomSingleton<MoneyBagsManager>.Instance.CanEscape())
		{
			MessageKit.post(55);
		}
		StopAllCoroutines();
		CloseWindowInput();
		UserGameSettings.lastGameDuration = Time.timeSinceLevelLoad;
		SceneManager.LoadScene("Over");
	}

	public bool IsReviveMenuOpened()
	{
		return isOpen;
	}

	public void BuyReviveWithCoins()
	{
		Game.ReviveFlow((Game.ReviveStage)2, "coins");
		if (!CustomSingleton<CoinsManager>.Instance.BuyIfCanReviveWithCoins())
		{
			Game.ReviveFlow((Game.ReviveStage)6, "coins", "insufficient_coins");
			Continue("revive_unavailable");
			return;
		}
		RevivePlayer();
		canvas.enabled = false;
	}

	private IEnumerator ShowWithDelay()
	{
		yield return new WaitForSecondsRealtime(3f);
		backToMenuButton.SetActive(true);
	}

	public void ShowAdsWithRewards(bool status)
	{
		SetBuyWithAds(status);
	}

	private void SetBuyWithAds(bool status)
	{
		areRewardedAdsEnabled = status;
		PlayerPrefz.SetBool("SHOW_REWARDED_ADS", status);
		reviveWithCoins.SetActive(!status);
		reviveWithAds.SetActive(status);
		coinsTotal.SetActive(!status);
		descAds.SetActive(status);
		descCoins.SetActive(!status);
		if (!status)
		{
			reviveWithCoinsButton.interactable = CustomSingleton<CoinsManager>.Instance.CanGetMoreEyesWithCoins();
		}
	}

	private void RefreshPaymentMethod()
	{
		bool useAds = !UserGameSettings.googleNBO && (!UserGameSettings.unlockedNoAds || PlayerPrefz.GetBool("SHOW_REWARDED_ADS", true));
		Toggle toggle = toggleAds.GetComponent<Toggle>();
		if (toggle != null)
		{
			toggle.isOn = useAds;
		}
		SetBuyWithAds(useAds);
	}

	private void OnDisable()
	{
		MessageKit.removeObserver(17, WaitAndShowMenu);
		StopAllCoroutines();
		CloseWindowInput();
	}

	protected override void OnDestroy()
	{
		MessageKit.removeObserver(17, WaitAndShowMenu);
		CloseWindowInput();
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}
