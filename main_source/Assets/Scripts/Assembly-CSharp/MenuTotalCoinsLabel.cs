using System;
using System.Collections;
using Bibloteka;
using Prime31.MessageKit;
using UnityEngine;
using UnityEngine.UI;

public class MenuTotalCoinsLabel : MonoBehaviour
{
	[SerializeField]
	private Text label;

	private int lastValue;

	private void Start()
	{
		lastValue = CoinsManager.GetCoins();
		label.text = lastValue.ToString();
		MessageKit<int>.addObserver(23, UpdateLabel);
	}

	private void UpdateLabel(int coins)
	{
		StartCoroutine(CoinsCounter(coins));
	}

	private IEnumerator CoinsCounter(int coins)
	{
		int numberOfSteps = (int)Math.Round(Mathf.Sqrt(coins + 1)) + 5;
		for (int i = 0; i <= numberOfSteps; i++)
		{
			label.text = ((int)Math.Round(Mathf.Lerp(lastValue, coins, (float)i / numberOfSteps))).ToString();
			yield return Utils.GetDelay(0.05f);
		}
		lastValue = coins;
	}

	private void OnDestroy()
	{
		MessageKit<int>.removeObserver(23, UpdateLabel);
	}
}
