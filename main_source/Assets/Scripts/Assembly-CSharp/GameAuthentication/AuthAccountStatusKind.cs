namespace GameAuthentication
{
	public enum AuthAccountStatusKind
	{
		SignedOut = 0,
		Guest = 1,
		LinkedKnownProvider = 2,
		LinkedUnknownProvider = 3,
		SessionExpired = 4
	}
}
