using System;
using System.Collections.Generic;

public sealed class PushNotificationRewardHandler
{
	public const string RewardTypeDataKey = "reward_type";

	public const string RewardAmountDataKey = "reward_amount";

	public const string RewardIdDataKey = "reward_id";

	public const string Chapter2RewardType = "chapter_2";

	public const string Chapter3RewardType = "chapter_3";

	public const string MultiplayerKeysRewardType = "multiplayer_keys";

	public const string CampaignMultiplayerKeysRewardType = "campaign_multiplayer_keys";

	public const int MaximumMultiplayerKeysReward = 100000;

	private const int ClaimIdHashBytes = 16;

	private readonly IPushNotificationRewardStore store;

	private readonly Func<PushNotificationReward, bool> grantReward;

	private readonly Func<PushNotificationRewardType, WinbackRewardAuthorization> getAuthorization;

	private readonly Action<PushNotificationRewardType> rewardClaimed;

	public PushNotificationRewardHandler(IPushNotificationRewardStore store, Func<PushNotificationReward, bool> grantReward, Func<PushNotificationRewardType, WinbackRewardAuthorization> getAuthorization = null, Action<PushNotificationRewardType> rewardClaimed = null)
	{
	}

	public PushNotificationRewardQueueResult TryStageOpenedNotification(bool notificationOpened, IDictionary<string, string> data, out PushNotificationReward reward)
	{
		reward = default;
		return PushNotificationRewardQueueResult.NotOpened;
	}

	public bool TryGetPendingReward(out PushNotificationReward reward)
	{
		reward = default;
		return false;
	}

	public bool IsPendingReward(PushNotificationReward expectedReward)
	{
		return false;
	}

	public PushNotificationPendingPresentation TryPreparePresentableReward(out PushNotificationReward reward)
	{
		reward = default;
		return PushNotificationPendingPresentation.None;
	}

	public PushNotificationRewardClaimResult TryClaimPending(out PushNotificationReward reward)
	{
		reward = default;
		return PushNotificationRewardClaimResult.NoPendingReward;
	}

	public PushNotificationRewardClaimResult TryClaimPending(PushNotificationReward expectedReward, out PushNotificationReward reward)
	{
		reward = default;
		return PushNotificationRewardClaimResult.NoPendingReward;
	}

	private PushNotificationRewardClaimResult TryClaimPendingInternal(bool hasExpectedReward, PushNotificationReward expectedReward, out PushNotificationReward reward)
	{
		reward = default;
		return PushNotificationRewardClaimResult.NoPendingReward;
	}

	internal static string GetRewardTypeDataValue(PushNotificationRewardType rewardType)
	{
		return null;
	}

	internal static bool TryParseRewardTypeDataValue(string value, out PushNotificationRewardType rewardType)
	{
		rewardType = default;
		return false;
	}

	private static PushNotificationRewardQueueResult TryParseOpenedReward(bool notificationOpened, IDictionary<string, string> data, out PushNotificationReward reward)
	{
		reward = default;
		return PushNotificationRewardQueueResult.NotOpened;
	}

	private void ClearInvalidPendingReward()
	{
	}

	private void RestorePendingAfterWriteFailure(bool shouldRestore, PushNotificationReward reward)
	{
	}

	private WinbackRewardAuthorization EvaluateRewardAuthorization(PushNotificationRewardType rewardType)
	{
		return WinbackRewardAuthorization.PendingValidation;
	}

	private PushNotificationPendingRejectionResult RejectUnauthorizedPending(PushNotificationReward reward)
	{
		return PushNotificationPendingRejectionResult.Rejected;
	}

	private static bool IsValidReward(PushNotificationReward reward)
	{
		return false;
	}

	private static bool AreSameReward(PushNotificationReward left, PushNotificationReward right)
	{
		return false;
	}

	internal static bool IsMultiplayerKeysRewardType(PushNotificationRewardType rewardType)
	{
		return false;
	}

	internal static bool IsCampaignRewardType(PushNotificationRewardType rewardType)
	{
		return false;
	}

	private static bool TryGetValue(IDictionary<string, string> data, string key, out string value)
	{
		value = null;
		return false;
	}

	private static string BuildClaimKey(string rewardId)
	{
		return null;
	}
}
