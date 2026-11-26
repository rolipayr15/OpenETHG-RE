namespace GameAuthentication
{
	public enum AuthOperation
	{
		Idle = 0,
		Bootstrapping = 1,
		CreatingGuest = 2,
		AcquiringCredential = 3,
		SigningInWithProvider = 4,
		CreatingLinkedAccount = 5,
		ConnectingProvider = 6,
		AwaitingSwitchDecision = 7,
		AwaitingAccountCreationDecision = 8,
		SwitchingAccount = 9,
		DisconnectingProvider = 10,
		SigningOut = 11,
		DeletingAccount = 12
	}
}
