using System.IO;
using System;
using CodeStage.AntiCheat.Storage;
using Prime31.MessageKit;
using System.Collections.Generic;
using UnityEngine;

public static class SaveGameProgress
{
	private static SaveGameProgressCoordinator coordinator;

	private static ISaveGameProgressCloud cloud;

	private static double nextInitialization;

	public static readonly List<string> _unlockedJournalEntries = new List<string>();

	private static readonly Dictionary<string, string> UnlockKeys = new Dictionary<string, string> { { "charlie", "ENEMY_CHARLIE" }, { "eyes.hospital", "HOSPITAL_LEVEL" }, { "school", "SCHOOL_LEVEL" }, { "endless", "MODE_ENDLESS" }, { "trickortreat", "MODE_TRICK_OR_TREAT" }, { "doubleenemy", "MODE_DOUBLE_ENEMY" }, { "dog", "DOG" }, { "charlie_hospital", "CHARLIE_HOSPITAL" }, { "good_boy_school", "GOOD_BOY_SCHOOL" }, { "pixel", "PIXEL" }, { "witch", "WITCH" }, { "immersive", "IMMERSIVE" } };

	private static readonly string[] DifficultyKeys = new string[9] { "DIFFICULTY_LEVEL_WON_KRASUE_MANSION", "DIFFICULTY_LEVEL_WON_CHARLIE_HOSPITAL", "DIFFICULTY_LEVEL_WON_GOOD_BOY_SCHOOL", "DIFFICULTY_LEVEL_WON_TRICK_OR_TREAT", "DIFFICULTY_LEVEL_WON_DOUBLE_TROUBLE", "DIFFICULTY_LEVEL_WON_PIXEL", "DIFFICULTY_LEVEL_WON_WITCH", "DIFFICULTY_LEVEL_WON_ENDLESS", "DIFFICULTY_LEVEL_WON_GOOSE_MANSION" };

	public static IReadOnlyList<string> DifficultyKeysForTests => DifficultyKeys;

	public static void Init()
	{
		if (coordinator != null)
		{
			return;
		}
		JournalProgressStore.RepairLegacyKeys();
		SaveGameProgressRecoveryStore recovery = new SaveGameProgressRecoveryStore(Path.Combine(Application.persistentDataPath, "SaveRecovery"));
		string error = null;
		if (!recovery.TryLoadOwner(out string owner, out error) || (owner == null && !recovery.TrySaveOwner("unassigned", out error)))
		{
			// The recovery directory is unusable right now: try again in 30 seconds, playing on with local data.
			nextInitialization = Time.realtimeSinceStartupAsDouble + 30.0;
			Debug.LogWarning("[SaveGameProgress] " + error);
			return;
		}
		cloud = new SaveGameProgressCloudAdapter(owner == null);
		LegacyMigrationStore legacyStore = new LegacyMigrationStore(Path.Combine(Application.persistentDataPath, "SaveRecovery", "Legacy"));
		coordinator = new SaveGameProgressCoordinator(cloud, recovery, CaptureLocal, ApplyLocal, () => Time.realtimeSinceStartupAsDouble, delegate(string message)
		{
			Debug.LogWarning("[SaveGameProgress] " + message);
		}, new LegacySnapshotReader(), legacyStore);
		MessageKit.addObserver(134, SaveGameData);
		MessageKit<string>.addObserver(41, OnJournalUnlocked);
		coordinator.Start();
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	public static void Shutdown()
	{
		MessageKit.removeObserver(134, SaveGameData);
		MessageKit<string>.removeObserver(41, OnJournalUnlocked);
		coordinator?.Dispose();
		coordinator = null;
		(cloud as IDisposable)?.Dispose();
		cloud = null;
		nextInitialization = 0.0;
		_unlockedJournalEntries.Clear();
	}

	public static void Tick()
	{
		if (coordinator == null && nextInitialization <= Time.realtimeSinceStartupAsDouble)
		{
			Init();
		}
		coordinator?.Tick();
	}

	public static void FlushLocal()
	{
		coordinator?.CaptureChanges();
	}

	private static void SaveGameData()
	{
		coordinator?.RequestSync();
	}

	private static void OnJournalUnlocked(string id)
	{
		if (!JournalProgressStore.IsKnown(id))
		{
			return;
		}
		if (!_unlockedJournalEntries.Contains(id))
		{
			_unlockedJournalEntries.Add(id);
		}
		coordinator?.RequestSync();
	}

	private static GameSaveData CaptureLocal()
	{
		GameSaveData data = SaveGameProgressData.Empty();
		foreach (KeyValuePair<string, string> item in UnlockKeys)
		{
			if (ObscuredPrefs.GetInt(item.Value, 0) == 1)
			{
				data.unlockedItems.Add(item.Key);
			}
		}
		foreach (string key in DifficultyKeys)
		{
			int value = PlayerPrefs.GetInt(key, 0);
			if (value >= 1 && value <= 5)
			{
				data.difficultyStars.Add(new DifficultyRecord
				{
					key = key,
					value = value
				});
			}
		}
		data.unlockedJournalEntries = JournalProgressStore.GetUnlockedIds();
		return data;
	}

	private static void ApplyLocal(GameSaveData data)
	{
		if (data == null)
		{
			return;
		}
		EncryptedValuesManager unlocks = null;
		if (data.unlockedItems != null)
		{
			foreach (string item in data.unlockedItems)
			{
				if (UnlockKeys.TryGetValue(item, out string key) && ObscuredPrefs.GetInt(key, 0) != 1)
				{
					if (unlocks == null)
					{
						unlocks = UnityEngine.Object.FindFirstObjectByType<EncryptedValuesManager>();
					}
					if (unlocks == null)
					{
						throw new InvalidOperationException("Local unlock manager is not ready.");
					}
					unlocks.UnlockProductWithCoins(item);
				}
			}
		}
		if (data.difficultyStars != null)
		{
			foreach (DifficultyRecord record in data.difficultyStars)
			{
				if (Array.IndexOf(DifficultyKeys, record.key) >= 0 && PlayerPrefs.GetInt(record.key, 0) < record.value)
				{
					PlayerPrefs.SetInt(record.key, record.value);
				}
			}
		}
		List<string> newlyUnlocked = new List<string>();
		if (data.unlockedJournalEntries != null)
		{
			foreach (string id in data.unlockedJournalEntries)
			{
				if (JournalProgressStore.Unlock(id))
				{
					newlyUnlocked.Add(id);
				}
			}
		}
		PlayerPrefs.Save();
		_unlockedJournalEntries.Clear();
		_unlockedJournalEntries.AddRange(JournalProgressStore.GetUnlockedIds());
		foreach (string id in newlyUnlocked)
		{
			MessageKit<string>.post(41, id);
		}
	}
}
