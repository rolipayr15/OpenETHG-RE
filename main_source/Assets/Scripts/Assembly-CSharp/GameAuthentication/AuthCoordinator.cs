using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Multiplayer.ExternalApi;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

namespace GameAuthentication
{
	// Compact replacement of the 8.3.0 coordinator: Unity Services session for the official backend,
	// Unity Player Accounts as the sign-in method. Facebook, Apple and Google Play SDKs are not in this build.
	public sealed class AuthCoordinator : IAuthCoordinator, IDisposable
	{
		private const string UnityIdentity = "unity";

		private readonly IUserApi userApi;

		private Task<AuthOperationResult> bootstrap;

		private TaskCompletionSource<bool> signInCompletion;

		public AuthState State { get; private set; } = AuthState.Initial;

		public event Action<AuthState> StateChanged;

		public AuthCoordinator(IUserApi userApi)
		{
			this.userApi = userApi;
		}

		private void SetState(GameAuthSessionStatus session, AuthOperation operation, GameAccountType account, IEnumerable<GameAuthProvider> providers, string error = null)
		{
			string player = UnityServices.State == ServicesInitializationState.Initialized ? AuthenticationService.Instance.PlayerId : string.Empty;
			State = new AuthState(session, operation, player ?? string.Empty, account, providers, null, error);
			StateChanged?.Invoke(State);
		}

		private void PublishSession(AuthOperation operation = AuthOperation.Idle)
		{
			IAuthenticationService auth = AuthenticationService.Instance;
			List<GameAuthProvider> providers = new List<GameAuthProvider>();
			List<Identity> identities = auth.PlayerInfo != null ? auth.PlayerInfo.Identities : null;
			if (identities != null)
			{
				foreach (Identity identity in identities)
				{
					switch (identity.TypeId)
					{
					case UnityIdentity:
						providers.Add(GameAuthProvider.UnityPlayerAccount);
						break;
					case "facebook.com":
						providers.Add(GameAuthProvider.Facebook);
						break;
					case "apple.com":
						providers.Add(GameAuthProvider.Apple);
						break;
					case "google-play-games":
					case "playgames.google.com":
						providers.Add(GameAuthProvider.GooglePlayGames);
						break;
					}
				}
			}
			SetState(auth.IsExpired ? GameAuthSessionStatus.Expired : GameAuthSessionStatus.Authorized, operation, providers.Count > 0 ? GameAccountType.Linked : GameAccountType.Guest, providers);
		}

		public Task<AuthOperationResult> BootstrapAsync()
		{
			if (OnlineConfig.IsCustomServer)
			{
				return Task.FromResult(new AuthOperationResult(AuthResultCode.Success));
			}
			if (bootstrap == null || bootstrap.IsFaulted || (bootstrap.IsCompleted && !bootstrap.Result.IsSuccess))
			{
				bootstrap = RunBootstrap();
			}
			else if (bootstrap.IsCompleted)
			{
				// Back on the official server: the session is alive, its data is refreshed.
				bootstrap = RunBootstrap();
			}
			return bootstrap;
		}

		private async Task<AuthOperationResult> RunBootstrap()
		{
			try
			{
				SetState(GameAuthSessionStatus.Restoring, AuthOperation.Bootstrapping, GameAccountType.None, Array.Empty<GameAuthProvider>());
				await UnityServices.InitializeAsync(new InitializationOptions().SetEnvironmentName("production"));
				IAuthenticationService auth = AuthenticationService.Instance;
				bool restored = auth.SessionTokenExists;
				if (!auth.IsSignedIn)
				{
					await auth.SignInAnonymouslyAsync();
				}
				await auth.GetPlayerInfoAsync();
				PublishSession();
				await SyncUser();
				return new AuthOperationResult(restored ? AuthResultCode.SessionRestored : AuthResultCode.GuestCreated, null, null, auth.PlayerId);
			}
			catch (Exception exception)
			{
				Debug.LogWarning("[Auth] bootstrap failed: " + exception.Message);
				SetState(GameAuthSessionStatus.SignedOut, AuthOperation.Idle, GameAccountType.None, Array.Empty<GameAuthProvider>(), exception.Message);
				return new AuthOperationResult(AuthResultCode.NetworkFailure, null, null, null, exception.Message);
			}
		}

		private async Task SyncUser()
		{
			if (State.AccountType != GameAccountType.Linked)
			{
				return;
			}
			try
			{
				// The account's name wins over whatever the guest session used.
				ExternalApi.Models.UsernameResponse? account = await userApi.GetUsernameAsync();
				if (account.HasValue && !string.IsNullOrEmpty(account.Value.name))
				{
					OnlineConfig.SavePlayerName(account.Value.name);
				}
				await userApi.SyncUserInfoAsync();
				Prime31.MessageKit.MessageKit.post(128);
				StateChanged?.Invoke(State);
			}
			catch (Exception exception)
			{
				Debug.LogWarning("[Auth] user sync failed: " + exception.Message);
			}
		}

