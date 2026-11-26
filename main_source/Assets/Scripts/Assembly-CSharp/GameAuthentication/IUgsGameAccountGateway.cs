using System;
using System.Threading.Tasks;

namespace GameAuthentication
{
	public interface IUgsGameAccountGateway
	{
		bool IsInitialized { get; }

		bool IsAuthorized { get; }

		bool IsSignedIn { get; }

		bool IsExpired { get; }

		bool SessionTokenExists { get; }

		string PlayerId { get; }

		event Action SessionExpired;

		Task InitializeAsync();

		Task RestoreSessionAsync();

		Task CreateGuestAsync();

		Task<GameAccountInfo> GetCurrentAccountInfoAsync();

		Task LinkProviderAsync(ProviderCredential credential);

		Task SignInWithProviderAsync(ProviderCredential credential, bool createAccount);

		Task UnlinkProviderAsync(GameAuthProvider provider);

		void SignOut(bool clearCredentials);

		Task DeleteGameAccountAsync();
	}
}
