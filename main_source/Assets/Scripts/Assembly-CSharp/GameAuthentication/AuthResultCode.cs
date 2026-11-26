namespace GameAuthentication
{
	public enum AuthResultCode
	{
		Success = 0,
		GuestCreated = 1,
		SessionRestored = 2,
		ConnectedToCurrentAccount = 3,
		ExistingAccountFound = 4,
		SwitchedAccount = 5,
		AlreadyConnected = 6,
		ProviderSlotOccupied = 7,
		NoExistingGameAccount = 8,
		LastSignInMethod = 9,
		UserCancelled = 10,
		SessionExpired = 11,
		ProviderUnavailable = 12,
		NetworkFailure = 13,
		DeletionFailed = 14,
		InvalidState = 15,
		UnknownFailure = 16
	}
}
