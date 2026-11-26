using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Scene-authored control options window; forwards every UI event to the OptionsControlMode that owns it.
public sealed class ControlOptionsWindow : MonoBehaviour
{
	[SerializeField]
	private RectTransform backdrop;

	[SerializeField]
	private List<Button> modeButtons = new List<Button>();

	[SerializeField]
	private Slider mouseSensitivity;

	[SerializeField]
	private Slider touchSensitivity;

	[SerializeField]
	private Toggle invertY;

	[SerializeField]
	private Toggle movingJoystick;

	[SerializeField]
	private Toggle showKeyboardHints;

	[Tooltip("Optional. Left empty, the host adds a copy of Show Keyboard Hints for the controller buttons.")]
	[SerializeField]
	private Toggle showControllerHints;

	[SerializeField]
	private Text status;

	[SerializeField]
	private Text hints;

	[SerializeField]
	private Text mouseValue;

	[SerializeField]
	private Text touchValue;

	[SerializeField]
	private ScrollRect scroll;

	[SerializeField]
	private OptionsControlMode.LocalizedLabel[] sceneLabels = new OptionsControlMode.LocalizedLabel[0];

	private OptionsControlMode owner;

	private readonly Vector3[] canvasCorners = new Vector3[4];

	public List<Button> ModeButtons => modeButtons;

	public Slider MouseSensitivity => mouseSensitivity;

	public Slider TouchSensitivity => touchSensitivity;

	public Toggle InvertY => invertY;

	public Toggle MovingJoystick => movingJoystick;

	public Toggle ShowKeyboardHints => showKeyboardHints;

	public Toggle ShowControllerHints => showControllerHints;

	public Text Status => status;

	public Text Hints => hints;

	public Text MouseValue => mouseValue;

	public Text TouchValue => touchValue;

	public ScrollRect Scroll => scroll;

	public OptionsControlMode.LocalizedLabel[] SceneLabels => sceneLabels;

	private void OnEnable()
	{
		FitBackdrop();
	}

	private void LateUpdate()
	{
		FitBackdrop();
	}

	// Stretches the dimming backdrop over the whole root canvas whatever the window's own anchoring.
	private void FitBackdrop()
	{
		if (backdrop == null)
		{
			return;
		}
		Canvas canvas = GetComponentInParent<Canvas>();
		if (canvas == null)
		{
			return;
		}
		RectTransform root = canvas.rootCanvas.transform as RectTransform;
		if (root == null)
		{
			return;
		}
		root.GetWorldCorners(canvasCorners);
		Vector3 min = transform.InverseTransformPoint(canvasCorners[0]);
		Vector3 max = transform.InverseTransformPoint(canvasCorners[2]);
		Vector3 center = (min + max) * 0.5f;
		if (backdrop.localPosition != center)
		{
			backdrop.localPosition = center;
		}
		Vector2 size = new Vector2(max.x - min.x, max.y - min.y);
		if (backdrop.sizeDelta != size)
		{
			backdrop.sizeDelta = size;
		}
	}

	public void Bind(OptionsControlMode host)
	{
		owner = host;
	}

	public void Close()
	{
		if (owner != null)
		{
			owner.Close();
		}
	}

	public void SelectMode(int value)
	{
		if (owner != null)
		{
			owner.SelectMode(value);
		}
	}

	public void SetMouseSensitivity(float value)
	{
		if (owner != null)
		{
			owner.SetMouseSensitivity(value);
		}
	}

	public void SetTouchSensitivity(float value)
	{
		if (owner != null)
		{
			owner.SetTouchSensitivity(value);
		}
	}

	public void SetMouseInvertY(bool value)
	{
		if (owner != null)
		{
			owner.SetMouseInvertY(value);
		}
	}

	public void SetMovingJoystick(bool value)
	{
		if (owner != null)
		{
			owner.SetMovingJoystick(value);
		}
	}

	public void SetShowKeyboardHints(bool value)
	{
		if (owner != null)
		{
			owner.SetShowKeyboardHints(value);
		}
	}

	public void SetShowControllerHints(bool value)
	{
		if (owner != null)
		{
			owner.SetShowControllerHints(value);
		}
	}
}
