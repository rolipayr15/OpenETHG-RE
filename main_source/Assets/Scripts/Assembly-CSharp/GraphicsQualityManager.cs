using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class GraphicsQualityManager
{
	public const GraphicsQuality DefaultQuality = GraphicsQuality.Normal;

	[CompilerGenerated]
	private static Action<GraphicsQuality> m_QualityChanged;

	public static GraphicsQuality CurrentQuality { get; private set; } = DefaultQuality;

	public static event Action<GraphicsQuality> QualityChanged
	{
		add { m_QualityChanged += value; }
		remove { m_QualityChanged -= value; }
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void ResetRuntimeState()
	{
		m_QualityChanged = null;
		CurrentQuality = DefaultQuality;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void ApplySavedQualityBeforeFirstScene()
	{
		GraphicsQuality quality = GetSavedQuality();
		if (!SetQuality(quality, false))
		{
			Debug.LogError(string.Format("Could not apply saved graphics quality '{0}' before the first scene. ", quality) + "Check the names in Project Settings > Quality.");
		}
	}

	public static GraphicsQuality GetSavedQuality()
	{
		int saved = PlayerPrefs.GetInt("quality", (int)DefaultQuality);
		if (!IsValidQualityValue(saved))
		{
			Debug.LogWarning(string.Format("Saved graphics quality value '{0}' is invalid. ", saved) + string.Format("Using '{0}'.", DefaultQuality));
			return DefaultQuality;
		}
		return (GraphicsQuality)saved;
	}

	public static bool SetQuality(GraphicsQuality quality, bool save = true)
	{
		if (!IsValidQualityValue((int)quality))
		{
			Debug.LogError(string.Format("Cannot apply invalid graphics quality value '{0}'.", (int)quality));
			return false;
		}
		string profileName = GetProfileName(quality);
		if (!TryFindProfileIndex(profileName, out int profileIndex))
		{
			string[] names = QualitySettings.names;
			string available = names == null || names.Length == 0 ? "none" : string.Join(", ", names);
			Debug.LogError("Graphics quality profile '" + profileName + "' was not found. Available profiles: " + available + ".");
			return false;
		}
		if (QualitySettings.GetQualityLevel() != profileIndex)
		{
			QualitySettings.SetQualityLevel(profileIndex, true);
		}
		CurrentQuality = quality;
		NotifyQualityChanged(quality);
		if (save)
		{
			PlayerPrefs.SetInt("quality", (int)quality);
			PlayerPrefs.Save();
		}
		return true;
	}

	private static void NotifyQualityChanged(GraphicsQuality quality)
	{
		if (m_QualityChanged == null)
		{
			return;
		}
		foreach (Delegate handler in m_QualityChanged.GetInvocationList())
		{
			try
			{
				((Action<GraphicsQuality>)handler)(quality);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	private static bool TryFindProfileIndex(string profileName, out int profileIndex)
	{
		profileIndex = -1;
		string[] names = QualitySettings.names;
		if (names == null || names.Length == 0)
		{
			Debug.LogError("Unity has no configured graphics quality profiles.");
			return false;
		}
		for (int i = 0; i < names.Length; i++)
		{
			if (string.Equals(names[i], profileName, StringComparison.OrdinalIgnoreCase))
			{
				profileIndex = i;
				return true;
			}
		}
		return false;
	}

	private static string GetProfileName(GraphicsQuality quality)
	{
		return IsValidQualityValue((int)quality) ? quality.ToString() : string.Empty;
	}

	private static bool IsValidQualityValue(int value)
	{
		return (uint)value < 3u;
	}
}
