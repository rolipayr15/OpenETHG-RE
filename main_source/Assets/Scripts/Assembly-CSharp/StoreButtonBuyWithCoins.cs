using UnityEngine;
using UnityEngine.UI;

public class StoreButtonBuyWithCoins : MonoBehaviour
{
	[SerializeField]
	private int price;

	private int coinsAvailable;

	[SerializeField]
	private Button button;

	[SerializeField]
	private Text coinLabel;

	[SerializeField]
	private Image coinImage;

	private Color yellow;

	private void Awake()
	{
	}

	private void OnEnable()
	{
	}

	private void UpdateButton(int coins)
	{
	}
}
