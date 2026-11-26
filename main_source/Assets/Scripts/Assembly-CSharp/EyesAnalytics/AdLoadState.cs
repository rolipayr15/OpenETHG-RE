namespace EyesAnalytics
{
	public enum AdLoadState
	{
		NotInitialized = 0,
		NoConsent = 1,
		Loading = 2,
		Backoff = 3,
		Failed = 4,
		Idle = 5
	}
}
