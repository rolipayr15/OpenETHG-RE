public interface IPushNotificationRewardStore
{
	bool HasClaim(string claimKey);

	void ReserveClaim(string claimKey);

	void ReleaseClaim(string claimKey);

	bool TryGetPendingReward(out PushNotificationReward reward);

	void SetPendingReward(PushNotificationReward reward);

	void ClearPendingReward();

	bool Commit();
}
