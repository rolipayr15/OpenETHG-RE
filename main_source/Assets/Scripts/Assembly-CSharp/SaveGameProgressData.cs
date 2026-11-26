using UnityEngine;
using System.Text;
using System.Globalization;
using System;
using System.Collections;
using System.Collections.Generic;

public static class SaveGameProgressData
{
	private sealed class JsonSyntaxCheck
	{
		private readonly string text;

		private int position;

		public JsonSyntaxCheck(string text)
		{
			this.text = text;
		}

		public void Validate()
		{
			Value(0);
			Space();
			if (position != text.Length)
			{
				Fail();
			}
		}

		private void Value(int depth)
		{
			if (depth >= 9)
			{
				Fail();
			}
			Space();
			if (position >= text.Length)
			{
				Fail();
			}
			switch (text[position])
			{
			case '"':
				String();
				return;
			case 'f':
				Literal("false");
				return;
			case 'n':
				Literal("null");
				return;
			case 't':
				Literal("true");
				return;
			case '[':
				position++;
				if (Take(']'))
				{
					return;
				}
				do
				{
					Value(depth + 1);
					if (Take(']'))
					{
						return;
					}
				}
				while (Take(','));
				Fail();
				return;
			case '{':
			{
				position++;
				HashSet<string> keys = new HashSet<string>(StringComparer.Ordinal);
				if (Take('}'))
				{
					return;
				}
				do
				{
					Space();
					if (!keys.Add(String()) || !Take(':'))
					{
						break;
					}
					Value(depth + 1);
					if (Take('}'))
					{
						return;
					}
				}
				while (Take(','));
				Fail();
				return;
			}
			default:
				Number();
				return;
			}
		}

		private string String()
		{
			if (position >= text.Length || text[position++] != '"')
			{
				Fail();
			}
			StringBuilder builder = new StringBuilder();
			while (position < text.Length)
			{
				char c = text[position++];
				if (c == '"')
				{
					return builder.ToString();
				}
				if (c < ' ')
				{
					break;
				}
				if (c != '\\')
				{
					builder.Append(c);
					continue;
				}
				if (position >= text.Length)
				{
					break;
				}
				c = text[position++];
				switch (c)
				{
				case '"':
				case '/':
				case '\\':
					builder.Append(c);
					continue;
				case 'b':
					builder.Append('\b');
					continue;
				case 'f':
					builder.Append('\f');
					continue;
				case 'n':
					builder.Append('\n');
					continue;
				case 'r':
					builder.Append('\r');
					continue;
				case 't':
					builder.Append('\t');
					continue;
				case 'u':
					ushort code = 0;
					if (text.Length < position + 4 || !ushort.TryParse(text.Substring(position, 4), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out code))
					{
						Fail();
					}
					builder.Append((char)code);
					position += 4;
					continue;
				}
				break;
			}
			Fail();
			return null;
		}

		private void Number()
		{
			if (position < text.Length && text[position] == '-')
			{
				position++;
			}
			if (position >= text.Length)
			{
				Fail();
			}
			if (text[position] == '0')
			{
				position++;
			}
			else
			{
				if (text[position] < '1' || text[position] > '9')
				{
					Fail();
				}
				Digits();
			}
			if (position < text.Length && text[position] == '.')
			{
				position++;
				Digits();
			}
			if (position < text.Length && (text[position] == 'e' || text[position] == 'E'))
			{
				position++;
				if (position < text.Length && (text[position] == '+' || text[position] == '-'))
				{
					position++;
				}
				Digits();
			}
		}

		private void Digits()
		{
			int start = position;
			while (position < text.Length && text[position] >= '0' && text[position] <= '9')
			{
				position++;
			}
			if (position == start)
			{
				Fail();
			}
		}

		private void Literal(string literal)
		{
			if (literal.Length + position > text.Length || string.CompareOrdinal(text, position, literal, 0, literal.Length) != 0)
			{
				Fail();
			}
			position += literal.Length;
		}

		private bool Take(char c)
		{
			Space();
			if (position < text.Length && text[position] == c)
			{
				position++;
				return true;
			}
			return false;
		}

		private void Space()
		{
			while (position < text.Length && (text[position] == ' ' || text[position] == '\t' || text[position] == '\r' || text[position] == '\n'))
			{
				position++;
			}
		}

