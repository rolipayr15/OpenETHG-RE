using System;
using System.Threading;
using System.Threading.Tasks;

namespace GameAuthentication
{
	public interface IAuthCoordinator
	{
		AuthState State { get; }

		event Action<AuthState> StateChanged;

		Task<AuthOperationResult> BootstrapAsync();

		Task<AuthOperationResult> ContinueWithProviderAsync(GameAuthProvider provider, CancellationToken cancellationToken = default(CancellationToken));

		Task<AuthOperationResult> CreateLinkedGameAccountAsync();

		Task<AuthOperationResult> ConfirmSwitchGameAccountAsync();

		void RemainOnCurrentAccount();

		void CancelPendingAction();

		void CancelProviderCredentialAcquisition();

		Task<AuthOperationResult> DisconnectSignInMethodAsync(GameAuthProvider provider);

		Task<AuthOperationResult> SignOutAsync();

		Task<AuthOperationResult> DeleteGameAccountAsync();

		void Tick();
	}
}
