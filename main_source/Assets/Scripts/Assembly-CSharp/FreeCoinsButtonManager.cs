using Bibloteka;
using EyesAnalytics;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class FreeCoinsButtonManager : CustomSingleton<FreeCoinsButtonManager>
{
	[SerializeField]
	private Text rewardInCoins;

	private bool isWaitingForRewardedAd;

	private Coroutine rewardedAdWaitCoroutine;

	private void Start()
	{
		if (UserGameSettings.googleNBO)
		{
			gameObject.SetActive(false);
		}
		if (rewardInCoins != null)
		{
			rewardInCoins.text = "+" + UserGameSettings.freeCoinsReward;
		}
	}

	public void ShowFreeCoinsRewardedAd()
	{
		if (isWaitingForRewardedAd)
		{
			return;
		}
		isWaitingForRewardedAd = true;
		rewardedAdWaitCoroutine = AdsManager.Instance.WhenRewardedAdAvailable(this, RewardedAdPlacement.FREE_COINS, () =>
		{
			FinishRewardedAdWait();
			AdsManager.Instance.ShowRewardedAd(RewardedAdPlacement.FREE_COINS, rewarded =>
			{
				if (rewarded)
				{
					CoinsManager.Instance.AddCoinsForRewardedAd();
				}
			});
			Breadcrumbs.Log("menu_button_free_coins");
		}, () =>
		{
			FinishRewardedAdWait();
			Utils.ShowToast(ScriptLocalization.AdNotAvailable);
		});
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

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}
