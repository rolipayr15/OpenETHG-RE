using System;
using System.Collections.Generic;
using Eyes.DailyChallenges;
using EyesAnalytics;
using I2.Loc;
using Prime31.MessageKit;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// In-game pause menu and the reference-counted game pause. Pausing the game (freezing the player) is driven by
// pause requests; input blocking is mirrored from GameplayInputContext and broadcast through MessageKit.
[DefaultExecutionOrder(-29900)]
public class PauseManager : CustomSingleton<PauseManager>
{
	private sealed class PauseRequest : IDisposable
	{
		private PauseManager manager;

		public PauseRequest(PauseManager manager)
		{
			this.manager = manager;
		}

		public void Dispose()
		{
			if (manager == null)
			{
				return;
			}
			PauseManager owner = manager;
			manager = null;
			owner.pauseRequests.Remove(this);
			owner.ApplyPauseState();
		}
	}

	// MessageKit ids.
	private const int InputUnblockedMessage = 39;

	private const int InputBlockedMessage = 40;

	private const int PauseMenuOpenedMessage = 69;

	private const int PauseMenuClosedMessage = 77;

	private const int MultiplayerGameFinishedMessage = 105;

	[SerializeField]
	private Canvas pauseMenu;

	[Tooltip("Optional: the button selected first for gamepad/keyboard navigation (default: first valid button).")]
	[SerializeField]
	private Selectable resumeButton;

	private bool isOpen;

	private bool isPaused;

	private bool isQuitConfirmationPending;

	private vp_FPPlayerEventHandler playerHandler;

	private GameplayInputContext policy;

	private GameplayInputContext.Lease pauseLease;

	private GameplayInputContext.Lease confirmationLease;

	private GameplayInputContext.Lease legacyLease;

	private GameplayInputContext.Lease transitionLease;

	private IDisposable menuPause;

	private IDisposable legacyPause;

	private readonly HashSet<PauseRequest> pauseRequests = new HashSet<PauseRequest>();

	private bool inputBlocked;

	private bool applyingPresentation;

	private bool subscribed;

	private bool cleanedUp;

	public bool isOtherWindowOpen;

	public GameplayInputContext.Lease InputLease => pauseLease;

	public bool InputBlocked
	{
		get
		{
			if (!subscribed)
			{
				return false;
			}
			return pauseRequests.Count > 0 || policy.Current.Context != GameplayInputContext.Kind.Gameplay;
		}
	}

	public bool ReportsInputBlocked => subscribed;

	internal static bool IsPostingInputMirror { get; private set; }

	public bool IsQuitConfirmationOpen => IsQuestionBoxActive();

	public static event Action InputBlockedChanged;

	private void Awake()
	{
		policy = GameplayInputContext.Shared;
		_ = CustomSingleton<PauseManager>.Instance;
	}

	private void OnEnable()
	{
		if (policy == null)
		{
			policy = GameplayInputContext.Shared;
		}
		cleanedUp = false;
		if (!subscribed)
		{
			subscribed = true;
			policy.Changed += ApplyPresentation;
			InputBlockedChanged?.Invoke();
		}
	}

	private void Start()
	{
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		playerHandler = CustomSingleton<PlayerManager>.Instance.playerHandler;
		MessageKit.addObserver(MultiplayerGameFinishedMessage, MultiplayerGameFinished);
		UserGameSettings.userQuitToMenu = false;
		if (!UserGameSettings.IsMultiplayer() && playerHandler != null)
		{
			playerHandler.PauseWrapper.Set(isPaused);
		}
		ApplyPauseState();
	}

	private void MultiplayerGameFinished()
	{
		CloseMenu();
	}

	private void Update()
	{
		if (confirmationLease != null && !IsQuestionBoxActive())
		{
			confirmationLease.Dispose();
			confirmationLease = null;
		}
		ControlModeManager controlMode = ControlModeManager.Instance;
		Rewired.Player player = controlMode != null ? controlMode.Player : null;
		bool cancel = false;
		if (player != null && Rewired.ReInput.isReady)
		{
			cancel = player.GetButtonDown(11) || player.GetButtonDown(39) || player.GetButtonDown(48) || player.GetButtonDown(34);
		}
		bool escape = Input.GetKeyDown(KeyCode.Escape);
		bool textEntryCancel = TextEntryInputGuard.CancelDown;
		bool nativeEscape = NativePointerInput.TryReadKey(KeyCode.Escape, true, out bool nativeDown) && nativeDown;
		HandleCancel(cancel || textEntryCancel || escape || nativeEscape);
		if (controlMode != null && controlMode.NeedsPauseMenuFallback && !IsQuestionBoxActive() && !isOpen)
		{
			ShowMenu();
		}
	}

