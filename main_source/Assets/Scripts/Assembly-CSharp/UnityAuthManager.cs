using System;
using System.Threading;
using System.Threading.Tasks;
using GameAuthentication;
using Prime31.MessageKit;
using Reflex.Attributes;
using UnityEngine;

// Menu-facing front for the injected auth coordinator: provider sign-in buttons, account switch/creation prompts.
public class UnityAuthManager : CustomSingleton<UnityAuthManager>
{
	// MessageKit ids: 100 auth error text, 109 auth busy flag, 110 Google Play Games unavailable.
	private const int AuthErrorMessage = 100;

	private const int AuthBusyMessage = 109;

	private const int GooglePlayGamesUnavailableMessage = 110;

	[Inject]
	private IAuthCoordinator authCoordinator;

	private CancellationTokenSource activeProviderFlow;

	public bool IsSignedIn => authCoordinator != null && authCoordinator.State.IsAuthorized;

	public string PlayerId
	{
		get
		{
			if (authCoordinator != null && authCoordinator.State.PlayerId != null)
			{
				return authCoordinator.State.PlayerId;
			}
			return string.Empty;
		}
	}

	private async void Start()
	{
		AuthOperationResult result = await authCoordinator.BootstrapAsync();
		if (!result.IsSuccess)
		{
			ReportFailure(result);
		}
	}

	private void Update()
	{
		authCoordinator?.Tick();
	}

	public void LoginWithFacebook()
	{
		StartProviderFlow(GameAuthProvider.Facebook);
	}

	public void LoginWithGooglePlayGames()
	{
		StartProviderFlow(GameAuthProvider.GooglePlayGames);
	}

	public void LoginWithUnityAccount()
	{
		StartProviderFlow(GameAuthProvider.UnityPlayerAccount);
	}

	public void LoginWithApple()
	{
		StartProviderFlow(GameAuthProvider.Apple);
	}

	public void CancelProviderSignIn()
	{
		CancellationTokenSource flow = activeProviderFlow;
		activeProviderFlow = null;
		flow?.Cancel();
		if (authCoordinator != null)
		{
			authCoordinator.CancelProviderCredentialAcquisition();
			AuthOperation operation = authCoordinator.State.Operation;
			if (operation == AuthOperation.AwaitingSwitchDecision || operation == AuthOperation.AwaitingAccountCreationDecision)
			{
				authCoordinator.CancelPendingAction();
			}
		}
		MessageKit<bool>.post(AuthBusyMessage, false);
	}

	public void SignOut()
	{
		AuthSignOutPresenter.TryRequestSignOut(authCoordinator, delegate(AuthOperationResult result)
		{
			MessageKit<bool>.post(AuthBusyMessage, false);
			if (!result.IsSuccess)
			{
				ReportFailure(result);
			}
		}, null, delegate
		{
			MessageKit<bool>.post(AuthBusyMessage, true);
		});
	}

	private void StartProviderFlow(GameAuthProvider provider)
	{
		CancelProviderSignIn();
		CancellationTokenSource flow = new CancellationTokenSource();
		activeProviderFlow = flow;
		_ = ContinueWithProviderAsync(provider, flow);
	}

