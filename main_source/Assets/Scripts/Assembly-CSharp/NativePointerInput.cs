using System;
using UnityEngine.EventSystems;
using Rewired;
using Rewired.Integration.UnityUI;
using Rewired.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-31050)]
public sealed class NativePointerInput : MonoBehaviour
{
	private sealed class PointerSource : IMouseInputSource
	{
		private readonly NativePointerInput owner;

		private PointerFrame Sample => owner.backend != null ? owner.backend.Sample(Time.frameCount) : default;

		public int playerId => 0;

		public bool enabled => owner.isActiveAndEnabled && owner.backend != null && owner.controls != null && owner.controls.Mode == ControlMode.KeyboardMouse && owner.controls.HasFocus && owner.backend.MousePresence != ControlModeManager.DevicePresence.Disconnected && Sample.HasPosition;

		public bool locked => owner.backend != null && owner.backend.IsCaptured;

		public int buttonCount => 3;

		private bool CanClick => enabled && !locked && (GameplayInputContext.Shared.Current.PointerUI || WithheldHudPointer);

		public Vector2 screenPosition => Sample.PointerPosition;

		public Vector2 screenPositionDelta => enabled && !locked ? Sample.PointerDelta : Vector2.zero;

		public Vector2 wheelDelta => CanClick ? Sample.Scroll : Vector2.zero;

		public PointerSource(NativePointerInput owner)
		{
			this.owner = owner;
		}

		public bool GetButton(int button)
		{
			return CanClick && (uint)button < 3u && ((Sample.HeldButtons >> button) & 1u) != 0;
		}

		public bool GetButtonDown(int button)
		{
			return CanClick && (uint)button < 3u && ((Sample.DownButtons >> button) & 1u) != 0;
		}

		public bool GetButtonUp(int button)
		{
			return CanClick && (uint)button < 3u && ((Sample.UpButtons >> button) & 1u) != 0;
		}
	}

	private static NativePointerInput active;

	private IPointerBackend backend;

	private ControlModeManager controls;

	private NativeKeyboardMouseMaps.Binding binding;

	private ControlModeManager.SourceRegistration registration;

	private MouseManager captureOwner;

	private RewiredPointerInputModule uiModule;

	private PointerSource pointerSource;

	private bool subscribed;

	private int fedFrame = -1;

	private int lossVersion;

	public IPointerBackend Backend => backend;

	public CustomController Controller => binding != null && binding.IsCurrent ? binding.Controller : null;

	public IMouseInputSource PointerUI => pointerSource;

	public static bool NativePointerUIAllowed => active != null && active.backend != null && active.controls != null && active.controls.Mode == ControlMode.KeyboardMouse && active.controls.HasFocus && !active.backend.IsCaptured && (GameplayInputContext.Shared.Current.PointerUI || WithheldHudPointer);

	private static bool WithheldHudPointer => MouseManager.CaptureWithheld && GameplayInputContext.Shared.Current.Context == GameplayInputContext.Kind.Gameplay && GameplayInputContext.Shared.Current.TopOwner == null;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void ResetStatic()
	{
		active = null;
	}

	public void Configure(IPointerBackend value)
	{
		if (value != null && !value.ReplacesMouse)
		{
			throw new ArgumentException("A native pointer must replace the physical mouse maps.", "value");
		}
		if (backend == value)
		{
			return;
		}
		Unbind();
		RestoreCaptureOwner();
		backend = value;
		fedFrame = -1;
		lossVersion = backend != null ? backend.ConnectionLossVersion : 0;
		if (isActiveAndEnabled)
		{
			EnsureBound();
		}
	}

	private void OnEnable()
	{
		if (active != null && active != this)
		{
			enabled = false;
			throw new InvalidOperationException("Only one native pointer bridge may be active.");
		}
		active = this;
		controls = ControlModeManager.Instance;
		pointerSource = new PointerSource(this);
		ReInput.InitializedEvent += OnRewiredInitialized;
		SceneManager.sceneLoaded += OnSceneLoaded;
		subscribed = true;
		if (ReInput.isReady)
		{
			OnRewiredInitialized();
		}
	}

