using Prime31.MessageKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MoneyBagsVisionEffect : PotionEffect
{


	[SerializeField]
	private GameObject moneyBagsCamera;

	private void Awake()
	{
		if (moneyBagsCamera.GetComponent<PixelModeCamera>() == null)
		{
			moneyBagsCamera.AddComponent<PixelModeCamera>();
		}
	}

	public override IEnumerator StartEffect()
	{
		moneyBagsCamera.SetActive(true);
		MessageKit<bool>.post(10, true);
		EyesAnalytics.Breadcrumbs.Log("potion_money_bags_vision");
		yield return null;
	}

	public override IEnumerator EndEffect()
	{
		moneyBagsCamera.SetActive(false);
		MessageKit<bool>.post(10, false);
		yield return null;
	}

	public GameObject GetEffectCamera()
	{
		return moneyBagsCamera;
	}
}
