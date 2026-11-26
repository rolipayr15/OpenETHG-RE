internal readonly struct PushNotificationRewardAnalyticsData
{
	public string RewardType { get; }

	public string RewardId { get; }

	public long? RewardAmount { get; }

	public PushNotificationRewardAnalyticsData(string rewardType, string rewardId, long? rewardAmount)
	{
		RewardType = null;
		RewardId = null;
		RewardAmount = null;
	}
}
