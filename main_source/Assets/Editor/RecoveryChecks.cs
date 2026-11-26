using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Self-checks for restored non-visual systems. They run without a device:
/// Unity -batchmode -projectPath . -executeMethod RecoveryChecks.Run -quit
/// </summary>
public static class RecoveryChecks
{
	private sealed class FakeCloud : ISaveGameProgressCloud
	{
		public string account;
		public string cloudJson;
		public string cachedJson;
		public bool owned = true;
		public int writes;

		public string Account => account;
		public bool IsAvailable => true;
		public event Action AccountChanged;
		public event Action DataChanged;

		public void Synchronize(Action<bool> completed)
		{
			completed(true);
		}

		public void Tick()
		{
		}

		public bool TryRead(out string cloud, out string cached, out bool cacheIsAccountOwned, out string error)
		{
			cloud = cloudJson;
			cached = cachedJson;
			cacheIsAccountOwned = owned;
			error = null;
			return true;
		}

		public bool TryWrite(string json, out string error)
		{
			cloudJson = json;
			writes++;
			error = null;
			return true;
		}

		public void Dispose()
		{
		}

		public void RaiseAccount()
		{
			AccountChanged?.Invoke();
		}

		public void RaiseData()
		{
			DataChanged?.Invoke();
		}
	}

	private static int failures;

	private static int passes;

	private static void Check(bool condition, string name)
	{
		if (condition)
		{
			passes++;
		}
		else
		{
			failures++;
			Debug.LogError("[RecoveryChecks] FAILED: " + name);
		}
	}

	private static GameSaveData Data(string item = null, string journal = null, string star = null, int stars = 0)
	{
		GameSaveData data = SaveGameProgressData.Empty();
		if (item != null)
		{
			data.unlockedItems.Add(item);
		}
		if (journal != null)
		{
			data.unlockedJournalEntries.Add(journal);
		}
		if (star != null)
		{
			data.difficultyStars.Add(new DifficultyRecord { key = star, value = stars });
		}
		return data;
	}