	public void HandleCancel(bool pressed)
	{
		if (!isActiveAndEnabled)
		{
			return;
		}
		switch (policy.TryConsumeCancel(pressed, out GameplayInputContext.Lease owner))
		{
		case GameplayInputContext.CancelResult.CloseOwner:
			if (!owner.TryHandleCancel())
			{
				ControlModeManager controlMode = ControlModeManager.Instance;
				if (controlMode != null && controlMode.OwnsRecoveryLease(owner))
				{
					Toggle();
				}
			}
			break;
		case GameplayInputContext.CancelResult.OpenPause:
			IconPressFeedback.PlayThen(11, ShowMenu);
			break;
		}
	}

	private void LateUpdate()
	{
		ApplyPresentation();
	}

	public void Toggle()
	{
		if (!isOpen)
		{
			ShowMenu();
		}
		else
		{
			HideMenu();
		}
	}

	private void ShowMenu()
	{
		if (!isActiveAndEnabled || isOpen || pauseMenu == null || IsQuestionBoxActive() || policy == null)
		{
			return;
		}
		// Only from gameplay-like contexts (gameplay, vision, map, journal, pause, focus lost).
		GameplayInputContext.Kind context = policy.Current.Context;
		if (context > GameplayInputContext.Kind.FocusLost || ((1 << (int)context) & 0x103D) == 0)
		{
			return;
		}
		pauseLease = policy.Acquire(this, GameplayInputContext.Kind.Pause, GameplayInputContext.WindowInput.PointerOnly, HideMenu);
		menuPause = AcquirePause(this);
		isOpen = true;
		pauseMenu.transform.SetAsLastSibling();
		pauseMenu.enabled = true;
		GamepadSelectionScope.Begin(pauseMenu.gameObject, pauseLease, () => resumeButton);
		Screen.sleepTimeout = SleepTimeout.SystemSetting;
		if (!UserGameSettings.IsMultiplayer())
		{
			MessageKit.post(PauseMenuOpenedMessage);
		}
	}

	public void HideMenu()
	{
		if (!isOpen || IsQuestionBoxActive())
		{
			LogResumeBlocked(isOpen ? "quit confirmation is active" : "pause menu is not open");
			return;
		}
		ControlModeManager controlMode = ControlModeManager.Instance;
		if (controlMode != null)
		{
			controlMode.RefreshApplicationFocus();
		}
		GameplayInputContext.Lease top = policy.Current.TopOwner;
		bool recoveryOnTop = top != pauseLease && controlMode != null && controlMode.OwnsRecoveryLease(top);
		if (top != pauseLease && !recoveryOnTop)
		{
			LogResumeBlocked("another input owner is active");
			return;
		}
		if (controlMode != null && controlMode.RequiresExplicitResume && !controlMode.TryResumeExplicitly())
		{
			LogResumeBlocked("input recovery is not ready");
			// Focus is back but the chosen device is gone: let the player pick another control mode.
			if (controlMode.HasFocus && !controlMode.SelectedSourceAvailable && pauseMenu != null)
			{
				OptionsControlMode options = pauseMenu.GetComponentInChildren<OptionsControlMode>();
				if (options != null)
				{
					options.Open();
				}
			}
			return;
		}
		if (policy.Current.TopOwner == pauseLease)
		{
			CloseMenu();
			return;
		}
		LogResumeBlocked("another input owner remains after recovery");
	}

