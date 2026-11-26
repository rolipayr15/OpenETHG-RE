using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Owns mouse capture (pointer lock) for keyboard & mouse play. Capture is requested whenever the control mode
// allows it, confirmed once the backend reports the pointer is locked, and "withheld" when the platform (e.g. a
// browser) needs a click before it grants the lock. Releasing capture always resets gameplay input.
[DefaultExecutionOrder(-30100)]
public class MouseManager : CustomSingleton<MouseManager>
{
	public interface ICaptureBackend
	{
		string Id { get; }

		bool IsAvailable { get; }

		bool IsCaptured { get; }

		bool PrimaryButtonDown { get; }

		Vector2 PointerPosition { get; }

		void RequestCapture();

		void ReleaseCapture();

		void SetPointerVisible(bool visible);

		void ResetInput();
	}

	public interface ICaptureReadiness
	{
		bool ReportsLockReadiness { get; }

		bool CaptureLockReady { get; }

		bool LockNeedsClick { get; }

		bool LooksWithoutLock { get; }
	}

	private static MouseManager owner;

	private GameplayInputContext policy;

	private ControlModeManager controls;

	private GameplayInputContext.Lease adLease;

	private ICaptureBackend backend;

	private bool subscribed;

	private bool synchronizing;

	private bool requested;

	private bool confirmed;

	private bool adWasBusy;

	private bool withheld;

	private bool grantedInScene;

	private float requestedAt;

	private float unlockedNoticeSince = float.NegativeInfinity;

	private int sceneChangedFrame;

	private readonly List<RaycastResult> uiHits = new List<RaycastResult>();

	private PointerEventData pointerData;

	private EventSystem pointerSystem;

	public const float UnlockedNoticeSeconds = 4f;

	private const float CaptureDeadline = 1f;

	private const float ClickLockDeadline = 0.25f;

	public bool CaptureRequested
	{
		get
		{
			if (!subscribed || !isActiveAndEnabled)
			{
				return false;
			}
			Synchronize();
			return requested;
		}
	}

	public bool CaptureConfirmed
	{
		get
		{
			if (!subscribed || !isActiveAndEnabled)
			{
				return false;
			}
			Synchronize();
			return confirmed;
		}
	}

	public string BackendId => backend != null ? backend.Id : "None";

	public static bool HasConfirmedCapture => owner != null && owner.CaptureConfirmed;

	public static bool CaptureWithheld => owner != null && owner.subscribed && owner.isActiveAndEnabled && owner.withheld;

	public static bool WithheldAwaitsClick => CaptureWithheld && owner.LockNeedsClick();

	public static bool WithheldNoticeDue
	{
		get
		{
			if (!CaptureWithheld)
			{
				return false;
			}
			return NoticeDue(owner.LockNeedsClick(), owner.grantedInScene, owner.LooksWithoutLock(), Time.realtimeSinceStartup - owner.unlockedNoticeSince);
		}
	}

	public static bool WithheldLooksWithoutLock => CaptureWithheld && owner.LooksWithoutLock();

	public static bool UnlockedLook => GameplayPointerWithheld && owner.LooksWithoutLock();

	// True while gameplay wants the pointer locked but the platform has not granted the lock yet.
	public static bool GameplayPointerWithheld
	{
		get
		{
			if (owner == null || !owner.subscribed || !owner.isActiveAndEnabled)
			{
				return false;
			}
			if (owner.withheld)
			{
				return true;
			}
			if (!owner.requested || owner.confirmed || owner.backend == null || owner.backend.IsCaptured)
			{
				return false;
			}
			return owner.LockReady();
		}
	}

	// A click on the game view can resume play after input recovery (keyboard & mouse only).
	public bool PointerResumeAvailable
	{
		get
		{
			if (!subscribed || controls == null || !controls.isActiveAndEnabled || backend == null)
			{
				return false;
			}
			if (controls.Mode != ControlMode.KeyboardMouse || !controls.HasFocus || !backend.IsAvailable || !controls.SelectedSourceAvailable)
			{
				return false;
			}
			if (!controls.RequiresExplicitResume || IsAdBusy() || sceneChangedFrame == Time.frameCount || policy == null)
			{
				return false;
			}
			GameplayInputContext.Lease top = policy.Current.TopOwner;
			return controls.OwnsRecoveryLease(top) && policy.IsGameplayExcept(top);
		}
	}

