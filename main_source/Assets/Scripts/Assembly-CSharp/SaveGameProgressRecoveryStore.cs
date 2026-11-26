using UnityEngine;
using System.Threading;
using System.Security.Cryptography;
using System.IO;
using System.Globalization;
using System.Collections;
using System;
using System.Text;

public sealed class SaveGameProgressRecoveryStore : ISaveGameProgressRecoveryStore
{
	[Serializable]
	private sealed class Envelope
	{
		public int storeVersion;

		public string account;

		public long revision;

		public string payload;

		public string checksum;
	}

	public const string UnassignedAccount = "unassigned";

	private const int StoreVersion = 1;

	private const int MaxFileBytes = 4194304;

	private const int MaxAccountCharacters = 4096;

	private static readonly object FileGate = new object();

	private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false, true);

	private readonly string directory;

	private string OwnerPath => Path.Combine(directory, "local-owner.json");

	public SaveGameProgressRecoveryStore(string directory)
	{
		if (string.IsNullOrEmpty(directory))
		{
			throw new ArgumentException("A recovery directory is required.", "directory");
		}
		this.directory = Path.GetFullPath(directory);
	}

	public bool TryLoad(string account, out RecoveryRecord record, out string error)
	{
		lock (FileGate)
		{
			record = null;
			string normalized = NormalizeAccount(account);
			if (!ValidAccount(normalized, out error) || !TryReadWithBackup(RecordPath(normalized), normalized, false, out Envelope envelope, out bool _, out error))
			{
				return false;
			}
			if (envelope != null)
			{
				if (!SaveGameProgressData.TryParse(envelope.payload, out GameSaveData data, out error))
				{
					return false;
				}
				record = new RecoveryRecord
				{
					account = normalized,
					revision = envelope.revision,
					data = data
				};
			}
			return true;
		}
	}

	public bool TrySave(string account, GameSaveData data, long revision, out string error)
	{
		lock (FileGate)
		{
			string normalized = NormalizeAccount(account);
			if (!ValidAccount(normalized, out error))
			{
				return false;
			}
			if (data == null || revision < 0 || data.schemaVersion > 2)
			{
				error = "Recovery data is null, has a negative revision, or uses a newer schema.";
				return false;
			}
			if (!SaveGameProgressData.TryParse(SaveGameProgressData.Serialize(data), out GameSaveData incoming, out error))
			{
				return false;
			}
			string path = RecordPath(normalized);
			if (!TryReadWithBackup(path, normalized, false, out Envelope existing, out bool primaryValid, out error))
			{
				return false;
			}
			if (existing != null)
			{
				// Never lose progress already on disk: the saved record only ever grows.
				if (!SaveGameProgressData.TryParse(existing.payload, out GameSaveData stored, out error))
				{
					return false;
				}
				incoming = SaveGameProgressData.Merge(stored, incoming);
				revision = Math.Max(revision, existing.revision);
			}
			string payload = SaveGameProgressData.Serialize(incoming);
			if (primaryValid && existing != null && existing.payload == payload && existing.revision == revision)
			{
				return true;
			}
			return TryWriteAtomic(path, CreateEnvelope(normalized, revision, payload), primaryValid, out error);
		}
	}

	public bool TryLoadOwner(out string owner, out string error)
	{
		lock (FileGate)
		{
			owner = null;
			if (!TryReadWithBackup(OwnerPath, null, true, out Envelope envelope, out bool primaryValid, out error))
			{
				return false;
			}
			if (envelope != null)
			{
				// An owner recovered only from the backup cannot be trusted to be the only account in the cache.
				owner = primaryValid ? envelope.account : "mixed";
			}
			return true;
		}
	}

	public bool TrySaveOwner(string owner, out string error)
	{
		lock (FileGate)
		{
			string normalized = NormalizeAccount(owner);
			if (!ValidAccount(normalized, out error) || !TryReadWithBackup(OwnerPath, null, true, out Envelope envelope, out bool primaryValid, out error))
			{
				return false;
			}
			if (primaryValid && envelope != null && envelope.account == normalized)
			{
				return true;
			}
			return TryWriteAtomic(OwnerPath, CreateEnvelope(normalized, 0L, "owner"), primaryValid, out error);
		}
	}

	private string RecordPath(string account)
	{
		return Path.Combine(directory, "progress-" + Hash(account) + ".json");
	}

	private static string NormalizeAccount(string account)
	{
		return string.IsNullOrEmpty(account) ? UnassignedAccount : account;
	}

	private static bool ValidAccount(string account, out string error)
	{
		error = account.Length > MaxAccountCharacters ? "Recovery account identifier is too long." : null;
		if (error != null)
		{
			return false;
		}
		try
		{
			Utf8.GetByteCount(account);
		}
		catch (ArgumentException)
		{
			error = "Recovery account identifier is not valid text.";
			return false;
		}
		return true;
	}

	private static Envelope CreateEnvelope(string account, long revision, string payload)
	{
		return new Envelope
		{
			storeVersion = StoreVersion,
			account = account,
			revision = revision,
			payload = payload,
			checksum = Checksum(account, revision, payload)
		};
	}

	private static bool TryReadWithBackup(string path, string account, bool ownerFile, out Envelope envelope, out bool primaryValid, out string error)
	{
		envelope = null;
		primaryValid = false;
		error = null;
		bool hasPrimary = File.Exists(path);
		string backup = path + ".bak";
		bool hasBackup = File.Exists(backup);
		if (!hasPrimary && !hasBackup)
		{
			return true;
		}
		bool refuseFallback = false;
		if (hasPrimary)
		{
			if (TryRead(path, account, ownerFile, out envelope, out refuseFallback, out error))
			{
				primaryValid = true;
				return true;
			}
			if (refuseFallback)
			{
				return false;
			}
		}
		string primaryError = error;
		if (hasBackup)
		{
			if (TryRead(backup, account, ownerFile, out envelope, out refuseFallback, out error))
			{
				error = null;
				return true;
			}
			if (primaryError != null)
			{
				error = primaryError;
			}
		}
		error = "Recovery data could not be read. " + error;
		return false;
	}

	private static bool TryRead(string path, string expectedAccount, bool ownerFile, out Envelope envelope, out bool refuseFallback, out string error)
	{
		envelope = null;
		refuseFallback = false;
		error = null;
		try
		{
			if (new FileInfo(path).Length > MaxFileBytes)
			{
				error = "Recovery file exceeds the size limit.";
				return false;
			}
			string json = File.ReadAllText(path, Utf8);
			if (!SaveGameProgressData.TryReadJsonObject(json, out IDictionary _, out error, MaxFileBytes))
			{
				return false;
			}
			Envelope parsed = JsonUtility.FromJson<Envelope>(json);
			if (parsed == null)
			{
				error = "Recovery file is empty.";
				return false;
			}
			if (parsed.storeVersion > StoreVersion)
			{
				// Written by a newer game version: the backup is older still and must not replace it.
				refuseFallback = true;
				error = "Recovery file uses a newer store version.";
				return false;
			}
			if (parsed.storeVersion != StoreVersion || string.IsNullOrEmpty(parsed.account) || parsed.payload == null || parsed.revision < 0)
			{
				error = "Recovery file has invalid fields.";
				return false;
			}
			if (!ownerFile && parsed.account != expectedAccount)
			{
				error = "Recovery file belongs to another account.";
				return false;
			}
			if (parsed.checksum != Checksum(parsed.account, parsed.revision, parsed.payload))
			{
				error = "Recovery file checksum does not match.";
				return false;
			}
			envelope = parsed;
			return true;
		}
		catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is ArgumentException)
		{
			error = exception.Message;
			return false;
		}
	}

	private static bool TryWriteAtomic(string path, Envelope envelope, bool primaryValid, out string error)
	{
		error = null;
		try
		{
			byte[] bytes = Utf8.GetBytes(JsonUtility.ToJson(envelope));
			if (bytes.Length > MaxFileBytes)
			{
				error = "Recovery document exceeds the size limit.";
				return false;
			}
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			string temporary = path + ".tmp";
			using (FileStream stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				stream.Write(bytes, 0, bytes.Length);
				stream.Flush(true);
			}
			if (!File.Exists(path))
			{
				File.Move(temporary, path);
			}
			else if (primaryValid)
			{
				File.Replace(temporary, path, path + ".bak");
			}
			else
			{
				// Keep the unreadable file for diagnosis instead of overwriting it.
				File.Move(path, path + ".corrupt-" + Guid.NewGuid().ToString("N"));
				File.Move(temporary, path);
			}
			return true;
		}
		catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is ArgumentException)
		{
			error = exception.Message;
			return false;
		}
	}

	private static string Checksum(string account, long revision, string payload)
	{
		CultureInfo culture = CultureInfo.InvariantCulture;
		return Hash(StoreVersion.ToString(culture) + "\n" + account.Length.ToString(culture) + ":" + account + "\n" + revision.ToString(culture) + "\n" + payload);
	}

	private static string Hash(string value)
	{
		using (SHA256 sha = SHA256.Create())
		{
			byte[] hash = sha.ComputeHash(Utf8.GetBytes(value));
			StringBuilder builder = new StringBuilder(hash.Length * 2);
			foreach (byte item in hash)
			{
				builder.Append(item.ToString("x2", CultureInfo.InvariantCulture));
			}
			return builder.ToString();
		}
	}
}
