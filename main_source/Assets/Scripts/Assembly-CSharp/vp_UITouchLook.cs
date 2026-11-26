using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class vp_UITouchLook : vp_UIControl
{
	public float AutoPitchSpeed = 2.75f;

	public float AutoPitchDelay = 3.5f;

	public bool AutoPitchWhenIdle = true;

	public Vector2 RotationSensitivity = new Vector2(0.3f, 0.2f);

	public bool AutopitchFoldout;

	protected Vector2 m_LookVector = Vector2.zero;

	protected bool m_ShouldAutoPitch;

	protected vp_Timer.Handle m_AutoPitchTimer = new vp_Timer.Handle();

	protected override void Start()
	{
		base.Start();
		OnEnable();
	}

	protected override void OnEnable()
	{
		if (m_Initialized)
		{
			base.OnEnable();
			vp_InputMobile.LookAxisCallback = (vp_RawAxisEventHandler)Delegate.Combine(vp_InputMobile.LookAxisCallback, new vp_RawAxisEventHandler(GetLook));
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		vp_InputMobile.LookAxisCallback = (vp_RawAxisEventHandler)Delegate.Remove(vp_InputMobile.LookAxisCallback, new vp_RawAxisEventHandler(GetLook));
	}

	protected override void Update()
	{
		AutoPitchUpdate();
	}

	protected virtual void AutoPitchUpdate()
	{
		if (base.LastFingerID != -1)
		{
			return;
		}
		m_LookVector = Vector2.zero;
		if (AutoPitchSpeed != 0f && m_ShouldAutoPitch && !Manager.Player.Climb.Active && (!(Manager.Player.Interactable.Get() != null) || !(Manager.Player.Interactable.Get().GetType() == typeof(vp_Grab))) && !Manager.Player.Zoom.Active && (vp_GlobalEventReturn<bool>.Send("SimulateTouchWithMouse") || !Application.isEditor) && !vp_GlobalEventReturn<bool>.Send("Tracking Enemy"))
		{
			bool flag = vp_Input.GetAxisRaw("Horizontal") != 0f || vp_Input.GetAxisRaw("Vertical") != 0f;
			if (flag || AutoPitchWhenIdle)
			{
				Manager.Player.Rotation.Set(Vector2.Lerp(Manager.Player.Rotation.Get(), new Vector2(0f, Manager.Player.Rotation.Get().y), Time.deltaTime * (flag ? AutoPitchSpeed : (AutoPitchSpeed * 0.5f))));
			}
		}
	}

	protected override void TouchesBegan(vp_Touch touch)
	{
		if (base.LastFingerID == -1 && RaycastControl(touch))
		{
			base.TouchesBegan(touch);
			m_ShouldAutoPitch = false;
			m_AutoPitchTimer.Cancel();
		}
	}

	protected virtual void TouchesMoved(vp_Touch touch)
	{
		if (base.LastFingerID == touch.FingerID)
		{
			m_LookVector = new Vector2((touch.DeltaPosition * 0.25f * RotationSensitivity.x).x, (touch.DeltaPosition * 0.25f * RotationSensitivity.y).y);
		}
	}

	protected virtual void TouchesStationary(vp_Touch touch)
	{
		if (base.LastFingerID == touch.FingerID)
		{
			m_LookVector = Vector2.zero;
		}
	}

	public override void TouchesFinished(vp_Touch touch)
	{
		if (base.LastFingerID != touch.FingerID)
		{
			return;
		}
		base.TouchesFinished(touch);
		if (AutoPitchSpeed != 0f)
		{
			vp_Timer.In(AutoPitchDelay, delegate
			{
				m_ShouldAutoPitch = true;
			}, m_AutoPitchTimer);
		}
	}

	protected virtual float GetLook(string id)
	{
		if (!(id == "Mouse X"))
		{
			if (!(id == "Mouse Y"))
			{
				return 0f;
			}
			return m_LookVector.y;
		}
		return m_LookVector.x;
	}
}
