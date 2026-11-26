using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class vp_UITouchButton : vp_UIControl
{
	public enum vp_UIButtonState
	{
		OnPress,
		OnRelease,
		OnHold,
		OnDoublePress,
		OnDoubleRelease
	}

	public bool RequireStayInBounds;

	public bool OverrideTouches;

	public string Action;

	public vp_UIButtonState Event;

	protected bool m_ButtonReleased = true;

	protected bool m_ButtonUp;

	protected bool m_IsEventBinding;

	protected bool m_ButtonOverride;

	protected override void Start()
	{
		m_IsEventBinding = Action == "Event Binding ⇆";
		base.Start();
	}

	protected override void OnEnable()
	{
		if (m_Initialized)
		{
			base.OnEnable();
			RegisterTouchCallbacks();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		UnregisterTouchCallbacks();
	}

	protected virtual void RegisterTouchCallbacks()
	{
		if (Event == vp_UIButtonState.OnHold && vp_InputMobile.ButtonHoldCallbacks.ContainsKey(Action))
		{
			Dictionary<string, vp_TouchButtonActionEventHandler> buttonHoldCallbacks = vp_InputMobile.ButtonHoldCallbacks;
			string action = Action;
			buttonHoldCallbacks[action] = (vp_TouchButtonActionEventHandler)Delegate.Combine(buttonHoldCallbacks[action], new vp_TouchButtonActionEventHandler(OnButtonHoldAction));
		}
		if ((Event == vp_UIButtonState.OnPress || Event == vp_UIButtonState.OnDoublePress) && vp_InputMobile.ButtonDownCallbacks.ContainsKey(Action))
		{
			Dictionary<string, vp_TouchButtonActionEventHandler> buttonHoldCallbacks = vp_InputMobile.ButtonDownCallbacks;
			string action = Action;
			buttonHoldCallbacks[action] = (vp_TouchButtonActionEventHandler)Delegate.Combine(buttonHoldCallbacks[action], new vp_TouchButtonActionEventHandler(OnButtonDownAction));
		}
		if ((Event == vp_UIButtonState.OnRelease || Event == vp_UIButtonState.OnDoubleRelease) && vp_InputMobile.ButtonUpCallbacks.ContainsKey(Action))
		{
			Dictionary<string, vp_TouchButtonActionEventHandler> buttonHoldCallbacks = vp_InputMobile.ButtonUpCallbacks;
			string action = Action;
			buttonHoldCallbacks[action] = (vp_TouchButtonActionEventHandler)Delegate.Combine(buttonHoldCallbacks[action], new vp_TouchButtonActionEventHandler(OnButtonUpAction));
		}
	}

	protected virtual void UnregisterTouchCallbacks()
	{
		if (Event == vp_UIButtonState.OnHold && vp_InputMobile.ButtonHoldCallbacks.ContainsKey(Action))
		{
			Dictionary<string, vp_TouchButtonActionEventHandler> buttonHoldCallbacks = vp_InputMobile.ButtonHoldCallbacks;
			string action = Action;
			buttonHoldCallbacks[action] = (vp_TouchButtonActionEventHandler)Delegate.Remove(buttonHoldCallbacks[action], new vp_TouchButtonActionEventHandler(OnButtonHoldAction));
		}
		if ((Event == vp_UIButtonState.OnPress || Event == vp_UIButtonState.OnDoublePress) && vp_InputMobile.ButtonDownCallbacks.ContainsKey(Action))
		{
			Dictionary<string, vp_TouchButtonActionEventHandler> buttonHoldCallbacks = vp_InputMobile.ButtonDownCallbacks;
			string action = Action;
			buttonHoldCallbacks[action] = (vp_TouchButtonActionEventHandler)Delegate.Remove(buttonHoldCallbacks[action], new vp_TouchButtonActionEventHandler(OnButtonDownAction));
		}
		if ((Event == vp_UIButtonState.OnRelease || Event == vp_UIButtonState.OnDoubleRelease) && vp_InputMobile.ButtonUpCallbacks.ContainsKey(Action))
		{
			Dictionary<string, vp_TouchButtonActionEventHandler> buttonHoldCallbacks = vp_InputMobile.ButtonUpCallbacks;
			string action = Action;
			buttonHoldCallbacks[action] = (vp_TouchButtonActionEventHandler)Delegate.Remove(buttonHoldCallbacks[action], new vp_TouchButtonActionEventHandler(OnButtonUpAction));
		}
	}

	protected override void Update()
	{
		HoldUpdate();
	}

	protected virtual void HoldUpdate()
	{
		if (Event == vp_UIButtonState.OnHold && base.LastFingerID != -1 && m_IsEventBinding && HoldControl != null)
		{
			HoldControl();
		}
	}

	protected virtual bool OnButtonHoldAction(string action, bool hold = false)
	{
		if (Action != action)
		{
			return false;
		}
		if (m_IsEventBinding)
		{
			return false;
		}
		if (base.LastFingerID == -1)
		{
			return false;
		}
		if (!m_ButtonReleased && !hold)
		{
			return false;
		}
		if (Event != vp_UIButtonState.OnHold)
		{
			return false;
		}
		m_ButtonReleased = false;
		return true;
	}

	protected virtual bool OnButtonDownAction(string action, bool hold = false)
	{
		if (Action != action)
		{
			return false;
		}
		if (m_IsEventBinding)
		{
			return false;
		}
		if (base.LastFingerID == -1)
		{
			return false;
		}
		if (!m_ButtonReleased)
		{
			return false;
		}
		if (Event != vp_UIButtonState.OnPress && Event != vp_UIButtonState.OnDoublePress)
		{
			return false;
		}
		if (Event == vp_UIButtonState.OnDoublePress && m_TapCount != 2f)
		{
			return false;
		}
		if (!base.enabled)
		{
			return false;
		}
		StartCoroutine(WaitFrames(5));
		return true;
	}

	protected virtual IEnumerator WaitFrames(int frames)
	{
		for (int i = 0; i < frames; i++)
		{
			yield return new WaitForEndOfFrame();
		}
		m_ButtonReleased = false;
	}

	protected virtual bool OnButtonUpAction(string action, bool hold = false)
	{
		if (Action != action)
		{
			return false;
		}
		if (m_IsEventBinding)
		{
			return false;
		}
		if (base.LastFingerID == -1)
		{
			return false;
		}
		if (!m_ButtonUp)
		{
			return false;
		}
		if (Event != vp_UIButtonState.OnRelease && Event != vp_UIButtonState.OnDoubleRelease)
		{
			return false;
		}
		if (!base.enabled)
		{
			return false;
		}
		base.LastFingerID = -1;
		m_ButtonUp = false;
		return true;
	}

	protected override void TouchesBegan(vp_Touch touch)
	{
		if (base.LastFingerID == -1 && RaycastControl(touch))
		{
			base.TouchesBegan(touch);
			m_ButtonReleased = true;
			if (PressControl != null)
			{
				PressControl();
			}
			if (m_TapCount == 2f && DoublePressControl != null)
			{
				DoublePressControl();
			}
		}
	}

	protected virtual void TouchesMoved(vp_Touch touch)
	{
		if (RequireStayInBounds && (Event == vp_UIButtonState.OnDoubleRelease || Event == vp_UIButtonState.OnRelease) && base.LastFingerID != 1 && !RaycastControl(touch))
		{
			base.LastFingerID = -1;
		}
		if (OverrideTouches && !(Manager == null) && RaycastControl(touch) && Manager.FingerIDs.ContainsKey(touch.FingerID) && !m_ButtonOverride)
		{
			List<vp_UIControl> list = Manager.FingerIDs[touch.FingerID];
			for (int i = 0; i < list.Count; i++)
			{
				list[i].TouchesFinished(touch);
			}
			base.LastFingerID = touch.FingerID;
			m_ButtonOverride = true;
		}
	}

	public override void TouchesFinished(vp_Touch touch)
	{
		if (base.LastFingerID != touch.FingerID)
		{
			return;
		}
		if ((Event != vp_UIButtonState.OnRelease && Event != vp_UIButtonState.OnDoubleRelease) || !RaycastControl(touch))
		{
			base.LastFingerID = -1;
		}
		if (RaycastControl(touch))
		{
			m_ButtonUp = true;
			if (ReleaseControl != null)
			{
				ReleaseControl();
			}
			if (m_TapCount == 2f && DoubleReleaseControl != null)
			{
				DoubleReleaseControl();
			}
			if (Event == vp_UIButtonState.OnDoubleRelease && m_TapCount != 2f)
			{
				m_LastFingerID = -1;
				m_ButtonUp = false;
			}
		}
		m_ButtonOverride = false;
		m_ButtonReleased = true;
	}
}
