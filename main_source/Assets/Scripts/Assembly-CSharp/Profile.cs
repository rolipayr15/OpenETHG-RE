using System.Collections;
using System.Text;
using Bibloteka;
using ExternalApi.Models;
using GameAuthentication;
using Multiplayer;
using Multiplayer.Chat;
using Multiplayer.ExternalApi;
using Prime31.MessageKit;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Profile : MonoBehaviour
{
	[Inject]
	private IStatsApi statsApi;

	[SerializeField]
	private Text username;

	[SerializeField]
	private Text stats;

	[SerializeField]
	private GameObject myAccountButtons;

	[SerializeField]
	private GameObject loginButton;

	[SerializeField]
	private GameObject myAccountLoginButton;

	private static bool isOpen;

	[SerializeField]
	private GameObject spinner;

	[SerializeField]
	private Text chatLabel;

	[SerializeField]
	private Text chatInfoLabel;

	[Inject]
	private IAuthCoordinator authCoordinator;

	[Inject]
	private IChatClient chatClient;

	[Inject]
	private IExperienceManager experienceManager;

	[Inject]
	private IUserApi userApi;

	[SerializeField]
	private Text infoLabel;

	private int activePanelIndex;

	private Coroutine showStatsCoroutine;

	[SerializeField]
	private GameObject profilePanel;

	[SerializeField]
	private GameObject statsPanel;

	[SerializeField]
	private GameObject leaguePanel;

	[SerializeField]
	private GameObject accountPanel;

	[SerializeField]
	private Text profileText;

	[SerializeField]
	private Text statsText;

	[SerializeField]
	private Text leagueText;

	[SerializeField]
	private Text accountText;

	[SerializeField]
	private GameObject accountLoginButton;

	[SerializeField]
	private GameObject changeUsernameButton;

	[SerializeField]
	private Text accountStatusLabel;

	private static bool BasicAccount => UserGameSettings.IsSignedInWithBasicAuth;

	public static void Show()
	{
		if (!isOpen)
		{
			isOpen = true;
			SceneManager.LoadSceneAsync("Profile", LoadSceneMode.Additive);
		}
	}

	private async void Start()
	{
		if (authCoordinator != null)
		{
			authCoordinator.StateChanged += OnAuthStateChanged;
		}
		MessageKit.addObserver(128, OnUsernameChanged);
		MessageKit<bool>.addObserver(138, OnBasicAuthChanged);
		if (spinner != null)
		{
			spinner.SetActive(false);
		}
		SetChatRowVisible(false);
		RenderCurrent();
		SetPanel(0);
		if (BasicAccount)
		{
			UserApi.LocalExp = experienceManager.GetTotalExp();
			SyncUserInfoResponse synced = await userApi.SyncUserInfoAsync();
			if (this != null && synced.newLocalExp != experienceManager.GetTotalExp())
			{
				experienceManager.OverwriteExp(synced.newLocalExp);
				RenderCurrent();
			}
		}
	}

	private void OnBasicAuthChanged(bool signedIn)
	{
		RenderCurrent();
	}

	private void RenderCurrent()
	{
		RenderAccountUi(AuthAccountStatusPresenter.Build(authCoordinator?.State));
	}

	private void OnAuthStateChanged(AuthState state)
	{
		if (state.Operation == AuthOperation.Idle)
		{
			RenderAccountUi(AuthAccountStatusPresenter.Build(state), true);
		}
	}

	public void CloseWindow()
	{
		if (showStatsCoroutine != null)
		{
			StopCoroutine(showStatsCoroutine);
		}
		Unsubscribe();
		if (Utils.IsSceneLoaded("Profile"))
		{
			SceneManager.UnloadSceneAsync("Profile", UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
		}
		isOpen = false;
	}

	private void Unsubscribe()
	{
		MessageKit.removeObserver(128, OnUsernameChanged);
		MessageKit<bool>.removeObserver(138, OnBasicAuthChanged);
		if (authCoordinator != null)
		{
			authCoordinator.StateChanged -= OnAuthStateChanged;
		}
	}

	private void OnDestroy()
	{
		Unsubscribe();
		isOpen = false;
	}

	public void ChangeUsernameButton()
	{
		bool linked = authCoordinator != null && authCoordinator.State.IsAuthorized && authCoordinator.State.AccountType == GameAccountType.Linked;
		if (linked || BasicAccount)
		{
			UsernameChange.Show();
		}
	}

	private void SetUsernameLabel()
	{
		if (username != null)
		{
			username.text = UserGameSettings.multiplayerUsername;
		}
	}

	private void OnUsernameChanged()
	{
		SetUsernameLabel();
		SetPanel(activePanelIndex);
	}

	private void RenderAccountUi(AuthAccountStatusView view, bool preserveActivePanel = false)
	{
		bool signedIn = BasicAccount || view.IsSignedInAccount;
		string status = BasicAccount ? "Logged in as: " + UserGameSettings.multiplayerUsername : view.CombinedStatusText;
		if (accountStatusLabel != null)
		{
			accountStatusLabel.text = status;
		}
		SetUsernameLabel();
		if (myAccountButtons != null)
		{
			myAccountButtons.SetActive(signedIn);
		}
		if (loginButton != null)
		{
			loginButton.SetActive(!signedIn);
		}
		if (myAccountLoginButton != null)
		{
			myAccountLoginButton.SetActive(false);
		}
		if (accountLoginButton != null)
		{
			accountLoginButton.SetActive(view.ShowProviderUpgradeActions && !BasicAccount);
		}
		if (changeUsernameButton != null)
		{
			changeUsernameButton.SetActive(BasicAccount || view.Kind == AuthAccountStatusKind.LinkedKnownProvider || view.Kind == AuthAccountStatusKind.LinkedUnknownProvider);
		}
		if (infoLabel != null)
		{
			StringBuilder info = new StringBuilder(status);
			info.Append("\n\nExp: ").Append(experienceManager.GetTotalExp());
			info.Append("\nLevel: ").Append(experienceManager.GetCurrentLevel());
			infoLabel.text = info.ToString();
		}
		if (preserveActivePanel && activePanelIndex == 1)
		{
			SetPanel(1);
		}
	}

	public void Login()
	{
		MyAuthentication.Show();
	}

	public IEnumerator ShowMyStats()
	{
		if (spinner != null)
		{
			spinner.SetActive(true);
		}
		if (stats != null)
		{
			stats.text = "";
		}
		if (OnlineConfig.IsCustomServer && !UserGameSettings.IsSignedInWithBasicAuth)
		{
			int attempts = 2;
			yield return OnlineConfig.AutoSignIn(() => attempts-- > 0);
		}
		statsApi.InvalidateStatsCache();
		var request = statsApi.GetStatsAsync();
		while (!request.IsCompleted)
		{
			yield return null;
		}
		if (spinner != null)
		{
			spinner.SetActive(false);
		}
		ShowRecentStats(request.Status == System.Threading.Tasks.TaskStatus.RanToCompletion ? request.Result : null);
	}

	private void ShowRecentStats(AllStats _stats)
	{
		if (stats != null)
		{
			stats.text = _stats != null ? _stats.GetFormattedTable() : "No statistics available";
		}
	}

	public void SignOut()
	{
		if (BasicAccount)
		{
			MultiplayerSettings.ClearReviewerBasicAuth();
			CloseWindow();
			return;
		}
		AuthSignOutPresenter.TryRequestSignOut(authCoordinator);
	}

	public void DeleteAccount()
	{
		if (!BasicAccount)
		{
			_ = authCoordinator.DeleteGameAccountAsync();
		}
	}

	public void SetPanel(int index)
	{
		activePanelIndex = index;
		profilePanel.SetActive(index == 0);
		statsPanel.SetActive(index == 1);
		leaguePanel.SetActive(index == 2);
		accountPanel.SetActive(index == 3);
		profileText.color = index == 0 ? Color.black : Color.white;
		statsText.color = index == 1 ? Color.black : Color.white;
		leagueText.color = index == 2 ? Color.black : Color.white;
		accountText.color = index == 3 ? Color.black : Color.white;
		if (index == 1)
		{
			if (showStatsCoroutine != null)
			{
				StopCoroutine(showStatsCoroutine);
			}
			showStatsCoroutine = StartCoroutine(ShowMyStats());
		}
	}

	public void ToggleChat()
	{
	}

	private void SetChatRowVisible(bool visible)
	{
		if (chatLabel != null && chatLabel.transform.parent != null)
		{
			chatLabel.transform.parent.gameObject.SetActive(visible);
		}
	}
}