	[MenuItem("Tools/Recovery/Run Checks")]
	public static void Run()
	{
		failures = 0;
		passes = 0;
		string root = Path.Combine(Path.GetTempPath(), "eyes-recovery-checks-" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(root);
		try
		{
			SaveDocument();
			RecoveryStore(Path.Combine(root, "recovery"));
			LegacyMigration(Path.Combine(root, "legacy"));
			Coordinator(Path.Combine(root, "coordinator"));
			Localization();
		}
		catch (Exception exception)
		{
			failures++;
			Debug.LogError("[RecoveryChecks] EXCEPTION: " + exception);
		}
		finally
		{
			Directory.Delete(root, true);
		}
		Debug.Log("[RecoveryChecks] passed=" + passes + " failed=" + failures);
		if (Application.isBatchMode)
		{
			EditorApplication.Exit(failures == 0 ? 0 : 3);
		}
	}

	private static void SaveDocument()
	{
		GameSaveData source = Data("charliehospital", "BASEMENT_KEY", "mansion", 3);
		string json = SaveGameProgressData.Serialize(source);
		Check(SaveGameProgressData.TryParse(json, out GameSaveData parsed, out string error), "parse serialized save: " + error);
		Check(parsed != null && parsed.unlockedItems.Contains("charlie_hospital"), "item alias is canonicalised");
		Check(parsed != null && parsed.difficultyStars.Count == 1 && parsed.difficultyStars[0].value == 3, "difficulty stars survive");
		Check(SaveGameProgressData.TryParse(null, out GameSaveData empty, out error) && empty.unlockedItems.Count == 0, "null text is an empty save");
		Check(!SaveGameProgressData.TryParse("{\"cheat\":1}", out _, out error), "unknown field is refused");
		Check(!SaveGameProgressData.TryParse("{\"schemaVersion\":3}", out _, out error), "newer schema is refused");
		Check(!SaveGameProgressData.TryParse("{\"unlockedItems\":[1]}", out _, out error), "non-string id is refused");
		Check(!SaveGameProgressData.TryParse("{\"a\":1,\"a\":2}", out _, out error), "duplicate key is refused");
		Check(!SaveGameProgressData.TryParse("{\"difficultyStars\":[{\"key\":\"x\",\"value\":6}]}", out _, out error), "difficulty 6 is refused");
		Check(!SaveGameProgressData.TryParse("[1,2]", out _, out error), "non-object is refused");
		Check(!SaveGameProgressData.TryParse("{\"unlockedItems\":[]} x", out _, out error), "trailing text is refused");
		Check(SaveGameProgressData.TryParse("{\"schemaVersion\":2,\"unlockedItems\":[\"a\\u0062\"],\"difficultyStars\":[{\"key\":\"x\",\"value\":5}],\"unlockedJournalEntries\":[]}", out parsed, out error) && parsed.unlockedItems[0] == "ab", "escapes and full document: " + error);

		GameSaveData merged = SaveGameProgressData.Merge(Data("a", null, "x", 1), Data("b", "j", "x", 4), Data(null, null, "x", 2));
		Check(merged.unlockedItems.Count == 2 && merged.unlockedJournalEntries.Count == 1 && merged.difficultyStars[0].value == 4, "merge unions and keeps the best stars");
		GameSaveData difference = SaveGameProgressData.Difference(merged, Data("a", null, "x", 4));
		Check(difference.unlockedItems.Count == 1 && difference.unlockedItems[0] == "b" && difference.difficultyStars.Count == 0 && difference.unlockedJournalEntries.Count == 1, "difference lists only what is new");
		Check(SaveGameProgressData.ContainsAll(merged, Data("a")) && !SaveGameProgressData.ContainsAll(Data("a"), merged), "containment is one-directional");
	}

	private static void RecoveryStore(string directory)
	{
		SaveGameProgressRecoveryStore store = new SaveGameProgressRecoveryStore(directory);
		Check(store.TryLoad("playgames:1", out RecoveryRecord record, out string error) && record == null, "missing record is not an error: " + error);
		Check(store.TrySave("playgames:1", Data("a"), 1L, out error), "first save: " + error);
		Check(store.TrySave("playgames:1", Data("b"), 2L, out error), "second save: " + error);
		Check(store.TryLoad("playgames:1", out record, out error) && record != null && record.revision == 2L && record.data.unlockedItems.Count == 2, "saves merge, revision grows: " + error);
		Check(store.TrySave("playgames:1", Data("b"), 1L, out error) && store.TryLoad("playgames:1", out record, out error) && record.revision == 2L, "an older revision never lowers the record");
		Check(store.TryLoad("playgames:2", out record, out error) && record == null, "accounts are separate");
		Check(store.TryLoadOwner(out string owner, out error) && owner == null, "no owner yet");
		Check(store.TrySaveOwner("playgames:1", out error) && store.TryLoadOwner(out owner, out error) && owner == "playgames:1", "owner round trip: " + error);

		string primary = null;
		foreach (string file in Directory.GetFiles(directory, "progress-*.json"))
		{
			primary = file;
		}
		Check(primary != null && File.Exists(primary + ".bak"), "a backup exists after replacing");
		File.WriteAllText(primary, "{broken");
		Check(store.TryLoad("playgames:1", out record, out error) && record != null, "corrupt primary falls back to backup: " + error);
		Check(store.TrySave("playgames:1", Data("c"), 3L, out error), "save after corruption: " + error);
		Check(Directory.GetFiles(directory, "*.corrupt-*").Length == 1, "corrupt file is kept for diagnosis");
		Check(store.TryLoad("playgames:1", out record, out error) && record != null && record.data.unlockedItems.Contains("c"), "save after corruption is readable");

		string text = File.ReadAllText(primary).Replace("\"revision\":3", "\"revision\":99");
		File.WriteAllText(primary, text);
		File.Delete(primary + ".bak");
		Check(!store.TryLoad("playgames:1", out record, out error), "tampered checksum is refused");
	}

	private static void Localization()
	{
		I2.Loc.LocalizationManager.Sources.Clear();
		I2.Loc.LocalizationManager.UpdateSources();
		Check(I2.Loc.LocalizationManager.Sources.Count == 1, "language source is found in Resources");
		Check(I2.Loc.LocalizationManager.Sources[0].mTerms.Count > 700, "terms deserialized: " + I2.Loc.LocalizationManager.Sources[0].mTerms.Count);
		Check(I2.Loc.LocalizationManager.GetAllLanguages().Count >= 30, "languages deserialized");
		I2.Loc.LocalizationManager.SetLanguageAndCode("English", "en", false, true);
		Check(I2.Loc.LocalizationManager.GetTranslation("Achievements") == "Achievements", "english term");
		I2.Loc.LocalizationManager.SetLanguageAndCode("Bulgarian", "bg", false, true);
		Check(I2.Loc.LocalizationManager.GetTranslation("Achievements") == "Постижения", "bulgarian term");
		Check(I2.Loc.ScriptLocalization.Unlocked != null, "ScriptLocalization resolves a term");
		Check(I2.Loc.LocalizationManager.GetTranslation("NoSuchTermAtAll") == null, "unknown term is null");
		Check(I2.Loc.LocalizationManager.IsRTL("ar") && !I2.Loc.LocalizationManager.IsRTL("en"), "rtl languages");
		string text = "Hello {[NAME]}";
		I2.Loc.LocalizationManager.ApplyLocalizationParams(ref text, new Dictionary<string, object> { { "NAME", "Bob" } });
		Check(text == "Hello Bob", "parameters are substituted");
		I2.Loc.LocalizationManager.SetLanguageAndCode("English", "en", false, true);
	}

	private static string Legacy(string json)
	{
		return Convert.ToBase64String(new UTF8Encoding(false).GetBytes(json));
	}

	private static void LegacyMigration(string directory)
	{
		Check(LegacyGameProgressMigration.TryConvert("save1", Legacy("{\"hospital\":true,\"witch\":false,\"coins\":5}"), out LegacyMigrationSource source, out string error), "legacy convert: " + error);
		Check(source != null && source.data.unlockedItems.Contains("charlie_hospital") && source.data.unlockedItems.Contains("eyes.hospital") && !source.data.unlockedItems.Contains("witch"), "legacy flags map to items");
		Check(!LegacyGameProgressMigration.TryConvert("cpnp-snapshot-1", Legacy("{\"hospital\":true}"), out _, out error), "plugin snapshot name is refused");
		Check(!LegacyGameProgressMigration.TryConvert("save1", Legacy("{\"coins\":5}"), out _, out error), "no unlock flags is refused");
		Check(!LegacyGameProgressMigration.TryConvert("save1", Legacy("{\"unknown\":true}"), out _, out error), "unknown field is refused");
		Check(!LegacyGameProgressMigration.TryConvert("save1", "@@@", out _, out error), "bad base64 is refused");

		LegacyMigrationStore store = new LegacyMigrationStore(directory);
		Check(store.TryLoad("playgames:1", out List<LegacyMigrationRecord> records, out error) && records.Count == 0, "empty archive: " + error);
		Check(store.TryRetain("playgames:1", new[] { source }, out error), "retain: " + error);
		Check(store.TryRetain("playgames:1", new[] { source }, out error), "retain twice is harmless: " + error);
		Check(store.TryLoad("playgames:1", out records, out error) && records.Count == 1 && !records[0].appliedLocally && records[0].providerAcceptedRevision == -1L, "retained record: " + error);
		string id = records[0].sourceId;
		Check(!store.TryMarkApplied("playgames:1", new[] { "nope" }, out error), "unknown id is refused");
		Check(store.TryMarkApplied("playgames:1", new[] { id }, out error), "mark applied: " + error);
		Check(store.TryMarkProviderAccepted("playgames:1", source.data, 7L, out error), "mark accepted: " + error);
		Check(store.TryLoad("playgames:1", out records, out error) && records[0].appliedLocally && records[0].providerAcceptedRevision == 7L, "markers survive reload: " + error);
		Check(store.TryLoad("playgames:2", out records, out error) && records.Count == 0, "archive is per account");
	}

	private static void Coordinator(string directory)
	{
		FakeCloud cloud = new FakeCloud();
		GameSaveData game = Data("played");
		double now = 0.0;
		List<string> log = new List<string>();
		SaveGameProgressRecoveryStore store = new SaveGameProgressRecoveryStore(directory);
		SaveGameProgressCoordinator coordinator = new SaveGameProgressCoordinator(cloud, store, () => game, delegate(GameSaveData applied)
		{
			game = SaveGameProgressData.Merge(game, applied);
		}, () => now, log.Add);
		coordinator.Start();
		Check(store.TryLoad(null, out RecoveryRecord record, out string error) && record != null && record.data.unlockedItems.Contains("played"), "signed-out progress goes to the unassigned record: " + error);
		Check(cloud.writes == 0, "nothing reaches the cloud without an account");

		cloud.account = "playgames:1";
		cloud.cloudJson = SaveGameProgressData.Serialize(Data("from_cloud", null, "mansion", 2));
		cloud.RaiseAccount();
		coordinator.Tick();
		Check(game.unlockedItems.Contains("from_cloud"), "cloud progress is applied to the game");
		Check(game.unlockedItems.Contains("played"), "local progress stays in the game");
		Check(!cloud.cloudJson.Contains("played"), "progress from before sign-in on an unowned device is not uploaded");

		game = SaveGameProgressData.Merge(game, Data("earned_online"));
		coordinator.CaptureChanges();
		now += 31.0;
		coordinator.Tick();
		Check(cloud.writes == 1 && cloud.cloudJson.Contains("earned_online"), "progress earned while signed in reaches the cloud");
		Check(store.TryLoad("playgames:1", out record, out error) && record != null && record.data.unlockedItems.Contains("earned_online"), "recovery copy has it too: " + error);

		cloud.cloudJson = SaveGameProgressData.Serialize(Data("other_device"));
		cloud.RaiseData();
		now += 31.0;
		coordinator.Tick();
		Check(game.unlockedItems.Contains("other_device") && cloud.cloudJson.Contains("earned_online") && cloud.cloudJson.Contains("other_device"), "another device's progress merges both ways");

		int writes = cloud.writes;
		now += 31.0;
		coordinator.Tick();
		Check(cloud.writes == writes, "no needless cloud write when nothing changed");

		cloud.account = "playgames:2";
		cloud.cloudJson = null;
		coordinator.Tick();
		Check(cloud.cloudJson == null || !cloud.cloudJson.Contains("earned_online"), "another account does not inherit the first account's cloud save");
		coordinator.Dispose();
		Check(log.Count == 0 || log.TrueForAll(line => line != null), "reports are readable: " + string.Join(" | ", log));
	}
}
