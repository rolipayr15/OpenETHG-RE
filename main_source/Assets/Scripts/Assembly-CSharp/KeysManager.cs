using Unity.Services.Core;
using Prime31.MessageKit;
using CodeStage.AntiCheat.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class KeysManager : MonoBehaviour
{

	[SerializeField]
	private Text errorLabel;

	private static int getStartingBalance()
	{
		return UserGameSettings.freeTickets;
	}

	public static int GetMultiplayerKeys()
	{
		return ObscuredPrefs.GetInt("TICKETS", UserGameSettings.freeTickets);
	}

	public static void OverwriteMultiplayerKeys(int newAmount)
	{
		ObscuredPrefs.SetInt("TICKETS", newAmount);
		MessageKit.post(131);
	}

	public static void AddMultiplayerKeys(int howMany)
	{
		OverwriteMultiplayerKeys(GetMultiplayerKeys() + howMany);
	}

	public static bool CanDeductMultiplayerKeys(int howMany)
	{
		return howMany <= GetMultiplayerKeys();
	}

	public static bool TryDeductMultiplayerKeys(int howMany)
	{
		if (howMany > GetMultiplayerKeys())
		{
			return false;
		}
		OverwriteMultiplayerKeys(GetMultiplayerKeys() - howMany);
		return true;
	}

	public void BuySub(StoreItems storeItem)
	{
		if (UnityServices.State == ServicesInitializationState.Initialized)
		{
			Purchaser purchaser = CustomSingleton<Purchaser>.Instance;
			string productId = purchaser.GetInAppName(storeItem);
			purchaser.BuyProductID(productId, new EyesAnalytics.IAP.PurchaseContext(productId, (EyesAnalytics.CartType)2, false));
		}
		else
		{
			errorLabel.text = "Unity Services not initialized";
		}
	}

	private IEnumerator TryInitializePurchasing()
	{
		while (UnityServices.State != ServicesInitializationState.Initialized)
		{
			yield return Bibloteka.Utils.GetDelay(1f);
		}
		errorLabel.text = "Unity Services initialized";
	}
}
