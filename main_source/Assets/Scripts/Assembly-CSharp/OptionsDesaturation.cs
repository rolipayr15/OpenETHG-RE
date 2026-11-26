using UnityEngine;
using UnityEngine.UI;

public class OptionsDesaturation : MonoBehaviour
{
	private const float MinDesaturation = 0.5f;

	[SerializeField]
	private Slider slider;

	private void Start()
	{
		slider.value = GetSavedValue(1f - Shader.GetGlobalFloat("_DesaturationLevel"));
	}

	public void SetDesaturation(float value)
	{
		float clamped = Mathf.Clamp(value, MinDesaturation, 1f);
		PlayerPrefs.SetFloat("desaturation", clamped);
		EyesLightingComposer.SetDesaturationOverride(1f - clamped);
	}

	public static void ApplySavedDesaturation()
	{
		if (!PlayerPrefs.HasKey("desaturation"))
		{
			EyesLightingComposer.SetDesaturationOverride(null);
			return;
		}
		EyesLightingComposer.SetDesaturationOverride(1f - GetSavedValue(MinDesaturation));
	}

	private static float GetSavedValue(float defaultValue)
	{
		float saved = PlayerPrefs.GetFloat("desaturation", defaultValue);
		return Mathf.Clamp(float.IsNaN(saved) || float.IsInfinity(saved) ? defaultValue : saved, MinDesaturation, 1f);
	}
}
