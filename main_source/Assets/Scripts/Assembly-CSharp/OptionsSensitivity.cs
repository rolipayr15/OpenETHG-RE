using UnityEngine;
using UnityEngine.UI;

public class OptionsSensitivity : MonoBehaviour
{
	public const float MinTouchSensitivity = 0.3f;

	public const float DefaultTouchSensitivity = 5f;

	public const float MaxTouchSensitivity = 14f;

	[SerializeField]
	private Slider slider;

	[SerializeField]
	private Text sensitivityLabel;

	private bool isInitialValueReady;

	private void Start()
	{
		RefreshSavedValue();
		isInitialValueReady = true;
		ApplySavedSensitivity();
	}

	public void RefreshSavedValue()
	{
		float value = LoadTouchSensitivity();
		slider.value = value;
		sensitivityLabel.text = value.ToString("F1");
	}

	public void SetSensitivity(float value)
	{
		if (isInitialValueReady)
		{
			SaveTouchSensitivity(value);
			sensitivityLabel.text = value.ToString("F1");
		}
	}

	public static void SaveTouchSensitivity(float value)
	{
		float sanitized = SanitizeTouchSensitivity(value);
		UserGameSettings.sensitivity = sanitized;
		PlayerPrefs.SetFloat("sensitivity", sanitized);
		PlayerPrefs.Save();
		ApplySavedSensitivity();
	}

	public static float LoadTouchSensitivity()
	{
		float value = SanitizeTouchSensitivity(PlayerPrefs.GetFloat("sensitivity", DefaultTouchSensitivity));
		UserGameSettings.sensitivity = value;
		return value;
	}

	private static float SanitizeTouchSensitivity(float value)
	{
		return float.IsNaN(value) || float.IsInfinity(value) ? DefaultTouchSensitivity : Mathf.Clamp(value, MinTouchSensitivity, MaxTouchSensitivity);
	}

	public static void ApplySavedSensitivity(vp_FPInput playerInput = null)
	{
		if (playerInput == null)
		{
			PlayerManager manager = CustomSingleton<PlayerManager>.Instance;
			if (manager == null || manager.Player == null)
			{
				return;
			}
			playerInput = manager.Player.GetComponent<vp_FPInput>();
			if (playerInput == null)
			{
				return;
			}
		}
		ControlModeManager controls = ControlModeManager.Instance;
		ControlMode mode = controls != null ? controls.Mode : UserGameSettings.controlMode;
		float sensitivity;
		bool invert = false;
		if (mode == ControlMode.KeyboardMouse)
		{
			sensitivity = UserGameSettings.mouseSensitivity;
			invert = UserGameSettings.mouseInvertY;
		}
		else
		{
			sensitivity = LoadTouchSensitivity();
		}
		playerInput.MouseLookSensitivity = new Vector2(sensitivity, sensitivity);
		playerInput.MouseLookInvert = invert;
		if (playerInput.DefaultState != null && playerInput.DefaultState.Preset != null)
		{
			playerInput.DefaultState.Preset.SetFieldValue("MouseLookSensitivity", playerInput.MouseLookSensitivity);
			playerInput.DefaultState.Preset.SetFieldValue("MouseLookInvert", playerInput.MouseLookInvert);
		}
	}
}
