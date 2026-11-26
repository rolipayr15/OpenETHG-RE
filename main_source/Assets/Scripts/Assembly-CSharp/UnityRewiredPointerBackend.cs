using UnityEngine;

public sealed class UnityRewiredPointerBackend : IPointerBackend, MouseManager.ICaptureBackend
{
	private int sampledFrame = -1;

	private int resetFrame = -1;

	private PointerFrame sample;

	public string Id => "UnityRewired";

	public bool IsAvailable => !Application.isMobilePlatform;

	public bool IsCaptured => Cursor.lockState == CursorLockMode.Locked;

	public bool ReplacesKeyboard => false;

	public bool ReplacesMouse => false;

	public ControlModeManager.DevicePresence KeyboardPresence => ControlModeManager.DevicePresence.Unknown;

	public ControlModeManager.DevicePresence MousePresence => ControlModeManager.DevicePresence.Unknown;

	public int ConnectionLossVersion => 0;

	public bool PrimaryButtonDown => Sample(Time.frameCount).ButtonDown(0);

	public Vector2 PointerPosition => Sample(Time.frameCount).PointerPosition;

	public void RequestCapture()
	{
		Cursor.lockState = CursorLockMode.Locked;
	}

	public void ReleaseCapture()
	{
		Cursor.lockState = CursorLockMode.None;
	}

	public void SetPointerVisible(bool visible)
	{
		Cursor.visible = visible;
	}

	public PointerFrame Sample(int unityFrame)
	{
		if (sampledFrame == unityFrame)
		{
			return sample;
		}
		sampledFrame = unityFrame;
		// Deltas and edges are suppressed for the frame an input reset happened on.
		bool fresh = resetFrame != unityFrame;
		sample = new PointerFrame
		{
			Frame = unityFrame,
			PointerPosition = Input.mousePosition,
			HasPosition = true
		};
		if (Rewired.ReInput.isReady)
		{
			Rewired.Mouse mouse = Rewired.ReInput.controllers.Mouse;
			if (mouse != null)
			{
				for (int i = 0; i < 3 && i < mouse.buttonCount; i++)
				{
					uint bit = 1u << i;
					if (mouse.GetButton(i))
					{
						sample.HeldButtons |= bit;
					}
					if (fresh && mouse.GetButtonDown(i))
					{
						sample.DownButtons |= bit;
					}
					if (fresh && mouse.GetButtonUp(i))
					{
						sample.UpButtons |= bit;
					}
				}
				if (fresh)
				{
					sample.RelativeDelta = new Vector2(mouse.GetAxisRaw(0), mouse.GetAxisRaw(1));
					sample.PointerDelta = mouse.screenPositionDelta;
					sample.Scroll = Input.mouseScrollDelta;
				}
			}
		}
		return sample;
	}

	public void ResetInput()
	{
		sampledFrame = -1;
		resetFrame = Time.frameCount;
	}
}
