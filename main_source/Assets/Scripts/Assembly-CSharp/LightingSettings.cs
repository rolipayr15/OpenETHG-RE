using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class LightingSettings
{
	public const float MinBrightness = 0.5f;

	public const float MaxBrightness = 2f;

	public const float MinStops = -1f;

	public const float MaxStops = 1f;

	public const float DefaultBrightness = 1f;

	public const LightingMode DefaultMode = LightingMode.Flashlight;

	[CompilerGenerated]
	private static Action m_Changed;

	public static LightingMode Mode { get; private set; }

	public static float BrightnessMultiplier { get; private set; } = DefaultBrightness;

	public static bool HasSavedMode => PlayerPrefs.HasKey("lighting_mode");

	public static event Action Changed
	{
		add { m_Changed += value; }
		remove { m_Changed -= value; }
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void ResetRuntimeState()
	{
		Mode = LightingMode.Flashlight;
		m_Changed = null;
		BrightnessMultiplier = DefaultBrightness;
	}

	private static void ResetValues()
	{
		Mode = LightingMode.Flashlight;
		BrightnessMultiplier = DefaultBrightness;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void LoadSavedBeforeFirstScene()
	{
		Mode = ParseMode(PlayerPrefs.GetInt("lighting_mode", 0));
		BrightnessMultiplier = SanitizeBrightness(PlayerPrefs.GetFloat("brightness_multiplier", DefaultBrightness));
		Push();
	}

	public static void ApplyRemoteDefault(LightingMode remoteDefault)
	{
		if (!HasSavedMode && remoteDefault != Mode)
		{
			SetMode(remoteDefault, false);
		}
	}

	public static void SetMode(LightingMode mode, bool save = true)
	{
		Mode = ParseMode((int)mode);
		if (save)
		{
			PlayerPrefs.SetInt("lighting_mode", (int)Mode);
		}
		Push();
	}

	public static void SetBrightness(float multiplier, bool save = true)
	{
		BrightnessMultiplier = SanitizeBrightness(multiplier);
		if (save)
		{
			PlayerPrefs.SetFloat("brightness_multiplier", BrightnessMultiplier);
		}
		Push();
	}

	public static float ToStops(float multiplier)
	{
		return (float)Math.Log(multiplier, 2.0);
	}

	public static float FromStops(float stops)
	{
		return SanitizeBrightness(Mathf.Pow(2f, stops));
	}

	public static float SanitizeBrightness(float value)
	{
		return float.IsNaN(value) || float.IsInfinity(value) ? DefaultBrightness : Mathf.Clamp(value, MinBrightness, MaxBrightness);
	}

	public static LightingMode ParseMode(int value)
	{
		if ((uint)value > 1u)
		{
			Debug.LogWarning(string.Format("Saved lighting mode value '{0}' is invalid. Using '{1}'.", value, DefaultMode));
			return DefaultMode;
		}
		return (LightingMode)value;
	}

	internal static void ResetForTests()
	{
		ResetValues();
	}

	private static void Push()
	{
		EyesLightingComposer.SetPlayerLayer(Mode, BrightnessMultiplier);
		EyesAnalytics.Lighting.PublishState(Mode, BrightnessMultiplier);
		m_Changed?.Invoke();
	}
}
