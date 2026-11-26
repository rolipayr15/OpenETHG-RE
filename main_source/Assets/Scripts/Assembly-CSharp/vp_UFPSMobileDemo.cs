using UnityEngine;

public class vp_UFPSMobileDemo : MonoBehaviour
{
	public vp_UITouchController TouchController;

	public GameObject Menu;

	public GameObject UI;

	public vp_UITouchLook TouchLookPad;

	public vp_SimpleHUDMobile SimpleHUD;

	protected bool m_Initialized;

	protected float m_AutoPitchSpeed;

	protected virtual void Awake()
	{
		if ((bool)TouchLookPad)
		{
			m_AutoPitchSpeed = TouchLookPad.AutoPitchSpeed;
		}
	}

	public virtual void ChangeControllerType(vp_UIControl control)
	{
		string currentItem = ((vp_UIDropdownList)control).CurrentItem;
		if (currentItem == "Dynamic Joystick")
		{
			TouchController.ControllerType = vp_UITouchController.vp_TouchControllerType.DynamicJoystick;
		}
		else if (currentItem == "Static Joystick")
		{
			TouchController.ControllerType = vp_UITouchController.vp_TouchControllerType.StaticJoystick;
		}
		else
		{
			TouchController.ControllerType = vp_UITouchController.vp_TouchControllerType.TouchPad;
		}
	}

	public virtual void ToggleMenu()
	{
		vp_Utility.Activate(Menu, !vp_Utility.IsActive(Menu));
		vp_Utility.Activate(UI, !vp_Utility.IsActive(UI));
		vp_TimeUtility.Paused = !vp_TimeUtility.Paused;
	}

	public virtual void AutoPitchToggle(vp_UIControl control)
	{
		if (!(TouchLookPad == null))
		{
			bool state = ((vp_UIToggle)control).State;
			TouchLookPad.AutoPitchSpeed = ((!state) ? 0f : ((m_AutoPitchSpeed == 0f) ? 2.75f : m_AutoPitchSpeed));
		}
	}

	public virtual void HelpTipsToggle(vp_UIControl control)
	{
		if (!(SimpleHUD == null))
		{
			bool state = ((vp_UIToggle)control).State;
			SimpleHUD.ShowTips = state;
		}
	}
}
