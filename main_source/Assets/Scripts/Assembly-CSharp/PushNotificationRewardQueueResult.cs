public enum PushNotificationRewardQueueResult
{
	NotOpened = 0,
	UnrelatedNotification = 1,
	InvalidRewardAmount = 2,
	MissingRewardId = 3,
	UnexpectedRewardStage = 4,
	AlreadyClaimed = 5,
	AlreadyPending = 6,
	AnotherRewardPending = 7,
	PendingWriteFailed = 8,
	QueuedNotPersisted = 9,
	Queued = 10
}
