using UnityEngine;

[CreateAssetMenu(fileName = "LightingModeOverride", menuName = "Eyes/Lighting Mode Override")]
public sealed class EyesLightingModeOverride : ScriptableObject
{
	[Range(0f, 3f)]
	[Tooltip("Baked lightmap scale in Classic. 1 = the bake as is; above 1 is allowed up to the probe's clip band. Tune against the current bake.")]
	public float lmScale;

	[Range(0f, 8f)]
	[Tooltip("Beam intensity in Classic. 0 = no beam at all.")]
	public float flashlightIntensity;

	[Tooltip("Hide the flashlight view model (MagLite mesh) in Classic. The Spotlight stays on.")]
	public bool hideFlashlightModel;

	[Tooltip("When on, this mode uses its own ambient for objects without a lightmap (monsters, meat, pumpkins, collectibles) instead of the profile's. Classic has no beam, so this is their only light there.")]
	public bool overrideAmbient;

	[Tooltip("Base light of non-lightmapped objects in this mode when overrideAmbient is on. Aim at the brightness of a wall next to them: roughly a typical lightmap texel times this mode's lmScale.")]
	public Color ambientColor;

	[Tooltip("When on, this mode uses its own fog start and end instead of the profile's (Classic: lit but close; Flashlight: dark but far).")]
	public bool overrideFalloff;

	[Range(0f, 20f)]
	[Tooltip("Fog start distance (m) for this mode when overrideFalloff is on.")]
	public float falloffStart;

	[Range(0.5f, 30f)]
	[Tooltip("Fog end distance (m) for this mode when overrideFalloff is on; fully closed here, keep it inside the camera far clip.")]
	public float falloffEnd;
}
