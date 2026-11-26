using System;
using System.Collections.Generic;
using Prime31.MessageKit;
using Rewired;
using Rewired.ComponentControls;
using Rewired.ComponentControls.Effects;
using Rewired.Integration.UnityUI;
using Rewired.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// World interaction (doors, items, ...) and the shared button helpers on top of Rewired.
// Touch: tap an Interactable. Keyboard/mouse and gamepad: aim the screen centre and press Interact.
// Input is sampled in Update and acted on in LateUpdate only if nothing (mode, context, capture) changed in between.
[DefaultExecutionOrder(-30000)]
public class InteractionManager : CustomSingleton<InteractionManager>
{
	private const float TouchCooldown = 0.25f;

	private const float RayRadius = 0.05f;

	private const float TouchReach = 1.7f;

	private const float CenterReach = 2f;

	private int layerMask;

	[SerializeField]
	private Image interactLabel;

	private Camera mainCamera;

	private bool isInputOn = true;

	private bool interactLabelVisible;

	private float lastInteraction;

	private GameplayInputContext policy;

	private readonly List<Touch> beganTouches = new List<Touch>();

	private readonly HashSet<int> uiTouches = new HashSet<int>();

	private readonly List<RaycastResult> uiHits = new List<RaycastResult>();

	private EventSystem pointerEventSystem;

	private PointerEventData pointerData;

	private int sampledFrame = -1;

	private uint sampledRevision;

	private ControlMode sampledMode;

	private bool capturedAtSample;

	private bool pointerBlockedAtSample;

	private bool touchOverUI;

	public static Player RewiredPlayer => ReInput.players.GetPlayer(0);

	public static void SetRewiredMapping(Action<InputActionEventData> action, int actionId)
	{
		if (!ReInput.isReady)
		{
			return;
		}
		RewiredPlayer.RemoveInputEventDelegate(action, actionId);
		RewiredPlayer.AddInputEventDelegate(action, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, actionId);
	}

	public static void RemoveRewiredMapping(Action<InputActionEventData> action, int actionId)
	{
		if (ReInput.isReady)
		{
			RewiredPlayer.RemoveInputEventDelegate(action, actionId);
		}
	}

	// Buttons are swallowed while a text field has focus or a scene transition is in progress.
	public static bool GetButtonDown(int actionId)
	{
		if (!ReInput.isReady || GameplayInputContext.Shared.HasActive(GameplayInputContext.Kind.TextEntry) || GameplayInputContext.Shared.Current.TransitionBarrier)
		{
			return false;
		}
		return RewiredPlayer.GetButtonDown(actionId);
	}

	public static bool GetNegativeButtonDown(int actionId)
	{
		if (!ReInput.isReady || GameplayInputContext.Shared.HasActive(GameplayInputContext.Kind.TextEntry) || GameplayInputContext.Shared.Current.TransitionBarrier)
		{
			return false;
		}
		return RewiredPlayer.GetNegativeButtonDown(actionId);
	}

	public static bool DismissDialogButtonPressed()
	{
		if (!ReInput.isReady)
		{
			return false;
		}
		return GetButtonDown(RewiredConsts.Action.Accept) || GetButtonDown(RewiredConsts.Action.Decline);
	}

	public static void ActivateRewiredCategory(int categoryId)
	{
		ControlModeManager.Instance.SetLegacyCategory(categoryId);
	}

	private void Awake()
	{
		MessageKit.addObserver(39, InputOn);
		MessageKit.addObserver(40, InputOff);
	}

	private void OnEnable()
	{
		policy = GameplayInputContext.Shared;
		policy.InputResetRequested += ResetPendingInput;
		policy.ResetInput(GameplayInputContext.ResetReason.SceneChanged);
	}

	private void OnDisable()
	{
		if (policy != null)
		{
			policy.InputResetRequested -= ResetPendingInput;
		}
		ResetPendingInput(GameplayInputContext.ResetReason.ContextChanged);
	}

	private void ResetPendingInput(GameplayInputContext.ResetReason reason)
	{
		sampledFrame = -1;
		beganTouches.Clear();
		uiTouches.Clear();
		SetInteractLabelVisible(false);
	}

	private void InputOn()
	{
		SetLegacyInput(true);
	}

	private void InputOff()
	{
		SetLegacyInput(false);
	}

	private void SetLegacyInput(bool enabled)
	{
		if (isInputOn != enabled)
		{
			isInputOn = enabled;
			if (isActiveAndEnabled)
			{
				policy.ResetInput(GameplayInputContext.ResetReason.ContextChanged);
			}
		}
	}