		public async Task<AuthOperationResult> ContinueWithProviderAsync(GameAuthProvider provider, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (provider != GameAuthProvider.UnityPlayerAccount)
			{
				return new AuthOperationResult(AuthResultCode.ProviderUnavailable, provider);
			}
			if (signInCompletion != null && !signInCompletion.Task.IsCompleted)
			{
				// A second start would close the local listener the browser is about to return to.
				return new AuthOperationResult(AuthResultCode.InvalidState, provider, null, null, "Finish the sign-in in the browser window");
			}
			AuthOperationResult boot = await BootstrapAsync();
			if (!boot.IsSuccess)
			{
				return boot;
			}
			IPlayerAccountService accounts = PlayerAccountService.Instance;
			IAuthenticationService auth = AuthenticationService.Instance;
			try
			{
				signInCompletion = new TaskCompletionSource<bool>();
				Action signedIn = delegate
				{
					signInCompletion.TrySetResult(true);
				};
				Action<RequestFailedException> failed = delegate(RequestFailedException exception)
				{
					signInCompletion.TrySetException(exception);
				};
				accounts.SignedIn += signedIn;
				accounts.SignInFailed += failed;
				try
				{
					SetState(GameAuthSessionStatus.Authorized, AuthOperation.AcquiringCredential, State.AccountType, State.LinkedProviders);
					using (cancellationToken.Register(delegate
					{
						signInCompletion.TrySetCanceled();
					}))
					{
						await accounts.StartSignInAsync();
						await signInCompletion.Task;
					}
				}
				finally
				{
					accounts.SignedIn -= signedIn;
					accounts.SignInFailed -= failed;
				}
				string token = accounts.AccessToken;
				AuthResultCode code = AuthResultCode.ConnectedToCurrentAccount;
				try
				{
					await auth.LinkWithUnityAsync(token);
				}
				catch (AuthenticationException exception) when (exception.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
				{
					auth.SignOut(true);
					await auth.SignInWithUnityAsync(token);
					code = AuthResultCode.SwitchedAccount;
				}
				await auth.GetPlayerInfoAsync();
				PublishSession();
				await SyncUser();
				return new AuthOperationResult(code, provider, null, auth.PlayerId);
			}
			catch (OperationCanceledException)
			{
				PublishSession();
				return new AuthOperationResult(AuthResultCode.UserCancelled, provider);
			}
			catch (Exception exception)
			{
				Debug.LogWarning("[Auth] sign-in failed: " + exception.Message);
				PublishSession();
				return new AuthOperationResult(AuthResultCode.UnknownFailure, provider, null, null, exception.Message);
			}
		}

		public Task<AuthOperationResult> CreateLinkedGameAccountAsync()
		{
			return Task.FromResult(new AuthOperationResult(AuthResultCode.InvalidState));
		}

		public Task<AuthOperationResult> ConfirmSwitchGameAccountAsync()
		{
			return Task.FromResult(new AuthOperationResult(AuthResultCode.InvalidState));
		}

		public void RemainOnCurrentAccount()
		{
		}

		public void CancelPendingAction()
		{
			signInCompletion?.TrySetCanceled();
		}

		public void CancelProviderCredentialAcquisition()
		{
			signInCompletion?.TrySetCanceled();
		}

		public async Task<AuthOperationResult> DisconnectSignInMethodAsync(GameAuthProvider provider)
		{
			if (provider != GameAuthProvider.UnityPlayerAccount)
			{
				return new AuthOperationResult(AuthResultCode.ProviderUnavailable, provider);
			}
			try
			{
				await AuthenticationService.Instance.UnlinkUnityAsync();
				await AuthenticationService.Instance.GetPlayerInfoAsync();
				PublishSession();
				return new AuthOperationResult(AuthResultCode.Success, provider);
			}
			catch (Exception exception)
			{
				return new AuthOperationResult(AuthResultCode.UnknownFailure, provider, null, null, exception.Message);
			}
		}

		public Task<AuthOperationResult> SignOutAsync()
		{
			if (UnityServices.State == ServicesInitializationState.Initialized)
			{
				AuthenticationService.Instance.SignOut(true);
				PlayerAccountService.Instance.SignOut();
			}
			bootstrap = null;
			SetState(GameAuthSessionStatus.SignedOut, AuthOperation.Idle, GameAccountType.None, Array.Empty<GameAuthProvider>());
			return Task.FromResult(new AuthOperationResult(AuthResultCode.Success));
		}

		public async Task<AuthOperationResult> DeleteGameAccountAsync()
		{
			try
			{
				await AuthenticationService.Instance.DeleteAccountAsync();
				await SignOutAsync();
				return new AuthOperationResult(AuthResultCode.Success);
			}
			catch (Exception exception)
			{
				return new AuthOperationResult(AuthResultCode.DeletionFailed, null, null, null, exception.Message);
			}
		}

		public void Tick()
		{
		}

		public void Dispose()
		{
		}
	}
}
