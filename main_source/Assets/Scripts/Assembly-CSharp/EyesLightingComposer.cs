using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class EyesLightingComposer
{
	public const string FlashlightBoostInterferenceMulProperty = "_FlashlightBoostInterferenceMul";

	private static EyesLightingInputs inputs = DefaultInputs();

	public static event Action<ResolvedLighting> Applied;

	public static EyesLightingInputs Inputs => inputs;

	public static ResolvedLighting Current { get; private set; } = Resolve(DefaultInputs());

	// Authored falloff of the active level profile expressed as a factor (1 / ramp length).
	public static float AuthoredFalloffFactor => inputs.profile != null ? 1f / Mathf.Max(inputs.profile.falloffEnd - inputs.profile.falloffStart, 0.001f) : 0.1f;

	public static Color AuthoredFalloffColor => inputs.profile != null ? inputs.profile.falloffColor : default(Color);

	private static EyesLightingInputs DefaultInputs()
	{
		return new EyesLightingInputs
		{
			mode = LightingMode.Flashlight,
			brightnessMultiplier = 1f,
			interferenceMultiplier = 1f
		};
	}

	public static void SetProfile(EyesLightingProfile profile)
	{
		inputs.profile = profile;
		Apply();
	}

	public static void SetPlayerLayer(LightingMode mode, float brightnessMultiplier)
	{
		inputs.mode = mode;
		inputs.brightnessMultiplier = brightnessMultiplier;
		Apply();
	}

	public static void SetFalloffFactorOverride(float? value)
	{
		inputs.falloffFactorOverride = value;
		Apply();
	}

	public static void SetFalloffColorOverride(Color? value)
	{
		inputs.falloffColorOverride = value;
		Apply();
	}

	public static void SetFlashMultiplier(float? value)
	{
		inputs.flashMultiplier = value;
		Apply();
	}

	public static void SetDesaturationOverride(float? value)
	{
		inputs.desaturationOverride = value;
		Apply();
	}

	public static void SetInterferenceMultiplier(float value)
	{
		inputs.interferenceMultiplier = value;
		Apply();
	}

	public static void ClearRuntimeOverrides()
	{
		inputs.falloffFactorOverride = null;
		inputs.falloffColorOverride = null;
		inputs.flashMultiplier = null;
		inputs.desaturationOverride = null;
		inputs.interferenceMultiplier = 1f;
	}

	internal static void ResetForTests()
	{
		inputs = DefaultInputs();
		Current = Resolve(inputs);
	}

	// Legacy "falloff factor" (bigger = darker) mapped onto the start/end distances of the shader ramp.
	public static void FactorToStartEnd(float f, out float start, out float end)
	{
		if (f <= 1E-06f)
		{
			start = 1000000f;
			end = 1000001f;
		}
		else
		{
			start = 0.76f / f;
			end = 1.76f / f;
		}
	}

	// Level profile, then the Classic-mode override, then runtime overrides (potions, settings, events).
	public static ResolvedLighting Resolve(in EyesLightingInputs i)
	{
		EyesLightingProfile p = i.profile;
		EyesLightingModeOverride classic = p != null && i.mode == LightingMode.Classic ? p.classicOverride : null;
		float lmScale = p != null ? p.lmScale : 1f;
		if (classic != null)
		{
			lmScale = classic.lmScale;
		}
		float mix = p != null ? p.flashlightMix : 0f;
		float start;
		float end;
		if (i.falloffFactorOverride.HasValue)
		{
			FactorToStartEnd(i.falloffFactorOverride.Value, out start, out end);
		}
		else if (classic != null && classic.overrideFalloff)
		{
			start = classic.falloffStart;
			end = classic.falloffEnd;
		}
		else
		{
			start = p != null ? p.falloffStart : 7.6f;
			end = p != null ? p.falloffEnd : 17.6f;
		}
		Color falloffColor = i.falloffColorOverride ?? (p != null ? p.falloffColor : new Color(0f, 0f, 0f, 1f));
		float desaturation = i.desaturationOverride ?? (p != null ? p.desaturationLevel : 1f);
		if (i.flashMultiplier.HasValue)
		{
			lmScale *= i.flashMultiplier.Value;
		}
		Color ambient = classic != null ? classic.ambientColor : (p != null ? p.ambientColor : new Color(0f, 0f, 0f, 1f));
		return new ResolvedLighting
		{
			falloffStart = start,
			falloffEnd = end,
			falloffSmooth = p != null && p.falloffSmooth,
			falloffColor = falloffColor,
			desaturationLevel = desaturation,
			desaturationAdditiveLevel = p != null ? p.desaturationAdditiveLevel : 1f,
			lmScale = lmScale,
			flashlightIntensity = classic != null ? classic.flashlightIntensity : (p != null ? p.flashlightIntensity : 1f),
			flashlightFloor = p != null ? p.flashlightFloor : 0f,
			revealNormalize = p != null ? p.revealNormalize : 1f,
			// Equal-power crossfade between the flashlight-lit and lightmap-lit shading paths.
			flashlightMixWeights = new Vector4(Mathf.Sqrt(1f - mix), Mathf.Sqrt(mix), 0f, 0f),
			ambientColor = ambient,
			shaderBrightness = i.brightnessMultiplier - 1f,
			interferenceMultiplier = i.interferenceMultiplier,
			hideFlashlightModel = classic != null && classic.hideFlashlightModel,
			collectibleBlinkUngated = classic != null ? 1f : 0f
		};
	}

	public static void Apply()
	{
		ResolvedLighting r = Resolve(inputs);
		Current = r;
		float scale = 1f / Mathf.Max(r.falloffEnd - r.falloffStart, 0.001f);
		Shader.SetGlobalFloat("_FalloffScale", scale);
		Shader.SetGlobalFloat("_FalloffOffset", -(r.falloffStart * scale));
		Shader.SetGlobalFloat("_FalloffSmooth", r.falloffSmooth ? 1f : 0f);
		Shader.SetGlobalColor("_FalloffColor", r.falloffColor);
		Shader.SetGlobalFloat("_DesaturationLevel", r.desaturationLevel);
		Shader.SetGlobalFloat("_DesaturationAdditiveLevel", r.desaturationAdditiveLevel);
		Shader.SetGlobalFloat("_LMScale", r.lmScale);
		Shader.SetGlobalFloat("_FlashlightIntensity", r.flashlightIntensity);
		Shader.SetGlobalFloat("_FlashlightFloor", r.flashlightFloor);
		Shader.SetGlobalFloat("_RevealNormalize", r.revealNormalize);
		Shader.SetGlobalVector("_FlashlightMixWeights", r.flashlightMixWeights);
		Shader.SetGlobalColor("_AmbientColor", r.ambientColor);
		Shader.SetGlobalFloat("_ShaderBrightness", r.shaderBrightness);
		Shader.SetGlobalFloat(FlashlightBoostInterferenceMulProperty, r.interferenceMultiplier);
		Shader.SetGlobalFloat("_CollectibleBlinkUngated", r.collectibleBlinkUngated);
		Applied?.Invoke(r);
	}
}
