using I2.Loc;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class ShowInAppPrice : MonoBehaviour
{
	[SerializeField]
	private StoreItems storeItem;

	[SerializeField]
	private Text priceBefore;

	[SerializeField]
	private Text priceNow;

	[SerializeField]
	private Text buyButtonLabel;

	private void Start()
	{
		if (CustomSingleton<EncryptedValuesManager>.Instance.IsProductUnlocked(storeItem))
		{
			buyButtonLabel.text = ScriptLocalization.Unlocked;
			buyButtonLabel.gameObject.GetComponentInParent<Button>().enabled = false;
			return;
		}
		if (!CustomSingleton<Purchaser>.HasInstance || !CustomSingleton<Purchaser>.Instance.IsInitialized())
		{
			return;
		}
		Purchaser purchaser = CustomSingleton<Purchaser>.Instance;
		string pattern = purchaser.GetProductPrice("multiplayer_keys");
		float maps = purchaser.GetProductPriceFloat("charlie_hospital") + purchaser.GetProductPriceFloat("good_boy_school") + purchaser.GetProductPriceFloat("witch");
		float modes = purchaser.GetProductPriceFloat("immersive") + purchaser.GetProductPriceFloat("trickortreat") + purchaser.GetProductPriceFloat("doubleenemy") + purchaser.GetProductPriceFloat("pixel") + purchaser.GetProductPriceFloat("endless");
		float before;
		string bundle;
		switch ((int)storeItem)
		{
		case 24:
			before = modes + maps + purchaser.GetProductPriceFloat("multiplayer_keys_unlimited") + purchaser.GetProductPriceFloat("eyes.noads");
			bundle = "bundle_3";
			break;
		case 23:
			before = modes + maps + purchaser.GetProductPriceFloat("multiplayer_keys");
			bundle = "bundle_2";
			break;
		case 22:
			before = maps;
			bundle = "bundle_1";
			break;
		default:
			return;
		}
		priceBefore.text = FormatPrice(pattern, before.ToString("n2"));
		priceNow.text = purchaser.GetProductPrice(bundle);
	}

	private static string FormatPrice(string patternPrice, string priceWithoutIsoSymbol)
	{
		return Regex.Replace(patternPrice, "([0-9,.]+[\\s]*[0-9,.]+)", priceWithoutIsoSymbol);
	}
}
