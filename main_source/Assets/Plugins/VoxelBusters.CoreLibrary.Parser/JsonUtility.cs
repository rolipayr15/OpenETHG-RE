using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace VoxelBusters.CoreLibrary.Parser
{
	/// <summary>
	/// Own replacement for the Essential Kit JSON parser (the shipped assembly was an empty stub).
	/// Objects become Dictionary&lt;string, object&gt;, arrays List&lt;object&gt;, integers long, other numbers double.
	/// </summary>
	public static class JsonUtility
	{
		public static object FromJson(string jsonStr)
		{
			int errorIndex = -1;
			return FromJson(jsonStr, ref errorIndex);
		}

		/// <param name="errorIndex">Stays -1 when the whole text was read; otherwise the position of the first problem.</param>
		public static object FromJson(string jsonStr, ref int errorIndex)
		{
			if (jsonStr == null)
			{
				errorIndex = 0;
				return null;
			}
			int position = 0;
			try
			{
				object value = ReadValue(jsonStr, ref position);
				SkipSpace(jsonStr, ref position);
				if (position != jsonStr.Length)
				{
					errorIndex = position;
					return null;
				}
				return value;
			}
			catch (FormatException)
			{
				errorIndex = position;
				return null;
			}
		}

		public static bool IsNull(string jsonStr)
		{
			return jsonStr == null || jsonStr.Trim() == "null";
		}

		public static string ToJson(object obj)
		{
			StringBuilder builder = new StringBuilder();
			WriteValue(builder, obj);
			return builder.ToString();
		}

		private static object ReadValue(string json, ref int position)
		{
			SkipSpace(json, ref position);
			if (position >= json.Length)
			{
				throw new FormatException();
			}
			switch (json[position])
			{
			case '{':
				return ReadObject(json, ref position);
			case '[':
				return ReadArray(json, ref position);
			case '"':
				return ReadString(json, ref position);
			case 't':
				ReadLiteral(json, ref position, "true");
				return true;
			case 'f':
				ReadLiteral(json, ref position, "false");
				return false;
			case 'n':
				ReadLiteral(json, ref position, "null");
				return null;
			default:
				return ReadNumber(json, ref position);
			}
		}

		private static IDictionary ReadObject(string json, ref int position)
		{
			Dictionary<string, object> result = new Dictionary<string, object>(StringComparer.Ordinal);
			position++;
			if (Take(json, ref position, '}'))
			{
				return result;
			}
			do
			{
				SkipSpace(json, ref position);
				string key = ReadString(json, ref position);
				if (!Take(json, ref position, ':'))
				{
					throw new FormatException();
				}
				result[key] = ReadValue(json, ref position);
				if (Take(json, ref position, '}'))
				{
					return result;
				}
			}
			while (Take(json, ref position, ','));
			throw new FormatException();
		}

		private static IList ReadArray(string json, ref int position)
		{
			List<object> result = new List<object>();
			position++;
			if (Take(json, ref position, ']'))
			{
				return result;
			}
			do
			{
				result.Add(ReadValue(json, ref position));
				if (Take(json, ref position, ']'))
				{
					return result;
				}
			}
			while (Take(json, ref position, ','));
			throw new FormatException();
		}

		private static string ReadString(string json, ref int position)
		{
			if (position >= json.Length || json[position] != '"')
			{
				throw new FormatException();
			}
			position++;
			StringBuilder builder = new StringBuilder();
			while (position < json.Length)
			{
				char c = json[position++];
				if (c == '"')
				{
					return builder.ToString();
				}
				if (c != '\\')
				{
					builder.Append(c);
					continue;
				}
				if (position >= json.Length)
				{
					break;
				}
				c = json[position++];
				switch (c)
				{
				case 'b':
					builder.Append('\b');
					break;
				case 'f':
					builder.Append('\f');
					break;
				case 'n':
					builder.Append('\n');
					break;
				case 'r':
					builder.Append('\r');
					break;
				case 't':
					builder.Append('\t');
					break;
				case 'u':
				{
					ushort code = 0;
					if (position + 4 > json.Length || !ushort.TryParse(json.Substring(position, 4), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out code))
					{
						throw new FormatException();
					}
					builder.Append((char)code);
					position += 4;
					break;
				}
				default:
					builder.Append(c);
					break;
				}
			}
			throw new FormatException();
		}

		private static object ReadNumber(string json, ref int position)
		{
			int start = position;
			bool integer = true;
			while (position < json.Length)
			{
				char c = json[position];
				if (c == '.' || c == 'e' || c == 'E')
				{
					integer = false;
				}
				else if ((c < '0' || c > '9') && c != '-' && c != '+')
				{
					break;
				}
				position++;
			}
			string text = json.Substring(start, position - start);
			if (integer && long.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out long whole))
			{
				return whole;
			}
			if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double number))
			{
				return number;
			}
			position = start;
			throw new FormatException();
		}

		private static void ReadLiteral(string json, ref int position, string literal)
		{
			if (string.CompareOrdinal(json, position, literal, 0, literal.Length) != 0)
			{
				throw new FormatException();
			}
			position += literal.Length;
		}

		private static bool Take(string json, ref int position, char expected)
		{
			SkipSpace(json, ref position);
			if (position < json.Length && json[position] == expected)
			{
				position++;
				return true;
			}
			return false;
		}

		private static void SkipSpace(string json, ref int position)
		{
			while (position < json.Length && (json[position] == ' ' || json[position] == '\t' || json[position] == '\r' || json[position] == '\n'))
			{
				position++;
			}
		}

		private static void WriteValue(StringBuilder builder, object value)
		{
			switch (value)
			{
			case null:
				builder.Append("null");
				break;
			case string text:
				WriteString(builder, text);
				break;
			case bool flag:
				builder.Append(flag ? "true" : "false");
				break;
			case IDictionary map:
			{
				builder.Append('{');
				bool first = true;
				foreach (DictionaryEntry entry in map)
				{
					if (!first)
					{
						builder.Append(',');
					}
					first = false;
					WriteString(builder, Convert.ToString(entry.Key, CultureInfo.InvariantCulture));
					builder.Append(':');
					WriteValue(builder, entry.Value);
				}
				builder.Append('}');
				break;
			}
			case IEnumerable list:
			{
				builder.Append('[');
				bool first = true;
				foreach (object item in list)
				{
					if (!first)
					{
						builder.Append(',');
					}
					first = false;
					WriteValue(builder, item);
				}
				builder.Append(']');
				break;
			}
			case IFormattable number:
				builder.Append(number.ToString(null, CultureInfo.InvariantCulture));
				break;
			default:
				WriteString(builder, value.ToString());
				break;
			}
		}

		private static void WriteString(StringBuilder builder, string text)
		{
			builder.Append('"');
			foreach (char c in text)
			{
				switch (c)
				{
				case '"':
					builder.Append("\\\"");
					break;
				case '\\':
					builder.Append("\\\\");
					break;
				case '\b':
					builder.Append("\\b");
					break;
				case '\f':
					builder.Append("\\f");
					break;
				case '\n':
					builder.Append("\\n");
					break;
				case '\r':
					builder.Append("\\r");
					break;
				case '\t':
					builder.Append("\\t");
					break;
				default:
					if (c < ' ')
					{
						builder.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
					}
					else
					{
						builder.Append(c);
					}
					break;
				}
			}
			builder.Append('"');
		}
	}
}