	private void OnDisable()
	{
		if (!subscribed)
		{
			return;
		}
		subscribed = false;
		ReInput.InitializedEvent -= OnRewiredInitialized;
		SceneManager.sceneLoaded -= OnSceneLoaded;
		OnRewiredShutdown();
		RestoreCaptureOwner();
		if (active == this)
		{
			active = null;
		}
	}

	private void OnRewiredInitialized()
	{
		ReInput.InputSourceUpdateEvent -= Feed;
		ReInput.InputSourceUpdateEvent += Feed;
		ReInput.PreShutDownEvent -= OnRewiredShutdown;
		ReInput.PreShutDownEvent += OnRewiredShutdown;
		EnsureBound();
	}

	private void OnRewiredShutdown()
	{
		ReInput.InputSourceUpdateEvent -= Feed;
		ReInput.PreShutDownEvent -= OnRewiredShutdown;
		Unbind();
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		EnsureBound();
	}

	private void Update()
	{
		EnsureBound();
		SynchronizeDevices();
		AttachUI();
	}

	private void EnsureBound()
	{
		if (!isActiveAndEnabled || backend == null || !ReInput.isReady || controls == null || controls.Player == null)
		{
			return;
		}
		if (binding == null || !binding.IsCurrent || registration == null || !registration.IsRegistered)
		{
			Unbind();
			binding = NativeKeyboardMouseMaps.Create(controls.Player, backend.Id);
			registration = controls.RegisterNativeKeyboardMouse(binding.Controller, backend.Id, backend.ReplacesKeyboard, true, backend.IsAvailable, ResetInput);
			SynchronizeDevices();
		}
		MouseManager manager = CustomSingleton<MouseManager>.Instance;
		if (manager != captureOwner || !manager.IsUsingCaptureBackend(backend))
		{
			RestoreCaptureOwner();
			captureOwner = manager;
			captureOwner.SetCaptureBackend(backend);
		}
		AttachUI();
	}

	private void SynchronizeDevices()
	{
		if (backend == null || registration == null)
		{
			return;
		}
		int version = backend.ConnectionLossVersion;
		if (version != lossVersion)
		{
			lossVersion = version;
			if (controls.Mode == ControlMode.KeyboardMouse)
			{
				controls.RequirePointerResumeKeepingAdResume();
			}
			ResetInput();
		}
		registration.ReportState(backend.IsAvailable, backend.KeyboardPresence, backend.MousePresence);
	}

	private void Feed()
	{
		EnsureBound();
		if (binding == null || !binding.IsCurrent || backend == null)
		{
			return;
		}
		SynchronizeDevices();
		if (fedFrame == Time.frameCount)
		{
			return;
		}
		fedFrame = Time.frameCount;
		PointerFrame frame = backend.Sample(fedFrame);
		CustomController controller = binding.Controller;
		bool selected = controls.Mode == ControlMode.KeyboardMouse && controls.HasFocus;
		bool lookBlocked = true;
		if (selected && (backend.IsCaptured || MouseManager.UnlockedLook) && controls.CanRequestMouseCapture)
		{
			lookBlocked = !GameplayInputContext.Shared.Current.Look;
		}
		bool withheld = MouseManager.GameplayPointerWithheld;
		controller.SetAxisValue(NativeKeyboardMouseMaps.AxisIndex.LookX, lookBlocked ? 0f : frame.RelativeDelta.x);
		controller.SetAxisValue(NativeKeyboardMouseMaps.AxisIndex.LookY, lookBlocked ? 0f : frame.RelativeDelta.y);
		controller.SetAxisValue(NativeKeyboardMouseMaps.AxisIndex.PointerX, frame.PointerPosition.x);
		controller.SetAxisValue(NativeKeyboardMouseMaps.AxisIndex.PointerY, frame.PointerPosition.y);
		controller.SetAxisValue(NativeKeyboardMouseMaps.AxisIndex.ScrollX, selected ? frame.Scroll.x : 0f);
		controller.SetAxisValue(NativeKeyboardMouseMaps.AxisIndex.ScrollY, selected ? frame.Scroll.y : 0f);
		for (int i = 0; i < NativeKeyboardMouseMaps.ButtonCount; i++)
		{
			bool pressed = false;
			if (i < NativeKeyboardMouseMaps.ButtonIndex.FirstKey)
			{
				// A click that will grant the pointer lock must not also act as a gameplay click.
				if (!(withheld && i == 0))
				{
					uint bit = 1u << i;
					pressed = (bit & frame.HeldButtons) != 0 || (bit & frame.DownButtons) != 0;
				}
			}
			else if (backend.ReplacesKeyboard)
			{
				ulong bit = 1UL << (i - NativeKeyboardMouseMaps.ButtonIndex.FirstKey);
				pressed = (bit & frame.HeldKeys) != 0 || (bit & frame.DownKeys) != 0;
			}
			controller.SetButtonValue(i, selected && pressed);
		}
		controls.Synchronize();
	}

