using System.Threading;
using GameAuthentication;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MyAuthentication : MonoBehaviour
{
	[SerializeField]
	private GameObject loginButtons;

	[SerializeField]
	private GameObject loginWithGoogle;

	[SerializeField]
	private GameObject loginWithApple;

	[SerializeField]
	private Text token;

	[SerializeField]
	private Text loginErrorText;

	[SerializeField]
	private GameObject downloadGooglePlayGamesButton;

	[SerializeField]
	private GameObject clearCache;

	[SerializeField]
	private GameObject deleteAccount;

	[SerializeField]
	private GameObject spinner;

	private static bool isOpen;

	private bool loginInProgress;

	private readonly CancellationTokenSource cancellation = new CancellationTokenSource();

	[Inject]
	private IAuthCoordinator authCoordinator;

	[SerializeField]
	private Text myAccountLabel;

	public static void Show()
	{
		if (!isOpen)
		{
			isOpen = true;
			SceneManager.LoadSceneAsync("Authentication", LoadSceneMode.Additive);
		}
	}

	public void CloseWindow()
	{
		authCoordinator?.CancelProviderCredentialAcquisition();
		isOpen = false;
		SceneManager.UnloadSceneAsync("Authentication", UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
	}

	private void Start()
	{
		ToggleSpinner(false);
		ClearAuthError();
		foreach (GameObject root in gameObject.scene.GetRootGameObjects())
		{
			foreach (Button button in root.GetComponentsInChildren<Button>(true))
			{
				for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
				{
					if (button.onClick.GetPersistentMethodName(i) == "LoginUsingFacebook")
					{
						button.gameObject.SetActive(false);
						Debug.Log("[Auth] Facebook sign-in hidden");
					}
				}
			}
		}
		if (loginWithGoogle != null)
		{
			loginWithGoogle.SetActive(false);
		}
		if (loginWithApple != null)
		{
			loginWithApple.SetActive(false);
		}
		if (downloadGooglePlayGamesButton != null)
		{
			downloadGooglePlayGamesButton.SetActive(false);
		}
		if (authCoordinator != null)
		{
			authCoordinator.StateChanged += OnAuthStateChanged;
			RenderGuestUpgradeStatus(authCoordinator.State);
			_ = authCoordinator.BootstrapAsync();
		}
	}

	private void OnDestroy()
	{
		isOpen = false;
		cancellation.Cancel();
		if (authCoordinator != null)
		{
			authCoordinator.StateChanged -= OnAuthStateChanged;
		}
	}

	private void OnAuthStateChanged(AuthState state)
	{
		RenderGuestUpgradeStatus(state);
	}

	private void RenderGuestUpgradeStatus(AuthState state)
	{
		if (myAccountLabel != null)
		{
			myAccountLabel.text = AuthAccountStatusPresenter.Build(state).CombinedStatusText;
		}
	}

	private void ToggleSpinner(bool showSpinner)
	{
		if (spinner != null)
		{
			spinner.SetActive(showSpinner);
		}
		if (loginButtons != null)
		{
			loginButtons.SetActive(!showSpinner);
		}
	}

	private void ProviderUnavailable()
	{
		ShowAuthError("This sign-in method is not available in this build");
	}

	public void LoginUsingFacebook()
	{
		ProviderUnavailable();
	}

	public void LoginUsingGooglePlayGames()
	{
		ProviderUnavailable();
	}

	public void DownloadGooglePlayGames()
	{
		ProviderUnavailable();
	}

	private void ShowAuthError(string message)
	{
		if (loginErrorText != null)
		{
			loginErrorText.text = message;
		}
	}

	private void ClearAuthError()
	{
		ShowAuthError("");
	}

	public void SignOut()
	{
		AuthSignOutPresenter.TryRequestSignOut(authCoordinator);
	}

	public async void LoginUsingUnity()
	{
		if (loginInProgress)
		{
			return;
		}
		loginInProgress = true;
		ClearAuthError();
		ToggleSpinner(true);
		AuthOperationResult result = await authCoordinator.ContinueWithProviderAsync(GameAuthProvider.UnityPlayerAccount, cancellation.Token);
		loginInProgress = false;
		if (this == null)
		{
			return;
		}
		ToggleSpinner(false);
		if (result.IsSuccess)
		{
			CloseWindow();
		}
		else if (result.Code != AuthResultCode.UserCancelled)
		{
			ShowAuthError(result.Error ?? result.Code.ToString());
		}
	}
}
