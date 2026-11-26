using System;
using System.Collections.Generic;

public static class JournalProgressStore
{
	public interface IPreferences
	{
		bool GetBool(string key, bool defaultValue);

		void SetBool(string key, bool value);

		void Save();
	}

	private sealed class PlayerPreferences : IPreferences
	{
		public bool GetBool(string key, bool defaultValue)
		{
			return PlayerPrefz.GetBool(key, defaultValue);
		}

		public void SetBool(string key, bool value)
		{
			PlayerPrefz.SetBool(key, value);
		}

		public void Save()
		{
			UnityEngine.PlayerPrefs.Save();
		}
	}

	private const string Prefix = "journal_";

	private const string RepairMarker = "JOURNAL_CANONICAL_KEYS_REPAIRED_V1";

	private static readonly string[] Ids = Enum.GetNames(typeof(JournalEntryId));

	private static readonly HashSet<string> IdSet = new HashSet<string>(Ids, StringComparer.Ordinal);

	private static readonly IReadOnlyList<string> ReadOnlyIds = Array.AsReadOnly(Ids);

	private static readonly IPreferences Preferences = new PlayerPreferences();

	public static IEnumerable<string> KnownIds => ReadOnlyIds;

	public static bool IsKnown(string id)
	{
		return id != null && IdSet.Contains(id);
	}

	public static string GetKey(string id)
	{
		if (!IsKnown(id))
		{
			throw new ArgumentException("Unknown journal entry ID.", "id");
		}
		return Prefix + id;
	}

	public static bool IsUnlocked(string id)
	{
		return IsUnlocked(id, Preferences);
	}

	public static bool IsUnlocked(string id, IPreferences preferences)
	{
		if (preferences == null)
		{
			throw new ArgumentNullException("preferences");
		}
		return IsKnown(id) && preferences.GetBool(GetKey(id), false);
	}

	public static bool Unlock(string id)
	{
		return Unlock(id, Preferences);
	}

	public static bool Unlock(string id, IPreferences preferences)
	{
		if (preferences == null)
		{
			throw new ArgumentNullException("preferences");
		}
		if (!IsKnown(id))
		{
			return false;
		}
		bool wasUnlocked = IsUnlocked(id, preferences);
		preferences.SetBool(GetKey(id), true);
		preferences.SetBool("WAS_ANY_JOURNAL_ENTRY_UNLOCKED", true);
		return !wasUnlocked;
	}

	public static List<string> GetUnlockedIds()
	{
		return GetUnlockedIds(Preferences);
	}

	public static List<string> GetUnlockedIds(IPreferences preferences)
	{
		if (preferences == null)
		{
			throw new ArgumentNullException("preferences");
		}
		List<string> unlocked = new List<string>();
		foreach (string id in Ids)
		{
			if (IsUnlocked(id, preferences))
			{
				unlocked.Add(id);
			}
		}
		return unlocked;
	}

	public static bool RepairLegacyKeys()
	{
		return RepairLegacyKeys(Preferences);
	}

	public static bool RepairLegacyKeys(IPreferences preferences)
	{
		if (preferences == null)
		{
			throw new ArgumentNullException("preferences");
		}
		if (preferences.GetBool(RepairMarker, false))
		{
			return false;
		}
		foreach (string id in Ids)
		{
			if (IsUnlocked(id, preferences) || preferences.GetBool(id, false))
			{
				Unlock(id, preferences);
			}
		}
		preferences.Save();
		preferences.SetBool(RepairMarker, true);
		preferences.Save();
		return true;
	}
}
