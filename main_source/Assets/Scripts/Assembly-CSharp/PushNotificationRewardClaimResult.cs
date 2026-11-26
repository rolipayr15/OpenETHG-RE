public enum PushNotificationRewardClaimResult
{
	NoPendingReward = 0,
	InvalidPendingReward = 1,
	AlreadyClaimed = 2,
	AuthorizationPending = 3,
	NotEligible = 4,
	NotEligibleNotPersisted = 5,
	PendingResolutionFailed = 6,
	ClaimReservationFailed = 7,
	GrantFailed = 8,
	GrantedNotPersisted = 9,
	Granted = 10,
	PendingRewardChanged = 11
}
