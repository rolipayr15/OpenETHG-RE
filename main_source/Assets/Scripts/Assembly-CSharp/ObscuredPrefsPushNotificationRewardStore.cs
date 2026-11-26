internal sealed class ObscuredPrefsPushNotificationRewardStore : IPushNotificationRewardStore
{
	public bool HasClaim(string claimKey)
	{
		return false;
	}

	public void ReserveClaim(string claimKey)
	{
	}

	public void ReleaseClaim(string claimKey)
	{
	}

	public bool TryGetPendingReward(out PushNotificationReward reward)
	{
		reward = default;
		return false;
	}

	public void SetPendingReward(PushNotificationReward reward)
	{
	}

	public void ClearPendingReward()
	{
	}

	public bool Commit()
	{
		return false;
	}
}
