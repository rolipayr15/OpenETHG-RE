using EyesAnalytics;
using Prime31.MessageKit;
using UnityEngine;
using UnityEngine.SceneManagement;

// "Unlock this content" popup (additive "MenuPurchase" scene) showing the offer for one store item.
public class MenuPurchase : MonoBehaviour
{
	private const string SceneName = "MenuPurchase";

	[SerializeField]
	private GameObject hospital;

	[SerializeField]
	private GameObject school;

	[SerializeField]
	private GameObject charlie;

	[SerializeField]
	private GameObject charlieHospital;

	[SerializeField]
	private GameObject witch;

	[SerializeField]
	private GameObject goodBoy;

	[SerializeField]
	private GameObject goodBoySchool;

	[SerializeField]
	private GameObject endless;

	[SerializeField]
	private GameObject pixel;

	[SerializeField]
	private GameObject doubleTrouble;

	[SerializeField]
	private GameObject trickOrTreat;

	[SerializeField]
	private GameObject immersive;

	[SerializeField]
	private GameObject oneTimeAd;

	[SerializeField]
	private GameObject noAds;

	[SerializeField]
	private GameObject[] bundles;

	private static StoreItems _storeItem;

	private static CartType _cartType;

	public void Back()
	{
		Close();
	}

	private void Start()
	{
		Screens.ViewOverlay(ScreenId.MenuPurchase);
		ShowStoreEntry(_storeItem);
	}

	private void OnDestroy()
	{
		Screens.CloseOverlay(ScreenId.MenuPurchase);
	}

	public void Buy(string purchaserItem)
	{
		Purchaser purchaser = CustomSingleton<Purchaser>.Instance;
		IAP.PurchaseContext context = new IAP.PurchaseContext(purchaserItem, _cartType, _cartType != CartType.BundleShop);
		purchaser.BuyProductID(purchaserItem, context, delegate
		{
			Close();
		});
	}

	public void BuyGold()
	{
		Buy("bundle_1");
	}

	// The platinum bundle also grants 1500 multiplayer keys.
	public void BuyPlatinum()
	{
		Purchaser purchaser = CustomSingleton<Purchaser>.Instance;
		IAP.PurchaseContext context = new IAP.PurchaseContext("bundle_2", _cartType);
		purchaser.BuyProductID("bundle_2", context, delegate
		{
			KeysManager.AddMultiplayerKeys(1500);
			Close();
		});
	}

	public void BuyDiamond()
	{
		Buy("bundle_3");
	}

	public static void Show(StoreItems storeItem, CartType cartType)
	{
		_storeItem = storeItem;
		_cartType = cartType;
		SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
	}

	public void GoToStore()
	{
		Close();
		Store.Show();
	}

	private void ShowStoreEntry(StoreItems storeEntry)
	{
		GameObject entry;
		switch ((int)storeEntry)
		{
		case 0:
			entry = charlie;
			break;
		case 1:
			entry = hospital;
			break;
		case 2:
			entry = doubleTrouble;
			break;
		case 3:
			entry = endless;
			break;
		case 4:
			entry = trickOrTreat;
			break;
		case 5:
			entry = noAds;
			break;
		case 6:
			entry = goodBoy;
			break;
		case 7:
			entry = school;
			break;
		case 9:
			entry = charlieHospital;
			break;
		case 10:
			entry = goodBoySchool;
			break;
		case 11:
			entry = pixel;
			break;
		case 12:
			// The witch offer can be rented for one game by watching an ad, but only from the main menu.
			witch.SetActive(true);
			if (_cartType != CartType.MainMenu && oneTimeAd != null)
			{
				oneTimeAd.SetActive(false);
			}
			return;
		case 13:
			entry = immersive;
			break;
		case 25:
			foreach (GameObject bundle in bundles)
			{
				RectTransform rect = bundle.GetComponent<RectTransform>();
				rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y + 100f);
			}
			return;
		default:
			return;
		}
		entry.SetActive(true);
	}

	public static void Close()
	{
		if (Bibloteka.Utils.IsSceneLoaded(SceneName))
		{
			SceneManager.UnloadSceneAsync(SceneName, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
		}
	}

	public void RentFor1Game()
	{
		CustomSingleton<AdsManager>.Instance.ShowRewardedAd(RewardedAdPlacement.CONTENT_ONE_TIME_ACCESS, delegate(bool rewarded)
		{
			if (rewarded)
			{
				AdTicketStore.Grant(_storeItem);
				MessageKit<StoreItems>.post(133, _storeItem);
				PlayerPrefz.SetBool("RENT_" + CustomSingleton<Purchaser>.Instance.GetInAppName(_storeItem), false);
				Close();
			}
		});
	}
}
