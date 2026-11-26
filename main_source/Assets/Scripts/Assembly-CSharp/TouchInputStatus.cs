using System;
using System.Diagnostics;
using Prime31.MessageKit;
using Rewired;
using Rewired.ComponentControls;
using UnityEngine;

[DefaultExecutionOrder(-31000)]
[RequireComponent(typeof(TouchController))]
public class TouchInputStatus : MonoBehaviour
{
	[Flags]
	private enum LegacyBlock
	{
		None = 0,
		Input = 1,
		Pause = 2,
		Vision = 4,
		Death = 8,
		GuestDeath = 0x10,
		Finished = 0x20,
		Hidden = 0x40
	}

	[SerializeField]
	private TouchJoystick joystick;

	[SerializeField]
	private TouchPad touchpad;

	private TouchController wrapper;

	private ControlModeManager manager;

	private GameplayInputContext policy;

	private ControlModeManager.SourceRegistration registration;

	private Rewired.CustomController controller;

	private Rewired.CustomController ownedController;

	private bool synchronizing;

	private bool releasingSource;

	private bool subscribed;

	private bool wrapperNeedsReinitialization;

	private LegacyBlock legacyBlocks;

	private string lastTraceState;

	private int lastResumeCount = -1;

	private bool awaitingDelivery;

	private void Awake()
	{
		wrapper = GetComponent<TouchController>();
		wrapper.disableMouseInputWhenEnabled = false;
		Suspend();
	}

	private void OnEnable()
	{
		if (subscribed)
		{
			return;
		}
		subscribed = true;
		wrapperNeedsReinitialization = !ReInput.isReady || wrapperNeedsReinitialization;
		policy = GameplayInputContext.Shared;
		BindManager();
		policy.Changed += Synchronize;
		ReInput.InitializedEvent += RewiredInitialized;
		if (ReInput.isReady)
		{
			BindRewired();
		}
		MessageKit.addObserver(39, InputOn);
		MessageKit.addObserver(40, InputOff);
		MessageKit<bool>.addObserver(6, Pause);
		MessageKit<bool>.addObserver(7, Vision);
		MessageKit<bool>.addObserver(103, ShowMenuButtons);
		MessageKit.addObserver(17, Death);
		MessageKit.addObserver(92, GuestDeath);
		MessageKit.addObserver(53, Revive);
		MessageKit.addObserver(105, Finished);
		MessageKit.addObserver(38, Synchronize);
		Synchronize();
	}

	private void OnDisable()
	{
		if (!subscribed)
		{
			return;
		}
		subscribed = false;
		policy.Changed -= Synchronize;
		ReInput.InitializedEvent -= RewiredInitialized;
		UnbindRewired();
		UnbindManager();
		MessageKit.removeObserver(39, InputOn);
		MessageKit.removeObserver(40, InputOff);
		MessageKit<bool>.removeObserver(6, Pause);
		MessageKit<bool>.removeObserver(7, Vision);
		MessageKit<bool>.removeObserver(103, ShowMenuButtons);
		MessageKit.removeObserver(17, Death);
		MessageKit.removeObserver(92, GuestDeath);
		MessageKit.removeObserver(53, Revive);
		MessageKit.removeObserver(105, Finished);
		MessageKit.removeObserver(38, Synchronize);
		ReleaseSource();
	}

	private void OnDestroy()
	{
		OnDisable();
		if (ReInput.isReady && ownedController != null && ReInput.controllers.GetCustomController(ownedController.id) == ownedController)
		{
			ReInput.controllers.DestroyCustomController(ownedController);
		}
		ownedController = null;
	}

	private void BindManager()
	{
		ControlModeManager current = ControlModeManager.Instance;
		if (manager != current)
		{
			ReleaseSource();
			UnbindManager();
			manager = current;
			if (manager != null)
			{
				manager.ModeChanged += ModeChanged;
				manager.AvailabilityChanged += Synchronize;
			}
		}
	}

	private void UnbindManager()
	{
		if (manager != null)
		{
			manager.ModeChanged -= ModeChanged;
			manager.AvailabilityChanged -= Synchronize;
		}
		manager = null;
	}

	private void ModeChanged(ControlMode mode)
	{
		Synchronize();
	}

	private void BindRewired()
	{
		ReInput.InputSourceUpdateEvent -= Synchronize;
		ReInput.InputSourceUpdateEvent += Synchronize;
		ReInput.PreShutDownEvent -= RewiredShuttingDown;
		ReInput.PreShutDownEvent += RewiredShuttingDown;
	}

	private void UnbindRewired()
	{
		ReInput.InputSourceUpdateEvent -= Synchronize;
		ReInput.PreShutDownEvent -= RewiredShuttingDown;
	}

	private void RewiredInitialized()
	{
		wrapperNeedsReinitialization = true;
		BindRewired();
	}

	private void RewiredShuttingDown()
	{
		wrapperNeedsReinitialization = true;
		UnbindRewired();
		ReleaseSource();
	}

