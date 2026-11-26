using System;
using System.Collections.Generic;
using System.Diagnostics;
using Rewired;
using UnityEngine;
using UnityEngine.SceneManagement;

// Owns the selected control mode (touch / keyboard & mouse / gamepad) and the input sources that feed Player0.
// When the selected source disappears, focus is lost or an ad interrupts play, gameplay is held behind a
// recovery lease until the player explicitly resumes (or an automatic resume condition is met).
[DefaultExecutionOrder(-32000)]
public sealed class ControlModeManager : MonoBehaviour
{
	public enum DevicePresence
	{
		Unknown = 0,
		Connected = 1,
		Disconnected = 2
	}

	public enum RecoveryStatus
	{
		None = 0,
		Unavailable = 1,
		ResumeRequired = 2
	}

	// A Rewired custom controller fed by our own code (on-screen touch controls or a native KB&M backend).
	public sealed class SourceRegistration : IDisposable
	{
		internal ControlModeManager Manager;

		internal Action Reset;

		internal int ReportedFrame = -1;

		public CustomController Controller { get; private set; }

		public ControlMode Mode { get; private set; }

		public string BackendId { get; private set; }

		public bool ReplacesKeyboard { get; private set; }

		public bool ReplacesMouse { get; private set; }

		public bool Available { get; private set; }

		public bool IsRegistered => Manager != null;

		public DevicePresence Keyboard { get; private set; }

		public DevicePresence Mouse { get; private set; }

		internal SourceRegistration(ControlModeManager manager, CustomController controller, ControlMode mode, string backendId, bool keyboard, bool mouse, bool available, Action reset)
		{
			Manager = manager;
			Controller = controller;
			Mode = mode;
			BackendId = backendId;
			Reset = reset;
			ReplacesKeyboard = keyboard;
			ReplacesMouse = mouse;
			Available = available;
		}

		public void ReportState(bool available, DevicePresence keyboard, DevicePresence mouse)
		{
			if (Manager == null)
			{
				return;
			}
			ReportedFrame = Time.frameCount;
			bool wasAvailable = Available;
			if (wasAvailable == available && Keyboard == keyboard && Mouse == mouse)
			{
				return;
			}
			bool deviceLost = (ReplacesKeyboard && keyboard == DevicePresence.Disconnected && Keyboard != DevicePresence.Disconnected) || (ReplacesMouse && mouse == DevicePresence.Disconnected && Mouse != DevicePresence.Disconnected);
			ControlModeManager manager = Manager;
			Keyboard = keyboard;
			Mouse = mouse;
			Available = available;
			bool selectedLost;
			if (deviceLost)
			{
				manager.RetireKeyboardMouseAdResume("deviceLost");
				selectedLost = manager.Mode == Mode;
			}
			else
			{
				selectedLost = wasAvailable && !available && manager.Mode == Mode;
			}
			manager.SourceChanged(selectedLost, !deviceLost);
		}

		public void Dispose()
		{
			if (Manager == null)
			{
				return;
			}
			ControlModeManager manager = Manager;
			Manager = null;
			ResetValues();
			manager.sources.Remove(this);
			if (manager.nativeKeyboardMouse == this)
			{
				manager.nativeKeyboardMouse = null;
			}
			manager.SourceChanged(manager.Mode == Mode);
		}

		// Clears every axis and button of the controller (and lets the owner clear its own state).
		internal void ResetValues()
		{
			Reset?.Invoke();
			if (!ReInput.isReady || Controller == null)
			{
				return;
			}
			for (int i = 0; i < Controller.axisCount; i++)
			{
				Controller.ClearAxisValue(i);
			}
			for (int i = 0; i < Controller.buttonCount; i++)
			{
				Controller.ClearButtonValue(i);
			}
		}
	}

	private const string PlayerName = "Player0";

	private const string TouchBackendId = "RewiredTouch";

	private static ControlModeManager instance;

	private GameplayInputContext policy;

	private Player player;

	private readonly List<SourceRegistration> sources = new List<SourceRegistration>();

	private readonly HashSet<CustomController> ownedTouch = new HashSet<CustomController>();

	private SourceRegistration nativeKeyboardMouse;

	private GameplayInputContext.Lease focusLease;

	private GameplayInputContext.Lease recoveryLease;

	private GameplayInputContext.Lease legacyLease;

	private bool focused;

	private bool applicationPaused;

	private bool applying;

	private bool updatingLeases;

	private bool resettingSources;

	private bool physicalBackendAvailable;

	private bool subscribed;

	private bool pendingLevelStart;

	private const float LevelStartSourceTimeout = 5f;

	private float levelStartSourceWait;

	private float levelStartSourceClock;

	private bool levelStartSourceClockValid;

