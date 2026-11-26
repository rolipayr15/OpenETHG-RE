using System.Security.Cryptography;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text;

public static class LegacyGameProgressMigration
{
	public const int MigrationVersion = 1;

	public const int MaxSourceBytes = 1048576;

	public const int MaxBase64Characters = 1463640;

	private const int MaxSnapshotNameCharacters = 256;

	private static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);

	private static readonly Dictionary<string, string[]> Unlocks = new Dictionary<string, string[]>(StringComparer.Ordinal) { { "hospital", new string[] { "eyes.hospital", "charlie_hospital" } }, { "charlie", new string[] { "charlie", "charlie_hospital" } }, { "trickOrTreat", new string[] { "trickortreat" } }, { "endless", new string[] { "endless" } }, { "dog", new string[] { "dog", "good_boy_school" } }, { "doubleTrouble", new string[] { "doubleenemy" } }, { "school", new string[] { "school", "good_boy_school" } }, { "charlie_hospital", new string[] { "charlie", "charlie_hospital" } }, { "good_boy_school", new string[] { "school", "good_boy_school" } }, { "immersive", new string[] { "immersive" } }, { "witch", new string[] { "witch" } }, { "pixel", new string[] { "pixel" } } };

	public static bool TryConvert(string snapshotName, string base64, out LegacyMigrationSource source, out string error)
	{
		source = null;
		error = null;
		if (!ValidSnapshotName(snapshotName))
		{
			error = "Invalid or non-legacy snapshot name.";
			return false;
		}
		if (string.IsNullOrEmpty(base64) || base64.Length > MaxBase64Characters)
		{
			error = "Missing or oversized legacy snapshot.";
			return false;
		}
		StringBuilder builder = new StringBuilder(base64.Length);
		foreach (char c in base64)
		{
			if (c != ' ' && c != '\t' && c != '\n' && c != '\r')
			{
				builder.Append(c);
			}
		}
		string compact = builder.ToString();
		if (compact.Length == 0 || compact.Length > 1398104)
		{
			error = "Missing or oversized legacy snapshot.";
			return false;
		}
		byte[] bytes;
		string text;
		try
		{
			bytes = Convert.FromBase64String(compact);
			if (bytes.Length == 0 || bytes.Length > MaxSourceBytes || Convert.ToBase64String(bytes) != compact)
			{
				error = "Invalid or oversized legacy Base64 payload.";
				return false;
			}
			text = StrictUtf8.GetString(bytes);
		}
		catch (Exception exception) when (exception is FormatException || exception is ArgumentException)
		{
			error = "Invalid or oversized legacy Base64 payload.";
			return false;
		}
		if (text.Length > 0 && text[0] == '﻿')
		{
			text = text.Substring(1);
		}
		if (!SaveGameProgressData.TryReadJsonObject(text, out IDictionary document, out error, MaxSourceBytes))
		{
			return false;
		}
		GameSaveData data = SaveGameProgressData.Empty();
		bool recognized = false;
		foreach (DictionaryEntry entry in document)
		{
			string key = entry.Key as string;
			if (key == "coins")
			{
				if (!(entry.Value is long coins) || coins >> 31 != 0)
				{
					error = "Legacy coins must be a nonnegative integer.";
					return false;
				}
			}
			else
			{
				if (key == null || !Unlocks.TryGetValue(key, out string[] items))
				{
					error = "Unsupported legacy field; preserve the original snapshot.";
					return false;
				}
				if (!(entry.Value is bool unlocked))
				{
					error = "Legacy unlock flags must be booleans.";
					return false;
				}
				recognized = true;
				if (unlocked)
				{
					data.unlockedItems.AddRange(items);
				}
			}
		}
		if (!recognized)
		{
			error = "The snapshot has no recognized legacy unlock flags.";
			return false;
		}
		string hash;
		using (SHA256 sha = SHA256.Create())
		{
			byte[] digest = sha.ComputeHash(bytes);
			StringBuilder hex = new StringBuilder(digest.Length * 2);
			foreach (byte item in digest)
			{
				hex.Append(item.ToString("x2"));
			}
			hash = hex.ToString();
		}
		source = new LegacyMigrationSource
		{
			snapshotName = snapshotName,
			sourceHash = hash,
			base64 = base64,
			data = SaveGameProgressData.Merge(data)
		};
		return true;
	}

	private static bool ValidSnapshotName(string name)
	{
		if (string.IsNullOrWhiteSpace(name) || name.Length > MaxSnapshotNameCharacters || name.StartsWith("cpnp-snapshot-", StringComparison.Ordinal))
		{
			return false;
		}
		foreach (char c in name)
		{
			if (char.IsControl(c))
			{
				return false;
			}
		}
		return true;
	}
}
