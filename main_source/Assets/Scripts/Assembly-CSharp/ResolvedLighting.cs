using UnityEngine;

public struct ResolvedLighting
{
	public float falloffStart;

	public float falloffEnd;

	public bool falloffSmooth;

	public Color falloffColor;

	public float desaturationLevel;

	public float desaturationAdditiveLevel;

	public float lmScale;

	public float flashlightIntensity;

	public float flashlightFloor;

	public float revealNormalize;

	public Vector4 flashlightMixWeights;

	public Color ambientColor;

	public float shaderBrightness;

	public float interferenceMultiplier;

	public bool hideFlashlightModel;

	public float collectibleBlinkUngated;

	public float FalloffScale => 1f / Mathf.Max(falloffEnd - falloffStart, 0.001f);

	public float FalloffOffset => -falloffStart * FalloffScale;
}