	private void Update()
	{
		if (wrapperNeedsReinitialization && ReInput.isReady && wrapper != null && wrapper.isActiveAndEnabled)
		{
			wrapperNeedsReinitialization = false;
			wrapper.enabled = false;
			wrapper.enabled = true;
		}
		Synchronize();
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("EYES_INPUT_DIAGNOSTICS")]
	private void TraceState()
	{
		TraceStateUnchecked();
	}

	private void TraceStateUnchecked()
	{
		string state = "registered=" + (registration != null && registration.IsRegistered) + " delivering=" + (wrapper != null && wrapper.useCustomController) + " blocks=" + legacyBlocks;
		if (state != lastTraceState)
		{
			lastTraceState = state;
		}
	}

	public void Synchronize()
	{
		if (!subscribed || synchronizing || releasingSource)
		{
			return;
		}
		synchronizing = true;
		BindManager();
		if (wrapper != null && wrapper.isActiveAndEnabled && manager != null && manager.isActiveAndEnabled && ReInput.isReady)
		{
			bool previousUse = wrapper.useCustomController;
			Rewired.CustomController current;
			try
			{
				wrapper.useCustomController = true;
				current = wrapper.GetCustomController();
			}
			finally
			{
				wrapper.useCustomController = previousUse;
			}
			if (registration != null && (!registration.IsRegistered || controller != current))
			{
				ReleaseSource();
			}
			if (registration == null && current != null && manager.Player != null && manager.Player.controllers.ContainsController(current))
			{
				controller = current;
				if (wrapper._createCustomControllerSettingsCreates)
				{
					ownedController = current;
				}
				registration = manager.RegisterTouchController(controller, Suspend);
			}
			GameplayInputContext.Permissions state = policy.Current;
			bool allowed = registration != null && registration.IsRegistered && (int)manager.Mode == 0 && manager.HasFocus && !manager.RequiresRecoveryUI && legacyBlocks == LegacyBlock.None;
			bool move = allowed && state.Move;
			bool look = allowed && state.Look;
			if (!move && !look)
			{
				Suspend();
			}
			else
			{
				if (!wrapper.useCustomController)
				{
					ClearValues();
				}
				if (joystick != null)
				{
					joystick.interactable = move;
					joystick.visible = move;
					if (joystick.touchRegion != null)
					{
						joystick.touchRegion.interactable = move && UserGameSettings.dynamicJoystickEnabled;
					}
				}
				if (touchpad != null)
				{
					touchpad.interactable = look;
				}
				wrapper.useCustomController = true;
			}
		}
		else
		{
			ReleaseSource();
		}
		synchronizing = false;
	}

	private void Suspend()
	{
		if (wrapper != null)
		{
			wrapper.useCustomController = false;
		}
		if (joystick != null)
		{
			joystick.interactable = false;
			joystick.CancelPointer();
			joystick.visible = false;
			if (joystick.touchRegion != null)
			{
				joystick.touchRegion.interactable = false;
			}
		}
		if (touchpad != null)
		{
			touchpad.interactable = false;
			touchpad.CancelPointer();
		}
		ClearValues();
	}

	private void ClearValues()
	{
		if (wrapper != null)
		{
			wrapper.ClearControlValues();
		}
		if (!ReInput.isReady || controller == null || manager == null || manager.Player == null || !manager.Player.controllers.ContainsController(controller))
		{
			return;
		}
		for (int i = 0; i < controller.axisCount; i++)
		{
			controller.ClearAxisValue(i);
		}
		for (int i = 0; i < controller.buttonCount; i++)
		{
			controller.ClearButtonValue(i);
		}
	}

	private void ReleaseSource()
	{
		if (!releasingSource)
		{
			releasingSource = true;
			Suspend();
			ControlModeManager.SourceRegistration previous = registration;
			registration = null;
			controller = null;
			if (previous != null)
			{
				previous.Dispose();
			}
			releasingSource = false;
		}
	}

	private void SetLegacyBlock(LegacyBlock block, bool active)
	{
		legacyBlocks = active ? (legacyBlocks | block) : (legacyBlocks & ~block);
		Synchronize();
	}

	private void InputOn()
	{
		SetLegacyBlock(LegacyBlock.Input, false);
	}

	private void InputOff()
	{
		SetLegacyBlock(LegacyBlock.Input, true);
	}

	private void Pause(bool active)
	{
		SetLegacyBlock(LegacyBlock.Pause, active);
	}

	private void Vision(bool active)
	{
		SetLegacyBlock(LegacyBlock.Vision, active);
	}

	private void ShowMenuButtons(bool show)
	{
		SetLegacyBlock(LegacyBlock.Hidden, !show);
	}

	private void Death()
	{
		SetLegacyBlock(LegacyBlock.Death, true);
	}

	private void GuestDeath()
	{
		SetLegacyBlock(LegacyBlock.GuestDeath, true);
	}

	private void Revive()
	{
		SetLegacyBlock(LegacyBlock.Death, false);
	}

	private void Finished()
	{
		SetLegacyBlock(LegacyBlock.Finished, true);
	}
}
