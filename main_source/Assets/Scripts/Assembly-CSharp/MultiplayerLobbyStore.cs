using System.Text.RegularExpressions;
using Prime31.MessageKit;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerLobbyStore : MonoBehaviour
{
	[SerializeField]
	private MultiplayerLobby lobby;

	[SerializeField]
	private GameObject storeWindow;

	[SerializeField]
	private GameObject hospital;

	[SerializeField]
	private GameObject school;

	[SerializeField]
	private GameObject charlie;

	[SerializeField]
	private GameObject witch;

	[SerializeField]
	private GameObject goodBoy;

	[SerializeField]
	private Text goldBundlePriceBefore;

	[SerializeField]
	private Text goldBundlePriceNow;

	[SerializeField]
	private GameObject notEnoughCoins;

	public void Back()
	{
		notEnoughCoins.SetActive(false);
		MessageKit<string>.removeObserver(34, Unlocked);
		hospital.SetActive(false);
		school.SetActive(false);
		charlie.SetActive(false);
		goodBoy.SetActive(false);
		witch.SetActive(false);
		storeWindow.SetActive(false);
	}

	private void Start()
	{
		MessageKit<string>.addObserver(34, Unlocked);
		Purchaser purchaser = CustomSingleton<Purchaser>.Instance;
		string pattern = purchaser.GetProductPrice("multiplayer_keys");
		float total = purchaser.GetProductPriceFloat("charlie_hospital") + purchaser.GetProductPriceFloat("good_boy_school") + purchaser.GetProductPriceFloat("endless") + purchaser.GetProductPriceFloat("witch") + purchaser.GetProductPriceFloat("immersive") + purchaser.GetProductPriceFloat("trickortreat") + purchaser.GetProductPriceFloat("doubleenemy") + purchaser.GetProductPriceFloat("pixel");
		goldBundlePriceBefore.text = FormatPrice(pattern, total.ToString("n2"));
		goldBundlePriceNow.text = purchaser.GetProductPrice("birthday_gold");
	}

	public void Buy()
	{
		CustomSingleton<Purchaser>.Instance.BuyProductID("birthday_gold", new EyesAnalytics.IAP.PurchaseContext("birthday_gold", (EyesAnalytics.CartType)1, false));
	}

	private void Unlocked(string productId)
	{
		if (productId == "birthday_gold")
		{
			Back();
		}
	}

	public void Show(Enemy enemy)
	{
		storeWindow.SetActive(true);
		if ((int)enemy == 6)
		{
			witch.SetActive(true);
		}
		else if ((int)enemy == 2)
		{
			goodBoy.SetActive(true);
		}
		else if ((int)enemy == 1)
		{
			charlie.SetActive(true);
		}
	}

	public void Show(Level level)
	{
		storeWindow.SetActive(true);
		if ((int)level == 3)
		{
			school.SetActive(true);
		}
		else if ((int)level == 1)
		{
			hospital.SetActive(true);
		}
	}

	public void GoToStore()
	{
		Back();
		lobby.Close();
		Store.Show();
	}

	private static string FormatPrice(string patternPrice, string priceWithoutIsoSymbol)
	{
		return Regex.Replace(patternPrice, "([0-9,.]+[\\s]*[0-9,.]+)", priceWithoutIsoSymbol);
	}
}
