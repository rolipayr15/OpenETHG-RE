using System;
using EyesAnalytics;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class WinCoinsSummary : MonoBehaviour
{
	private int award;

	private int summaryReward;

	[SerializeField]
	private GameObject doubleYourScore;

	[SerializeField]
	private Text rewardInCoins;

	[SerializeField]
	private GameObject coinsBox;

	private bool isWaitingForRewardedAd;

	private Coroutine rewardedAdWaitCoroutine;

	public bool IsResultReady { get; private set; }

	public bool IsWaitingForRewardedAd => isWaitingForRewardedAd;

	private void Start()
	{
		string scene = SceneManager.GetActiveScene().name;
		if (scene == "Win" || scene == "Over")
		{
			PrintResult();
		}
	}

	public void PrintResult()
	{
		award = UserGameSettings.bagsCollected;
		if ((int)UserGameSettings.currentMode == 2)
		{
			award = UserGameSettings.halloweenMultiplier * award;
			award = (int)((float)UserGameSettings.gameplayRewardsTrickOrTreat / 100f * (float)award);
		}
		summaryReward = ((int)UserGameSettings.currentEnemy == 4 || (int)UserGameSettings.currentMode == 1) ? award / 2 : award;
		rewardInCoins.text = summaryReward.ToString();
		if (summaryReward < 1)
		{
			if (doubleYourScore != null)
			{
				doubleYourScore.SetActive(false);
				if (!UserGameSettings.IsMultiplayer())
				{
					RectTransform rect = coinsBox.GetComponent<RectTransform>();
					rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -60f);
				}
			}
		}
		else
		{
			CurrencyItemId item = UserGameSettings.IsMultiplayer() ? (CurrencyItemId)17 : Currency.ItemIdFromGameMode(UserGameSettings.currentMode);
			CoinsManager.AddCoins(summaryReward, (CurrencyItemType)2, item);
			if (doubleYourScore != null)
			{
				doubleYourScore.SetActive(true);
			}
		}
		IsResultReady = true;
	}

	public int IsTheFirstWinOfTheDay()
	{
		if (!UserGameSettings.lastWinDay.HasValue)
		{
			UserGameSettings.lastWinDay = PlayerPrefs.GetInt("LAST_WIN_DAY", 0);
		}
		if (UserGameSettings.lastWinDay.GetValueOrDefault() == DateTime.Today.DayOfYear && UserGameSettings.lastWinDay.HasValue)
		{
			return 1;
		}
		UserGameSettings.lastWinDay = DateTime.Today.DayOfYear;
		PlayerPrefs.SetInt("LAST_WIN_DAY", UserGameSettings.lastWinDay.Value);
		return 2;
	}

	public void WatchAVideo()
	{
		if (isWaitingForRewardedAd)
		{
			return;
		}
		isWaitingForRewardedAd = true;
		rewardedAdWaitCoroutine = CustomSingleton<AdsManager>.Instance.WhenRewardedAdAvailable(this, RewardedAdPlacement.DOUBLE_SCORE, delegate
		{
			FinishRewardedAdWait();
			CustomSingleton<AdsManager>.Instance.ShowRewardedAd(RewardedAdPlacement.DOUBLE_SCORE, delegate(bool success)
			{
				if (success)
				{
					doubleYourScore.SetActive(false);
					GiveRewardAfterWatchingTheAd();
				}
			});
		}, FinishRewardedAdWait);
	}

	private void FinishRewardedAdWait()
	{
		rewardedAdWaitCoroutine = null;
		isWaitingForRewardedAd = false;
	}

	private void CancelRewardedAdWait()
	{
		if (rewardedAdWaitCoroutine != null)
		{
			StopCoroutine(rewardedAdWaitCoroutine);
		}
		rewardedAdWaitCoroutine = null;
		isWaitingForRewardedAd = false;
	}

	private void OnDisable()
	{
		CancelRewardedAdWait();
	}

	private void GiveRewardAfterWatchingTheAd()
	{
		CoinsManager.AddCoins(summaryReward, (CurrencyItemType)1, (CurrencyItemId)12);
		rewardInCoins.text = (summaryReward * 2).ToString();
	}
}
