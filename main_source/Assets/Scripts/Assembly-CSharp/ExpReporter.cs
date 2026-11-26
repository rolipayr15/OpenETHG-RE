using System;
using EyesAnalytics;
using ExternalApi.Models;
using Multiplayer.ExternalApi;
using UnityEngine;

public sealed class ExpReporter
{
	private static ExpReporter instance;

	private readonly IExperienceManager experienceManager;

	private readonly IExpApi expApi;

	private ExpReporter(IExperienceManager experienceManager, IExpApi expApi)
	{
		if (experienceManager == null)
		{
			throw new ArgumentNullException("experienceManager");
		}
		if (expApi == null)
		{
			throw new ArgumentNullException("expApi");
		}
		this.experienceManager = experienceManager;
		this.expApi = expApi;
		experienceManager.OnExpChanged += OnExpChanged;
		experienceManager.OnLevelUp += OnLevelUp;
	}

	public static void EnsureInitialized(IExperienceManager experienceManager, IExpApi expApi)
	{
		if (instance == null)
		{
			instance = new ExpReporter(experienceManager, expApi);
		}
	}

	private void OnExpChanged(GrantExpData expContext, bool isOverwrite)
	{
		if (!isOverwrite)
		{
			Debug.Log("EXP: Exp gained: " + expContext.exp);
			Debug.Log("EXP: Sending exp to server for game id: " + (UserGameSettings.IsMultiplayer() ? expContext.gameMultiId : expContext.gameSingleId));
			expApi.SendGrantExpAsync(expContext);
		}
	}

	private void OnLevelUp(int previousLevel, int newLevel)
	{
		int reward = UserGameSettings.levelUpReward;
		KeysManager.AddMultiplayerKeys(reward);
		Currency.SendCurrencyEvent((CurrencyFlowType)1, (CurrencyType)1, reward, (CurrencyItemType)7, (CurrencyItemId)6);
		UserGameSettings.hasPlayerLeveledUp = true;
	}
}
