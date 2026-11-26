using System.Collections.Generic;
using Firebase.Messaging;

public static class FirebasePushNotifications
{
	private static bool isInitialized;

	private static bool messageListenerAttached;

	private static bool tokenListenerAttached;

	private static readonly IPushNotificationRewardStore rewardStore;

	private static readonly PushNotificationRewardHandler rewardHandler;

	public static string CurrentToken { get; private set; }

	internal static bool IsInitialized => false;

	public static void Initialize()
	{
	}

	internal static void EnsureMessagingForStatus(PushPermissionStatus status)
	{
	}

	private static void AttachMessagingListeners()
	{
	}

	private static void OnTokenReceived(object sender, TokenReceivedEventArgs eventArgs)
	{
	}

	private static void SaveToken(string token)
	{
	}

	private static void OnMessageReceived(object sender, MessageReceivedEventArgs eventArgs)
	{
	}

	private static PushNotificationRewardQueueResult HandleRewardMessage(bool notificationOpened, IDictionary<string, string> data)
	{
		return PushNotificationRewardQueueResult.NotOpened;
	}

	private static string DescribeDataKeys(IDictionary<string, string> data)
	{
		return null;
	}

	private static bool TryGrantReward(PushNotificationReward reward)
	{
		return false;
	}

	private static bool TryGrantOneGameAccess(StoreItems item)
	{
		return false;
	}

	private static bool TryGrantMultiplayerKeys(int amount)
	{
		return false;
	}

	private static string DescribeReward(PushNotificationReward reward)
	{
		return null;
	}
}