	private void LogResumeBlocked(string reason)
	{
		ControlModeManager controlMode = ControlModeManager.Instance;
		if (policy == null)
		{
			return;
		}
		GameplayInputContext.Permissions current = policy.Current;
		object owner = current.TopOwner != null ? current.TopOwner.Owner : null;
		Debug.LogWarning("[Input] Resume blocked: " + reason
			+ "; context=" + current.Context
			+ "; owner=" + (owner != null ? owner.GetType().Name : "none")
			+ "; mode=" + (controlMode != null ? controlMode.Mode.ToString() : "none")
			+ "; focus=" + (controlMode != null && controlMode.HasFocus)
			+ "; unityFocus=" + Application.isFocused
			+ "; available=" + (controlMode != null && controlMode.SelectedSourceAvailable)
			+ "; recovery=" + (controlMode != null && controlMode.RequiresExplicitResume), this);
	}

	private void CloseMenu()
	{
		bool wasOpen = isOpen;
		isOpen = false;
		if (pauseMenu != null)
		{
			GamepadSelectionScope.End(pauseMenu.gameObject);
			pauseMenu.enabled = false;
		}
		pauseLease?.Dispose();
		pauseLease = null;
		IDisposable pause = menuPause;
		menuPause = null;
		pause?.Dispose();
		if (!wasOpen || pause == null)
		{
			return;
		}
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		if (!UserGameSettings.IsMultiplayer())
		{
			MessageKit.post(PauseMenuClosedMessage);
		}
	}

	public IDisposable AcquirePause(object owner)
	{
		if (owner == null)
		{
			throw new ArgumentNullException("owner");
		}
		PauseRequest request = new PauseRequest(this);
		pauseRequests.Add(request);
		ApplyPauseState();
		return request;
	}

	// Legacy pause used by older systems (minigames, cut-scenes): blocks gameplay input and pauses the game.
	public void PauseOn()
	{
		if (legacyPause != null)
		{
			return;
		}
		legacyLease = policy.Acquire(this, GameplayInputContext.Kind.GameBlock);
		legacyPause = AcquirePause(this);
	}

	public void PauseOff()
	{
		legacyLease?.Dispose();
		legacyLease = null;
		legacyPause?.Dispose();
		legacyPause = null;
	}

	public void MultiplayerPauseOff()
	{
		PauseOff();
	}

	private void ApplyPauseState()
	{
		bool paused = !UserGameSettings.IsMultiplayer() && pauseRequests.Count > 0;
		if (isPaused != paused)
		{
			isPaused = paused;
			if (playerHandler != null && !UserGameSettings.IsMultiplayer())
			{
				playerHandler.PauseWrapper.Set(paused);
			}
		}
		ApplyPresentation();
	}

	private void ApplyPresentation()
	{
		if (!subscribed || applyingPresentation)
		{
			return;
		}
		applyingPresentation = true;
		try
		{
			bool blocked = pauseRequests.Count > 0 || policy.Current.Context != GameplayInputContext.Kind.Gameplay;
			if (inputBlocked != blocked)
			{
				inputBlocked = blocked;
				IsPostingInputMirror = true;
				try
				{
					MessageKit.post(blocked ? InputBlockedMessage : InputUnblockedMessage);
				}
				finally
				{
					IsPostingInputMirror = false;
				}
				InputBlockedChanged?.Invoke();
			}
			if (CustomSingleton<MouseManager>.HasInstance)
			{
				CustomSingleton<MouseManager>.Instance.Synchronize();
			}
		}
		finally
		{
			applyingPresentation = false;
		}
	}

	public bool IsPaused()
	{
		return isPaused;
	}

	public bool IsPauseMenuOpen()
	{
		return isOpen;
	}

	private void OnDisable()
	{
		Cleanup();
	}

	private void Cleanup()
	{
		if (cleanedUp)
		{
			return;
		}
		cleanedUp = true;
		if (subscribed)
		{
			policy.Changed -= ApplyPresentation;
			subscribed = false;
		}
		CloseMenu();
		confirmationLease?.Dispose();
		confirmationLease = null;
		transitionLease?.Dispose();
		transitionLease = null;
		PauseOff();
		foreach (PauseRequest request in new List<PauseRequest>(pauseRequests))
		{
			request.Dispose();
		}
		if (inputBlocked && pauseRequests.Count == 0 && policy.Current.Context == GameplayInputContext.Kind.Gameplay)
		{
			inputBlocked = false;
			MessageKit.post(InputUnblockedMessage);
		}
		isQuitConfirmationPending = false;
		InputBlockedChanged?.Invoke();
		if (CustomSingleton<MouseManager>.HasInstance)
		{
			CustomSingleton<MouseManager>.Instance.SuspendCapture();
		}
	}

