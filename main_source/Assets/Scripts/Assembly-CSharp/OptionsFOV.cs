using UnityEngine;
using UnityEngine.UI;

public class OptionsFOV : MonoBehaviour
{
	public const float DefaultFov = 60f;

	public const float MinFov = 50f;

	public const float MaxFov = 85f;

	[SerializeField]
	private Slider slider;

	[SerializeField]
	private Text fovLabel;

	private static bool presetPatchWarningLogged;

	private void Start()
	{
		float saved = GetSavedFov();
		if (slider != null)
		{
			slider.minValue = 50f;
			slider.maxValue = 85f;
			slider.wholeNumbers = true;
			slider.value = saved;
		}
		else
		{
			Debug.LogWarning("OptionsFOV slider is not assigned.", this);
		}
		if (fovLabel == null)
		{
			Debug.LogWarning("OptionsFOV label is not assigned.", this);
		}
		UpdateLabel(saved);
		ApplyFov(saved);
	}

	public void SetFov(float value)
	{
		float fov = Mathf.Round(Mathf.Clamp(value, 50f, 85f));
		UpdateLabel(fov);
		ApplyFov(fov);
	}

	public static float GetSavedFov()
	{
		if (!UserGameSettings.fov.HasValue)
		{
			UserGameSettings.fov = PlayerPrefs.GetFloat("fov", 60f);
		}
		float fov = Mathf.Round(Mathf.Clamp(UserGameSettings.fov.Value, 50f, 85f));
		UserGameSettings.fov = fov;
		return fov;
	}

	public static void ApplySavedFov()
	{
		ApplyFov(GetSavedFov());
	}

	public static void ApplyFov(float value)
	{
		float fov = Mathf.Round(Mathf.Clamp(value, 50f, 85f));
		UserGameSettings.fov = fov;
		PlayerPrefs.SetFloat("fov", fov);
		PlayerManager manager = CustomSingleton<PlayerManager>.Instance;
		if (manager == null || manager.PlayerCamera == null)
		{
			return;
		}
		vp_FPCamera camera = manager.PlayerCamera.GetComponent<vp_FPCamera>();
		if (camera == null)
		{
			return;
		}
		camera.RenderingFieldOfView = fov;
		if (camera.DefaultState != null && camera.DefaultState.Preset != null && !camera.DefaultState.Preset.SetFieldValue("RenderingFieldOfView", fov) && !presetPatchWarningLogged)
		{
			presetPatchWarningLogged = true;
			Debug.LogWarning("Could not update the UFPS default camera FOV preset.");
		}
		PatchStatePresetFov(camera, "Reload", fov);
		camera.Refresh();
	}

	private static void PatchStatePresetFov(vp_FPCamera camera, string stateName, float value)
	{
		if (camera == null || camera.States == null)
		{
			return;
		}
		foreach (vp_State state in camera.States)
		{
			if (state != null && state.Name == stateName && state.Preset != null)
			{
				state.Preset.SetFieldValue("RenderingFieldOfView", value);
			}
		}
	}

	private void UpdateLabel(float value)
	{
		if (fovLabel != null)
		{
			fovLabel.text = value.ToString("F0");
		}
	}
}
