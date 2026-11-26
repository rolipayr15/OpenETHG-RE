using Bibloteka;
using Prime31.MessageKit;
using EyesAnalytics;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class ImmersiveModePurchase : MonoBehaviour
{

	[SerializeField]
	private Text price;

	[SerializeField]
	private Text priceInCoins;

	[SerializeField]
	private Button buyWithCoins;

	[SerializeField]
	private RectTransform[] vanishingIcons;

	private static Action<bool> callbackWithResult;

	private Coroutine mainCoroutine;

	private static bool isWindowOpen;

	public static void Show(Action<bool> callback)
	{
		SceneManager.LoadSceneAsync("Immersive", LoadSceneMode.Additive);
		callbackWithResult = callback;
		isWindowOpen = true;
	}

	private void Start()
	{
		Screens.ViewOverlay((ScreenId)15);
		mainCoroutine = StartCoroutine(MainCoroutine());
	}

	private IEnumerator MainCoroutine()
	{
		MessageKit<string>.addObserver(34, UnlockProduct);
		price.text = CustomSingleton<Purchaser>.Instance.GetProductPrice("immersive");
		priceInCoins.text = UserGameSettings.UnlockPrices_Immersive.ToString();
		if (!CustomSingleton<CoinsManager>.Instance.CanBuyImmersiveModeWithCoins())
		{
			buyWithCoins.interactable = false;
		}
		yield return null;
		vanishingIcons.Shuffle();
		RectTransform[] icons = vanishingIcons;
		foreach (RectTransform icon in icons)
		{
			foreach (Graphic graphic in icon.GetComponentsInChildren<Graphic>())
			{
				graphic.CrossFadeAlpha(0f, 1f, true);
			}
			yield return Utils.GetDelay(1f);
		}
	}

	public void CloseWindow()
	{
		isWindowOpen = false;
		SceneManager.UnloadSceneAsync("Immersive", UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
	}

	public void WatchVideo()
	{
		CustomSingleton<AdsManager>.Instance.ShowRewardedAd(RewardedAdPlacement.IMMERSIVE, delegate(bool success)
		{
			if (success)
			{
				Breadcrumbs.Log("immersive_unlocked_ad");
				UserGameSettings.unlockedImmersiveOneTime = true;
				callbackWithResult(true);
				if (isWindowOpen)
				{
					CloseWindow();
				}
			}
		});
	}

	public void BuyWithUsd()
	{
		CustomSingleton<Purchaser>.Instance.BuyProductID("immersive", new IAP.PurchaseContext("immersive", CartType.ExpertShop));
	}

	public void BuyWithCoins()
	{
		if (CoinsManager.TryDeductCoins(UserGameSettings.UnlockPrices_Immersive, (CurrencyItemType)9, (CurrencyItemId)31))
		{
			Breadcrumbs.Log("immersive_unlocked_coins");
			CustomSingleton<EncryptedValuesManager>.Instance.UnlockProductWithCoins("immersive");
			callbackWithResult(true);
			if (isWindowOpen)
			{
				CloseWindow();
			}
		}
	}

	private void UnlockProduct(string productId)
	{
		if (productId == "immersive")
		{
			Breadcrumbs.Log("immersive_unlocked_iap");
			callbackWithResult?.Invoke(true);
			if (isWindowOpen)
			{
				CloseWindow();
			}
		}
	}

	private void OnDestroy()
	{
		Screens.CloseOverlay((ScreenId)15);
		MessageKit<string>.removeObserver(34, UnlockProduct);
		if (mainCoroutine != null)
		{
			StopCoroutine(mainCoroutine);
		}
	}
}
