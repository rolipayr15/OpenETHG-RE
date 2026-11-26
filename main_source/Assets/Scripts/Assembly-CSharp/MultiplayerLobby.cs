using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Bibloteka;
using BestHTTP;
using EyesAnalytics;
using GameAuthentication;
using I2.Loc;
using Multiplayer;
using Multiplayer.ExternalApi;
using Prime31.MessageKit;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MultiplayerLobby : MonoBehaviour
{
	[Inject]
	private IAuthCoordinator authCoordinator;

	[SerializeField]
	private GameObject commonPanel;

	private readonly Dictionary<int, Difficulty> difficultyDict = new Dictionary<int, Difficulty>
	{
		{ 0, (Difficulty)2 },
		{ 1, (Difficulty)3 },
		{ 2, (Difficulty)4 }
	};

	[SerializeField]
	private Text codeLabel;

	[SerializeField]
	private GameObject panelWaitingWithCode;

	[SerializeField]
	private GameObject sendThisCodeLabel;

	[SerializeField]
	private GameObject panelCommon;

	[SerializeField]
	private InputField joinInput;

	[SerializeField]
	private Text errorLabel;

	[SerializeField]
	private GameObject randomPanel;

	[SerializeField]
	private GameObject friendPanel;

	[SerializeField]
	private Text ticketsAvailable;

	[SerializeField]
	private GameObject loginPanel;

	[SerializeField]
	private GameObject myAccountButton;

	[SerializeField]
	private GameObject myAccountPanel;

	[SerializeField]
	private GameObject signOutButton;

	[SerializeField]
	private GameObject fbButton;

	[SerializeField]
	private Text myAccountLabel;

	[SerializeField]
	private Text signOutLabel;

	[SerializeField]
	private GameObject deleteAccountButton;

	[SerializeField]
	private GameObject loginButton;

	[SerializeField]
	private GameObject shareButton;

	[SerializeField]
	private GameObject keysCounter;

	[Inject]
	private IExpApi expApi;

	[SerializeField]
	private WsBestHttpRouteTester serverTester;

	[SerializeField]
	private Button[] multiplayerEntryButtons;

	private readonly CancellationTokenSource networkTestCts = new CancellationTokenSource();

	private string networkStatusMessage;

	private bool networkTestInitialized;

	private bool networkRefreshInProgress;

	private bool closing;

	[SerializeField]
	private GameObject loginForReview;

	[SerializeField]
	private InputField reviewerLoginInput;

	[SerializeField]
	private InputField reviewerPasswordInput;

	[SerializeField]
	private GameObject reviewerSubmitButton;

	[SerializeField]
	private GameObject reviewerSpinner;

	[SerializeField]
	private Text reviewerErrorLabel;

	private bool reviewerLoginInProgress;

	private int reviewerLoginAttemptId;

	private HTTPRequest activeReviewerLoginRequest;

	[SerializeField]
	private Text stats;

	[SerializeField]
	private Text myAccountButtonLabel;

	[SerializeField]
	private Text infoLabel;

	[SerializeField]
	private Text panelWaitingGameDetails;

	[SerializeField]
	private GameObject goButton;

	[SerializeField]
	private GameObject spinnerInput;

	[SerializeField]
	private Text contentLockedLabel;

	[SerializeField]
	private GameObject goUnlockContentButton;

	private string lockedContentFromMsg;

	private readonly Dictionary<Enemy, StoreItems> enemyToStoreItem = new Dictionary<Enemy, StoreItems>
	{
		{ (Enemy)1, StoreItems.CHARLIE },
		{ (Enemy)2, StoreItems.GOOD_BOY },
		{ (Enemy)6, StoreItems.WITCH }
	};

	private readonly Dictionary<Level, StoreItems> levelToStoreItem = new Dictionary<Level, StoreItems>
	{
		{ (Level)1, StoreItems.HOSPITAL },
		{ (Level)3, StoreItems.SCHOOL }
	};

	private string gameToken = "";

	[SerializeField]
	private Text subLabel;

	[SerializeField]
	private GameObject keysMainMenuButton;

	[SerializeField]
	private GameObject keyImage;

	[SerializeField]
	private Button coopButton;

	[SerializeField]
	private Button vsButton;

	private Enemy enemySelected;

	public bool isEnemyUnlocked = true;

	public bool isMapUnlocked = true;

	private Level mapSelected;

	private AuthState CurrentAuthState => authCoordinator?.State;

	private void Start()
	{
		OnlineConfig.Apply();
		Screens.View((ScreenId)13);
		MultiplayerServer.LobbyOpened(UserGameSettings.isPlayingWithRandom);
		if (authCoordinator != null)
		{
			authCoordinator.StateChanged += OnAuthStateChanged;
		}
		UserGameSettings.currentMode = (Mode)0;
		UserGameSettings.currentEnemy = (Enemy)0;
		UserGameSettings.currentLevel = (Level)0;
		myAccountButton.SetActive(false);
		MessageKit<MultiGame>.addObserver(83, OnGameFound);
		MessageKit<GameWithToken>.addObserver(84, OnGameWithTokenFound);
		MessageKit.addObserver(86, OnAllUsersConnected);
		MessageKit<bool>.addObserver(138, OnBasicAuthStateChanged);
		MessageKit<string>.addObserver(100, ShowDebug);
		MessageKit<string>.addObserver(32, UnlockSub);
		MessageKit<string>.addObserver(34, UnlockSub);
		MessageKit<string>.addObserver(87, OnError);
		codeLabel.text = "";
		commonPanel.SetActive(true);
		loginPanel.SetActive(false);
		ticketsAvailable.text = KeysManager.GetMultiplayerKeys().ToString();
		randomPanel.SetActive(UserGameSettings.isPlayingWithRandom);
		friendPanel.SetActive(!UserGameSettings.isPlayingWithRandom);
		UserGameSettings.isInvitedPlayer = false;
		if (UserGameSettings.unlockedMultiplayerSub || UserGameSettings.unlockedMultiplayerUnlimited)
		{
			SetActiveSub();
		}
		if (fbButton != null)
		{
			fbButton.SetActive(false);
		}
		Back();
		RenderAccountStatus(CurrentAuthState);
		if (UserGameSettings.googleNBO)
		{
			keysCounter.SetActive(false);
		}
		MultiplayerNetworkTest.Changed += RefreshNetworkTestState;
		networkTestInitialized = true;
		RunTest();
		StartCoroutine(MonitorNetworkTestFreshness());
		StartCoroutine(AutoSignIn());
		if (!OnlineConfig.IsCustomServer)
		{
			_ = authCoordinator?.BootstrapAsync();
		}
	}

	// The account service of the lost backend is replaced by a device account on the restored server.
	private IEnumerator AutoSignIn()
	{
		return OnlineConfig.AutoSignIn(() => !closing);
	}

	private void OnAuthStateChanged(AuthState state)
	{
		if (state.Operation == (AuthOperation)0)
		{
			RenderAccountStatus(state);
		}
	}

	private void OnBasicAuthStateChanged(bool signedIn)
	{
		RenderAccountStatus(CurrentAuthState);
	}

	private static bool IsLinkedGameAccount(AuthState state)
	{
		return AuthAccountStatusPresenter.IsLinkedGameAccount(state);
	}

	private static bool IsMultiplayerAuthenticationReady(AuthState state)
	{
		return AuthAccountStatusPresenter.IsMultiplayerReady(state, UserGameSettings.IsSignedInWithBasicAuth);
	}

	private async void RunTest()
	{
		if (closing || !networkTestInitialized || networkRefreshInProgress)
		{
			return;
		}
		networkRefreshInProgress = true;
		RefreshNetworkTestState();
		try
		{
			await MultiplayerNetworkTest.EnsureFreshAsync(serverTester, networkTestCts.Token);
		}
		catch (OperationCanceledException)
		{
		}
		networkRefreshInProgress = false;
		RefreshNetworkTestState();
	}

	private IEnumerator MonitorNetworkTestFreshness()
	{
		WaitForSecondsRealtime delay = new WaitForSecondsRealtime(1f);
		while (!closing)
		{
			yield return delay;
			if (!MultiplayerNetworkTest.HasFreshResult && !MultiplayerNetworkTest.IsRunning)
			{
				RefreshNetworkTestState();
			}
		}
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (hasFocus)
		{
			RefreshNetworkTestState();
		}
	}

	private void RefreshNetworkTestState()
	{
		if (this == null || closing || !networkTestInitialized)
		{
			return;
		}
		if (commonPanel.activeInHierarchy && !spinnerInput.activeSelf && !MultiplayerNetworkTest.HasFreshResult && !MultiplayerNetworkTest.IsRunning)
		{
			RunTest();
		}
		bool canPlay = MultiplayerNetworkTest.CanPlay;
		foreach (Button button in multiplayerEntryButtons)
		{
			button.interactable = canPlay;
		}
		SetButtons(isEnemyUnlocked && isMapUnlocked);
		string status = "";
		if (MultiplayerNetworkTest.IsRunning)
		{
			status = ScriptLocalization.NetworkTestInProgress + string.Format(" {0:F0}%", MultiplayerNetworkTest.Progress * 100f);
		}
		else if (!canPlay && MultiplayerNetworkTest.HasFreshResult)
		{
			status = ScriptLocalization.NetworkTestMultiplayerBlocked;
		}
		if (!string.IsNullOrEmpty(status) || errorLabel.text == networkStatusMessage)
		{
			errorLabel.text = status;
			networkStatusMessage = status;
		}
	}

	private bool EnsureNetworkReady()
	{
		if (closing)
		{
			return false;
		}
		RunTest();
		return MultiplayerNetworkTest.CanPlay;
	}

	private static bool IsReviewerLoginEnabled()
	{
		return UserGameSettings.adsDisabledViaRemoteConfig;
	}

	private void ShowLoginPanel()
	{
		(UserGameSettings.adsDisabledViaRemoteConfig ? loginForReview : loginPanel).SetActive(true);
	}

	private void EnsureMultiplayerReady(Action action)
	{
		AuthState state = CurrentAuthState;
		if (IsMultiplayerAuthenticationReady(state))
		{
			action?.Invoke();
			return;
		}
		if (UserGameSettings.adsDisabledViaRemoteConfig)
		{
			Back();
			ShowLoginPanel();
			return;
		}
		if (AuthAccountStatusPresenter.IsGuestAccount(state))
		{
			bool shown = QuestionBox.TryShow(AuthAccountStatusPresenter.BuildMultiplayerGuestGateMessage(), AuthAccountStatusPresenter.MultiplayerGuestGateConfirmLabel, AuthAccountStatusPresenter.MultiplayerGuestNotNowLabel, MyAuthentication.Show, delegate
			{
			}, null, false, false, (GameplayInputContext.Kind)7);
			if (!shown)
			{
				Debug.LogWarning("Multiplayer Guest gate was not shown because its confirmation dialog could not be displayed.");
				if (errorLabel != null)
				{
					errorLabel.text = AuthAccountStatusPresenter.MultiplayerGuestGateTitle;
				}
			}
		}
		else if (AuthAccountStatusPresenter.IsExpiredGameAccount(state))
		{
			bool shown = QuestionBox.TryShow(AuthAccountStatusPresenter.BuildMultiplayerExpiredGateMessage(), AuthAccountStatusPresenter.MultiplayerMyAccountLabel, AuthAccountStatusPresenter.MultiplayerGuestNotNowLabel, Profile.Show, delegate
			{
			}, null, false, false, (GameplayInputContext.Kind)7);
			if (!shown)
			{
				Debug.LogWarning("Expired Game Account gate was not shown because its confirmation dialog could not be displayed.");
				if (errorLabel != null)
				{
					errorLabel.text = AuthAccountStatusPresenter.SessionExpiredTitle;
				}
			}
		}
		else
		{
			MyAuthentication.Show();
		}
	}

	public void ShowMyAccount()
	{
		if (UserGameSettings.IsSignedInWithBasicAuth)
		{
			Back();
			myAccountPanel.SetActive(true);
		}
		else if (AuthAccountStatusPresenter.IsSignedInGameAccount(CurrentAuthState))
		{
			Profile.Show();
		}
		else
		{
			MyAuthentication.Show();
		}
	}

	private void RenderAccountStatus(AuthState state)
	{
		if (myAccountButton == null)
		{
			return;
		}
		AuthAccountStatusView view = AuthAccountStatusPresenter.Build(state);
		bool ready = IsMultiplayerAuthenticationReady(state);
		bool basic = UserGameSettings.IsSignedInWithBasicAuth;
		string label = AuthAccountStatusPresenter.ShouldShowMyAccountLabel(state) || basic ? ScriptLocalization.MyAccount : ScriptLocalization.SignIn;
		Text childLabel = myAccountButton.GetComponentInChildren<Text>();
		if (childLabel != null)
		{
			childLabel.text = label;
		}
		Back();
		myAccountButton.SetActive(true);
		if (myAccountButtonLabel != null)
		{
			myAccountButtonLabel.text = label;
		}
		string username = UserGameSettings.multiplayerUsername;
		bool showDelete;
		if (basic)
		{
			myAccountLabel.text = string.IsNullOrEmpty(username) ? "Signed in for multiplayer review" : "Logged in as: " + username;
			signOutLabel.text = "sign out";
			showDelete = false;
		}
		else if (ready && AuthAccountStatusPresenter.IsLinkedGameAccount(state))
		{
			myAccountLabel.text = string.IsNullOrEmpty(username) ? view.Title : view.Title + "\nLogged in as: " + username;
			signOutLabel.text = "sign out";
			showDelete = true;
		}
		else if (view.Kind == (AuthAccountStatusKind)4 || view.Kind == (AuthAccountStatusKind)1)
		{
			myAccountLabel.text = view.CombinedStatusText;
			signOutLabel.text = "sign out";
			showDelete = true;
		}
		else
		{
			UserGameSettings.multiplayerUsername = "";
			myAccountLabel.text = view.CombinedStatusText;
			signOutLabel.text = "clear cache";
			deleteAccountButton.SetActive(false);
			loginButton.SetActive(true);
			return;
		}
		deleteAccountButton.SetActive(showDelete);
		if (loginButton != null)
		{
			loginButton.SetActive(false);
		}
	}

	public void ShowLogin()
	{
		Back();
		ShowLoginPanel();
	}

	public void LoginWithReviewerBasicAuth()
	{
		if (!UserGameSettings.adsDisabledViaRemoteConfig || reviewerLoginInProgress)
		{
			return;
		}
		string login = reviewerLoginInput != null ? reviewerLoginInput.text : "";
		string password = reviewerPasswordInput != null ? reviewerPasswordInput.text : "";
		if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
		{
			SetReviewerLoginError("Enter username and password");
			return;
		}
		string credential = Convert.ToBase64String(Encoding.UTF8.GetBytes(login + ":" + password));
		reviewerPasswordInput.text = "";
		reviewerLoginInProgress = true;
		reviewerLoginAttemptId++;
		StartCoroutine(SendReviewerLoginRequest(credential, reviewerLoginAttemptId));
	}

	private IEnumerator SendReviewerLoginRequest(string candidateCredential, int attemptId)
	{
		SetReviewerLoginBusy(true);
		SetReviewerLoginError("");
		HTTPRequest loginRequest = CustomSingleton<MultiplayerSettings>.Instance.SendReviewerBasicAuthLogin(candidateCredential);
		activeReviewerLoginRequest = loginRequest;
		while (loginRequest.State < HTTPRequestStates.Finished)
		{
			if (!IsReviewerLoginAttemptCurrent(attemptId))
			{
				yield break;
			}
			yield return Utils.GetDelay(0.1f);
		}
		if (!IsReviewerLoginAttemptCurrent(attemptId))
		{
			yield break;
		}
		bool finished = loginRequest.State == HTTPRequestStates.Finished;
		bool success = finished && loginRequest.Response.IsSuccess;
		CompleteReviewerLoginAttempt(attemptId);
		if (success)
		{
			MultiplayerSettings.ActivateReviewerBasicAuth(candidateCredential);
			if (reviewerLoginInput != null)
			{
				reviewerLoginInput.text = "";
			}
			yield break;
		}
		MultiplayerSettings.ClearReviewerBasicAuth();
		bool unauthorized = finished && loginRequest.Response != null && loginRequest.Response.StatusCode == 401;
		SetReviewerLoginError(unauthorized ? "Invalid login or password" : "Try again later");
	}

	private bool IsReviewerLoginAttemptCurrent(int attemptId)
	{
		return reviewerLoginInProgress && reviewerLoginAttemptId == attemptId;
	}

	private void CompleteReviewerLoginAttempt(int attemptId)
	{
		if (reviewerLoginInProgress && reviewerLoginAttemptId == attemptId)
		{
			activeReviewerLoginRequest = null;
			reviewerLoginInProgress = false;
			SetReviewerLoginBusy(false);
		}
	}

	private void CancelReviewerLoginAttempt(bool updateUi = true)
	{
		if (!reviewerLoginInProgress)
		{
			return;
		}
		reviewerLoginInProgress = false;
		HTTPRequest request = activeReviewerLoginRequest;
		activeReviewerLoginRequest = null;
		reviewerLoginAttemptId++;
		if (request != null && request.State < HTTPRequestStates.Finished)
		{
			request.Abort();
		}
		if (updateUi)
		{
			SetReviewerLoginBusy(false);
		}
	}

	private void SetReviewerLoginBusy(bool busy)
	{
		if (reviewerSubmitButton != null)
		{
			reviewerSubmitButton.SetActive(!busy);
		}
		if (reviewerSpinner != null)
		{
			reviewerSpinner.SetActive(busy);
		}
	}

	private void SetReviewerLoginError(string message)
	{
		if (reviewerErrorLabel != null)
		{
			reviewerErrorLabel.text = message;
		}
	}

	public void SignOutAccount()
	{
		if (!UserGameSettings.IsSignedInWithBasicAuth)
		{
			AuthSignOutPresenter.TryRequestSignOut(authCoordinator, delegate(AuthOperationResult result)
			{
				if (!result.IsSuccess)
				{
					ShowDebug(result.Code.ToString());
				}
			});
			return;
		}
		bool shown = QuestionBox.TryShow("<b>Sign out?</b>\n\nAre you sure?", "Sign Out", "Cancel", CompleteReviewerBasicAuthSignOut, delegate
		{
		}, null, false, false, (GameplayInputContext.Kind)7);
		if (!shown)
		{
			Debug.LogWarning("Reviewer sign-out was not started because its confirmation dialog could not be shown.");
		}
	}

	private void CompleteReviewerBasicAuthSignOut()
	{
		MultiplayerSettings.ClearReviewerBasicAuth();
		Back();
		RenderAccountStatus(CurrentAuthState);
	}

	private void ShowDebug(string debug)
	{
		errorLabel.text = debug + "\n" + errorLabel.text;
	}

	public void CreateCoopGameWithRandom()
	{
		CreateGameWithRandom((GameMode)1);
	}

	public void CreateVsGameWithRandom()
	{
		CreateGameWithRandom((GameMode)2);
	}

	public void CreateCoopGameWithFriend()
	{
		CreateGameWithFriend((GameMode)1);
	}

	public void CreateVsGameFriend()
	{
		CreateGameWithFriend((GameMode)2);
	}

	private bool PassesEntryChecks(Action retry)
	{
		if (!EnsureNetworkReady())
		{
			if (!closing)
			{
				MultiplayerServer.Blocked(BlockReason.Network);
			}
			return false;
		}
		if (!IsMultiplayerAuthenticationReady(CurrentAuthState))
		{
			MultiplayerServer.Blocked(BlockReason.SignIn);
			EnsureMultiplayerReady(retry);
			return false;
		}
		errorLabel.text = "";
		if (KeysManager.GetMultiplayerKeys() < UserGameSettings.gamePriceInKeys && !UserGameSettings.unlockedMultiplayerSub && !UserGameSettings.unlockedMultiplayerUnlimited)
		{
			errorLabel.text = ScriptLocalization.MultiplayerRunOut;
			MultiplayerServer.Blocked(BlockReason.Keys);
			MultiplayerKeys.Show((CartType)2, ScriptLocalization.MultiplayerRunOut);
			return false;
		}
		return true;
	}

	private void ShowGameDetails()
	{
		panelWaitingGameDetails.text = "Multiplayer mode: " + UserGameSettings.currentGameMode + "\nDifficulty: " + UserGameSettings.currentDifficulty + "\nMode: " + UserGameSettings.currentMode + "\nEnemy: " + UserGameSettings.currentEnemy + "\nLevel: " + UserGameSettings.currentLevel + "\n";
	}

	private void CreateGameWithRandom(GameMode multiplayerMode)
	{
		if (!PassesEntryChecks(delegate
		{
			CreateGameWithRandom(multiplayerMode);
		}))
		{
			return;
		}
		panelCommon.SetActive(false);
		shareButton.SetActive(false);
		panelWaitingWithCode.SetActive(true);
		sendThisCodeLabel.SetActive(false);
		codeLabel.text = "";
		UserGameSettings.currentEnemy = (Enemy)0;
		UserGameSettings.currentLevel = (Level)0;
		UserGameSettings.currentGameMode = multiplayerMode;
		UserGameSettings.platform = (Platform)1;
		UserGameSettings.currentMode = (Mode)0;
		ShowGameDetails();
		CustomSingleton<MultiplayerSettings>.Instance.JoinOrCreate(UserGameSettings.currentGameMode, UserGameSettings.currentDifficulty, UserGameSettings.currentMode, UserGameSettings.currentEnemy, UserGameSettings.currentLevel);
		MultiplayerServer.SearchStarted();
	}

	private void CreateGameWithFriend(GameMode multiplayerMode)
	{
		if (!PassesEntryChecks(delegate
		{
			CreateGameWithFriend(multiplayerMode);
		}))
		{
			return;
		}
		panelCommon.SetActive(false);
		shareButton.SetActive(true);
		panelWaitingWithCode.SetActive(true);
		sendThisCodeLabel.SetActive(true);
		codeLabel.text = "";
		UserGameSettings.currentGameMode = multiplayerMode;
		UserGameSettings.platform = (Platform)1;
		UserGameSettings.currentMode = (Mode)0;
		ShowGameDetails();
		CustomSingleton<MultiplayerSettings>.Instance.CreateGameWithToken(UserGameSettings.currentGameMode, UserGameSettings.currentDifficulty, UserGameSettings.currentMode, UserGameSettings.currentEnemy, UserGameSettings.currentLevel);
	}

	public void JoinAGame()
	{
		if (!EnsureNetworkReady())
		{
			if (!closing)
			{
				MultiplayerServer.Blocked(BlockReason.Network);
			}
			return;
		}
		string code = joinInput.textComponent.text;
		if (code == null || code.Length != 5)
		{
			errorLabel.text = "Invalid invitation code";
			return;
		}
		if (!IsMultiplayerAuthenticationReady(CurrentAuthState))
		{
			MultiplayerServer.Blocked(BlockReason.SignIn);
			EnsureMultiplayerReady(JoinAGame);
			return;
		}
		UserGameSettings.isInvitedPlayer = true;
		MessageKit<ErrorResponse>.addObserver(88, Error);
		MessageKit<string>.addObserver(111, ContentLocked);
		CustomSingleton<MultiplayerSettings>.Instance.FindByToken(code);
		MultiplayerServer.CodeEntered();
		ReplaceGoButtonWithSpinner(true);
	}

	private void ContentLocked(string content)
	{
		MultiplayerServer.Blocked(BlockReason.ContentLocked);
		UserGameSettings.isInvitedPlayer = false;
		ReplaceGoButtonWithSpinner(false);
		lockedContentFromMsg = content;
		contentLockedLabel.text = content + " unavailable";
		goUnlockContentButton.SetActive(true);
	}

	public void GoToLockedContentButton()
	{
		GoToLockedContent(enemyToStoreItem.ContainsKey(enemySelected) && !isEnemyUnlocked);
	}

	public void GoToLockedContent(bool isEnemy)
	{
		contentLockedLabel.text = "";
		goUnlockContentButton.SetActive(false);
		if (isEnemy)
		{
			MenuPurchase.Show(enemyToStoreItem[enemySelected], (CartType)2);
		}
		else if (levelToStoreItem.ContainsKey(mapSelected))
		{
			MenuPurchase.Show(levelToStoreItem[mapSelected], (CartType)2);
		}
	}

	private void ReplaceGoButtonWithSpinner(bool showSpiner)
	{
		goButton.SetActive(!showSpiner);
		spinnerInput.SetActive(showSpiner);
	}

	private void Error(ErrorResponse error)
	{
		MultiplayerServer.LobbyClosed();
		Debug.Log(error.code);
		UserGameSettings.isInvitedPlayer = false;
		if (error.code == "token-not-found")
		{
			errorLabel.text = "Invitation code not found";
		}
		else if (error.code == "invalid-token")
		{
			errorLabel.text = "Invalid invitation code";
		}
		else if (error.code == "invalid-provider-anonymous")
		{
			RenderAccountStatus(CurrentAuthState);
			errorLabel.text = AuthAccountStatusPresenter.MultiplayerGuestGateTitle;
			EnsureMultiplayerReady(null);
		}
		else
		{
			errorLabel.text = error.code;
			Debug.Log(error.code);
		}
		ReplaceGoButtonWithSpinner(false);
		panelWaitingWithCode.SetActive(false);
		panelWaitingGameDetails.text = "";
	}

	public void OnGameFound(MultiGame game)
	{
		Debug.Log("Connect to the game");
		UserGameSettings.currentGameMode = game.type == "vs" ? (GameMode)2 : (GameMode)1;
		UserGameSettings.platform = (Platform)1;
		Enum.TryParse<Difficulty>(game.difficulty, true, out Difficulty difficulty);
		Enum.TryParse<Level>(game.level, true, out Level level);
		(Enemy enemy, Mode mode) wire = MultiplayerWireSettings.FromWire(game.enemy, game.mode);
		UserGameSettings.currentEnemy = wire.enemy;
		UserGameSettings.currentLevel = level;
		UserGameSettings.currentMode = wire.mode;
		UserGameSettings.currentDifficulty = difficulty;
		CustomSingleton<MultiplayerSettings>.Instance.ConnectToTheGame(game.gameId, game.playerId, false);
	}

	public void OnGameWithTokenFound(GameWithToken game)
	{
		gameToken = game.token;
		codeLabel.text = gameToken;
		gameToken.CopyToClipboard();
		Utils.ShowToast("Code copied to clipboard");
		(Enemy enemy, Mode mode) wire = MultiplayerWireSettings.FromWire(game.enemy, game.mode);
		UserGameSettings.currentEnemy = wire.enemy;
		UserGameSettings.currentMode = wire.mode;
		CustomSingleton<MultiplayerSettings>.Instance.ConnectToTheGame(game.gameId, game.playerId, false);
		MultiplayerServer.RoomCreated();
	}

	public void ShareCode()
	{
		if (!string.IsNullOrEmpty(gameToken))
		{
			("Join my Eyes game: " + gameToken).CopyToClipboard();
			Utils.ShowToast("Code copied to clipboard");
			MultiplayerServer.CodeShared();
		}
	}

	public void OnAllUsersConnected()
	{
		MultiplayerServer.Matched();
		UserGameSettings.isImmersiveMode = false;
		CustomSingleton<ChooseLevelManager>.Instance.Play();
	}

	public static void Show(bool isPlayingWithRandom)
	{
		UserGameSettings.isPlayingWithRandom = isPlayingWithRandom;
		SceneManager.LoadSceneAsync("MultiplayerLobby", LoadSceneMode.Additive);
	}

	public void ExitWaitingWindow()
	{
		ReplaceGoButtonWithSpinner(false);
		MultiplayerServer.MatchmakingCancelled();
		MultiplayerSettings net = CustomSingleton<MultiplayerSettings>.Instance;
		net.SendFinishGame(GameFinishedReason.CANCEL);
		codeLabel.text = "";
		commonPanel.SetActive(true);
		panelWaitingWithCode.SetActive(false);
		loginPanel.SetActive(false);
		panelWaitingGameDetails.text = "";
		myAccountPanel.SetActive(false);
		net.Clear();
		net.CloseWebsocket();
		panelCommon.SetActive(true);
		RefreshNetworkTestState();
	}

	public void Back()
	{
		CancelReviewerLoginAttempt(true);
		errorLabel.text = "";
		commonPanel.SetActive(true);
		panelWaitingWithCode.SetActive(false);
		loginPanel.SetActive(false);
		myAccountPanel.SetActive(false);
		infoLabel.text = "";
		if (loginForReview != null)
		{
			loginForReview.SetActive(false);
		}
		RefreshNetworkTestState();
	}

	private void RemoveObservers()
	{
		MessageKit<MultiGame>.removeObserver(83, OnGameFound);
		MessageKit<GameWithToken>.removeObserver(84, OnGameWithTokenFound);
		MessageKit.removeObserver(86, OnAllUsersConnected);
		MessageKit<bool>.removeObserver(138, OnBasicAuthStateChanged);
		MessageKit<string>.removeObserver(100, ShowDebug);
		MessageKit<string>.removeObserver(32, UnlockSub);
		MessageKit<string>.removeObserver(34, UnlockSub);
		MessageKit<string>.removeObserver(87, OnError);
		if (authCoordinator != null)
		{
			authCoordinator.StateChanged -= OnAuthStateChanged;
		}
	}

	public void Close()
	{
		MultiplayerServer.LobbyClosed();
		closing = true;
		networkTestCts.Cancel();
		CancelReviewerLoginAttempt(false);
		CustomSingleton<MultiplayerSettings>.Instance.Clear();
		RemoveObservers();
		SceneManager.UnloadSceneAsync("MultiplayerLobby", UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
	}

	private void SetActiveSub()
	{
		keysMainMenuButton.SetActive(false);
		ticketsAvailable.gameObject.SetActive(false);
		keyImage.SetActive(false);
		keysCounter.SetActive(false);
	}

	private void UnlockSub(string productId)
	{
		if (productId == "multiplayer_week" || productId == "multiplayer_month" || productId == "multiplayer_keys_unlimited" || productId == "bundle_3")
		{
			SetActiveSub();
		}
		if (productId == "multiplayer_keys" && ticketsAvailable != null)
		{
			ticketsAvailable.text = KeysManager.GetMultiplayerKeys().ToString();
		}
		if ((productId == "charlie" && (int)enemySelected == 1) || (productId == "dog" && (int)enemySelected == 2) || (productId == "witch" && (int)enemySelected == 6))
		{
			isEnemyUnlocked = true;
			UserGameSettings.currentEnemy = enemySelected;
		}
		if ((productId == "eyes.hospital" && (int)mapSelected == 1) || (productId == "school" && (int)mapSelected == 3))
		{
			isMapUnlocked = true;
			UserGameSettings.currentLevel = mapSelected;
		}
		RefreshNetworkTestState();
	}

	public void ShowTicketsPanel()
	{
		MultiplayerKeys.Show((CartType)2, "");
	}

	private void OnError(string error)
	{
		MultiplayerServer.LobbyClosed();
		Back();
		errorLabel.text = error;
		ReplaceGoButtonWithSpinner(false);
		if (error == "User is unauthorized")
		{
			if (UserGameSettings.IsSignedInWithBasicAuth)
			{
				MultiplayerSettings.ClearReviewerBasicAuth();
				RenderAccountStatus(CurrentAuthState);
				ShowLoginPanel();
			}
			else
			{
				RenderAccountStatus(CurrentAuthState);
				MyAuthentication.Show();
			}
		}
		else if (error == "please update the game")
		{
			Close();
		}
	}

	public void SetDifficulty(int difficultySelected)
	{
		UserGameSettings.currentDifficulty = difficultyDict[difficultySelected];
	}

	public void ChooseMap(int map)
	{
		switch (map)
		{
		case 0:
			mapSelected = (Level)0;
			isMapUnlocked = true;
			UserGameSettings.currentLevel = (Level)0;
			break;
		case 1:
			mapSelected = (Level)1;
			isMapUnlocked = RecoveryTestBuild.LevelUnlocked(Level.Hospital) && OnlineConfig.IsSceneInBuild("Hospital");
			if (isMapUnlocked)
			{
				UserGameSettings.currentLevel = (Level)1;
			}
			break;
		case 2:
			mapSelected = (Level)3;
			isMapUnlocked = RecoveryTestBuild.LevelUnlocked(Level.School) && OnlineConfig.IsSceneInBuild("School");
			if (isMapUnlocked)
			{
				UserGameSettings.currentLevel = (Level)3;
			}
			break;
		default:
			return;
		}
		RefreshNetworkTestState();
		Debug.Log("map selected: " + mapSelected);
		Debug.Log("map chosen: " + UserGameSettings.currentLevel);
	}

	public void ChooseEnemy(int enemy)
	{
		switch (enemy)
		{
		case 0:
			enemySelected = (Enemy)0;
			isEnemyUnlocked = true;
			UserGameSettings.currentEnemy = (Enemy)0;
			return;
		case 1:
			enemySelected = (Enemy)1;
			isEnemyUnlocked = RecoveryTestBuild.Unlocked(UserGameSettings.unlockedCharlie);
			break;
		case 2:
			enemySelected = (Enemy)2;
			isEnemyUnlocked = RecoveryTestBuild.Unlocked(UserGameSettings.unlockedDog);
			break;
		case 3:
			enemySelected = (Enemy)6;
			isEnemyUnlocked = RecoveryTestBuild.Unlocked(UserGameSettings.unlockedWitch);
			break;
		case 4:
			enemySelected = (Enemy)7;
			isEnemyUnlocked = true;
			UserGameSettings.currentEnemy = (Enemy)7;
			return;
		default:
			return;
		}
		if (isEnemyUnlocked)
		{
			UserGameSettings.currentEnemy = enemySelected;
		}
		RefreshNetworkTestState();
	}

	public void CheckIfCanPlay()
	{
		RefreshNetworkTestState();
	}

	private void SetButtons(bool canPlay)
	{
		coopButton.interactable = canPlay && MultiplayerNetworkTest.CanPlay;
		vsButton.interactable = canPlay && MultiplayerNetworkTest.CanPlay;
	}

	private void OnDestroy()
	{
		closing = true;
		MultiplayerNetworkTest.Changed -= RefreshNetworkTestState;
		networkTestCts.Cancel();
		networkTestCts.Dispose();
		CancelReviewerLoginAttempt(false);
		RemoveObservers();
	}
}