	public void QuitToMenu()
	{
		if (IsQuestionBoxActive())
		{
			return;
		}
		isQuitConfirmationPending = true;
		SetQuitConfirmationInputBlocked(true);
		string message = GetLocalizedText("QuitConfirmation", "Do you really want to quit?");
		string yes = GetLocalizedText("MainMenu", "Main menu");
		string no = GetLocalizedText("Cancel", "Cancel");
		bool shown = QuestionBox.TryShow(message, yes, no, delegate
		{
			FinishQuitConfirmation();
			QuitToMenuConfirmed();
		}, CancelQuitConfirmation, Color.white, false, false, GameplayInputContext.Kind.DeathWindow, true);
		if (!shown)
		{
			isQuitConfirmationPending = false;
			SetQuitConfirmationInputBlocked(false);
			Debug.LogWarning("Quit to menu was cancelled because its confirmation dialog could not be shown.");
		}
	}

	private void FinishQuitConfirmation()
	{
		isQuitConfirmationPending = false;
		SetQuitConfirmationInputBlocked(false);
	}

	private void CancelQuitConfirmation()
	{
		isQuitConfirmationPending = false;
		SetQuitConfirmationInputBlocked(false);
	}

	// The confirmation lease stays registered until the question box is gone (disposed in Update).
	private void SetQuitConfirmationInputBlocked(bool blocked)
	{
		if (blocked && confirmationLease == null)
		{
			confirmationLease = policy.Register(this, GameplayInputContext.Kind.DeathWindow, () => isQuitConfirmationPending || QuestionBox.IsSceneLoaded);
		}
		policy?.Synchronize();
	}

	private bool IsQuestionBoxActive()
	{
		return isQuitConfirmationPending || QuestionBox.IsSceneLoaded;
	}

	private static string GetLocalizedText(string term, string fallback)
	{
		string translation = LocalizationManager.GetTranslation(term);
		return string.IsNullOrEmpty(translation) ? fallback : translation;
	}

	private void QuitToMenuConfirmed()
	{
		PushNotificationPermissionPresenter.CancelGame(this);
		DailyChallengeRuntime.Finish(this, "quit_to_menu");
		UserGameSettings.userQuitToMenu = true;
		UserGameSettings.lastGameDuration = Time.timeSinceLevelLoad;
		CustomEvents.FirebaseEventLevelFinished(UserGameSettings.currentLevel, UserGameSettings.currentEnemy, UserGameSettings.currentMode, UserGameSettings.currentDifficulty, GameFinishedResult.EXIT_TO_MENU, UserGameSettings.expertMode, UserGameSettings.bagsCollected, UserGameSettings.eyesCollected);
		if (UserGameSettings.IsMultiplayer())
		{
			CustomSingleton<MultiplayerSettings>.Instance.SendFinishGame(GameFinishedReason.EXIT_TO_MENU);
		}
		if (transitionLease == null && policy != null)
		{
			transitionLease = policy.Acquire(this, GameplayInputContext.Kind.Transition);
		}
		CloseMenu();
		PauseOff();
		UserGameSettings.isOneTimeUnlocked = false;
		Ads.SetNextInterstitialTrigger(AdTrigger.Quit);
		SceneManager.LoadScene("PostGameAd");
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (!hasFocus)
		{
			OnApplicationInterrupted();
		}
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		if (pauseStatus)
		{
			OnApplicationInterrupted();
		}
	}

	// Losing focus / backgrounding opens the pause menu in single player.
	private void OnApplicationInterrupted()
	{
		if (isPaused || UserGameSettings.IsMultiplayer())
		{
			return;
		}
		ControlModeManager controlMode = ControlModeManager.Instance;
		if (controlMode != null && !controlMode.AllowsAutomaticPauseMenu)
		{
			return;
		}
		ShowMenu();
	}

	protected override void OnDestroy()
	{
		Cleanup();
		MessageKit.removeObserver(MultiplayerGameFinishedMessage, MultiplayerGameFinished);
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}
