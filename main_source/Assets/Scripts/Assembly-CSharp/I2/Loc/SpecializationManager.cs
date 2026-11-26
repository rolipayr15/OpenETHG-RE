using System;
using System.Text;
using System.Collections.Generic;

namespace I2.Loc
{
	public class SpecializationManager : BaseSpecializationManager
	{
		public static SpecializationManager Singleton = new SpecializationManager();

		private SpecializationManager()
		{
			InitializeSpecializations();
		}

		public static string GetSpecializedText(string text, string specialization = null)
		{
			if (string.IsNullOrEmpty(text) || text.IndexOf("[i2s_", StringComparison.Ordinal) < 0)
			{
				return text;
			}
			Dictionary<string, string> parts = GetSpecializations(text);
			string current = specialization ?? Singleton.GetCurrentSpecialization();
			while (true)
			{
				if (parts.TryGetValue(current, out string value))
				{
					return value;
				}
				if (current == "Any")
				{
					return text;
				}
				current = Singleton.GetFallbackSpecialization(current);
			}
		}

		public static string SetSpecializedText(string text, string newText, string specialization)
		{
			if (string.IsNullOrEmpty(specialization))
			{
				specialization = "Any";
			}
			if ((string.IsNullOrEmpty(text) || text.IndexOf("[i2s_", StringComparison.Ordinal) < 0) && specialization == "Any")
			{
				return newText;
			}
			Dictionary<string, string> parts = GetSpecializations(text);
			parts[specialization] = newText;
			return SetSpecializedText(parts);
		}

		public static string SetSpecializedText(Dictionary<string, string> specializations)
		{
			if (!specializations.TryGetValue("Any", out string any))
			{
				any = string.Empty;
			}
			StringBuilder builder = new StringBuilder(any);
			foreach (KeyValuePair<string, string> pair in specializations)
			{
				if (pair.Key != "Any" && !string.IsNullOrEmpty(pair.Value))
				{
					builder.Append("[i2s_").Append(pair.Key).Append(']').Append(pair.Value);
				}
			}
			return builder.ToString();
		}

		public static Dictionary<string, string> GetSpecializations(string text, Dictionary<string, string> buffer = null)
		{
			Dictionary<string, string> parts = buffer ?? new Dictionary<string, string>(StringComparer.Ordinal);
			parts.Clear();
			if (text == null)
			{
				return parts;
			}
			int end = text.IndexOf("[i2s_", StringComparison.Ordinal);
			if (end < 0)
			{
				parts["Any"] = text;
				return parts;
			}
			parts["Any"] = text.Substring(0, end);
			while (end >= 0)
			{
				int close = text.IndexOf(']', end);
				if (close < 0)
				{
					break;
				}
				string name = text.Substring(end + 5, close - end - 5);
				int next = text.IndexOf("[i2s_", close, StringComparison.Ordinal);
				parts[name] = next < 0 ? text.Substring(close + 1) : text.Substring(close + 1, next - close - 1);
				end = next;
			}
			return parts;
		}

		public static void AppendSpecializations(string text, List<string> list = null)
		{
			if (string.IsNullOrEmpty(text) || list == null)
			{
				return;
			}
			foreach (string key in GetSpecializations(text).Keys)
			{
				if (!list.Contains(key))
				{
					list.Add(key);
				}
			}
		}
	}
}
