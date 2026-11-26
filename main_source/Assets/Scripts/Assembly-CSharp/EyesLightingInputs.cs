using UnityEngine;

public struct EyesLightingInputs
{
	public EyesLightingProfile profile;

	public LightingMode mode;

	public float brightnessMultiplier;

	public float? falloffFactorOverride;

	public Color? falloffColorOverride;

	public float? flashMultiplier;

	public float? desaturationOverride;

	public float interferenceMultiplier;

	public static EyesLightingInputs Default => default;
}
