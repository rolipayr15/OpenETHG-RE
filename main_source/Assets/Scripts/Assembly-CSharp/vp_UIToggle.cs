using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class vp_UIToggle : vp_UIControl
{
	public GameObject Background;

	public GameObject Checkmark;

	public bool State = true;

	protected virtual void OnPressControl()
	{
		State = !State;
		vp_Utility.Activate(Checkmark, State);
		if (ChangeControl != null)
		{
			ChangeControl(this);
		}
	}
}