		private static void Fail()
		{
			throw new FormatException("Invalid or excessively nested JSON syntax.");
		}
	}

	public const int CurrentVersion = 2;

	public const int MaxJsonCharacters = 1048576;

	private const int MaxRecords = 4096;

	private const int MaxIdentifierCharacters = 256;

	public static GameSaveData Empty()
	{
		return new GameSaveData();
	}

	public static bool TryParse(string json, out GameSaveData data, out string error)
	{
		data = null;
		error = null;
		if (string.IsNullOrEmpty(json))
		{
			data = Empty();
			return true;
		}
		if (!TryReadJsonObject(json, out IDictionary document, out error))
		{
			return false;
		}
		try
		{
			RequireFields(document, "schemaVersion", "unlockedItems", "difficultyStars", "unlockedJournalEntries");
			if (document.Contains("schemaVersion"))
			{
				int version = ReadInteger(document["schemaVersion"], "schemaVersion");
				if (version < 1 || version > CurrentVersion)
				{
					throw new FormatException("Unsupported save schema version.");
				}
			}
			GameSaveData parsed = Empty();
			parsed.unlockedItems = ReadIds(document["unlockedItems"], "unlockedItems");
			parsed.unlockedJournalEntries = ReadIds(document["unlockedJournalEntries"], "unlockedJournalEntries");
			object stars = document["difficultyStars"];
			if (stars != null)
			{
				if (!(stars is IList list))
				{
					throw new FormatException("difficultyStars must be a list.");
				}
				RequireCount(list.Count);
				foreach (object item in list)
				{
					if (!(item is IDictionary record))
					{
						throw new FormatException("Invalid difficulty record.");
					}
					RequireFields(record, "key", "value");
					string key = ReadId(record["key"]);
					int value = ReadInteger(record["value"], "difficulty value");
					RequireDifficulty(value);
					parsed.difficultyStars.Add(new DifficultyRecord
					{
						key = key,
						value = value
					});
				}
			}
			data = Merge(parsed);
			return true;
		}
		catch (FormatException exception)
		{
			error = exception.Message;
			return false;
		}
	}

	public static string Serialize(GameSaveData data)
	{
		string json = JsonUtility.ToJson(Merge(data));
		if (json.Length > MaxJsonCharacters)
		{
			throw new FormatException("Save document is too large.");
		}
		return json;
	}

	public static GameSaveData Merge(params GameSaveData[] sources)
	{
		SortedSet<string> items = new SortedSet<string>(StringComparer.Ordinal);
		SortedSet<string> journal = new SortedSet<string>(StringComparer.Ordinal);
		SortedDictionary<string, int> difficulty = new SortedDictionary<string, int>(StringComparer.Ordinal);
		if (sources != null)
		{
			foreach (GameSaveData source in sources)
			{
				if (source == null)
				{
					continue;
				}
				if (source.schemaVersion < 1 || source.schemaVersion > CurrentVersion)
				{
					throw new FormatException("Unsupported save schema version.");
				}
				AddIds(items, source.unlockedItems, true);
				AddIds(journal, source.unlockedJournalEntries, false);
				if (source.difficultyStars == null)
				{
					continue;
				}
				RequireCount(source.difficultyStars.Count);
				foreach (DifficultyRecord record in source.difficultyStars)
				{
					if (record == null)
					{
						throw new FormatException("Invalid difficulty record.");
					}
					string key = ReadId(record.key);
					RequireDifficulty(record.value);
					difficulty.TryGetValue(key, out int current);
					difficulty[key] = Math.Max(current, record.value);
				}
			}
		}
		RequireCount(items.Count);
		RequireCount(journal.Count);
		RequireCount(difficulty.Count);
		GameSaveData merged = Empty();
		merged.unlockedItems.AddRange(items);
		merged.unlockedJournalEntries.AddRange(journal);
		foreach (KeyValuePair<string, int> pair in difficulty)
		{
			merged.difficultyStars.Add(new DifficultyRecord
			{
				key = pair.Key,
				value = pair.Value
			});
		}
		return merged;
	}

