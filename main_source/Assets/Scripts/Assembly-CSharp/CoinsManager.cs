using System.Reflection;
using CodeStage.AntiCheat.Storage;
using EyesAnalytics;
using Prime31.MessageKit;
using UnityEngine;

[Obfuscation(Exclude = false)]
public class CoinsManager : CustomSingleton<CoinsManager>
{
	private const int REVIVE_PRICE = 30;

	private const int MORE_EYES_PRICE = 20;

	private const int STARTING_BALANCE_NBO = 50;

	private const int STARTING_BALANCE = 0;

	private const int RUN_BOOST_PRICE = 20;

	public void AddCoinsForRewardedAd()
	{
		AddCoins(UserGameSettings.freeCoinsReward, CurrencyItemType.AdReward, CurrencyItemId.AdReward_FreeCoins);
	}

	public static void AddCoinsForDailyChallenge()
	{
		AddCoins(75, CurrencyItemType.GameplayReward, CurrencyItemId.DailyChallengeReward);
	}

	public void AddCoinsForGDPR()
	{
	}

	public void SetCoinsFromSnapshot(int amount)
	{
		ReplaceCoins(amount, true);
	}

	public void BuyMoreEyesWithCoins()
	{
		if (TryDeductCoins(MORE_EYES_PRICE, CurrencyItemType.InGamePurchase, CurrencyItemId.InGamePurchase_MoreEyes))
		{
			MoreEyesManager.Instance.GetMoreEyes();
		}
	}

	public bool BuyRunBoostWithCoins()
	{
		return TryDeductCoins(RUN_BOOST_PRICE, CurrencyItemType.InGamePurchase, CurrencyItemId.InGamePurchase_Sprint);
	}

	public bool BuyIfCanReviveWithCoins()
	{
		return TryDeductCoins(REVIVE_PRICE, CurrencyItemType.InGamePurchase, CurrencyItemId.InGamePurchase_Revive);
	}

	public bool CanReviveWithCoins()
	{
		return GetCoins() >= REVIVE_PRICE;
	}

	public bool CanGetMoreEyesWithCoins()
	{
		return GetCoins() >= MORE_EYES_PRICE;
	}

	public bool CanBuyImmersiveModeWithCoins()
	{
		int price = UserGameSettings.UnlockPrices_Immersive;
		return price >= 0 && price <= GetCoins();
	}

	private static int getStartingBalance()
	{
		return UserGameSettings.googleNBO ? STARTING_BALANCE_NBO : STARTING_BALANCE;
	}

	public static int GetCoins()
	{
		return ObscuredPrefs.GetInt("COINS", getStartingBalance());
	}

	public static void OverwriteCoins(int newAmount)
	{
		ReplaceCoins(newAmount, false);
	}

	public static void AddCoins(int howMany)
	{
		AddCoins(howMany, CurrencyItemType.Giveaway, CurrencyItemId.Other);
	}

	public static void AddCoins(int howMany, CurrencyItemType itemType, CurrencyItemId itemId)
	{
		if (howMany <= 0)
		{
			return;
		}
		int before = GetCoins();
		long after = (long)before + howMany;
		if (after > int.MaxValue || !WriteCoins((int)after))
		{
			return;
		}
		Currency.SendCoinTransaction(before, (int)after, itemType, itemId);
		MessageKit<int>.post(23, (int)after);
	}

	public static bool CanDeductCoins(int howMany)
	{
		return howMany >= 0 && howMany <= GetCoins();
	}

	public static bool TryDeductCoins(int howMany)
	{
		return TryDeductCoins(howMany, CurrencyItemType.InGamePurchase, CurrencyItemId.Other);
	}

	public static bool TryDeductCoins(int howMany, CurrencyItemType itemType, CurrencyItemId itemId)
	{
		int before = GetCoins();
		if (howMany < 0 || howMany > before)
		{
			return false;
		}
		if (howMany != 0)
		{
			int after = before - howMany;
			if (!WriteCoins(after))
			{
				return false;
			}
			Currency.SendCoinTransaction(before, after, itemType, itemId);
			MessageKit<int>.post(23, after);
		}
		return true;
	}

	private static bool WriteCoins(int newAmount)
	{
		ObscuredPrefs.SetInt("COINS", newAmount);
		bool written = GetCoins() == newAmount;
		if (!written)
		{
			Debug.LogWarning("[Coins] Balance write failed.");
		}
		return written;
	}

	private static void ReplaceCoins(int newAmount, bool fromSnapshot)
	{
		if (newAmount < 0)
		{
			return;
		}
		int before = GetCoins();
		if (WriteCoins(newAmount))
		{
			Currency.ReportCoinBalanceAdjustment(before, newAmount, fromSnapshot);
			MessageKit<int>.post(23, newAmount);
		}
	}

	public static void ReportBalanceSnapshot()
	{
		Currency.ReportCoinBalanceSnapshot(GetCoins(), ObscuredPrefs.HasKey("COINS"), UserGameSettings.googleNBO);
	}

	public int GetMoreCoinsPriceWithCoins()
	{
		return MORE_EYES_PRICE;
	}

	public int GetRevivePriceWithCoins()
	{
		return REVIVE_PRICE;
	}

	public int GetRunBoostPriceWithCoins()
	{
		return RUN_BOOST_PRICE;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}