	private int adResumeSession;

	private bool adEnded;

	private bool adResumeKeyboardMouse;

	private const float AdResumeSourceTimeout = 3f;

	private int adEndedFrame;

	private int physicalReportedFrame = -1;

	private float adResumeWait;

	private float adResumeClock;

	private bool adResumeClockValid;

	private string physicalBackendId = "UnityRewired";

	private static Func<bool> nativeApplicationActive = NativeApplicationActive;

	private int stalePauseFrames;

	public static bool HasInstance => instance != null;

	public static ControlModeManager Instance
	{
		get
		{
			if (instance == null && Application.isPlaying)
			{
				new GameObject("Local Control Mode").AddComponent<ControlModeManager>();
			}
			return instance;
		}
	}

	public ControlMode Mode { get; private set; }

	public bool KeepTouchMenuAccess => Application.isMobilePlatform;

	public bool RequiresExplicitResume { get; private set; }

	public bool HasFocus => focused && !applicationPaused;

	public bool RequiresRecoveryUI => RequiresExplicitResume || !SelectedSourceAvailable;

	public bool AutomaticResumePending => adResumeSession != 0 || pendingLevelStart;

	public bool SuppressesAutomaticPauseMenu
	{
		get
		{
			if (Mode == ControlMode.KeyboardMouse && !adResumeKeyboardMouse)
			{
				return false;
			}
			return FullscreenAdSessions.IsShowing || adResumeSession != 0 || pendingLevelStart;
		}
	}

	public bool AllowsAutomaticPauseMenu
	{
		get
		{
			if (SuppressesAutomaticPauseMenu)
			{
				return false;
			}
			return policy.IsGameplayExcept((GameplayInputContext.Lease lease) => lease == focusLease || lease == recoveryLease || lease.Context == GameplayInputContext.Kind.Vision);
		}
	}

	// Touch/gamepad (or KB&M without a usable source) has no pointer gesture to resume: show the pause menu.
	public bool NeedsPauseMenuFallback
	{
		get
		{
			if (Mode == ControlMode.KeyboardMouse && SelectedSourceAvailable)
			{
				return false;
			}
			if (!focused || applicationPaused || !RequiresExplicitResume || FullscreenAdSessions.IsShowing)
			{
				return false;
			}
			return AllowsAutomaticPauseMenu;
		}
	}

	public RecoveryStatus Status
	{
		get
		{
			if (Mode == ControlMode.Touch && policy.BaseContext != GameplayInputContext.Kind.Gameplay)
			{
				return RecoveryStatus.None;
			}
			if (!SelectedSourceAvailable)
			{
				return RecoveryStatus.Unavailable;
			}
			return RequiresExplicitResume ? RecoveryStatus.ResumeRequired : RecoveryStatus.None;
		}
	}

	public bool CanRequestMouseCapture
	{
		get
		{
			GameplayInputContext.Kind context = policy.Current.Context;
			if (Mode != ControlMode.KeyboardMouse || !focused || applicationPaused || RequiresExplicitResume || !SelectedSourceAvailable)
			{
				return false;
			}
			return context == GameplayInputContext.Kind.Gameplay || context == GameplayInputContext.Kind.Vision || context == GameplayInputContext.Kind.Map;
		}
	}

	public float MouseSensitivity => UserGameSettings.mouseSensitivity;

	public bool MouseInvertY => UserGameSettings.mouseInvertY;

	public Player Player => player;

	public string KeyboardMouseBackendId => nativeKeyboardMouse != null ? nativeKeyboardMouse.BackendId : physicalBackendId;

	public DevicePresence PhysicalKeyboard { get; private set; }

	public DevicePresence PhysicalMouse { get; private set; }

	public bool SelectedSourceAvailable
	{
		get
		{
			if (!ReInput.isReady || player == null)
			{
				return false;
			}
			switch (Mode)
			{
			case ControlMode.Gamepad:
				foreach (Joystick joystick in player.controllers.Joysticks)
				{
					if (joystick.isConnected)
					{
						return true;
					}
				}
				return false;
			case ControlMode.Touch:
				foreach (SourceRegistration source in sources)
				{
					if (source.Mode == ControlMode.Touch && source.Available && IsAssigned(source.Controller))
					{
						return true;
					}
				}
				return false;
			default:
			{
				SourceRegistration native = nativeKeyboardMouse;
				bool keyboard;
				bool mouse;
				if (native != null && native.ReplacesKeyboard)
				{
					keyboard = native.Available && native.Keyboard != DevicePresence.Disconnected && IsAssigned(native.Controller);
				}
				else
				{
					keyboard = PhysicalKeyboard != DevicePresence.Disconnected && player.controllers.Keyboard != null;
				}
				if (native != null && native.ReplacesMouse)
				{
					mouse = native.Available && native.Mouse != DevicePresence.Disconnected && IsAssigned(native.Controller);
				}
				else
				{
					mouse = physicalBackendAvailable && PhysicalMouse != DevicePresence.Disconnected && player.controllers.Mouse != null;
				}
				return keyboard && mouse;
			}
			}
		}
	}

