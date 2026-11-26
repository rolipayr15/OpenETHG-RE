using UnityEngine;

[CreateAssetMenu(fileName = "BrightnessCalibrationConfig", menuName = "Eyes/Brightness Calibration Config")]
public sealed class BrightnessCalibrationConfig : ScriptableObject
{
	[Range(0.005f, 0.5f)]
	[Tooltip("L = T: luminance (0..1, gamma space) of the darkest step visible on the reference phone. Written by the Luminance Probe window from the targets asset.")]
	public float darkSymbolLuminance;
}
