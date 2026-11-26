using System.Collections;
using UnityEngine;
using System.Security.Cryptography;
using System.IO;
using System.Globalization;
using System;
using System.Collections.Generic;
using System.Text;

public sealed class LegacyMigrationStore : ILegacyMigrationStore
{
	[Serializable]
	private sealed class Envelope
	{
		public int storeVersion;

		public int migrationVersion;

		public string account;

		public string sourceId;

		public string snapshotName;

		public string sourceHash;

		public string base64;

		public string payload;

		public bool appliedLocally;

		public long providerAcceptedRevision;

		public string checksum;
	}

	private sealed class StoredRecord
	{
		public string path;

		public bool primaryValid;

		public Envelope envelope;

		public LegacyMigrationRecord record;
	}

	private const int StoreVersion = 1;

	private const int MaxFileBytes = 4194304;

	private const int MaxSourcesPerAccount = 128;

	private static readonly object FileGate = new object();

	private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false, true);

	private readonly string directory;

	public LegacyMigrationStore(string directory)
	{
		if (string.IsNullOrEmpty(directory))
		{
			throw new ArgumentException("A migration archive directory is required.", "directory");
		}
		this.directory = Path.GetFullPath(directory);
	}

	public static string GetSourceId(string account, LegacyMigrationSource source)
	{
		if (source == null || string.IsNullOrEmpty(account) || string.IsNullOrEmpty(source.snapshotName) || string.IsNullOrEmpty(source.sourceHash))
		{
			throw new ArgumentException("Account and source identity are required.");
		}
		return Hash(Part(account) + Part(1.ToString(CultureInfo.InvariantCulture)) + Part(source.snapshotName) + Part(source.sourceHash));
	}

	public bool TryLoad(string account, out List<LegacyMigrationRecord> records, out string error)
	{
		lock (FileGate)
		{
			records = new List<LegacyMigrationRecord>();
			if (!TryReadAccount(account, out List<StoredRecord> stored, out error))
			{
				return false;
			}
			foreach (StoredRecord item in stored)
			{
				records.Add(item.record);
			}
			return true;
		}
	}

	public bool TryRetain(string account, IEnumerable<LegacyMigrationSource> sources, out string error)
	{
		lock (FileGate)
		{
			if (!TryReadAccount(account, out List<StoredRecord> stored, out error))
			{
				return false;
			}
			if (sources == null)
			{
				error = "Migration sources are missing.";
				return false;
			}
			Dictionary<string, StoredRecord> known = new Dictionary<string, StoredRecord>(StringComparer.Ordinal);
			foreach (StoredRecord item in stored)
			{
				known.Add(item.record.sourceId, item);
			}
			List<StoredRecord> created = new List<StoredRecord>();
			int count = 0;
			foreach (LegacyMigrationSource source in sources)
			{
				if (count == MaxSourcesPerAccount)
				{
					error = "Too many migration sources.";
					return false;
				}
				if (!LegacyGameProgressMigration.TryConvert(source.snapshotName, source.base64, out LegacyMigrationSource converted, out error))
				{
					return false;
				}
				if (source.sourceHash != converted.sourceHash || source.data == null || SaveGameProgressData.Serialize(source.data) != SaveGameProgressData.Serialize(converted.data))
				{
					error = "Migration source bytes, hash and converted rights disagree.";
					return false;
				}
				string id = GetSourceId(account, converted);
				if (!known.ContainsKey(id))
				{
					if (known.Count >= MaxSourcesPerAccount)
					{
						error = "Migration archive contains too many sources.";
						return false;
					}
					Envelope envelope = CreateEnvelope(account, converted, id);
					StoredRecord record = new StoredRecord
					{
						path = Path.Combine(AccountDirectory(account), id + ".json"),
						envelope = envelope,
						record = MakeRecord(envelope, converted)
					};
					known.Add(id, record);
					created.Add(record);
				}
				count++;
			}
			foreach (StoredRecord item in created)
			{
				if (!TryWrite(item, out error))
				{
					return false;
				}
			}
			return true;
		}
	}

	public bool TryMarkApplied(string account, IEnumerable<string> sourceIds, out string error)
	{
		lock (FileGate)
		{
			if (!TryReadAccount(account, out List<StoredRecord> stored, out error))
			{
				return false;
			}
			if (sourceIds == null)
			{
				error = "Migration source identifiers are missing.";
				return false;
			}
			HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
			foreach (string id in sourceIds)
			{
				if (string.IsNullOrEmpty(id) || ids.Count >= MaxSourcesPerAccount)
				{
					error = "Invalid migration source identifiers.";
					return false;
				}
				ids.Add(id);
			}
			Dictionary<string, StoredRecord> known = new Dictionary<string, StoredRecord>(StringComparer.Ordinal);
			foreach (StoredRecord item in stored)
			{
				known.Add(item.record.sourceId, item);
			}
			foreach (string id in ids)
			{
				if (!known.ContainsKey(id))
				{
					error = "Cannot mark a source that was not retained.";
					return false;
				}
			}
			try
			{
				foreach (string id in ids)
				{
					StoredRecord item = known[id];
					if (!item.envelope.appliedLocally)
					{
						item.envelope.appliedLocally = true;
						if (!TryWrite(item, out error))
						{
							return false;
						}
					}
				}
			}
			catch (Exception exception)
			{
				error = "Migration application marker failed: " + exception.GetType().Name;
				return false;
			}
			return true;
		}
	}

	public bool TryMarkProviderAccepted(string account, GameSaveData payload, long revision, out string error)
	{
		lock (FileGate)
		{
			if (!TryReadAccount(account, out List<StoredRecord> stored, out error))
			{
				return false;
			}
			if (payload == null || revision < 0)
			{
				error = "Invalid provider-accepted migration payload or revision.";
				return false;
			}
			GameSaveData merged = SaveGameProgressData.Merge(payload);
			foreach (StoredRecord item in stored)
			{
				if (item.envelope.appliedLocally && revision > item.envelope.providerAcceptedRevision && SaveGameProgressData.ContainsAll(merged, item.record.source.data))
				{
					item.envelope.providerAcceptedRevision = revision;
					if (!TryWrite(item, out error))
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	private bool TryReadAccount(string account, out List<StoredRecord> records, out string error)
	{
		records = new List<StoredRecord>();
		error = null;
		if (string.IsNullOrEmpty(account) || account.Length > 4096)
		{
			error = "A known migration account is required.";
			return false;
		}
		string folder = AccountDirectory(account);
		if (!Directory.Exists(folder))
		{
			return true;
		}
		SortedSet<string> paths = new SortedSet<string>(StringComparer.Ordinal);
		foreach (string file in Directory.EnumerateFiles(folder))
		{
			if (file.EndsWith(".json", StringComparison.Ordinal))
			{
				paths.Add(file);
			}
			else if (file.EndsWith(".json.bak", StringComparison.Ordinal))
			{
				paths.Add(file.Substring(0, file.Length - 4));
			}
			else
			{
				continue;
			}
			if (paths.Count > MaxSourcesPerAccount)
			{
				error = "Migration archive contains too many sources.";
				return false;
			}
		}
		foreach (string path in paths)
		{
			if (!TryReadWithBackup(path, account, out StoredRecord record, out error))
			{
				return false;
			}
			records.Add(record);
		}
		return true;
	}

	private static bool TryReadWithBackup(string path, string account, out StoredRecord record, out string error)
	{
		record = null;
		error = null;
		bool refuseFallback = false;
		if (File.Exists(path))
		{
			if (TryRead(path, path, account, out record, out refuseFallback, out error))
			{
				record.primaryValid = true;
				return true;
			}
			if (refuseFallback)
			{
				return false;
			}
		}
		if (File.Exists(path + ".bak") && TryRead(path + ".bak", path, account, out record, out refuseFallback, out error))
		{
			return true;
		}
		error = "Migration archive has no readable copy. " + error;
		return false;
	}

	private static bool TryRead(string readPath, string primaryPath, string account, out StoredRecord record, out bool refuseFallback, out string error)
	{
		record = null;
		refuseFallback = false;
		error = null;
		string text;
		try
		{
			if (new FileInfo(readPath).Length > MaxFileBytes)
			{
				error = "Migration archive exceeds the size limit.";
				return false;
			}
			text = File.ReadAllText(readPath, Utf8);
		}
		catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is ArgumentException)
		{
			error = exception.Message;
			return false;
		}
		if (!SaveGameProgressData.TryReadJsonObject(text, out IDictionary document, out error, MaxFileBytes))
		{
			return false;
		}
		if (!(document["storeVersion"] is long storeVersion) || storeVersion != StoreVersion || !(document["migrationVersion"] is long migrationVersion) || migrationVersion != LegacyGameProgressMigration.MigrationVersion)
		{
			// Only a file from a newer game must stop us falling back to the older backup.
			refuseFallback = (document["storeVersion"] is long newerStore && newerStore >= 2) || (document["migrationVersion"] is long newerMigration && newerMigration > 1);
			error = "Unsupported migration archive version.";
			return false;
		}
		if (!(document["account"] is string owner) || owner != account)
		{
			refuseFallback = true;
			error = "Migration source belongs to another account.";
			return false;
		}
		if (!(document["appliedLocally"] is bool applied) || !(document["providerAcceptedRevision"] is long accepted) || accepted < -1 || (!applied && accepted >= 0))
		{
			error = "Invalid migration marker state.";
			return false;
		}
		Envelope envelope = new Envelope
		{
			storeVersion = StoreVersion,
			migrationVersion = LegacyGameProgressMigration.MigrationVersion,
			account = owner,
			sourceId = document["sourceId"] as string,
			snapshotName = document["snapshotName"] as string,
			sourceHash = document["sourceHash"] as string,
			base64 = document["base64"] as string,
			payload = document["payload"] as string,
			appliedLocally = applied,
			providerAcceptedRevision = accepted,
			checksum = document["checksum"] as string
		};
		if (envelope.checksum != Checksum(envelope))
		{
			error = "Migration archive checksum does not match.";
			return false;
		}
		if (!LegacyGameProgressMigration.TryConvert(envelope.snapshotName, envelope.base64, out LegacyMigrationSource source, out error))
		{
			return false;
		}
		string id = GetSourceId(account, source);
		if (source.sourceHash != envelope.sourceHash || id != envelope.sourceId || Path.GetFileName(primaryPath) != id + ".json")
		{
			error = "Migration source identity does not match retained bytes.";
			return false;
		}
		if (!SaveGameProgressData.TryReadJsonObject(envelope.payload, out IDictionary payload, out error, SaveGameProgressData.MaxJsonCharacters))
		{
			return false;
		}
		if (payload["schemaVersion"] is long schema && schema > SaveGameProgressData.CurrentVersion)
		{
			refuseFallback = true;
			error = "Migration archive contains a newer save schema.";
			return false;
		}
		if (!SaveGameProgressData.TryParse(envelope.payload, out GameSaveData data, out error))
		{
			return false;
		}
		if (SaveGameProgressData.Serialize(data) != SaveGameProgressData.Serialize(source.data))
		{
			error = "Retained rights differ from the source conversion.";
			return false;
		}
		record = new StoredRecord
		{
			path = primaryPath,
			envelope = envelope,
			record = MakeRecord(envelope, source)
		};
		return true;
	}

	private static Envelope CreateEnvelope(string account, LegacyMigrationSource source, string id)
	{
		return new Envelope
		{
			storeVersion = StoreVersion,
			migrationVersion = LegacyGameProgressMigration.MigrationVersion,
			account = account,
			sourceId = id,
			snapshotName = source.snapshotName,
			sourceHash = source.sourceHash,
			base64 = source.base64,
			payload = SaveGameProgressData.Serialize(source.data),
			appliedLocally = false,
			providerAcceptedRevision = -1L
		};
	}

	private static LegacyMigrationRecord MakeRecord(Envelope envelope, LegacyMigrationSource source)
	{
		return new LegacyMigrationRecord
		{
			sourceId = envelope.sourceId,
			source = source,
			appliedLocally = envelope.appliedLocally,
			providerAcceptedRevision = envelope.providerAcceptedRevision
		};
	}

	private static bool TryWrite(StoredRecord record, out string error)
	{
		error = null;
		record.envelope.checksum = Checksum(record.envelope);
		try
		{
			byte[] bytes = Utf8.GetBytes(JsonUtility.ToJson(record.envelope));
			if (bytes.Length > MaxFileBytes)
			{
				error = "Migration archive exceeds the size limit.";
				return false;
			}
			string path = record.path;
			string temporary = path + ".tmp";
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			using (FileStream stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				stream.Write(bytes, 0, bytes.Length);
				stream.Flush(true);
			}
			if (!File.Exists(path))
			{
				File.Move(temporary, path);
			}
			else if (record.primaryValid)
			{
				File.Replace(temporary, path, path + ".bak");
			}
			else
			{
				File.Move(path, path + ".corrupt-" + Guid.NewGuid().ToString("N"));
				File.Move(temporary, path);
			}
			record.primaryValid = true;
			return true;
		}
		catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
		{
			error = exception.Message;
			return false;
		}
	}

	private string AccountDirectory(string account)
	{
		return Path.Combine(directory, Hash(account));
	}

	private static string Part(string value)
	{
		if (value == null)
		{
			return "-1:";
		}
		return value.Length.ToString(CultureInfo.InvariantCulture) + ":" + value;
	}

	private static string Checksum(Envelope value)
	{
		CultureInfo culture = CultureInfo.InvariantCulture;
		return Hash(string.Concat(new string[]
		{
			Part(value.storeVersion.ToString(culture)),
			Part(value.migrationVersion.ToString(culture)),
			Part(value.account),
			Part(value.sourceId),
			Part(value.snapshotName),
			Part(value.sourceHash),
			Part(value.base64),
			Part(value.payload),
			Part(value.appliedLocally ? "true" : "false"),
			Part(value.providerAcceptedRevision.ToString(culture))
		}));
	}

	private static string Hash(string value)
	{
		using (SHA256 sha = SHA256.Create())
		{
			StringBuilder builder = new StringBuilder(64);
			foreach (byte item in sha.ComputeHash(Utf8.GetBytes(value)))
			{
				builder.Append(item.ToString("x2", CultureInfo.InvariantCulture));
			}
			return builder.ToString();
		}
	}
}
