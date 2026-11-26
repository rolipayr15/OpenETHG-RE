using UnityEngine;

public abstract class BufferedPointerBackend : IPointerBackend, MouseManager.ICaptureBackend
{
	private readonly object gate;

	private Vector2 delta;

	private Vector2 position;

	private Vector2 positionDelta;

	private Vector2 scroll;

	private uint buttons;

	private uint down;

	private uint up;

	private ulong keys;

	private ulong keyDown;

	private ulong keyUp;

	private bool hasPosition;

	private bool available;

	private bool captured;

	private bool captureRequested;

	private int generation;

	private int lossVersion;

	private int sampledFrame;

	private int resetFrame;

	private PointerFrame cached;

	private ControlModeManager.DevicePresence keyboard;

	private ControlModeManager.DevicePresence mouse;

	public abstract string Id { get; }

	public abstract bool ReplacesKeyboard { get; }

	public bool ReplacesMouse => false;

	public bool IsAvailable => false;

	public bool IsCaptured => false;

	public ControlModeManager.DevicePresence KeyboardPresence => ControlModeManager.DevicePresence.Unknown;

	public ControlModeManager.DevicePresence MousePresence => ControlModeManager.DevicePresence.Unknown;

	public int ConnectionLossVersion => 0;

	public bool PrimaryButtonDown => false;

	public Vector2 PointerPosition => default;

	protected virtual bool KeepsUncapturedDelta => false;

	private bool KeepsDelta => false;

	public void RequestCapture()
	{
	}

	public void ReleaseCapture()
	{
	}

	protected abstract void RequestPlatformCapture(int generation);

	protected abstract void ReleasePlatformCapture(int generation);

	public abstract void SetPointerVisible(bool visible);

	public void ReportCapture(int token, bool confirmed)
	{
	}

	public void ReportDevices(bool relativeCaptureAvailable, ControlModeManager.DevicePresence keyboardPresence, ControlModeManager.DevicePresence mousePresence)
	{
	}

	public void ReportRelativeDelta(float x, float y)
	{
	}

	public void ReportPosition(float x, float y)
	{
	}

	protected void InvalidatePointerPosition()
	{
	}

	public void ReportScroll(float x, float y)
	{
	}

	public void ReportButton(int index, bool pressed)
	{
	}

	public void ReportKey(int index, bool pressed)
	{
	}

	public PointerFrame Sample(int unityFrame)
	{
		return default;
	}

	public virtual void ResetInput()
	{
	}

	private static bool Finite(float value)
	{
		return false;
	}
}
