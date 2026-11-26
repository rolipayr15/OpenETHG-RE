using System;

public static class FirebaseWinbackRewardSequence
{
	internal const string PropertyName = "winback_next_reward";

	internal const string Chapter2Value = "chapter_2";

	internal const string Chapter3Value = "chapter_3";

	internal const string MultiplayerKeysValue = "multiplayer_keys";

	internal const string DoneValue = "done";

	private static bool playerStateHydrated;

	public static void MarkPlayerStateHydrated()
	{
	}

	public static void Refresh()
	{
	}

	public static void Sync()
	{
	}

	public static WinbackRewardAuthorization GetRewardAuthorization(PushNotificationRewardType rewardType)
	{
		return WinbackRewardAuthorization.PendingValidation;
	}

	public static bool CanStageReward(PushNotificationRewardType rewardType)
	{
		return false;
	}

	public static void AdvanceAfterClaim(PushNotificationRewardType claimedRewardType)
	{
	}

	internal static WinbackNextReward ResolveInitial(WinbackRewardAvailability availability)
	{
		return WinbackNextReward.Invalid;
	}

	internal static WinbackRewardAuthorization EvaluateAuthorization(bool isPlayerStateHydrated, bool hasReadableSequence, WinbackNextReward storedSequence, WinbackRewardAvailability availability, PushNotificationRewardType rewardType)
	{
		return WinbackRewardAuthorization.PendingValidation;
	}

	internal static WinbackNextReward Normalize(WinbackNextReward current, WinbackRewardAvailability availability)
	{
		return WinbackNextReward.Invalid;
	}

	internal static WinbackNextReward AdvanceAfterClaim(WinbackNextReward current, PushNotificationRewardType claimedRewardType, WinbackRewardAvailability availability)
	{
		return WinbackNextReward.Invalid;
	}

	internal static void Publish(WinbackNextReward nextReward, Action<string, string> setUserProperty)
	{
	}

	private static WinbackNextReward GetNormalizedNextReward()
	{
		return WinbackNextReward.Invalid;
	}

	private static WinbackRewardAvailability GetCurrentAvailability()
	{
		return default;
	}

	private static bool TryReadNextReward(out WinbackNextReward nextReward)
	{
		nextReward = default;
		return false;
	}

	private static void PersistIfChanged(WinbackNextReward nextReward)
	{
	}

	private static void PublishIfFirebaseReady(WinbackNextReward nextReward)
	{
	}

	private static PushNotificationRewardType ToRewardType(WinbackNextReward nextReward)
	{
		return PushNotificationRewardType.Invalid;
	}

	private static string GetDataValue(WinbackNextReward nextReward)
	{
		return null;
	}

	internal static bool TryParseDataValue(string value, out WinbackNextReward nextReward)
	{
		nextReward = default;
		return false;
	}
}
