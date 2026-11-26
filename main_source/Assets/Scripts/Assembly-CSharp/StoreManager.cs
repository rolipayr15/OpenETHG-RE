using EyesAnalytics;
using UnityEngine;

public class StoreManager : MonoBehaviour
{
	private const string BirthdayGiftKey = "BIRTHDAY_GIFT";

	private const int BirthdayGiftKeys = 50;

	public void ShowStore()
	{
		MenuPurchase.Show(StoreItems.MULTIPLAYER_KEYS_UNLIMITED, CartType.BundleShop);
	}

	// One-time anniversary gift; the message is shown again on later taps but keys are granted once.
	public void CollectGift()
	{
		if (!PlayerPrefs.HasKey(BirthdayGiftKey))
		{
			PlayerPrefz.SetBool(BirthdayGiftKey, true);
			Currency.SendCurrencyEvent(CurrencyFlowType.Source, CurrencyType.MultiplayerKeys, BirthdayGiftKeys, CurrencyItemType.Giveaway, CurrencyItemId.Anniversary10);
			KeysManager.AddMultiplayerKeys(BirthdayGiftKeys);
		}
		MessageBox.Show(string.Format("{0} multiplayer keys added to your account!\nDon't forget to check our\n<color=#FADD86>awesome birthday bundle packs</color>!", BirthdayGiftKeys), null);
	}
}
