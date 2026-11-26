using System;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(1000)]
public sealed class HudKeyboardHints : MonoBehaviour
{
	[Serializable]
	private sealed class Hint
	{
		public Graphic icon;

		public string key;

		public bool leftOfIcon;

		[Tooltip("Rewired Gameplay action whose gamepad button is shown in Gamepad mode (-1: none).")]
		public int action = -1;

		[NonSerialized]
		public Text label;
	}

	[SerializeField]
	private Font font;

	[SerializeField]
	private Hint[] hints = new Hint[0];

	private void Awake()
	{
		foreach (Hint hint in hints)
		{
			if (!(hint.icon == null))
			{
				RectTransform rect = new GameObject("KeyboardHint", typeof(RectTransform)).GetComponent<RectTransform>();
				rect.SetParent(hint.icon.transform, false);
				rect.gameObject.layer = hint.icon.gameObject.layer;
				float anchorX = (hint.leftOfIcon ? 0f : 1f);
				rect.anchorMax = new Vector2(anchorX, 0.2f);
				rect.anchorMin = new Vector2(anchorX, 0.2f);
				rect.pivot = new Vector2(hint.leftOfIcon ? 1f : 0f, 0.5f);
				rect.anchoredPosition = new Vector2(hint.leftOfIcon ? (-12f) : 12f, 0f);
				rect.sizeDelta = new Vector2(100f, 40f);
				Text text = rect.gameObject.AddComponent<Text>();
				text.font = ((font != null) ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));
				text.text = hint.key;
				text.fontSize = 32;
				text.color = new Color(1f, 1f, 1f, 0.85f);
				text.alignment = (hint.leftOfIcon ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft);
				text.horizontalOverflow = HorizontalWrapMode.Overflow;
				text.supportRichText = false;
				text.raycastTarget = false;
				Shadow shadow = rect.gameObject.AddComponent<Shadow>();
				shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
				shadow.effectDistance = new Vector2(1.5f, -1.5f);
				hint.label = text;
				text.enabled = false;
			}
		}
	}

	private void OnEnable()
	{
		Refresh();
	}

	private void LateUpdate()
	{
		Refresh();
	}

	private void Refresh()
	{
		ControlModeManager manager = ControlModeManager.Instance;
		int mode = (int)((manager != null) ? manager.Mode : UserGameSettings.controlMode);
		bool gamepad = mode == 2 && UserGameSettings.showControllerHints;
		bool keyboard = mode == 1 && UserGameSettings.showKeyboardHints;
		foreach (Hint hint in hints)
		{
			if (!(hint.label == null))
			{
				string gamepadButton = ((gamepad && hint.action >= 0) ? GamepadBindingLabels.Label(hint.action) : null);
				string value = HintText(keyboard, gamepad, hint.key, gamepadButton);
				bool visible = !string.IsNullOrEmpty(value) && hint.icon != null && hint.icon.isActiveAndEnabled;
				if (visible && hint.label.text != value)
				{
					hint.label.text = value;
				}
				hint.label.enabled = visible;
			}
		}
	}

	public static string HintText(bool keyboard, bool gamepad, string key, string gamepadButton)
	{
		if (keyboard)
		{
			return key;
		}
		return gamepad ? gamepadButton : null;
	}

	private void OnDisable()
	{
		foreach (Hint hint in hints)
		{
			if (hint.label != null)
			{
				hint.label.enabled = false;
			}
		}
	}

	private void OnDestroy()
	{
		foreach (Hint hint in hints)
		{
			if (hint.label != null)
			{
				UnityEngine.Object.Destroy(hint.label.gameObject);
			}
		}
	}
}