	private void ResetInput()
	{
		backend?.ResetInput();
		binding?.Reset();
		fedFrame = Time.frameCount;
	}

	private void Unbind()
	{
		DetachUI();
		registration?.Dispose();
		registration = null;
		binding?.Dispose();
		binding = null;
		fedFrame = -1;
	}

	private void RestoreCaptureOwner()
	{
		if (captureOwner != null && captureOwner.IsUsingCaptureBackend(backend))
		{
			captureOwner.SetCaptureBackend(null);
		}
		captureOwner = null;
	}

	private void AttachUI()
	{
		EventSystem eventSystem = EventSystem.current;
		RewiredPointerInputModule module = null;
		if (backend != null && controls != null && controls.Mode == ControlMode.KeyboardMouse && eventSystem != null)
		{
			module = eventSystem.currentInputModule as RewiredPointerInputModule;
		}
		if (module != uiModule)
		{
			DetachUI();
			uiModule = module;
		}
		if (uiModule == null || pointerSource == null)
		{
			return;
		}
		int count = uiModule.GetMouseInputSourceCount(0);
		for (int i = 0; i < count; i++)
		{
			if (uiModule.GetMouseInputSource(0, i) == pointerSource)
			{
				return;
			}
		}
		uiModule.AddMouseInputSource(pointerSource);
	}

	private void DetachUI()
	{
		if (uiModule != null && pointerSource != null)
		{
			uiModule.RemoveMouseInputSource(pointerSource);
		}
		uiModule = null;
	}

	public static bool TryReadKey(KeyCode key, bool edge, out bool value)
	{
		value = false;
		if (active == null || active.backend == null || !active.backend.ReplacesKeyboard || active.controls == null || active.controls.Mode != ControlMode.KeyboardMouse)
		{
			return false;
		}
		int button = NativeKeyboardMouseMaps.ButtonForKey(key);
		if (button < NativeKeyboardMouseMaps.ButtonIndex.FirstKey)
		{
			return false;
		}
		if (!active.controls.HasFocus || active.backend.KeyboardPresence == ControlModeManager.DevicePresence.Disconnected)
		{
			return true;
		}
		PointerFrame frame = active.backend.Sample(Time.frameCount);
		value = (((edge ? frame.DownKeys : frame.HeldKeys) >> (button - NativeKeyboardMouseMaps.ButtonIndex.FirstKey)) & 1UL) != 0;
		return true;
	}

	public static bool TryReadPointer(out PointerFrame sample)
	{
		sample = default;
		if (active == null || active.backend == null || active.controls == null || active.controls.Mode != ControlMode.KeyboardMouse)
		{
			return false;
		}
		sample = active.backend.Sample(Time.frameCount);
		if (NativePointerUIAllowed && active.backend.MousePresence != ControlModeManager.DevicePresence.Disconnected)
		{
			return true;
		}
		sample.PointerDelta = Vector2.zero;
		sample.Scroll = Vector2.zero;
		sample.HeldButtons = 0u;
		sample.DownButtons = 0u;
		sample.UpButtons = 0u;
		return true;
	}
}
