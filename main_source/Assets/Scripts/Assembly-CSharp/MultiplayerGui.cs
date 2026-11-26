using System;
using System.Collections;
using System.Text;
using Bibloteka;
using DarkTonic.MasterAudio;
using EyesAnalytics;
using I2.Loc;
using Multiplayer;
using Prime31.MessageKit;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MultiplayerGui : MonoBehaviour
{
	[SerializeField]
	private GameObject objectivesVs;

	[SerializeField]
	private Text result;

	[SerializeField]
	private Text reason;

	[SerializeField]
	private Canvas multiplayerCanvas;

	[SerializeField]
	private Text keysLabel;

	[SerializeField]
	private GameObject getMoreKeysButton;

	[SerializeField]
	private Text rewardedAdKeysLabel;

	[SerializeField]
	private Canvas mainCanvas;

	[SerializeField]
	private Canvas waitingForTheSecondPlayerCanvas;

	[SerializeField]
	private Text waitingForTheSecondPlayerLabel;

	private bool arePlayersReady;

	[SerializeField]
	private GameObject backToMenuButtonWaiting;

	[SerializeField]
	private GameObject kickOther;

	[SerializeField]
	private Button kickOtherButton;

	[SerializeField]
	private Text kickOtherCountdown;

	private bool remotePlayerReady;

	[SerializeField]
	private GameObject playAgainButton;

	[SerializeField]
	private Text playAgainlabel;

	[SerializeField]
	private GameObject summaryWindow;

	private GameplayInputContext.Lease waitingLease;

	private bool waitingVisible;

	private bool kickCountdownActive;

	private int kickCountdownValue = UserGameSettings.kickCountdownInitialValue;

	private Coroutine kickCountdownCoroutine;

	private bool isWaitingOtherPlayer;

	private bool serverDisconnected;

	[SerializeField]
	private Exp exp;

	[SerializeField]
	private ExpTable expTable;

	[SerializeField]
	private WinCoinsSummary coinsSummary;

	private GameplayInputContext.Lease resultsLease;

	private GameplayInputContext.Lease exitLease;

	private GameObject resultsScopeRoot;

	private GameObject summarySelection;

	private bool resultsRequested;

	private bool buildingSummary;

	private bool statsOpen;

	[SerializeField]
	private Text playAgainButtonLabel;

	[SerializeField]
	private GameObject keysPanel;

	[SerializeField]
	private GameObject myStatsPanel;

	[SerializeField]
	private GameObject spinner;

	[SerializeField]
	private Text stats;

	private static string WaitingText => ScriptLocalization.MultiplayerWaitingForAnotherPlayer + " " + UserGameSettings.multiplayerSecondPlayerUsername;

	public void Awake()
	{
		if (!UserGameSettings.IsMultiplayer())
		{
			if (multiplayerCanvas != null)
			{
				multiplayerCanvas.enabled = false;
			}
			return;
		}
		waitingForTheSecondPlayerLabel.text = WaitingText;
		playAgainlabel.text = "";
		summaryWindow.SetActive(true);
		SetWaitingScreenVisible(true);
		StartCoroutine(WaitingForTheSecondPlayerCoroutine());
		MessageKit.post(40);
		CustomSingleton<MultiplayerSettings>.Instance.RegisterOnPlayAgainReady(OnPlayAgainReady);
		MessageKit.addObserver(102, GameStarted);
		MessageKit<string>.addObserver(32, UnlockSub);
		MessageKit<string>.addObserver(34, UnlockSub);
		MessageKit<bool>.addObserver(107, WaitingOtherPlayer);
		MessageKit<bool>.addObserver(104, ServerDisconnected);
	}

	private void ToggleWaitingCanvas(bool show, bool showKickOtherPlayer)
	{
		PlayerManager players = CustomSingleton<PlayerManager>.Instance;
		if (show)
		{
			SetWaitingScreenVisible(true);
			players.playerHandler.SetState("Freeze", true, true, false);
		}
		else
		{
			players.playerHandler.SetState("Freeze", false, true, false);
			SetWaitingScreenVisible(false);
			showKickOtherPlayer = false;
		}
		ToggleKickOtherPlayer(showKickOtherPlayer);
	}

	private bool WaitingScreenShown()
	{
		if (!waitingVisible || !isActiveAndEnabled || QuitConfirmationOpen())
		{
			return false;
		}
		return !MultiplayerKeys.OwnsGameplayInput;
	}

	private static bool QuitConfirmationOpen()
	{
		return CustomSingleton<PauseManager>.HasInstance && CustomSingleton<PauseManager>.Instance.IsQuitConfirmationOpen;
	}

	private void SetWaitingScreenVisible(bool visible)
	{
		if (!visible && waitingVisible && arePlayersReady)
		{
			RequireClickToRecapture();
		}
		mainCanvas.enabled = !visible;
		waitingForTheSecondPlayerCanvas.enabled = visible;
		if (waitingVisible == visible)
		{
			return;
		}
		waitingVisible = visible;
		if (visible)
		{
			if (waitingLease == null || waitingLease.IsDisposed)
			{
				waitingLease = GameplayInputContext.Shared.Register(waitingForTheSecondPlayerCanvas, (GameplayInputContext.Kind)13, WaitingScreenShown, (GameplayInputContext.WindowInput)6);
			}
			GamepadSelectionScope.Begin(waitingForTheSecondPlayerCanvas.gameObject, waitingLease, PreferredWaitingButton);
		}
		else
		{
			GamepadSelectionScope.End(waitingForTheSecondPlayerCanvas.gameObject);
			GameplayInputContext.Shared.Synchronize();
		}
	}

	private static void RequireClickToRecapture()
	{
		if (ControlModeManager.HasInstance)
		{
			ControlModeManager.Instance.RequirePointerResume();
		}
	}

	private Selectable PreferredWaitingButton()
	{
		if (kickOtherButton != null && GamepadSelectionScope.IsValid(kickOtherButton))
		{
			return kickOtherButton;
		}
		return backToMenuButtonWaiting != null ? backToMenuButtonWaiting.GetComponent<Selectable>() : null;
	}

	private IEnumerator KickCountdown()
	{
		kickCountdownActive = true;
		kickOtherButton.interactable = false;
		kickCountdownValue = UserGameSettings.kickCountdownInitialValue;
		while (kickCountdownValue >= 1)
		{
			kickOtherCountdown.text = kickCountdownValue.ToString();
			yield return Utils.GetDelay(1f);
			kickCountdownValue--;
		}
		kickOtherCountdown.text = "";
		kickOtherButton.interactable = true;
	}

	private void ToggleKickOtherPlayer(bool show)
	{
		kickOther.SetActive(show);
		if (!show)
		{
			if (kickCountdownCoroutine != null)
			{
				StopCoroutine(kickCountdownCoroutine);
			}
			kickCountdownActive = false;
		}
		else if (!kickCountdownActive)
		{
			kickCountdownCoroutine = StartCoroutine(KickCountdown());
		}
	}

	private void WaitingOtherPlayer(bool isWaiting)
	{
		isWaitingOtherPlayer = isWaiting;
		if (isWaiting)
		{
			waitingForTheSecondPlayerLabel.text = WaitingText;
			ToggleWaitingCanvas(true, true);
		}
		else if (serverDisconnected)
		{
			waitingForTheSecondPlayerLabel.text = ScriptLocalization.Reconnecting;
			ToggleWaitingCanvas(true, false);
		}
		else
		{
			ToggleWaitingCanvas(false, false);
		}
	}

	private void ServerDisconnected(bool isDisconnected)
	{
		serverDisconnected = isDisconnected;
		if (isDisconnected)
		{
			waitingForTheSecondPlayerLabel.text = ScriptLocalization.Reconnecting;
			ToggleWaitingCanvas(true, false);
		}
		else if (isWaitingOtherPlayer)
		{
			waitingForTheSecondPlayerLabel.text = WaitingText;
			ToggleWaitingCanvas(true, true);
		}
		else
		{
			ToggleWaitingCanvas(false, false);
		}
	}

	private void GameStarted()
	{
		MessageKit.post(39);
		SetWaitingScreenVisible(false);
		arePlayersReady = true;
	}

	private IEnumerator WaitingForTheSecondPlayerCoroutine()
	{
		ToggleKickOtherPlayer(true);
		int counter = 0;
		while (!arePlayersReady)
		{
			yield return Utils.GetDelay(0.5f);
			counter++;
			if (counter >= 31)
			{
				backToMenuButtonWaiting.SetActive(true);
			}
		}
	}

	private void Start()
	{
		objectivesVs.SetActive(UserGameSettings.IsMultiplayerVs());
	}

	private bool ResultsVisible()
	{
		if (!resultsRequested || !isActiveAndEnabled || waitingVisible || QuitConfirmationOpen())
		{
			return false;
		}
		if (buildingSummary)
		{
			return true;
		}
		return multiplayerCanvas != null && multiplayerCanvas.enabled;
	}

	private void BeginResultsInput()
	{
		resultsRequested = true;
		if (resultsLease == null || resultsLease.IsDisposed)
		{
			resultsLease = GameplayInputContext.Shared.Register(this, (GameplayInputContext.Kind)13, ResultsVisible, (GameplayInputContext.WindowInput)6);
			resultsLease.RetainsOrder = true;
		}
		ScopeResults(summaryWindow, PreferredSummaryButton);
	}

	private void OnEnable()
	{
		if (waitingVisible)
		{
			GamepadSelectionScope.Begin(waitingForTheSecondPlayerCanvas.gameObject, waitingLease, PreferredWaitingButton);
		}
		if (resultsRequested && resultsScopeRoot == null)
		{
			if (statsOpen)
			{
				ScopeResults(myStatsPanel, null);
			}
			else
			{
				ScopeResults(summaryWindow, PreferredSummaryButton);
			}
		}
	}

	private void OnDisable()
	{
		if (waitingVisible)
		{
			RequireClickToRecapture();
			GamepadSelectionScope.End(waitingForTheSecondPlayerCanvas.gameObject);
		}
		if (resultsScopeRoot != null)
		{
			GamepadSelectionScope.End(resultsScopeRoot);
		}
		resultsScopeRoot = null;
	}

	private void EndResultsInput()
	{
		resultsRequested = false;
		if (resultsScopeRoot != null)
		{
			GamepadSelectionScope.End(resultsScopeRoot);
		}
		resultsScopeRoot = null;
		resultsLease?.Dispose();
		resultsLease = null;
	}

	private void BeginSceneExit()
	{
		if (exitLease == null || exitLease.IsDisposed)
		{
			exitLease = GameplayInputContext.Shared.Acquire(this, (GameplayInputContext.Kind)11, (GameplayInputContext.WindowInput)0);
		}
	}

	private void ScopeResults(GameObject root, Func<Selectable> preferred)
	{
		if (resultsScopeRoot != null && resultsScopeRoot != root)
		{
			GamepadSelectionScope.End(resultsScopeRoot);
		}
		resultsScopeRoot = root;
		if (root != null && resultsLease != null && !resultsLease.IsDisposed)
		{
			GamepadSelectionScope.Begin(root, resultsLease, preferred);
		}
	}

	private Selectable PreferredSummaryButton()
	{
		Selectable selectable = summarySelection != null ? summarySelection.GetComponent<Selectable>() : null;
		if (selectable != null && GamepadSelectionScope.IsValid(selectable))
		{
			return selectable;
		}
		selectable = playAgainButton != null ? playAgainButton.GetComponent<Selectable>() : null;
		if (selectable != null && GamepadSelectionScope.IsValid(selectable))
		{
			return selectable;
		}
		return BackToMenuButton();
	}

	private Selectable BackToMenuButton()
	{
		if (summaryWindow == null)
		{
			return null;
		}
		foreach (Button button in summaryWindow.GetComponentsInChildren<Button>(true))
		{
			for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
			{
				if (button.onClick.GetPersistentTarget(i) == this && button.onClick.GetPersistentMethodName(i) == "BackToMenu")
				{
					return button;
				}
			}
		}
		return null;
	}

	private void OnDestroy()
	{
		EndResultsInput();
		exitLease?.Dispose();
		exitLease = null;
		waitingLease?.Dispose();
		waitingLease = null;
	}

	public void ShowSummary(bool isWon, bool wasIKilled, bool wasSecondPlayerKilled, bool secondPlayerExitToMenu, bool networkError, bool abuserRemoved)
	{
		BeginResultsInput();
		Screens.View((ScreenId)20);
		buildingSummary = true;
		BuildSummary(isWon, wasIKilled, wasSecondPlayerKilled, secondPlayerExitToMenu, networkError, abuserRemoved);
		buildingSummary = false;
	}

	private void BuildSummary(bool isWon, bool wasIKilled, bool wasSecondPlayerKilled, bool secondPlayerExitToMenu, bool networkError, bool abuserRemoved)
	{
		_ = CustomSingleton<MultiplayerSettings>.Instance.GetStats();
		MessageKit.post(105);
		ServerDisconnected(false);
		WaitingOtherPlayer(false);
		MessageKit.post(64);
		UserGameSettings.lastGameDuration = Time.timeSinceLevelLoad;
		coinsSummary.PrintResult();
		expTable.PrintTable(exp.expCalculator.Calculate());
		result.text = isWon ? ScriptLocalization.MultiplayerWin : ScriptLocalization.MultiplayerGameOver;
		string other = UserGameSettings.multiplayerSecondPlayerUsername;
		string text = null;
		bool hidePlayAgain = false;
		if ((int)UserGameSettings.currentGameMode == 1)
		{
			if (isWon)
			{
				if (abuserRemoved)
				{
					text = other + " " + ScriptLocalization.MultiplayerReasonHasDisconnected;
				}
				else
				{
					text = ScriptLocalization.MultiplayerReasonWinEscape;
					MessageKit.post(132);
				}
			}
			else if (wasIKilled)
			{
				text = ScriptLocalization.MultiplayerReasonCaught;
			}
			else if (wasSecondPlayerKilled)
			{
				text = other + " " + ScriptLocalization.MultiplayerReasonNoRevive;
			}
			else if (secondPlayerExitToMenu)
			{
				text = other + " " + ScriptLocalization.MultiplayerReasonExitToMenu;
				hidePlayAgain = true;
			}
			else if (networkError)
			{
				text = ScriptLocalization.MultiplayerReasonDisconnected;
				hidePlayAgain = true;
			}
			else if (abuserRemoved)
			{
				text = ScriptLocalization.MultiplayerDisconnectedDueToLack;
				hidePlayAgain = true;
			}
			else
			{
				text = "???";
				Debug.Log("koniec coopa, ale czemu?");
			}
		}
		else if ((int)UserGameSettings.currentGameMode == 2)
		{
			if (isWon)
			{
				if (wasSecondPlayerKilled)
				{
					text = other + " " + ScriptLocalization.MultiplayerReasonNoRevive;
				}
				else if (secondPlayerExitToMenu)
				{
					text = other + " " + ScriptLocalization.MultiplayerReasonExitToMenu;
					hidePlayAgain = true;
				}
				else if (networkError || abuserRemoved)
				{
					text = other + " " + ScriptLocalization.MultiplayerReasonHasDisconnected;
					hidePlayAgain = true;
				}
				else
				{
					text = ScriptLocalization.MultiplayerReasonWinEscape;
					MessageKit.post(132);
				}
			}
			else if (wasIKilled)
			{
				text = ScriptLocalization.MultiplayerReasonCaught;
			}
			else if (abuserRemoved)
			{
				text = ScriptLocalization.MultiplayerDisconnectedDueToLack;
				hidePlayAgain = true;
			}
			else if (networkError)
			{
				text = ScriptLocalization.MultiplayerReasonDisconnected;
				hidePlayAgain = true;
			}
			else
			{
				text = other + " " + ScriptLocalization.MultiplayerReasonLostSecondPlayerEscaped;
			}
		}
		if (text != null)
		{
			reason.text = text;
		}
		if (hidePlayAgain)
		{
			playAgainButton.SetActive(false);
		}
		MessageKit.post(40);
		CustomSingleton<PlayerManager>.Instance.playerHandler.SetState("Freeze", true, true, false);
		CustomSingleton<ScreenFader>.Instance.fadeIn = false;
		MasterAudio.MuteGroup("fx", true);
		MasterAudio.PlaySound3DAtTransform("music", CustomSingleton<PlayerManager>.Instance.Player.transform);
		multiplayerCanvas.enabled = true;
		bool showMoreKeys = false;
		if (!UserGameSettings.unlockedMultiplayerSub && !UserGameSettings.unlockedMultiplayerUnlimited)
		{
			keysLabel.text = KeysManager.GetMultiplayerKeys().ToString();
			if (!KeysManager.CanDeductMultiplayerKeys(UserGameSettings.gamePriceInKeys))
			{
				playAgainlabel.text = ScriptLocalization.MultiplayerRunOut;
				playAgainButton.GetComponent<Image>().color = Color.grey;
				playAgainButton.GetComponent<Button>().interactable = false;
				playAgainButtonLabel.color = Color.grey;
			}
			showMoreKeys = true;
		}
		getMoreKeysButton.SetActive(showMoreKeys);
		if (!UserGameSettings.isSignedIn)
		{
			playAgainButton.SetActive(false);
			playAgainlabel.text = "Session expired, sign in again";
		}
	}

	public void SendPlayAgainReady()
	{
		if (!UserGameSettings.unlockedMultiplayerSub && !KeysManager.CanDeductMultiplayerKeys(UserGameSettings.gamePriceInKeys) && !UserGameSettings.unlockedMultiplayerUnlimited)
		{
			playAgainlabel.text = ScriptLocalization.MultiplayerRunOut;
			return;
		}
		MessageKit.addObserver(86, OnBothPlayersReady);
		CustomSingleton<MultiplayerSettings>.Instance.SendPlayAgainReady(true);
		playAgainButton.SetActive(false);
		if (!remotePlayerReady)
		{
			playAgainlabel.text = ScriptLocalization.MultiplayerInvitation + " - " + ScriptLocalization.MultiplayerWaitingLobby;
		}
	}

	public void OnPlayAgainReady(bool isReady)
	{
		if (!isReady)
		{
			playAgainButton.SetActive(false);
			playAgainlabel.text = UserGameSettings.multiplayerSecondPlayerUsername + " " + ScriptLocalization.MultiplayerReasonExitToMenu;
		}
		else
		{
			remotePlayerReady = true;
			playAgainlabel.text = UserGameSettings.multiplayerSecondPlayerUsername + " " + ScriptLocalization.MultiplayerIsWaiting;
		}
	}

	private void OnBothPlayersReady()
	{
		PlayAdAndPlayAgain();
	}

	private void PlayAdAndPlayAgain()
	{
		UserGameSettings.multiplayerPlayAgain = true;
		UserGameSettings.lastGameDuration = Time.timeSinceLevelLoad;
		CustomSingleton<MultiplayerSettings>.Instance.Clear();
		BeginSceneExit();
		Ads.SetNextInterstitialTrigger((AdTrigger)2);
		SceneManager.LoadSceneAsync("PostGameAd");
	}

	public void BackToMenu()
	{
		CustomSingleton<MultiplayerSettings>.Instance.SendPlayAgainReady(false);
		UserGameSettings.lastGameDuration = Time.timeSinceLevelLoad;
		BeginSceneExit();
		Ads.SetNextInterstitialTrigger(AdTrigger.MpRound);
		SceneManager.LoadSceneAsync("PostGameAd");
		CustomSingleton<MultiplayerSettings>.Instance.CloseWebsocket();
	}

	public void ShowMoreKeysWindow()
	{
		MultiplayerKeys.Show((CartType)5, "");
	}

	private void SetActiveSub()
	{
		if (playAgainlabel == null)
		{
			return;
		}
		if (playAgainlabel.text == ScriptLocalization.MultiplayerRunOut)
		{
			playAgainlabel.text = "";
		}
		Color normal = new Color(255f, 221f, 134f, 1f);
		playAgainButton.GetComponent<Image>().color = normal;
		playAgainButtonLabel.color = normal;
		playAgainButton.GetComponent<Button>().interactable = true;
		getMoreKeysButton.SetActive(false);
		keysPanel.SetActive(false);
	}

	private void UnlockSub(string productId)
	{
		if (productId == "multiplayer_week" || productId == "multiplayer_month" || productId == "multiplayer_keys_unlimited" || productId == "bundle_3")
		{
			SetActiveSub();
		}
		if (productId == "multiplayer_keys" && keysLabel != null)
		{
			keysLabel.text = KeysManager.GetMultiplayerKeys().ToString();
			keysPanel.SetActive(false);
		}
	}

	public async void ShowMyStats()
	{
		OpenStatsPanel();
		await CustomSingleton<MultiplayerSettings>.Instance.GetStats();
		StringBuilder builder = new StringBuilder();
		builder.Append("\n\n");
		builder.Append("Last game distance: ");
		builder.Append(UserGameSettings.currentDistance);
		builder.Append("m. \nTotal distance: ");
		builder.Append(UserGameSettings.totalDistance);
		builder.Append("m.");
		if (UserGameSettings.stats != null)
		{
			ShowRecentStats(UserGameSettings.stats, builder.ToString());
		}
		spinner.SetActive(false);
	}

	private void OpenStatsPanel()
	{
		stats.text = "";
		GameObject selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
		if (selected != null && summaryWindow != null && selected.transform.IsChildOf(summaryWindow.transform))
		{
			summarySelection = selected;
		}
		myStatsPanel.SetActive(true);
		statsOpen = true;
		if (resultsLease != null)
		{
			ScopeResults(myStatsPanel, null);
		}
		spinner.SetActive(true);
	}

	private void ShowRecentStats(AllStats _stats, string distance)
	{
		if (_stats != null)
		{
			stats.text = _stats.GetFormattedTable() + distance;
		}
	}

	public void CloseStatsWindow()
	{
		summaryWindow.SetActive(true);
		myStatsPanel.SetActive(false);
		statsOpen = false;
		if (resultsLease != null)
		{
			ScopeResults(summaryWindow, PreferredSummaryButton);
		}
	}
}
