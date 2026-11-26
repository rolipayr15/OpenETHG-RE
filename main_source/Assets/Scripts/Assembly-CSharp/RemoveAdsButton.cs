using EyesAnalytics;
using Prime31.MessageKit;
using UnityEngine;

public class RemoveAdsButton : MonoBehaviour
{
	private void Start()
	{
		if (UserGameSettings.unlockedNoAds || UserGameSettings.googleNBO)
		{
			gameObject.SetActive(false);
			return;
		}
		MessageKit<string>.addObserver(34, UnlockIfNoAds);
	}

	private void OnDestroy()
	{
		MessageKit<string>.removeObserver(34, UnlockIfNoAds);
	}

	public void BuyNoAdsBundles()
	{
		MenuPurchase.Show(StoreItems.REMOVE_ADS, CartType.MainMenu);
	}

	public void BuyNoAds()
	{
		Purchaser.Instance.BuyProductID("eyes.noads", new IAP.PurchaseContext("eyes.noads", CartType.MainMenu), null, null);
		Breadcrumbs.Log("menu_button_remove_ads");
	}

	private void UnlockIfNoAds(string product)
	{
		if (product == "eyes.noads" || product == "birthday_platinium" || product == "birthday_diamond" || product == "bundle_3")
		{
			MessageKit<string>.removeObserver(34, UnlockIfNoAds);
			gameObject.SetActive(false);
		}
	}
}