	internal int ResumeCount { get; private set; }

	internal bool RawFocused => focused;

	internal bool RawApplicationPaused => applicationPaused;

	internal string AutomaticResumeReason
	{
		get
		{
			string ad = null;
			if (adResumeSession != 0)
			{
				if (!adEnded)
				{
					ad = "adShowing";
				}
				else if (adResumeKeyboardMouse && focused && !applicationPaused)
				{
					ad = "adClosedAwaitingSource";
				}
				else
				{
					ad = "adClosedAwaitingFocus";
				}
			}
			if (!pendingLevelStart)
			{
				return ad ?? "none";
			}
			return ad != null ? "levelStart+" + ad : "levelStart";
		}
	}

	internal string NativeActiveForDiagnostics => "notIOS";

	public event Action<ControlMode> ModeChanged;

	public event Action AvailabilityChanged;

	public event Action MouseSettingsChanged;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void ResetStatic()
	{
		instance = null;
		nativeApplicationActive = NativeApplicationActive;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Bootstrap()
	{
		_ = Instance;
	}

	public static ControlMode DefaultMode(bool mobile)
	{
		return mobile ? ControlMode.Touch : ControlMode.KeyboardMouse;
	}

	public static bool IsValidMode(ControlMode mode)
	{
		return (uint)mode < 3u;
	}

	public static bool OffersTouchMode(bool mobile, bool editor)
	{
		return mobile || editor;
	}

	public static bool OffersTouchMode()
	{
		return Application.isMobilePlatform || Application.isEditor;
	}

	// Touch is only offered on mobile (and in the editor); elsewhere a saved touch mode becomes keyboard & mouse.
	public static ControlMode CoerceForPlatform(ControlMode persisted, bool mobile, bool editor)
	{
		if (persisted == ControlMode.Touch && !mobile && !editor)
		{
			return ControlMode.KeyboardMouse;
		}
		return persisted;
	}

	public bool OwnsRecoveryLease(GameplayInputContext.Lease lease)
	{
		return recoveryLease != null && recoveryLease == lease;
	}

	public bool OwnsInterruptionLease(GameplayInputContext.Lease lease)
	{
		return lease != null && (focusLease == lease || recoveryLease == lease);
	}

	private void Awake()
	{
		if (instance != null && instance != this)
		{
			Destroy(gameObject);
			return;
		}
		instance = this;
		DontDestroyOnLoad(gameObject);
		policy = GameplayInputContext.Shared;
		Mode = CoerceForPlatform(UserGameSettings.controlMode, Application.isMobilePlatform, Application.isEditor);
		if (Mode != UserGameSettings.controlMode)
		{
			UserGameSettings.controlMode = Mode;
		}
		policy.SetControlMode(Mode);
		focused = Application.isFocused;
		physicalBackendAvailable = !Application.isMobilePlatform;
		if (GetComponent<TextEntryInputGuard>() == null)
		{
			gameObject.AddComponent<TextEntryInputGuard>();
		}
		if (GetComponent<TouchInputRecovery>() == null)
		{
			gameObject.AddComponent<TouchInputRecovery>();
		}
	}

	private void OnEnable()
	{
		if (instance != this || subscribed)
		{
			return;
		}
		subscribed = true;
		policy.Changed += Synchronize;
		policy.InputResetRequested += ResetSources;
		ReInput.InitializedEvent += OnRewiredInitialized;
		SceneManager.sceneLoaded += OnSceneLoaded;
		SceneManager.activeSceneChanged += OnActiveSceneChanged;
		FullscreenAdSessions.Started += OnAdStarted;
		FullscreenAdSessions.Ended += OnAdEnded;
		if (ReInput.isReady)
		{
			OnRewiredInitialized();
		}
		UpdateFocusLease();
		UpdateRecoveryLease();
	}

	private void OnDisable()
	{
		if (!subscribed)
		{
			return;
		}
		pendingLevelStart = false;
		adResumeSession = 0;
		adEnded = false;
		adResumeKeyboardMouse = false;
		subscribed = false;
		FullscreenAdSessions.Started -= OnAdStarted;
		FullscreenAdSessions.Ended -= OnAdEnded;
		policy.Changed -= Synchronize;
		ReInput.InitializedEvent -= OnRewiredInitialized;
		SceneManager.sceneLoaded -= OnSceneLoaded;
		SceneManager.activeSceneChanged -= OnActiveSceneChanged;
		UnbindRewired();
		RequiresExplicitResume = true;
		policy.ResetInput(GameplayInputContext.ResetReason.SceneChanged);
		policy.InputResetRequested -= ResetSources;
		focusLease?.Dispose();
		focusLease = null;
		recoveryLease?.Dispose();
		recoveryLease = null;
		legacyLease?.Dispose();
		legacyLease = null;
		DropSources();
	}

	private void OnDestroy()
	{
		if (instance == this)
		{
			instance = null;
		}
	}

	private void OnRewiredInitialized()
	{
		UnbindRewired();
		Player first = ReInput.players.GetPlayer(0);
		player = first != null && first.name == PlayerName && ReInput.players.GetPlayer(PlayerName) == first ? first : null;
		ReInput.InputSourceUpdateEvent += Synchronize;
		ReInput.ControllerPreDisconnectEvent += OnPreDisconnect;
		ReInput.ControllerDisconnectedEvent += OnDisconnected;
		ReInput.ControllerConnectedEvent += OnConnected;
		ReInput.PreShutDownEvent += OnRewiredShutdown;
		DiscoverLegacyTouch();
		policy.ResetInput(GameplayInputContext.ResetReason.SceneChanged);
		Synchronize();
	}

	private void UnbindRewired()
	{
		ReInput.InputSourceUpdateEvent -= Synchronize;
		ReInput.ControllerPreDisconnectEvent -= OnPreDisconnect;
		ReInput.ControllerDisconnectedEvent -= OnDisconnected;
		ReInput.ControllerConnectedEvent -= OnConnected;
		ReInput.PreShutDownEvent -= OnRewiredShutdown;
	}

	private void OnRewiredShutdown()
	{
		RequireResume(GameplayInputContext.ResetReason.DeviceDisconnected);
		UnbindRewired();
		DropSources();
	}

	private void DropSources()
	{
		foreach (SourceRegistration source in sources)
		{
			source.Manager = null;
		}
		sources.Clear();
		ownedTouch.Clear();
		nativeKeyboardMouse = null;
		player = null;
	}

	private void Update()
	{
		ExpireLevelStartWithoutTouchSource();
		Synchronize();
	}

	// After an ad closes in KB&M mode we wait (focused) up to 3 s for the keyboard/mouse source to report back.
	private bool ExpireKeyboardMouseAdResume()
	{
		if (adResumeSession == 0 || !adResumeKeyboardMouse || !adEnded || !focused || applicationPaused)
		{
			adResumeClockValid = false;
			return false;
		}
		float now = Time.realtimeSinceStartup;
		if (adResumeClockValid)
		{
			adResumeWait += now - adResumeClock;
		}
		adResumeClock = now;
		adResumeClockValid = true;
		if (adResumeWait < AdResumeSourceTimeout)
		{
			return false;
		}
		adResumeSession = 0;
		adEnded = false;
		adResumeKeyboardMouse = false;
		return true;
	}

	private bool KeyboardMouseReportedAfter(int frame)
	{
		SourceRegistration native = nativeKeyboardMouse;
		bool nativeMouse = native != null && native.ReplacesKeyboard && native.ReplacesMouse;
		int physicalFrame = physicalReportedFrame < 0 ? Time.frameCount : physicalReportedFrame;
		if (native != null && frame >= native.ReportedFrame)
		{
			return false;
		}
		return nativeMouse || frame < physicalFrame;
	}

	// A level started in touch mode without any touch source: after 5 s give up and ask for an explicit resume.
	private void ExpireLevelStartWithoutTouchSource()
	{
		if (!pendingLevelStart || Mode != ControlMode.Touch || !focused || applicationPaused || SelectedSourceAvailable)
		{
			levelStartSourceClockValid = false;
			return;
		}
		float now = Time.realtimeSinceStartup;
		if (levelStartSourceClockValid)
		{
			levelStartSourceWait += now - levelStartSourceClock;
		}
		levelStartSourceClock = now;
		levelStartSourceClockValid = true;
		if (levelStartSourceWait < LevelStartSourceTimeout)
		{
			return;
		}
		CancelLevelStart("touchSourceTimeout");
		if (!RequiresExplicitResume)
		{
			RequireResume(GameplayInputContext.ResetReason.DeviceDisconnected);
		}
	}

	// Native application state (iOS only); other platforms report inactive.
	private static bool NativeApplicationActive()
	{
		return false;
	}

	// Recovers from a missed OnApplicationPause(false): focused, no ad, native side active for two frames.
	private void RepairStaleApplicationPause()
	{
		if (applicationPaused && Application.isFocused && !FullscreenAdSessions.IsShowing && nativeApplicationActive())
		{
			if (++stalePauseFrames < 2)
			{
				return;
			}
			stalePauseFrames = 0;
			ReportFocus(true, false);
			return;
		}
		stalePauseFrames = 0;
	}

	private bool IsAssigned(Controller controller)
	{
		if (controller == null || player == null)
		{
			return false;
		}
		foreach (Controller assigned in player.controllers.Controllers)
		{
			if (assigned == controller)
			{
				return true;
			}
		}
		return false;
	}

	private SourceRegistration FindSource(Controller controller)
	{
		foreach (SourceRegistration source in sources)
		{
			if (source.Controller == controller)
			{
				return source;
			}
		}
		return null;
	}

	// Touch controllers created by older scene setups that never registered themselves.
	private void DiscoverLegacyTouch()
	{
		if (!ReInput.isReady || player == null)
		{
			return;
		}
		foreach (CustomController controller in ReInput.controllers.CustomControllersWithSourceId(0))
		{
			if (IsAssigned(controller) && !ownedTouch.Contains(controller) && FindSource(controller) == null)
			{
				sources.Add(new SourceRegistration(this, controller, ControlMode.Touch, TouchBackendId, false, false, true, null));
			}
		}
	}

	public SourceRegistration RegisterTouchController(CustomController controller, Action reset)
	{
		if (!IsAssigned(controller) || controller.sourceControllerId != 0)
		{
			throw new ArgumentException("Expected Player0's MobileUFPSControler instance.", "controller");
		}
		if (!ownedTouch.Add(controller) && FindSource(controller) != null)
		{
			throw new InvalidOperationException("Touch instance already has an owner.");
		}
		SourceRegistration source = FindSource(controller);
		if (source == null)
		{
			source = new SourceRegistration(this, controller, ControlMode.Touch, TouchBackendId, false, false, true, reset);
			sources.Add(source);
		}
		else
		{
			source.Reset = reset;
		}
		SourceChanged(false);
		return source;
	}

	public SourceRegistration RegisterNativeKeyboardMouse(CustomController controller, string backendId, bool replacesKeyboard, bool replacesMouse, bool available, Action reset)
	{
		if (!IsAssigned(controller))
		{
			throw new ArgumentException("Assign the controller to Player0 first.", "controller");
		}
		if (controller.sourceControllerId == 0)
		{
			throw new ArgumentException("Native KBM requires a separate definition from touch.", "controller");
		}
		if (string.IsNullOrEmpty(backendId))
		{
			throw new ArgumentException("A backend identity is required.", "backendId");
		}
		if (!replacesKeyboard && !replacesMouse)
		{
			throw new ArgumentException("Declare the replaced input source.");
		}
		if (nativeKeyboardMouse != null || FindSource(controller) != null)
		{
			throw new InvalidOperationException("Controller already registered, or another native KBM backend owns the source.");
		}
		SourceRegistration source = new SourceRegistration(this, controller, ControlMode.KeyboardMouse, backendId, replacesKeyboard, replacesMouse, available, reset);
		sources.Add(source);
		nativeKeyboardMouse = source;
		SourceChanged(false);
		return source;
	}

	// State of the physical keyboard & mouse as seen by the platform backend.
	public void ReportPhysicalKeyboardMouse(string backendId, bool relativeMouseAvailable, DevicePresence keyboard, DevicePresence mouse)
	{
		if (string.IsNullOrEmpty(backendId))
		{
			throw new ArgumentException("A backend identity is required.", "backendId");
		}
		physicalReportedFrame = Time.frameCount;
		if (physicalBackendId == backendId && physicalBackendAvailable == relativeMouseAvailable && PhysicalKeyboard == keyboard && PhysicalMouse == mouse)
		{
			return;
		}
		SourceRegistration native = nativeKeyboardMouse;
		bool mouseReplaced = native != null && native.ReplacesMouse;
		bool keyboardLost = (native == null || !native.ReplacesKeyboard) && keyboard == DevicePresence.Disconnected && PhysicalKeyboard != DevicePresence.Disconnected;
		bool mouseLost = !mouseReplaced && !keyboardLost && mouse == DevicePresence.Disconnected && PhysicalMouse != DevicePresence.Disconnected;
		bool lost = keyboardLost || mouseLost;
		// Losing relative mouse input needs a resume, but keeps a pending automatic resume after an ad.
		bool relativeMouseLost = !mouseReplaced && !lost && physicalBackendAvailable && !relativeMouseAvailable;
		physicalBackendId = backendId;
		physicalBackendAvailable = relativeMouseAvailable;
		PhysicalKeyboard = keyboard;
		PhysicalMouse = mouse;
		if (lost && adResumeKeyboardMouse)
		{
			adResumeSession = 0;
			adEnded = false;
			adResumeKeyboardMouse = false;
		}
		SourceChanged((lost || relativeMouseLost) && Mode == ControlMode.KeyboardMouse, !lost);
	}

	public void SetMode(ControlMode mode)
	{
		if (!IsValidMode(mode))
		{
			throw new ArgumentOutOfRangeException("mode");
		}
		if (Mode == mode)
		{
			if (RequiresExplicitResume)
			{
				TryResumeExplicitly();
			}
			return;
		}
		pendingLevelStart = false;
		adEnded = false;
		adResumeKeyboardMouse = false;
		adResumeSession = 0;
		Mode = mode;
		UserGameSettings.controlMode = mode;
		OptionsSensitivity.ApplySavedSensitivity();
		RequiresExplicitResume = !focused || applicationPaused || !SelectedSourceAvailable;
		UpdateRecoveryLease();
		policy.SetControlMode(mode);
		Synchronize();
		ModeChanged?.Invoke(mode);
		AvailabilityChanged?.Invoke();
	}

	public void SetMouseSettings(float sensitivity, bool invertY)
	{
		UserGameSettings.mouseSensitivity = sensitivity;
		UserGameSettings.mouseInvertY = invertY;
		OptionsSensitivity.ApplySavedSensitivity();
		policy.ResetInput(GameplayInputContext.ResetReason.ControlModeChanged);
		MouseSettingsChanged?.Invoke();
	}

	public bool TryResumeExplicitly()
	{
		if (!focused || applicationPaused || !SelectedSourceAvailable)
		{
			return false;
		}
		pendingLevelStart = false;
		adResumeSession = 0;
		adEnded = false;
		adResumeKeyboardMouse = false;
		RequiresExplicitResume = false;
		ResumeCount++;
		UpdateRecoveryLease();
		policy.ResetInput(GameplayInputContext.ResetReason.CaptureChanged);
		AvailabilityChanged?.Invoke();
		return true;
	}

	public void RequirePointerResume()
	{
		RetireKeyboardMouseAdResume("pointer");
		if (Mode == ControlMode.KeyboardMouse && !RequiresExplicitResume)
		{
			RequireResume(GameplayInputContext.ResetReason.CaptureChanged);
		}
	}

	public void RequirePointerResumeKeepingAdResume()
	{
		if (Mode == ControlMode.KeyboardMouse && !RequiresExplicitResume)
		{
			RequireResume(GameplayInputContext.ResetReason.CaptureChanged, true);
		}
	}

	public void RetireKeyboardMouseAdResume(string reason)
	{
		if (adResumeKeyboardMouse)
		{
			CancelAutomaticResume(reason);
		}
	}

	private void RequireResume(GameplayInputContext.ResetReason reason, bool keepKeyboardMouseAdResume = false)
	{
		CancelLevelStart(reason.ToString());
		if (!keepKeyboardMouseAdResume || !adResumeKeyboardMouse)
		{
			CancelAutomaticResume(reason.ToString());
		}
		RequiresExplicitResume = true;
		UpdateRecoveryLease();
		policy.ResetInput(reason);
		AvailabilityChanged?.Invoke();
	}

	private void SourceChanged(bool lost, bool keepKeyboardMouseAdResume = false)
	{
		if (lost)
		{
			RequireResume(GameplayInputContext.ResetReason.DeviceDisconnected, keepKeyboardMouseAdResume);
			Synchronize();
			return;
		}
		policy.ResetInput(GameplayInputContext.ResetReason.ControlModeChanged);
		Synchronize();
		AvailabilityChanged?.Invoke();
	}

	private void OnPreDisconnect(ControllerStatusChangedEventArgs args)
	{
		if (player == null)
		{
			return;
		}
		foreach (Controller controller in player.controllers.Controllers)
		{
			if (controller.id == args.controllerId && controller.type == args.controllerType && IsSelectedController(controller))
			{
				RequireResume(GameplayInputContext.ResetReason.DeviceDisconnected);
				return;
			}
		}
	}

	private void OnDisconnected(ControllerStatusChangedEventArgs args)
	{
		Synchronize();
		AvailabilityChanged?.Invoke();
	}

	private void OnConnected(ControllerStatusChangedEventArgs args)
	{
		SourceChanged(false);
	}

	public void ReportFocus(bool hasFocus, bool paused)
	{
		if (focused == hasFocus && applicationPaused == paused)
		{
			return;
		}
		levelStartSourceClockValid = false;
		adResumeClockValid = false;
		TouchInputRecovery recovery = GetComponent<TouchInputRecovery>();
		if (recovery != null)
		{
			recovery.CancelGesture();
		}
		focused = hasFocus;
		applicationPaused = paused;
		if (paused || !hasFocus)
		{
			RequiresExplicitResume = true;
		}
		UpdateFocusLease();
		UpdateRecoveryLease();
		policy.ResetInput(GameplayInputContext.ResetReason.FocusChanged);
		AvailabilityChanged?.Invoke();
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("EYES_INPUT_DIAGNOSTICS")]
	public static void TraceInputState(string phase, string detail = null)
	{
	}

	private void OnApplicationFocus(bool value)
	{
		stalePauseFrames = 0;
		ReportFocus(value, applicationPaused);
	}

	private void OnApplicationPause(bool value)
	{
		stalePauseFrames = 0;
		ReportFocus(focused, value);
	}

	public void RefreshApplicationFocus()
	{
		ReportFocus(Application.isFocused, applicationPaused);
	}

	private void UpdateFocusLease()
	{
		updatingLeases = true;
		if (focused && !applicationPaused)
		{
			if (focusLease != null)
			{
				GameplayInputContext.Lease lease = focusLease;
				focusLease = null;
				lease.Dispose();
			}
		}
		else if (focusLease == null)
		{
			focusLease = policy.Acquire(this, GameplayInputContext.Kind.FocusLost);
		}
		updatingLeases = false;
	}

	private void UpdateRecoveryLease()
	{
		updatingLeases = true;
		if (!RequiresExplicitResume)
		{
			if (recoveryLease != null)
			{
				GameplayInputContext.Lease lease = recoveryLease;
				recoveryLease = null;
				lease.Dispose();
			}
		}
		else if (recoveryLease == null)
		{
			recoveryLease = policy.Register(this, GameplayInputContext.Kind.Pause, RecoveryBlocksInput);
		}
		updatingLeases = false;
	}

	// In the main menu keyboard & mouse need no explicit resume; everything else does.
	private bool RecoveryBlocksInput()
	{
		if (Mode != ControlMode.KeyboardMouse)
		{
			return true;
		}
		return policy.BaseContext != GameplayInputContext.Kind.MainMenu;
	}

	private void ResetSources(GameplayInputContext.ResetReason reason)
	{
		if (resettingSources)
		{
			return;
		}
		resettingSources = true;
		try
		{
			foreach (SourceRegistration source in sources.ToArray())
			{
				if (source.Manager == this && IsAssigned(source.Controller))
				{
					source.ResetValues();
				}
			}
		}
		finally
		{
			resettingSources = false;
		}
	}

	private bool IsSelectedController(Controller controller)
	{
		if (controller == null)
		{
			return false;
		}
		switch (controller.type)
		{
		case ControllerType.Joystick:
			return Mode == ControlMode.Gamepad;
		case ControllerType.Keyboard:
			return Mode == ControlMode.KeyboardMouse && (nativeKeyboardMouse == null || !nativeKeyboardMouse.ReplacesKeyboard);
		case ControllerType.Mouse:
			return Mode == ControlMode.KeyboardMouse && (nativeKeyboardMouse == null || !nativeKeyboardMouse.ReplacesMouse);
		default:
		{
			SourceRegistration source = FindSource(controller);
			return source != null && source.Mode == Mode && source.Available;
		}
		}
	}

	// Gameplay maps only accept the selected source (and nothing while a resume is required); menus also accept
	// the physical keyboard & mouse whatever the mode.
	public bool IsSourceAllowed(ControllerMap map)
	{
		if (map == null || !IsAssigned(map.controller))
		{
			return false;
		}
		GameplayInputContext.Kind context = policy.Current.Context;
		if (context == GameplayInputContext.Kind.Gameplay || context == GameplayInputContext.Kind.Vision)
		{
			if (RequiresExplicitResume || !SelectedSourceAvailable)
			{
				return false;
			}
		}
		else if ((map.controllerType == ControllerType.Keyboard || map.controllerType == ControllerType.Mouse) && Mode != ControlMode.KeyboardMouse)
		{
			return true;
		}
		return IsSelectedController(map.controller);
	}

	public void Synchronize()
	{
		if (applying || updatingLeases || !subscribed)
		{
			return;
		}
		applying = true;
		try
		{
			bool selectedLost = false;
			for (int i = sources.Count - 1; i >= 0; i--)
			{
				SourceRegistration source = sources[i];
				if (!ReInput.isReady || IsAssigned(source.Controller))
				{
					continue;
				}
				source.Manager = null;
				sources.RemoveAt(i);
				ownedTouch.Remove(source.Controller);
				if (nativeKeyboardMouse == source)
				{
					nativeKeyboardMouse = null;
				}
				selectedLost |= Mode == source.Mode;
			}
			if (selectedLost)
			{
				RequireResume(GameplayInputContext.ResetReason.DeviceDisconnected);
			}
			DiscoverLegacyTouch();
			if (pendingLevelStart)
			{
				if (Mode == ControlMode.Gamepad && !SelectedSourceAvailable)
				{
					CancelLevelStart("gamepadUnavailable");
				}
				else if (focused && !applicationPaused && SelectedSourceAvailable)
				{
					TryResumeExplicitly();
				}
			}
			if (adResumeSession != 0 && adEnded && focused && !applicationPaused)
			{
				if (!adResumeKeyboardMouse)
				{
					if (RequiresExplicitResume && SelectedSourceAvailable)
					{
						TryResumeExplicitly();
					}
					CancelAutomaticResume("adClosed");
				}
				else if (!ExpireKeyboardMouseAdResume() && RequiresExplicitResume && SelectedSourceAvailable && KeyboardMouseReportedAfter(adEndedFrame))
				{
					TryResumeExplicitly();
				}
			}
			policy.Synchronize();
			if (ReInput.isReady && player != null)
			{
				if (!SelectedSourceAvailable && !RequiresExplicitResume && !pendingLevelStart)
				{
					RequireResume(GameplayInputContext.ResetReason.DeviceDisconnected, true);
				}
				policy.ApplyRewiredMaps(player, IsSourceAllowed);
			}
		}
		finally
		{
			applying = false;
		}
	}

	private void LateUpdate()
	{
		if (ReInput.isReady)
		{
			policy.ObserveRewiredButtons(player, IsSourceAllowed);
		}
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		DiscoverLegacyTouch();
		Synchronize();
	}

	// A fullscreen ad started while playing: remember the session so play resumes automatically afterwards.
	private void OnAdStarted(int session)
	{
		adResumeSession = RequiresExplicitResume ? 0 : session;
		adResumeKeyboardMouse = !RequiresExplicitResume && Mode == ControlMode.KeyboardMouse;
		adEnded = false;
		adResumeWait = 0f;
		adResumeClockValid = false;
	}

	private void OnAdEnded(int session)
	{
		if (adResumeSession != session)
		{
			return;
		}
		adEnded = true;
		adResumeWait = 0f;
		adEndedFrame = Time.frameCount;
		adResumeClockValid = false;
		Synchronize();
	}

	private void CancelAutomaticResume(string reason)
	{
		adResumeSession = 0;
		adEnded = false;
		adResumeKeyboardMouse = false;
	}

	private void CancelLevelStart(string reason)
	{
		if (pendingLevelStart)
		{
			pendingLevelStart = false;
		}
	}

	private void OnActiveSceneChanged(Scene previous, Scene next)
	{
		CancelLevelStart("sceneChanged");
		CancelAutomaticResume("sceneChanged");
		legacyLease?.Dispose();
		legacyLease = null;
		policy.SetBaseContext(GameplayInputContext.Kind.MainMenu);
		policy.ResetInput(GameplayInputContext.ResetReason.SceneChanged);
	}

	// Called when a level's starting Rewired category is applied (4 = gameplay). Touch (and a connected
	// gamepad) resume automatically once their source shows up; KB&M resumes immediately.
	public void SetStartingCategory(int category)
	{
		bool pending = false;
		if (category == 4 && !applicationPaused)
		{
			if (Mode == ControlMode.Touch)
			{
				pending = true;
			}
			else if (Mode == ControlMode.Gamepad)
			{
				pending = SelectedSourceAvailable;
			}
		}
		levelStartSourceWait = 0f;
		pendingLevelStart = pending;
		levelStartSourceClockValid = false;
		SetLegacyCategory(category);
		if (category == 4 && Mode == ControlMode.KeyboardMouse)
		{
			TryResumeExplicitly();
		}
	}

	// Maps the old Rewired map-category switches onto input contexts: 3 = main menu, 4 = gameplay,
	// 5 = cancellable/submittable window, 6 = pause-like menu, anything else = pointer-only modal.
	public void SetLegacyCategory(int category)
	{
		if (category != 4)
		{
			CancelLevelStart("category");
		}
		legacyLease?.Dispose();
		legacyLease = null;
		if (category == 3 || category == 4)
		{
			policy.SetBaseContext(category == 4 ? GameplayInputContext.Kind.Gameplay : GameplayInputContext.Kind.MainMenu);
		}
		else
		{
			GameplayInputContext.WindowInput window = category == 5 ? (GameplayInputContext.WindowInput.Cancel | GameplayInputContext.WindowInput.Submit) : GameplayInputContext.WindowInput.PointerOnly;
			GameplayInputContext.Kind kind = category == 6 ? GameplayInputContext.Kind.Pause : GameplayInputContext.Kind.Modal;
			legacyLease = policy.Acquire(this, kind, window);
		}
		Synchronize();
	}
}
