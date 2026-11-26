using System;

public interface IAuthListener
{
	string CurrentPlayerId { get; }

	bool IsPlayerAuthenticated { get; }

	bool ExternalProviderLoggedIn { get; }

	event Action<string> OnPlayerSignedIn;

	event Action<string> OnPlayerSignedOut;

	event Action<string> OnPlayerSessionExpired;
}
