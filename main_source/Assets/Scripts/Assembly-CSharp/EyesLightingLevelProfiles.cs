using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LightingLevelProfiles", menuName = "Eyes/Lighting Level Profiles")]
public sealed class EyesLightingLevelProfiles : ScriptableObject
{
	[Serializable]
	public struct Entry
	{
		[Tooltip("Exact scene name, e.g. Hospital.")]
		public string scene;

		public EyesLightingProfile profile;
	}

	public const string ResourceName = "LightingLevelProfiles";

	public List<Entry> entries = new List<Entry>();

	public static EyesLightingProfile For(IEnumerable<Entry> entries, string scene, EyesLightingProfile fallback)
	{
		if (entries == null)
		{
			return fallback;
		}
		foreach (Entry entry in entries)
		{
			if (entry.profile != null && string.Equals(entry.scene, scene, StringComparison.Ordinal))
			{
				return entry.profile;
			}
		}
		return fallback;
	}

	public static EyesLightingProfile Resolve(string scene, EyesLightingProfile shared)
	{
		EyesLightingLevelProfiles profiles = Resources.Load<EyesLightingLevelProfiles>(ResourceName);
		return profiles == null ? shared : For(profiles.entries, scene, shared);
	}
}
