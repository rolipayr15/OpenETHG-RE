using UnityEngine;
using UnityEngine.UI;

public sealed class HudInputNotice : MonoBehaviour
{
	public enum Message
	{
		None = 0,
		CaptureWithheld = 1,
		ClickToContinue = 2
	}

	[SerializeField]
	private Text label;

	private Message shown;

	private void Awake()
	{
		if (label != null)
		{
			label.raycastTarget = false;
			label.enabled = false;
		}
	}

	private void LateUpdate()
	{
		Message message = label != null ? Choose(MouseManager.WithheldNoticeDue, MouseManager.PointerResumeHintVisible) : Message.None;
		if (message == shown)
		{
			return;
		}
		shown = message;
		if (message == Message.ClickToContinue)
		{
			label.text = OptionsControlMode.Translate("Input/ClickToContinue");
		}
		else if (message == Message.CaptureWithheld)
		{
			label.text = OptionsControlMode.Translate(WithheldTerm(SystemInfo.deviceModel, OSVersion(), MouseManager.WithheldAwaitsClick, MouseManager.WithheldLooksWithoutLock));
		}
		if (label != null)
		{
			label.enabled = message != Message.None;
		}
	}

	private void OnDisable()
	{
		shown = Message.None;
		if (label != null)
		{
			label.enabled = false;
		}
	}

	public static Message Choose(bool captureWithheld, bool clickToResume)
	{
		if (captureWithheld)
		{
			return Message.CaptureWithheld;
		}
		return clickToResume ? Message.ClickToContinue : Message.None;
	}

	public static string WithheldTerm(string deviceModel, string systemVersion, bool awaitsClick, bool looksWithoutLock = false)
	{
		if (awaitsClick)
		{
			return "Input/ClickToContinue";
		}
		if (looksWithoutLock)
		{
			return "Input/MouseCaptureWithheldPhone";
		}
		bool notIPad = deviceModel == null || !deviceModel.StartsWith("iPad", System.StringComparison.Ordinal);
		if (systemVersion != null)
		{
			int dot = systemVersion.IndexOf('.');
			if (dot >= 0)
			{
				systemVersion = systemVersion.Substring(0, dot);
			}
		}
		bool parsed = int.TryParse(systemVersion, out int major);
		return major < 26 || !parsed || notIPad ? "Input/MouseCaptureWithheldLegacy" : "Input/MouseCaptureWithheld";
	}

	private static string OSVersion()
	{
		return "";
	}
}
