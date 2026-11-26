namespace GameAuthentication
{
	public enum GameAuthSessionStatus
	{
		Uninitialized = 0,
		Restoring = 1,
		Authorized = 2,
		Expired = 3,
		SignedOut = 4
	}
}