	private async Task ContinueWithProviderAsync(GameAuthProvider provider, CancellationTokenSource flow)
	{
		MessageKit<bool>.post(AuthBusyMessage, true);
		try
		{
			AuthOperationResult result = await authCoordinator.ContinueWithProviderAsync(provider, flow.Token);
			// A newer flow (or a cancel) supersedes this one; its result is dropped silently.
			if (flow.IsCancellationRequested || activeProviderFlow != flow)
			{
				return;
			}
			switch (result.Code)
			{
			case AuthResultCode.Success:
			case AuthResultCode.UserCancelled:
				break;
			case AuthResultCode.ConnectedToCurrentAccount:
				MessageBox.Show("Your current progress is now connected to " + GetProviderName(provider) + ".");
				break;
			case AuthResultCode.ExistingAccountFound:
				await SwitchToExistingProviderAccountAsync();
				break;
			case AuthResultCode.SwitchedAccount:
				MessageBox.Show("Signed in with " + GetProviderName(provider) + ".");
				break;
			case AuthResultCode.AlreadyConnected:
				MessageBox.Show(GetProviderName(provider) + " is already connected to this Game Account.");
				break;
			case AuthResultCode.NoExistingGameAccount:
				ShowCreateLinkedAccountPrompt(provider);
				break;
			default:
				if (result.Code == AuthResultCode.ProviderUnavailable && provider == GameAuthProvider.GooglePlayGames)
				{
					MessageKit.post(GooglePlayGamesUnavailableMessage);
				}
				ReportFailure(result);
				break;
			}
		}
		catch (Exception exception)
		{
			if (!flow.IsCancellationRequested && activeProviderFlow == flow)
			{
				ReportUnexpectedFailure(exception);
			}
		}
		finally
		{
			if (activeProviderFlow == flow)
			{
				activeProviderFlow = null;
				MessageKit<bool>.post(AuthBusyMessage, false);
			}
			flow.Dispose();
		}
	}

	private void ShowCreateLinkedAccountPrompt(GameAuthProvider provider)
	{
		string message = "<b>No Game Account found</b>\n\n" + ("No existing Game Account is connected to this " + GetProviderName(provider) + " profile. Create a new Game Account connected to it?");
		if (!QuestionBox.TryShow(message, "Create", "Cancel", delegate
		{
			_ = CreateLinkedAccountAsync();
		}, authCoordinator.CancelPendingAction, null, false, false, GameplayInputContext.Kind.Modal, false))
		{
			authCoordinator.CancelPendingAction();
			Debug.LogWarning("Game Account creation was cancelled because its confirmation dialog could not be shown.");
		}
	}

	private async Task SwitchToExistingProviderAccountAsync()
	{
		AuthOperationResult result = await authCoordinator.ConfirmSwitchGameAccountAsync();
		if (!result.IsSuccess)
		{
			ReportFailure(result);
		}
		else if (result.Code == AuthResultCode.SwitchedAccount && result.Provider.HasValue)
		{
			MessageBox.Show("Signed in with " + GetProviderName(result.Provider.Value) + ".");
		}
	}

	private async Task CreateLinkedAccountAsync()
	{
		MessageKit<bool>.post(AuthBusyMessage, true);
		try
		{
			AuthOperationResult result = await authCoordinator.CreateLinkedGameAccountAsync();
			if (!result.IsSuccess)
			{
				ReportFailure(result);
			}
		}
		finally
		{
			MessageKit<bool>.post(AuthBusyMessage, false);
		}
	}

	private static void ReportUnexpectedFailure(Exception exception)
	{
		Debug.LogError(string.Format("[Auth] Unexpected authentication flow failure: {0}", exception));
		MessageKit<string>.post(AuthErrorMessage, exception.Message);
	}

	private static void ReportFailure(AuthOperationResult result)
	{
		string message = !string.IsNullOrEmpty(result.Error) ? result.Error : string.Format("Authentication failed: {0}", result.Code);
		Debug.LogWarning(string.Format("[Auth] {0}: {1} ({2})", result.Code, message, result.CorrelationId));
		MessageKit<string>.post(AuthErrorMessage, message);
	}

	private static string GetProviderName(GameAuthProvider provider)
	{
		switch (provider)
		{
		case GameAuthProvider.UnityPlayerAccount:
			return "Unity Player Account";
		case GameAuthProvider.Facebook:
			return "Facebook";
		case GameAuthProvider.Apple:
			return "Apple";
		case GameAuthProvider.GooglePlayGames:
			return "Google Play Games";
		default:
			return provider.ToString();
		}
	}

	protected override void OnDestroy()
	{
		CancelProviderSignIn();
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}
