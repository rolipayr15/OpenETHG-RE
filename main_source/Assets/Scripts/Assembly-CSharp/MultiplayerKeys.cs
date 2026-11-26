using System.Collections;
using Bibloteka;
using EyesAnalytics;
using I2.Loc;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MultiplayerKeys : MonoBehaviour
{
	private const string SceneName = "MultiplayerKeys";

	private static bool _isClosing;

	[SerializeField]
	private Text errorLabel;

	[SerializeField]
	private Text titleLabel;

	[SerializeField]
	private GameObject watchVideoToGetTicketsButton;

	private Button watchVideoButton;

	private static string _title = "";

	private GameplayInputContext.Lease inputLease;

	private static MultiplayerKeys inputOwner;

	private static CartType _cartType;

	public static bool OwnsGameplayInput => inputOwner != null;

	private void OnEnable()
	{
		if (CustomSingleton<PauseManager>.HasInstance && (inputLease == null || inputLease.IsDisposed))
		{
			inputLease = GameplayInputContext.Shared.Acquire(this, (GameplayInputContext.Kind)13, (GameplayInputContext.WindowInput)7, Back);
			inputOwner = this;
			GamepadSelectionScope.Begin(gameObject, inputLease, BackButton);
		}
	}

	private void OnDisable()
	{
		GamepadSelectionScope.End(gameObject);
		if (inputOwner == this)
		{
			inputOwner = null;
		}
		inputLease?.Dispose();
		inputLease = null;
	}

	private Selectable BackButton()
	{
		foreach (Button button in GetComponentsInChildren<Button>(true))
		{
			for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
			{
				if (button.onClick.GetPersistentTarget(i) == this && button.onClick.GetPersistentMethodName(i) == "Back")
				{
					return button;
				}
			}
		}
		return null;
	}

	private void Start()
	{
		Screens.ViewOverlay((ScreenId)14);
		titleLabel.text = _title;
		watchVideoButton = watchVideoToGetTicketsButton.GetComponent<Button>();
		StartCoroutine(CheckIfAdIsAvailable());
	}

	private void OnDestroy()
	{
		Screens.CloseOverlay((ScreenId)14);
	}

	public void Back()
	{
		TryClose();
	}

	public static void Close()
	{
		TryClose();
	}

	private static void TryClose()
	{
		_title = "";
		if (_isClosing)
		{
			return;
		}
		Scene scene = SceneManager.GetSceneByName(SceneName);
		if (!scene.IsValid() || !scene.isLoaded)
		{
			return;
		}
		_isClosing = true;
		AsyncOperation operation = SceneManager.UnloadSceneAsync(scene, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
		if (operation == null)
		{
			_isClosing = false;
			return;
		}
		operation.completed += delegate
		{
			_isClosing = false;
		};
	}

	public static void Show(CartType cartType, string title)
	{
		if (_isClosing)
		{
			return;
		}
		Scene scene = SceneManager.GetSceneByName(SceneName);
		if (!scene.IsValid() || !scene.isLoaded)
		{
			_title = title;
			_cartType = cartType;
			SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
		}
	}

	public void WatchVideoToGetTickets()
	{
		AdsManager ads = CustomSingleton<AdsManager>.Instance;
		ads.WhenRewardedAdAvailable(this, (RewardedAdPlacement)8, delegate
		{
			ads.ShowRewardedAd((RewardedAdPlacement)8, delegate(bool rewarded)
			{
				if (rewarded)
				{
					Currency.SendCurrencyEvent((CurrencyFlowType)1, (CurrencyType)1, UserGameSettings.rewardedAdTickets, (CurrencyItemType)1, (CurrencyItemId)5);
					KeysManager.AddMultiplayerKeys(UserGameSettings.rewardedAdTickets);
					if (this != null)
					{
						errorLabel.text = "";
						TryClose();
					}
				}
			});
		}, delegate
		{
			Utils.ShowToast(ScriptLocalization.AdNotAvailable);
		});
	}

	private IEnumerator CheckIfAdIsAvailable()
	{
		watchVideoButton.interactable = false;
		AdsManager ads = CustomSingleton<AdsManager>.Instance;
		ads.EnsureRewardedAdsLoading();
		while (!ads.IsRewardedAdAvailable())
		{
			yield return Utils.GetDelay(0.5f);
		}
		watchVideoButton.interactable = true;
	}

	private void SetActiveSub()
	{
		Debug.Log("sub is active");
	}

	public void BuyDiamond()
	{
		CustomSingleton<Purchaser>.Instance.BuyProductID("bundle_3", new EyesAnalytics.IAP.PurchaseContext("bundle_3", (EyesAnalytics.CartType)_cartType, false), delegate
		{
			TryClose();
		});
	}
}
