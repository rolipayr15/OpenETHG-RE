using UnityEngine;
using UnityEngine.UI;

public class OptionsBrightness : MonoBehaviour
{
	[SerializeField]
	private Slider slider;

	[Tooltip("Optional. Shows the multiplier as a percent of the authored look, like the calibrator.")]
	[SerializeField]
	private Text readout;

	private bool suppress;

	private void Start()
	{
		suppress = true;
		slider.minValue = -1f;
		slider.maxValue = 1f;
		slider.value = LightingSettings.ToStops(LightingSettings.BrightnessMultiplier);
		suppress = false;
		WriteReadout();
	}

	private void OnEnable()
	{
		LightingSettings.Changed += Refresh;
	}

	private void OnDisable()
	{
		LightingSettings.Changed -= Refresh;
	}

	private void Refresh()
	{
		if (slider != null)
		{
			slider.value = LightingSettings.ToStops(LightingSettings.BrightnessMultiplier);
		}
		WriteReadout();
	}

	public void WriteReadout()
	{
		if (readout != null)
		{
			readout.text = BrightnessCalibration.ReadoutText(LightingSettings.BrightnessMultiplier);
		}
	}

	public void SetBrightness(float stops)
	{
		if (!suppress)
		{
			LightingSettings.SetBrightness(LightingSettings.FromStops(stops), true);
		}
	}
}
