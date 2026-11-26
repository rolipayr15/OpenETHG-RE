using Prime31.MessageKit;
using System;
using UnityEngine;
using UnityEngine.UI;

public class MoreEyesManager : CustomSingleton<MoreEyesManager>
{
	[SerializeField]
	private Canvas moreEyesCanvas;

	private bool isOpen;

	private bool isWaitingForRewardedAd;

	private Coroutine rewardedAdWaitCoroutine;

	private GameplayInputWindow inputWindow;

	[SerializeField]
	private GameObject moreEyesCoins;

	[SerializeField]
	private GameObject moreEyesAds;

	[SerializeField]
	private Button getMoreEyesWithCoinsButton;

	[SerializeField]
	private GameObject coinsTotal;

	[SerializeField]
	private Text moreEyesPriceWithCoinsLabel;

	[SerializeField]
	private GameObject toggleAds;

	private bool showRewardedAds;

	private TimeSpan cooldownRewardedGetMoreEyes = TimeSpan.FromMinutes(1.0);

	private DateTime lastTimeShownAdMoreEyes;

	private void OnEnable()
	{
		MessageKit.addObserver(64, Close);
	}

	private void Start()
	{
		showRewardedAds = PlayerPrefz.GetBool("SHOW_REWARDED_ADS", true);
		if (UserGameSettings.googleNBO)
		{
			moreEyesPriceWithCoinsLabel.text = CustomSingleton<CoinsManager>.Instance.GetMoreCoinsPriceWithCoins().ToString();
			showRewardedAds = false;
			toggleAds.SetActive(false);
		}
		else if (!UserGameSettings.unlockedNoAds)
		{
			showRewardedAds = true;
			toggleAds.SetActive(false);
		}
		else
		{
			toggleAds.SetActive(true);
			moreEyesPriceWithCoinsLabel.text = CustomSingleton<CoinsManager>.Instance.GetMoreCoinsPriceWithCoins().ToString();
		}
		Toggle toggle = toggleAds.GetComponent<Toggle>();
		if (toggle != null)
		{
			toggle.isOn = showRewardedAds;
		}
		SetBuyWithAds(showRewardedAds);
	}

	public void OpenWindow()
	{
		if (UserGameSettings.adsDisabledViaRemoteConfig || !isActiveAndEnabled || isWaitingForRewardedAd || isOpen)
		{
			return;
		}
		RefreshPaymentMethod();
		inputWindow = GameplayInputWindow.GetOrAdd(this);
		inputWindow.Open(GameplayInputContext.Kind.Modal, GameplayInputContext.WindowInput.Cancel | GameplayInputContext.WindowInput.Navigate | GameplayInputContext.WindowInput.Submit, Close, true);
		if (!showRewardedAds)
		{
			getMoreEyesWithCoinsButton.interactable = CustomSingleton<CoinsManager>.Instance.CanGetMoreEyesWithCoins();
		}
		moreEyesCanvas.enabled = true;
		isOpen = true;
		GamepadSelectionScope.Begin(moreEyesCanvas.gameObject, inputWindow.Lease, PreferredOption);
		if (showRewardedAds)
		{
			CustomSingleton<AdsManager>.Instance.EnsureRewardedAdsLoading();
		}
	}

	private Selectable PreferredOption()
	{
		Selectable option = showRewardedAds ? moreEyesAds.GetComponentInChildren<Selectable>() : getMoreEyesWithCoinsButton;
		if (option == null || !GamepadSelectionScope.IsValid(option))
		{
			option = GamepadSelectionScope.FindPersistentButton(moreEyesCanvas.gameObject, this, "Close");
		}
		return option;
	}

	private void FinishRewardedAdWait()
	{
		rewardedAdWaitCoroutine = null;
		isWaitingForRewardedAd = false;
	}

	private void CancelRewardedAdWait()
	{
		if (rewardedAdWaitCoroutine != null)
		{
			StopCoroutine(rewardedAdWaitCoroutine);
		}
		rewardedAdWaitCoroutine = null;
		isWaitingForRewardedAd = false;
	}

	private void OnDisable()
	{
		MessageKit.removeObserver(64, Close);
		Close();
	}

	private void ReleaseWindowInput()
	{
		if (inputWindow != null)
		{
			inputWindow.Close();
		}
	}

	public void Close()
	{
		CancelRewardedAdWait();
		if (moreEyesCanvas != null)
		{
			GamepadSelectionScope.End(moreEyesCanvas.gameObject);
			moreEyesCanvas.enabled = false;
		}
		isOpen = false;
		ReleaseWindowInput();
	}

	public void ShowAd()
	{
		if (!isActiveAndEnabled || !isOpen || !showRewardedAds || isWaitingForRewardedAd)
		{
			return;
		}
		isWaitingForRewardedAd = true;
		rewardedAdWaitCoroutine = CustomSingleton<AdsManager>.Instance.WhenRewardedAdAvailable(this, (RewardedAdPlacement)0, delegate
		{
			FinishRewardedAdWait();
			if (isActiveAndEnabled && isOpen)
			{
				ShowAvailableRewardedAd();
			}
		}, delegate
		{
			FinishRewardedAdWait();
			if (isActiveAndEnabled && isOpen)
			{
				CustomSingleton<InternetConnectionRequired>.Instance.ShowWindow();
			}
		});
	}

	private void ShowAvailableRewardedAd()
	{
		CustomSingleton<AdsManager>.Instance.ShowRewardedAd(RewardedPlacement(), delegate(bool completed)
		{
			if (completed)
			{
				GetMoreEyes();
			}
		});
	}

	public void GetMoreEyes()
	{
		CustomSingleton<EyesManager>.Instance.AddRewardedEyes();
		MessageKit.post(2);
		Close();
	}

	public void BuyMoreEyesWithCoins()
	{
		CustomSingleton<CoinsManager>.Instance.BuyMoreEyesWithCoins();
	}

	public void ShowAdsWithRewards(bool status)
	{
		SetBuyWithAds(status);
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

	private void SetBuyWithAds(bool status)
	{
		if (!status)
		{
			CancelRewardedAdWait();
		}
		showRewardedAds = status;
		PlayerPrefz.SetBool("SHOW_REWARDED_ADS", status);
		moreEyesCoins.SetActive(!status);
		moreEyesAds.SetActive(status);
		coinsTotal.SetActive(!status);
		if (!status)
		{
			moreEyesPriceWithCoinsLabel.text = CustomSingleton<CoinsManager>.Instance.GetMoreCoinsPriceWithCoins().ToString();
			getMoreEyesWithCoinsButton.interactable = CustomSingleton<CoinsManager>.Instance.CanGetMoreEyesWithCoins();
		}
	}

	protected override void OnDestroy()
	{
		MessageKit.removeObserver(64, Close);
		Close();
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}

	private static RewardedAdPlacement RewardedPlacement()
	{
		return UserGameSettings.IsMultiplayer() ? (RewardedAdPlacement)9 : (RewardedAdPlacement)0;
	}
}