	private void Start()
	{
		layerMask = LayerMask.GetMask("Interactable");
		mainCamera = Camera.main;
		interactLabelVisible = interactLabel != null && interactLabel.enabled;
		SetInteractLabelVisible(false);
	}

	private void SetInteractLabelVisible(bool visible)
	{
		if (interactLabelVisible != visible)
		{
			interactLabelVisible = visible;
			if (interactLabel != null)
			{
				interactLabel.enabled = visible;
			}
		}
	}

	private bool CanInteract(ControlModeManager manager)
	{
		if (!isActiveAndEnabled || !isInputOn || manager == null || !manager.isActiveAndEnabled)
		{
			return false;
		}
		if (!ReInput.isReady || manager.Player == null || !manager.HasFocus || manager.RequiresRecoveryUI)
		{
			return false;
		}
		if (!policy.Current.StartActions)
		{
			return false;
		}
		// With keyboard & mouse the cursor must be captured (or capture deliberately withheld by the OS).
		return manager.Mode != ControlMode.KeyboardMouse || MouseManager.HasConfirmedCapture || MouseManager.CaptureWithheld;
	}

	private void Update()
	{
		beganTouches.Clear();
		uiTouches.Clear();
		touchOverUI = false;
		ControlModeManager manager = ControlModeManager.Instance;
		sampledMode = manager.Mode;
		sampledRevision = policy.Current.Revision;
		capturedAtSample = MouseManager.HasConfirmedCapture || MouseManager.CaptureWithheld;
		pointerBlockedAtSample = IsMouseOverUI(manager);
		RewiredPointerInputModule module = EventSystem.current != null ? EventSystem.current.currentInputModule as RewiredPointerInputModule : null;
		ITouchInputSource touchSource = module != null ? module.GetTouchInputSource(0, 0) : null;
		int touchCount = touchSource != null ? touchSource.touchCount : Input.touchCount;
		for (int i = 0; i < touchCount; i++)
		{
			Touch touch = touchSource != null ? touchSource.GetTouch(i) : Input.GetTouch(i);
			CaptureTouch(touch);
		}
		pointerBlockedAtSample |= touchOverUI;
		sampledFrame = Time.frameCount;
	}

	private void CaptureTouch(Touch touch)
	{
		if (touch.phase == TouchPhase.Canceled)
		{
			return;
		}
		if (IsPointerOverUI(touch.position, touch.fingerId))
		{
			uiTouches.Add(touch.fingerId);
			touchOverUI = true;
		}
		if (touch.phase == TouchPhase.Began)
		{
			beganTouches.Add(touch);
		}
	}

	private void LateUpdate()
	{
		ControlModeManager manager = ControlModeManager.Instance;
		if (sampledFrame == Time.frameCount && CanInteract(manager) && sampledMode == manager.Mode && sampledRevision == policy.Current.Revision && (sampledMode != ControlMode.KeyboardMouse || capturedAtSample))
		{
			if (mainCamera == null)
			{
				mainCamera = Camera.main;
			}
			if (mainCamera != null)
			{
				if (sampledMode == ControlMode.Touch)
				{
					ProcessTouch();
				}
				else
				{
					ProcessCenter(manager);
				}
				return;
			}
		}
		SetInteractLabelVisible(false);
	}

	private void ProcessTouch()
	{
		SetInteractLabelVisible(false);
		if (lastInteraction + TouchCooldown >= Time.time)
		{
			return;
		}
		foreach (Touch touch in beganTouches)
		{
			if (uiTouches.Contains(touch.fingerId) || IsPointerOverUI(touch.position, touch.fingerId))
			{
				continue;
			}
			Ray ray = mainCamera.ViewportPointToRay(new Vector3(touch.position.x / Screen.width, touch.position.y / Screen.height, 0f));
			if (!Physics.SphereCast(ray, RayRadius, out RaycastHit hit, TouchReach, layerMask))
			{
				continue;
			}
			GameObject target = hit.collider.gameObject;
			if (target.CompareTag("InteractableBlocker"))
			{
				continue;
			}
			if (policy.TryConsumeAction(RewiredConsts.Action.Interact, true))
			{
				lastInteraction = Time.time;
				TryInteract(target);
			}
			break;
		}
	}

