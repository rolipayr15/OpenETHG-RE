public readonly struct PushNotificationReward
{
	public PushNotificationRewardType Type { get; }

	public string Id { get; }

	public int Amount { get; }

	public PushNotificationReward(PushNotificationRewardType type, string id, int amount = 0)
	{
		Type = PushNotificationRewardType.Invalid;
		Id = null;
		Amount = 0;
	}
}
