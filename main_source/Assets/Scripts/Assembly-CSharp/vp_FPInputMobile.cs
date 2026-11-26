using UnityEngine;

public class vp_FPInputMobile : vp_FPInput
{
	protected vp_FPCamera m_FPCamera;

	public vp_FPCamera FPCamera
	{
		get
		{
			if (m_FPCamera == null)
			{
				m_FPCamera = base.transform.root.GetComponentInChildren<vp_FPCamera>();
			}
			return m_FPCamera;
		}
	}

	protected override void Start()
	{
		FPCamera.SetRotation(FPCamera.Transform.eulerAngles, stopZoomAndSprings: false, obsolete: true);
		base.FPPlayer.Zoom.MinPause = 0.25f;
		base.FPPlayer.Zoom.MinDuration = 0.25f;
		MouseCursorBlocksMouseLook = false;
	}

	protected override void InputInteract()
	{
		if (base.FPPlayer.CanInteract.Get())
		{
			if (vp_GlobalEventReturn<bool>.Send("SimulateTouchWithMouse") ? vp_Input.GetButtonAny("Interact") : vp_Input.GetButtonUp("Interact"))
			{
				base.FPPlayer.Interact.TryStart();
			}
			else
			{
				base.FPPlayer.Interact.TryStop();
			}
		}
	}

	protected override void InputAttack()
	{
		if (!base.FPPlayer.Reload.Active)
		{
			if (vp_Input.GetButtonAny("Attack"))
			{
				base.FPPlayer.Attack.TryStart();
			}
			else
			{
				base.FPPlayer.Attack.TryStop();
			}
		}
	}

	protected override void InputJump()
	{
		if (vp_Input.GetButtonAny("Jump"))
		{
			base.FPPlayer.Jump.TryStart();
		}
		else
		{
			base.FPPlayer.Jump.Stop();
		}
	}

	protected override void InputReload()
	{
		if (vp_Input.GetButtonAny("Reload"))
		{
			base.FPPlayer.Reload.TryStart();
		}
	}

	protected virtual bool CanStart_Zoom()
	{
		return true;
	}

	protected virtual void OnStart_Reload()
	{
		base.FPPlayer.Zoom.Stop();
	}

	protected override void InputCamera()
	{
		if ((vp_GlobalEventReturn<bool>.Send("SimulateTouchWithMouse") ? vp_Input.GetButtonAny("Zoom") : vp_Input.GetButtonDown("Zoom")) && !(Time.time < base.FPPlayer.Zoom.NextAllowedStartTime))
		{
			if (base.FPPlayer.Zoom.Active)
			{
				base.FPPlayer.Zoom.TryStop();
			}
			else
			{
				base.FPPlayer.Zoom.TryStart();
			}
		}
	}

	protected override void InputRun()
	{
		if (vp_Input.GetButtonAny("Run"))
		{
			if (vp_GlobalEventReturn<bool>.Send("SimulateTouchWithMouse"))
			{
				base.FPPlayer.InputMoveVector.Set(new Vector2(0f, 1f));
			}
			else
			{
				base.FPPlayer.InputMoveVector.Set(new Vector2(vp_Input.GetAxisRaw("Horizontal"), vp_Input.GetAxisRaw("Vertical")));
			}
			base.FPPlayer.Run.TryStart();
		}
		else
		{
			base.FPPlayer.Run.TryStop();
		}
	}

	protected override void UpdatePause()
	{
		if (vp_Input.GetButtonAny("Pause"))
		{
			base.FPPlayer.Pause.Set(!base.FPPlayer.Pause.Get());
		}
	}

	protected override void UpdateCursorLock()
	{
		if (vp_GlobalEventReturn<bool>.Send("SimulateTouchWithMouse"))
		{
			return;
		}
		m_MousePos.x = Input.mousePosition.x;
		m_MousePos.y = (float)Screen.height - Input.mousePosition.y;
		if (MouseCursorForced)
		{
			if (vp_Utility.LockCursor)
			{
				vp_Utility.LockCursor = false;
			}
			return;
		}
		if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
		{
			if (MouseCursorZones.Length != 0)
			{
				Rect[] mouseCursorZones = MouseCursorZones;
				int num = 0;
				while (num < mouseCursorZones.Length)
				{
					Rect rect = mouseCursorZones[num];
					if (!rect.Contains(m_MousePos))
					{
						num++;
						continue;
					}
					goto IL_0097;
				}
			}
			if (!vp_Utility.LockCursor)
			{
				vp_Utility.LockCursor = true;
			}
		}
		goto IL_00bd;
		IL_00bd:
		if (vp_Input.GetButtonUp("Accept1") || vp_Input.GetButtonUp("Accept2") || vp_Input.GetButtonUp("Menu"))
		{
			vp_Utility.LockCursor = !vp_Utility.LockCursor;
		}
		return;
		IL_0097:
		if (vp_Utility.LockCursor)
		{
			vp_Utility.LockCursor = false;
		}
		goto IL_00bd;
	}
}
