using UnityEngine;

[CreateAssetMenu(fileName = "LightingProfile", menuName = "Eyes/Lighting Profile")]
public sealed class EyesLightingProfile : ScriptableObject
{
	[Range(0f, 20f)]
	[Tooltip("Distance (m) where the fog starts (clear up to here). Old single factor: start = 0.76 / factor.")]
	public float falloffStart = 7.6f;

	[Range(0.5f, 30f)]
	[Tooltip("Distance (m) where the fog is fully closed. Keep it at or inside the camera far clip (11 m in the levels). Old single factor: end = 1.76 / factor.")]
	public float falloffEnd = 17.6f;

	[Tooltip("Fog curve between Start and End: off = linear, on = smoothstep (eases in and out, same endpoints). Both modes follow this.")]
	public bool falloffSmooth;

	public Color falloffColor = Color.black;

	[Range(0f, 1f)]
	[Tooltip("Desaturation factor for the base shader pass")]
	public float desaturationLevel = 1f;

	[Range(0f, 1f)]
	[Tooltip("Desaturation factor for the additive shader pass (flashlight)")]
	public float desaturationAdditiveLevel = 0.5f;

	[Range(0f, 3f)]
	[Tooltip("Scales baked lightmap brightness in the base pass. Above 1 is allowed (RGBM headroom); the luminance probe's clip band is the limit")]
	public float lmScale = 1f;

	[Range(0f, 8f)]
	[Tooltip("Overall flashlight brightness. Scales both beam components equally - use Mix to change their balance without changing brightness")]
	public float flashlightIntensity = 1f;

	[Range(0f, 1f)]
	[Tooltip("Beam flavour: 0 = pure lightmap reveal (keeps baked AO/shadows), 1 = pure Lambert fill (lights black-lightmap areas, ignores baked occlusion)")]
	public float flashlightMix;

	[Range(0f, 1f)]
	[Tooltip("Lifts the reveal gate so pitch-black lightmap areas still respond to the flashlight (0 = pure reveal)")]
	public float flashlightFloor;

	[Tooltip("Fake ambient color applied via shader global - does not use or affect Unity's RenderSettings ambient or lightmap baking")]
	public Color ambientColor = Color.black;

	[Header("Advanced - calibration (set once per bake)")]
	[Range(0.1f, 20f)]
	[Tooltip("Rescales the reveal gate into the Lambert fill's magnitude range, so Mix changes flavour without changing brightness. 1 = un-normalized (reproduces the original reveal-only look); expect roughly 4-10 once calibrated. Calibrate by sweeping Mix 0->1 at fixed Intensity and adjusting until both ends look equally bright. Re-check after a rebake")]
	public float revealNormalize = 1f;

	[Header("Classic mode")]
	[Tooltip("Optional. What Classic lighting changes for this look. Leave empty for looks where the mode must not change anything (menu).")]
	public EyesLightingModeOverride classicOverride;
}
