using Rewired;
using EyesAnalytics;
using Prime31.MessageKit;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class SprintManager : MonoBehaviour
{
	public enum HoldAction
	{
		None = 0,
		Start = 1,
		Stop = 2
	}

	[SerializeField]
	private Button buttonSprint;

	private StringBuilder sb;

	private bool isRewardAcquired;

	private Coroutine countdownCoroutine;

	[SerializeField]
	private Sprite sprintButtonImageAfterUpgrade;

	[SerializeField]
	private GameObject backToMenuButton;

	[SerializeField]
	private GameObject boostPlusButton;

	[SerializeField]
	private Image buttonImage;

	private bool showRewardedAds;

	[SerializeField]
	private GameObject toggleAds;

	[SerializeField]
	private GameObject coinsTotal;

	[SerializeField]
	private Text runBoostPriceWithCoinsLabel;

	[SerializeField]
	private GameObject buyWithAds;

	[SerializeField]
	private GameObject butWithCoins;

	[SerializeField]
	private Button runBoostWithCoinsButton;

	private vp_PlayerFootFXHandler footstepsManager;

	private bool initialized;

	private bool subscribed;

	[SerializeField]
	private Canvas sprintBoostWindow;

	private bool isOpen;

	private GameplayInputContext.Lease windowInputLease;

	private IDisposable windowPauseLease;

	[SerializeField]
	private SprintStaminaUI sprintStamina;

	private void OnEnable()
	{
		if (!subscribed)
		{
			subscribed = true;
			MessageKit<bool>.addObserver(13, StartSprintEffect);
		}
	}

	private void Start()
	{
		showRewardedAds = PlayerPrefz.GetBool("SHOW_REWARDED_ADS", true);
		if (UserGameSettings.googleNBO)
		{
			runBoostPriceWithCoinsLabel.text = CustomSingleton<CoinsManager>.Instance.GetRunBoostPriceWithCoins().ToString();
			toggleAds.SetActive(false);
			showRewardedAds = false;
		}
		else if (!UserGameSettings.unlockedNoAds)
		{
			showRewardedAds = true;
			toggleAds.SetActive(false);
		}
		else
		{
			toggleAds.SetActive(true);
			runBoostPriceWithCoinsLabel.text = CustomSingleton<CoinsManager>.Instance.GetRunBoostPriceWithCoins().ToString();
		}
		Toggle toggle = toggleAds.GetComponent<Toggle>();
		if (toggle != null)
		{
			toggle.isOn = showRewardedAds;
		}
		SetBuyWithAds(showRewardedAds);
		sb = new StringBuilder();
		footstepsManager = CustomSingleton<PlayerManager>.Instance.Player.GetComponent<vp_PlayerFootFXHandler>();
		boostPlusButton.SetActive(true);
		initialized = true;
	}

	private void OnDisable()
	{
		if (subscribed)
		{
			MessageKit<bool>.removeObserver(13, StartSprintEffect);
			subscribed = false;
		}
		if (sprintStamina != null)
		{
			sprintStamina.StopSprint();
		}
		CloseWindow();
	}

	private void OnDestroy()
	{
		OnDisable();
	}

	private void LateUpdate()
	{
		if (sprintStamina == null)
		{
			return;
		}
		ControlModeManager manager = ControlModeManager.Instance;
		Player player = manager != null ? manager.Player : null;
		if (!initialized || player == null || !ReInput.isReady)
		{
			sprintStamina.StopSprint();
			return;
		}
		if (IsHoldMode(manager.Mode))
		{
			bool allowed = manager.isActiveAndEnabled && manager.HasFocus && manager.SelectedSourceAvailable && !manager.RequiresExplicitResume && GameplayInputContext.Shared.Current.StartActions;
			switch (DecideHold(allowed, player.GetButton(8), player.GetButtonDown(8), sprintStamina.IsSprinting))
			{
			case HoldAction.Start:
				sprintStamina.StartSprintFromHold();
				break;
			case HoldAction.Stop:
				sprintStamina.StopSprint();
				break;
			}
		}
		else if (player.GetButtonDown(8))
		{
			sprintStamina.OnSprintButtonClicked();
		}
	}

	public static bool IsHoldMode(ControlMode mode)
	{
		return (uint)((int)mode - 1) < 2u;
	}

	public static HoldAction DecideHold(bool allowed, bool held, bool pressedThisFrame, bool sprinting)
	{
		if (!allowed || !held)
		{
			return HoldAction.Stop;
		}
		return pressedThisFrame && !sprinting ? HoldAction.Start : HoldAction.None;
	}

	public void IsSprinting(bool isSprinting)
	{
		if (isSprinting)
		{
			StartSprint();
		}
		else
		{
			StopSprint();
		}
	}

	public void StartSprint()
	{
		LevelUI.SkillUsed((LevelUI.Skill)1, true);
		CustomSingleton<PlayerManager>.Instance.playerHandler.Run.Start();
		if (footstepsManager != null)
		{
			footstepsManager.TimeInterval = 0.35f;
		}
	}

	public void StopSprint()
	{
		PlayerManager manager = CustomSingleton<PlayerManager>.HasInstance ? CustomSingleton<PlayerManager>.Instance : null;
		vp_FPPlayerEventHandler handler = manager != null ? manager.playerHandler : null;
		if (handler != null)
		{
			handler.Run.Stop();
		}
		if (!isRewardAcquired && boostPlusButton != null)
		{
			boostPlusButton.SetActive(true);
		}
		if (footstepsManager != null)
		{
			footstepsManager.TimeInterval = 0.5f;
		}
	}

	private void StartSprintEffect(bool shouldStart)
	{
		if (shouldStart)
		{
			boostPlusButton.SetActive(false);
			isRewardAcquired = true;
			SetRunBoost();
		}
	}

	public void ShowRewardedAdToGetSprintBoost()
	{
		CustomSingleton<AdsManager>.Instance.ShowRewardedAd(UserGameSettings.IsMultiplayer() ? (RewardedAdPlacement)10 : (RewardedAdPlacement)5, delegate(bool completed)
		{
			if (completed)
			{
				SetRunBoost();
				isRewardAcquired = true;
				CloseWindow();
			}
		});
	}

	public void BuyRunBoostWithCoins()
	{
		if (CustomSingleton<CoinsManager>.Instance.BuyRunBoostWithCoins())
		{
			SetRunBoost();
			isRewardAcquired = true;
			CloseWindow();
		}
	}

	public void OpenWindow()
	{
		if (isOpen || !isActiveAndEnabled)
		{
			return;
		}
		LevelUI.SkillUsed((LevelUI.Skill)1, false);
		if (UserGameSettings.adsDisabledViaRemoteConfig)
		{
			return;
		}
		RefreshPaymentMethod();
		windowInputLease = GameplayInputContext.Shared.Acquire(this, GameplayInputContext.Kind.Modal, GameplayInputContext.WindowInput.Cancel | GameplayInputContext.WindowInput.Navigate | GameplayInputContext.WindowInput.Submit, CloseWindow);
		windowPauseLease = CustomSingleton<PauseManager>.Instance.AcquirePause(this);
		backToMenuButton.SetActive(false);
		sprintBoostWindow.enabled = true;
		backToMenuButton.SetActive(true);
		isOpen = true;
		GamepadSelectionScope.Begin(sprintBoostWindow.gameObject, windowInputLease, PreferredOption);
	}

	private Selectable PreferredOption()
	{
		Selectable option = showRewardedAds ? buyWithAds.GetComponentInChildren<Selectable>() : runBoostWithCoinsButton;
		if (option == null || !GamepadSelectionScope.IsValid(option))
		{
			option = GamepadSelectionScope.FindPersistentButton(sprintBoostWindow.gameObject, this, "CloseWindow");
		}
		return option;
	}

	public void CloseWindow()
	{
		if (sprintBoostWindow != null)
		{
			GamepadSelectionScope.End(sprintBoostWindow.gameObject);
			sprintBoostWindow.enabled = false;
		}
		isOpen = false;
		windowInputLease?.Dispose();
		windowInputLease = null;
		windowPauseLease?.Dispose();
		windowPauseLease = null;
	}

	private void SetRunBoost()
	{
		sprintStamina.SetSprintBoost();
		buttonImage.sprite = sprintButtonImageAfterUpgrade;
		boostPlusButton.SetActive(false);
	}

	public void SetBuyWithAds(bool status)
	{
		showRewardedAds = status;
		PlayerPrefz.SetBool("SHOW_REWARDED_ADS", status);
		coinsTotal.SetActive(!status);
		buyWithAds.SetActive(status);
		butWithCoins.SetActive(!status);
		if (!status)
		{
			runBoostPriceWithCoinsLabel.text = CustomSingleton<CoinsManager>.Instance.GetRunBoostPriceWithCoins().ToString();
			runBoostWithCoinsButton.interactable = CustomSingleton<CoinsManager>.Instance.CanGetMoreEyesWithCoins();
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
}