	private void ProcessCenter(ControlModeManager manager)
	{
		if (pointerBlockedAtSample || IsMouseOverUI(manager))
		{
			SetInteractLabelVisible(false);
			return;
		}
		foreach (Touch touch in beganTouches)
		{
			if (IsPointerOverUI(touch.position, touch.fingerId))
			{
				SetInteractLabelVisible(false);
				return;
			}
		}
		Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
		if (!Physics.SphereCast(ray, RayRadius, out RaycastHit hit, CenterReach, layerMask))
		{
			SetInteractLabelVisible(false);
			return;
		}
		bool blocked = hit.collider.CompareTag("InteractableBlocker");
		SetInteractLabelVisible(!blocked);
		if (blocked)
		{
			return;
		}
		bool pressed = manager.Player.GetButtonDown(RewiredConsts.Action.Interact);
		if (policy.TryConsumeAction(RewiredConsts.Action.Interact, pressed))
		{
			TryInteract(hit.collider.gameObject);
		}
	}

	private void TryInteract(GameObject target)
	{
		Interactable interactable = target.GetComponent<Interactable>();
		if (interactable != null)
		{
			interactable.TryInteract(false);
			MessageKit.post(81);
		}
	}

	private bool IsMouseOverUI(ControlModeManager manager)
	{
		if (manager == null || manager.Mode != ControlMode.KeyboardMouse || MouseManager.CaptureWithheld)
		{
			return false;
		}
		RewiredPointerInputModule module = EventSystem.current != null ? EventSystem.current.currentInputModule as RewiredPointerInputModule : null;
		if (module == null || manager.Player == null)
		{
			// A captured mouse is a look device, not a pointer.
			return !MouseManager.HasConfirmedCapture && IsPointerOverUI(Input.mousePosition, -1);
		}
		int playerId = manager.Player.id;
		for (int i = 0; i < module.GetMouseInputSourceCount(playerId); i++)
		{
			IMouseInputSource source = module.GetMouseInputSource(playerId, i);
			if (source != null && source.enabled && !source.locked && IsPointerOverUI(source.screenPosition, -1))
			{
				return true;
			}
		}
		return false;
	}

	// UI hits count only on GraphicRaycaster canvases, and on-screen gameplay controls (sticks, touch pads) don't block.
	private bool IsPointerOverUI(Vector2 position, int pointerId)
	{
		EventSystem eventSystem = EventSystem.current;
		if (eventSystem == null || !eventSystem.isActiveAndEnabled)
		{
			return false;
		}
		if (pointerData == null || pointerEventSystem != eventSystem)
		{
			pointerEventSystem = eventSystem;
			pointerData = new PointerEventData(eventSystem);
		}
		pointerData.Reset();
		pointerData.pointerId = pointerId;
		pointerData.position = position;
		pointerData.button = PointerEventData.InputButton.Left;
		eventSystem.RaycastAll(pointerData, uiHits);
		foreach (RaycastResult hit in uiHits)
		{
			if (hit.module is GraphicRaycaster && hit.gameObject != null && !IsGameplayTouchGraphic(hit.gameObject, sampledMode))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsGameplayTouchGraphic(GameObject target, ControlMode mode)
	{
		TouchPad touchPad = target.GetComponent<TouchPad>();
		if (touchPad != null && touchPad.GetComponentInParent<TouchInputStatus>() != null)
		{
			return true;
		}
		TouchRegion touchRegion = target.GetComponent<TouchRegion>();
		if (touchRegion != null && touchRegion.GetComponentInParent<TouchInputStatus>() != null && (mode != ControlMode.Touch || !touchRegion.isActiveAndEnabled || !touchRegion.interactable))
		{
			return true;
		}
		TouchJoystick joystick = target.GetComponentInParent<TouchJoystick>();
		if (joystick == null || joystick.GetComponentInParent<TouchInputStatus>() == null)
		{
			return false;
		}
		if (mode == ControlMode.Touch && joystick.isActiveAndEnabled && joystick.interactable)
		{
			return false;
		}
		// Only the joystick's own visuals pass through, not buttons that happen to sit inside it.
		GameObject handler = ExecuteEvents.GetEventHandler<IPointerDownHandler>(target);
		if (handler != null && handler != joystick.gameObject)
		{
			return false;
		}
		Graphic graphic = target.GetComponent<Graphic>();
		if (graphic == null)
		{
			return false;
		}
		TouchInteractableTransitioner transitioner = target.GetComponent<TouchInteractableTransitioner>();
		if (graphic == joystick.targetGraphic || target.transform == joystick.stickTransform)
		{
			return true;
		}
		return transitioner != null && transitioner.targetGraphic == graphic;
	}

	protected override void OnDestroy()
	{
		MessageKit.removeObserver(39, InputOn);
		MessageKit.removeObserver(40, InputOff);
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}