	public static GameSaveData Difference(GameSaveData newer, GameSaveData older)
	{
		GameSaveData current = Merge(newer);
		GameSaveData previous = Merge(older);
		GameSaveData difference = Empty();
		HashSet<string> knownItems = new HashSet<string>(previous.unlockedItems, StringComparer.Ordinal);
		HashSet<string> knownJournal = new HashSet<string>(previous.unlockedJournalEntries, StringComparer.Ordinal);
		Dictionary<string, int> knownStars = new Dictionary<string, int>(StringComparer.Ordinal);
		foreach (DifficultyRecord record in previous.difficultyStars)
		{
			knownStars.Add(record.key, record.value);
		}
		foreach (string item in current.unlockedItems)
		{
			if (!knownItems.Contains(item))
			{
				difference.unlockedItems.Add(item);
			}
		}
		foreach (string entry in current.unlockedJournalEntries)
		{
			if (!knownJournal.Contains(entry))
			{
				difference.unlockedJournalEntries.Add(entry);
			}
		}
		foreach (DifficultyRecord record in current.difficultyStars)
		{
			knownStars.TryGetValue(record.key, out int known);
			if (record.value > known)
			{
				difference.difficultyStars.Add(new DifficultyRecord
				{
					key = record.key,
					value = record.value
				});
			}
		}
		return difference;
	}

	public static bool ContainsAll(GameSaveData actual, GameSaveData expected)
	{
		GameSaveData missing = Difference(expected, actual);
		return missing.unlockedItems.Count == 0 && missing.unlockedJournalEntries.Count == 0 && missing.difficultyStars.Count == 0;
	}

	private static List<string> ReadIds(object value, string field)
	{
		List<string> result = new List<string>();
		if (value == null)
		{
			return result;
		}
		if (!(value is IList list))
		{
			throw new FormatException(field + " must be a list.");
		}
		RequireCount(list.Count);
		foreach (object item in list)
		{
			result.Add(ReadId(item));
		}
		return result;
	}

	private static void AddIds(SortedSet<string> target, List<string> source, bool aliases)
	{
		if (source == null)
		{
			return;
		}
		RequireCount(source.Count);
		foreach (string item in source)
		{
			string id = ReadId(item);
			if (aliases)
			{
				id = id == "charliehospital" ? "charlie_hospital" : id;
				id = id == "goodboyschool" ? "good_boy_school" : id;
			}
			target.Add(id);
		}
	}

	private static string ReadId(object value)
	{
		string id = value as string;
		if (!string.IsNullOrWhiteSpace(id) && id.Length <= MaxIdentifierCharacters)
		{
			bool valid = true;
			foreach (char c in id)
			{
				if (char.IsControl(c))
				{
					valid = false;
					break;
				}
			}
			if (valid)
			{
				return id;
			}
		}
		throw new FormatException("Invalid save identifier.");
	}

	private static int ReadInteger(object value, string field)
	{
		if (value is long number && number == (int)number)
		{
			return (int)number;
		}
		throw new FormatException(field + " must be an integer.");
	}

	private static void RequireDifficulty(int value)
	{
		if (value < 0 || value > 5)
		{
			throw new FormatException("Difficulty is outside the supported range 0–5.");
		}
	}

	private static void RequireCount(int count)
	{
		if (count > MaxRecords)
		{
			throw new FormatException("Save document has too many records.");
		}
	}

	private static void RequireFields(IDictionary fields, params string[] allowed)
	{
		foreach (object key in fields.Keys)
		{
			if (!(key is string name) || Array.IndexOf(allowed, name) < 0)
			{
				throw new FormatException("Unsupported save field; preserve the source document.");
			}
		}
	}

	public static bool TryReadJsonObject(string json, out IDictionary document, out string error, int maxCharacters = 1048576)
	{
		document = null;
		error = null;
		if (string.IsNullOrWhiteSpace(json) || json.Length > maxCharacters)
		{
			error = "Missing or oversized JSON document.";
			return false;
		}
		try
		{
			string trimmed = json.Trim();
			if (trimmed[0] != '{')
			{
				throw new FormatException("Expected a JSON object.");
			}
			new JsonSyntaxCheck(trimmed).Validate();
			int errorIndex = -1;
			object parsed = VoxelBusters.CoreLibrary.Parser.JsonUtility.FromJson(trimmed, ref errorIndex);
			document = parsed as IDictionary;
			if (errorIndex != -1 || document == null)
			{
				throw new FormatException("Expected a complete JSON object.");
			}
			return true;
		}
		catch (FormatException exception)
		{
			document = null;
			error = exception.Message;
			return false;
		}
	}
}
