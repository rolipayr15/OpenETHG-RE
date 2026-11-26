using Prime31.MessageKit;
using EyesAnalytics;
using UnityEngine;
using UnityEngine.UI;

public class InAppDetails : MonoBehaviour
{
	[SerializeField]
	private StoreItems storeItem;

	private string inAppName;

	[SerializeField]
	private Text price;

	[SerializeField]
	private int coinsPrice;

	[SerializeField]
	private Text priceInCoins;

	[SerializeField]
	private GameObject buttonBuyWithCoins;

	[SerializeField]
	private GameObject notEnoughCoins;

	[SerializeField]
	private CartType cartType;

	[SerializeField]
	private GameObject rentButton;

	public StoreItems GetStoreItem()
	{
		return storeItem;
	}

	private void Start()
	{
		inAppName = CustomSingleton<Purchaser>.Instance.GetInAppName(storeItem);
		MessageKit<string>.addObserver(34, UnlockProduct);
		UpdateDisplayedPrice();
		UpdatePriceInCoins();
		if (!PlayerPrefz.GetBool("RENT_" + inAppName, true) && rentButton != null)
		{
			rentButton.SetActive(false);
		}
	}

	private void UpdateDisplayedPrice()
	{
		if (CustomSingleton<Purchaser>.Instance.IsInitialized())
		{
			price.text = CustomSingleton<Purchaser>.Instance.GetProductPrice(inAppName);
		}
	}

	private void UpdatePriceInCoins()
	{
		switch ((int)storeItem)
		{
		case 1:
			coinsPrice = UserGameSettings.UnlockPrices_Hospital;
			break;
		case 2:
			coinsPrice = UserGameSettings.UnlockPrices_DoubleTrouble;
			break;
		case 3:
			coinsPrice = UserGameSettings.UnlockPrices_Endless;
			break;
		case 4:
			coinsPrice = UserGameSettings.UnlockPrices_TrickOrTreat;
			break;
		case 6:
		case 12:
			coinsPrice = UserGameSettings.UnlockPrices_Witch;
			break;
		case 7:
			coinsPrice = UserGameSettings.UnlockPrices_School;
			break;
		case 9:
			coinsPrice = UserGameSettings.UnlockPrices_Charlie_Hospital;
			break;
		case 10:
			coinsPrice = UserGameSettings.UnlockPrices_GoodBoy_School;
			break;
		case 11:
			coinsPrice = UserGameSettings.UnlockPrices_Pixel;
			break;
		case 13:
			coinsPrice = UserGameSettings.UnlockPrices_Immersive;
			break;
		}
		if (priceInCoins == null)
		{
			return;
		}
		priceInCoins.text = coinsPrice.ToString();
		if (!CoinsManager.CanDeductCoins(coinsPrice))
		{
			buttonBuyWithCoins.GetComponent<Button>().interactable = false;
			notEnoughCoins.SetActive(true);
		}
	}

	public void Buy()
	{
		CustomSingleton<Purchaser>.Instance.BuyProductID(inAppName, new IAP.PurchaseContext(inAppName, cartType), delegate
		{
			if (cartType == CartType.MainMenu)
			{
				MenuPurchase.Close();
			}
		});
	}

	public void BuyWithCoins()
	{
		if (!CoinsManager.TryDeductCoins(coinsPrice, (CurrencyItemType)9, Currency.ItemIdFromInAppName(inAppName)))
		{
			buttonBuyWithCoins.GetComponent<Button>().interactable = false;
			notEnoughCoins.SetActive(true);
		}
		else
		{
			CustomSingleton<EncryptedValuesManager>.Instance.UnlockProductWithCoins(inAppName);
			MessageKit.post(134);
			Back();
			MenuPurchase.Close();
		}
	}

	private void UnlockProduct(string productId)
	{
		if (productId == inAppName || productId == "birthday_diamond" || productId == "birthday_gold" || productId == "birthday_platinium")
		{
			Back();
		}
	}

	public void Back()
	{
		if (notEnoughCoins != null)
		{
			notEnoughCoins.SetActive(false);
		}
	}

	private void OnDestroy()
	{
		MessageKit<string>.removeObserver(34, UnlockProduct);
	}
}
