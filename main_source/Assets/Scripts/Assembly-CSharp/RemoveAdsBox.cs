using Prime31.MessageKit;
using EyesAnalytics;
using UnityEngine.SceneManagement;
using System;
using Rewired;
using UnityEngine;

public class RemoveAdsBox : MonoBehaviour
{
	private static bool isRemoveAdsBoxActive;

	private static Action callAfterDismiss;

	private bool closing;

	private static Action<string> unload = delegate(string scene) { SceneManager.UnloadSceneAsync(scene, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects); };

	private static Action purchase = delegate { CustomSingleton<Purchaser>.Instance.BuyProductID("eyes.noads", new IAP.PurchaseContext("eyes.noads", CartType.MainMenu)); };

	public static void Show()
	{
		if (!isRemoveAdsBoxActive)
		{
			SceneManager.LoadSceneAsync("RemoveAdsBox", LoadSceneMode.Additive);
			isRemoveAdsBoxActive = true;
		}
	}

	public void OkButtonClicked()
	{
		if (closing)
		{
			return;
		}
		closing = true;
		isRemoveAdsBoxActive = false;
		callAfterDismiss?.Invoke();
		unload?.Invoke("RemoveAdsBox");
	}

	private void Awake()
	{
		Screens.ViewOverlay((ScreenId)12);
		InteractionManager.SetRewiredMapping(DismissMessageBoxRewired, 33);
		InteractionManager.SetRewiredMapping(DismissMessageBoxRewired, 34);
		MessageKit<string>.addObserver(34, UnlockIfNoAds);
	}

	private void OnDestroy()
	{
		InteractionManager.RemoveRewiredMapping(DismissMessageBoxRewired, 33);
		InteractionManager.RemoveRewiredMapping(DismissMessageBoxRewired, 34);
		MessageKit<string>.removeObserver(34, UnlockIfNoAds);
		Screens.CloseOverlay((ScreenId)12);
	}

	private void DismissMessageBoxRewired(InputActionEventData _not_used)
	{
		OkButtonClicked();
	}

	private void Update()
	{
		if (!TextEntryInputGuard.BlocksShortcuts && Input.GetKeyDown(KeyCode.Escape))
		{
			OkButtonClicked();
		}
	}

	public void BuyNoAds()
	{
		if (!closing)
		{
			purchase();
		}
	}

	private void UnlockIfNoAds(string product)
	{
		if (product == "eyes.noads" || product == "birthday_platinium" || product == "birthday_diamond")
		{
			MessageKit<string>.removeObserver(34, UnlockIfNoAds);
			OkButtonClicked();
		}
	}
}