	public static bool PointerResumeHintVisible
	{
		get
		{
			if (owner == null || !owner.isActiveAndEnabled || !owner.PointerResumeAvailable)
			{
				return false;
			}
			return owner.controls != null && !owner.controls.AutomaticResumePending;
		}
	}

	public bool IsUsingCaptureBackend(ICaptureBackend value)
	{
		return backend == value;
	}

	public static bool NoticeDue(bool awaitsClick, bool grantedInScene, bool looksWithoutLock, float unlockedNoticeAge)
	{
		if (!looksWithoutLock)
		{
			return !(awaitsClick && grantedInScene);
		}
		return unlockedNoticeAge >= 0f && unlockedNoticeAge < UnlockedNoticeSeconds;
	}

	protected MouseManager()
	{
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void ResetOwner()
	{
		owner = null;
	}

	private void Awake()
	{
		if (owner != null && owner != this)
		{
			enabled = false;
			Destroy(this);
			return;
		}
		owner = this;
		policy = GameplayInputContext.Shared;
		backend = new UnityRewiredPointerBackend();
	}

	private void OnEnable()
	{
		if (owner != this || subscribed)
		{
			return;
		}
		controls = ControlModeManager.Instance;
		subscribed = true;
		policy.InputResetRequested += ResetBackendInput;
		policy.Changed += Synchronize;
		if (controls != null)
		{
			controls.AvailabilityChanged += Synchronize;
		}
		SceneManager.activeSceneChanged += OnActiveSceneChanged;
		SceneManager.sceneLoaded += OnSceneLoaded;
		// Fullscreen ads block gameplay input while they are on screen.
		adLease = policy.Register(this, GameplayInputContext.Kind.GameBlock, () => FullscreenAdSessions.IsShowing);
		Synchronize();
	}

	private static bool IsAdBusy()
	{
		return CustomSingleton<AdsManager>.HasInstance && CustomSingleton<AdsManager>.Instance.IsFullscreenAdBusy;
	}

	public void SetCaptureBackend(ICaptureBackend value)
	{
		if (backend == value)
		{
			return;
		}
		if (controls != null && controls.Mode == ControlMode.KeyboardMouse)
		{
			controls.RequirePointerResume();
		}
		Release();
		backend = value ?? new UnityRewiredPointerBackend();
		backend.ReleaseCapture();
		backend.SetPointerVisible(true);
		backend.ResetInput();
		Synchronize();
	}

	private void Update()
	{
		Synchronize();
		if (backend != null && backend.PrimaryButtonDown)
		{
			TryResumeFromPointerGesture();
		}
	}

	private void LateUpdate()
	{
		Synchronize();
	}

	public bool TryResumeFromPointerGesture()
	{
		if (!PointerResumeAvailable)
		{
			return false;
		}
		if (IsPointerOverUI(backend.PointerPosition) || !controls.TryResumeExplicitly())
		{
			return false;
		}
		Synchronize();
		return true;
	}

	// UI under the pointer (other than gameplay touch controls) keeps the click for itself.
	private bool IsPointerOverUI(Vector2 position)
	{
		EventSystem eventSystem = EventSystem.current;
		if (eventSystem == null || !eventSystem.isActiveAndEnabled)
		{
			return false;
		}
		if (pointerData == null || pointerSystem != eventSystem)
		{
			pointerSystem = eventSystem;
			pointerData = new PointerEventData(eventSystem);
		}
		pointerData.Reset();
		pointerData.position = position;
		pointerData.pointerId = -1;
		eventSystem.RaycastAll(pointerData, uiHits);
		ControlMode mode = controls != null ? controls.Mode : ControlMode.KeyboardMouse;
		foreach (RaycastResult hit in uiHits)
		{
			if (hit.module is GraphicRaycaster && hit.gameObject != null && !InteractionManager.IsGameplayTouchGraphic(hit.gameObject, mode))
			{
				return true;
			}
		}
		return false;
	}

	public void Synchronize()
	{
		if (!subscribed || synchronizing || backend == null)
		{
			return;
		}
		synchronizing = true;
		try
		{
			GameplayInputContext.Permissions state = policy.Current;
			bool adBusy = IsAdBusy();
			if (adBusy && !adWasBusy && controls != null)
			{
				controls.RequirePointerResumeKeepingAdResume();
			}
			adWasBusy = adBusy;
			bool wanted = controls != null && controls.isActiveAndEnabled && controls.CanRequestMouseCapture && !state.RequiresPointer && !adBusy && backend.IsAvailable && sceneChangedFrame != Time.frameCount;
			if (!wanted)
			{
				Release();
				return;
			}
			if (!requested)
			{
				requested = true;
				requestedAt = Time.realtimeSinceStartup;
				backend.ResetInput();
				backend.RequestCapture();
				policy.ResetInput(GameplayInputContext.ResetReason.CaptureChanged);
			}
			else if (confirmed && !backend.IsCaptured)
			{
				// The platform dropped the lock (Esc in a browser, alt-tab...): require an explicit resume.
				Release();
				controls.RequirePointerResume();
				return;
			}
			if (withheld && !LockReady())
			{
				Release();
				controls.RequirePointerResume();
				return;
			}
			bool captured = backend.IsCaptured;
			if (confirmed != captured)
			{
				confirmed = captured;
				if (captured)
				{
					grantedInScene = true;
				}
				policy.ResetInput(GameplayInputContext.ResetReason.CaptureChanged);
			}
			if (confirmed && withheld)
			{
				withheld = false;
			}
			backend.SetPointerVisible(!confirmed);
			if (confirmed || withheld)
			{
				return;
			}
			float elapsed = Time.realtimeSinceStartup - requestedAt;
			if (!LockReady())
			{
				if (elapsed > CaptureDeadline)
				{
					Release();
					controls.RequirePointerResume();
				}
			}
			else if (!LooksWithoutLock())
			{
				if (elapsed > (LockNeedsClick() ? ClickLockDeadline : CaptureDeadline))
				{
					withheld = true;
				}
			}
			else
			{
				withheld = true;
				if (float.IsNegativeInfinity(unlockedNoticeSince))
				{
					unlockedNoticeSince = Time.realtimeSinceStartup;
				}
			}
		}
		finally
		{
			synchronizing = false;
		}
	}

	private bool LockReady()
	{
		return backend is ICaptureReadiness readiness && readiness.ReportsLockReadiness && readiness.CaptureLockReady;
	}

	private bool LockNeedsClick()
	{
		return backend is ICaptureReadiness readiness && readiness.ReportsLockReadiness && readiness.LockNeedsClick;
	}

	private bool LooksWithoutLock()
	{
		return backend is ICaptureReadiness readiness && readiness.ReportsLockReadiness && readiness.LooksWithoutLock && !readiness.LockNeedsClick;
	}

	private void ResetBackendInput(GameplayInputContext.ResetReason reason)
	{
		backend?.ResetInput();
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("EYES_INPUT_DIAGNOSTICS")]
	private void TraceCapture(string phase)
	{
	}

	private void Release()
	{
		bool hadCapture = requested || confirmed || (backend != null && backend.IsCaptured);
		withheld = false;
		requested = false;
		confirmed = false;
		if (backend != null)
		{
			backend.ReleaseCapture();
			backend.SetPointerVisible(true);
		}
		if (hadCapture && policy != null)
		{
			policy.ResetInput(GameplayInputContext.ResetReason.CaptureChanged);
		}
	}

	public void lockMouse()
	{
		Synchronize();
	}

	public void unlockMouse()
	{
		SuspendCapture();
	}

	public void SuspendCapture()
	{
		if (controls != null)
		{
			controls.RequirePointerResume();
		}
		Release();
	}

	private void OnActiveSceneChanged(Scene previous, Scene next)
	{
		grantedInScene = false;
		unlockedNoticeSince = float.NegativeInfinity;
		sceneChangedFrame = Time.frameCount;
		Release();
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (mode != LoadSceneMode.Single)
		{
			return;
		}
		grantedInScene = false;
		unlockedNoticeSince = float.NegativeInfinity;
		sceneChangedFrame = Time.frameCount;
		Release();
	}

	private void OnDisable()
	{
		Cleanup();
	}

	private void Cleanup()
	{
		if (owner != this)
		{
			return;
		}
		if (subscribed)
		{
			subscribed = false;
			policy.InputResetRequested -= ResetBackendInput;
			policy.Changed -= Synchronize;
			if (controls != null)
			{
				controls.AvailabilityChanged -= Synchronize;
			}
			SceneManager.activeSceneChanged -= OnActiveSceneChanged;
			SceneManager.sceneLoaded -= OnSceneLoaded;
			if (controls != null)
			{
				controls.RequirePointerResume();
			}
			adLease?.Dispose();
			adLease = null;
		}
		Release();
		backend?.ResetInput();
	}

	protected override void OnDestroy()
	{
		if (owner != this)
		{
			return;
		}
		Cleanup();
		owner = null;
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		Cleanup();
		base.OnApplicationQuit();
	}
}
