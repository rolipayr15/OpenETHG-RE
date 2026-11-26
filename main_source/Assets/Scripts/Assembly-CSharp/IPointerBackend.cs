public interface IPointerBackend : MouseManager.ICaptureBackend
{
	bool ReplacesKeyboard { get; }

	bool ReplacesMouse { get; }

	ControlModeManager.DevicePresence KeyboardPresence { get; }

	ControlModeManager.DevicePresence MousePresence { get; }

	int ConnectionLossVersion { get; }

	PointerFrame Sample(int unityFrame);
}
