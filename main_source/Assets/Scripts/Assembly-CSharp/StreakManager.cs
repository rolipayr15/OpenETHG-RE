using System;
using System.Collections;
using System.Globalization;
using Bibloteka;
using EyesAnalytics;
using UnityEngine;
using UnityEngine.UI;

public class StreakManager : CustomSingleton<StreakManager>
{
	[SerializeField]
	private Text streakCounterLabel;

	[SerializeField]
	private Text streakCounterLabelInMenu;

	[SerializeField]
	private GameObject streakWindow;

	[SerializeField]
	private Image streakMenuIconImage;

	[SerializeField]
	private Text rewardLabel;

	[SerializeField]
	private GameObject labelSuccess;

	[SerializeField]
	private GameObject labelFail;

	private DateTime lastTimePlayData;

	private string lastTimePlay;

	private int streakCounter;

	private int lastLaunchDay;

	[SerializeField]
	private GameObject doubleDailyRewardButton;

	[SerializeField]
	private Text labelDaily;

	[SerializeField]
	private Text labelWeekly;

	private int reward;

	private int lastValue;

	private Coroutine iconFlashingCoroutine;

	private void Start()
	{
		UserGameSettings.playingForTheFirstTime = false;
	}

	public void OpenWindow()
	{
		streakCounterLabel.text = streakCounter.ToString();
		if (InternetConnectionMonitor.Instance.IsInternetConnectionAvailable())
		{
			RewardPlayer();
			labelFail.SetActive(false);
		}
		else
		{
			labelFail.SetActive(true);
			labelSuccess.SetActive(false);
		}
	}

	private void RewardPlayer()
	{
		labelSuccess.SetActive(false);
		doubleDailyRewardButton.SetActive(!UserGameSettings.googleNBO);
		if (lastTimePlay == "")
		{
			reward = UserGameSettings.streakDailyReward;
			streakCounter = 1;
			GrantReward(CurrencyItemId.Streak_Daily);
		}
		else if (lastTimePlayData == DateTime.Today.AddDays(-1.0))
		{
			// Came back the next day: the streak grows, every seventh day pays the weekly reward.
			streakCounter++;
			if (streakCounter % 7 == 0)
			{
				reward = UserGameSettings.streakWeeklyReward;
				GrantReward(CurrencyItemId.Streak_Weekly);
			}
			else
			{
				reward = UserGameSettings.streakDailyReward;
				GrantReward(CurrencyItemId.Streak_Daily);
			}
		}
		else if (lastTimePlayData != DateTime.Today)
		{
			streakCounter = 1;
			reward = UserGameSettings.streakDailyReward;
			GrantReward(CurrencyItemId.Streak_Daily);
		}
		else
		{
			// Already rewarded today: only show what was granted.
			reward = UserGameSettings.streakDailyReward;
			labelSuccess.SetActive(true);
			doubleDailyRewardButton.SetActive(false);
			rewardLabel.text = reward.ToString();
		}
		lastTimePlayData = DateTime.Today;
		streakCounterLabel.text = streakCounter.ToString();
		streakCounterLabelInMenu.text = streakCounter.ToString();
		lastTimePlay = lastTimePlayData.ToString(CultureInfo.InvariantCulture);
		PlayerPrefs.SetString("LAST_TIME_PLAY", lastTimePlay);
		PlayerPrefs.SetInt("STREAK", streakCounter);
		if (iconFlashingCoroutine != null)
		{
			StopCoroutine(iconFlashingCoroutine);
			streakCounterLabelInMenu.enabled = true;
		}
	}

	private void GrantReward(CurrencyItemId itemId)
	{
		CoinsManager.AddCoins(reward, CurrencyItemType.StreakReward, itemId);
		StartCoroutine(CoinsCounter(reward));
	}

	public void DoubleDailyStreak()
	{
		AdsManager.Instance.ShowRewardedAd(RewardedAdPlacement.DOUBLE_DAILY_STREAK, rewarded =>
		{
			if (rewarded)
			{
				CoinsManager.AddCoins(reward, CurrencyItemType.AdReward, CurrencyItemId.AdReward_DoubleStreakCoins);
				StartCoroutine(CoinsCounter(reward * 2));
				doubleDailyRewardButton.SetActive(false);
			}
		});
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
			rewardLabel.text = ((int)Math.Round(Mathf.Lerp(lastValue, coins, (float)i / numberOfSteps))).ToString();
			yield return Utils.GetDelay(0.05f);
		}
		lastValue = coins;
	}

	private IEnumerator IconFlashing()
	{
		while (true)
		{
			yield return Utils.GetDelay(1f);
			streakCounterLabelInMenu.enabled = !streakCounterLabelInMenu.enabled;
		}
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
