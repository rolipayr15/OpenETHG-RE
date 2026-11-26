using UnityEngine;
using UnityEngine.UI;

public class Store : MonoBehaviour
{
	[SerializeField]
	private Text goldPrice;

	[SerializeField]
	private Text platPrice;

	[SerializeField]
	private Text diamondPrice;

	[SerializeField]
	private Text goldPriceBefore;

	[SerializeField]
	private Text platinumPriceBefore;

	[SerializeField]
	private Text diamondPriceBefore;

	private bool isOpen;

	private static string FormatPrice(string patternPrice, string priceWithoutIsoSymbol)
	{
		return null;
	}

	public static void Show()
	{
	}

	private void Start()
	{
	}

	private void OnDestroy()
	{
	}

	public void CloseWindow()
	{
	}

	public void Buy(string productId)
	{
	}

	private void Unlocked(string productId)
	{
	}
}
