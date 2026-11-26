// Stand-in for Anti-Cheat Toolkit's ObscuredPrefs, written for this project while the licensed
// package is unavailable. It offers only the calls the game makes and stores values in
// PlayerPrefs without encryption, under an "ACTk." key prefix so they never collide with the
// game's plain PlayerPrefs keys. Delete the Replacements/ACTk folder when the real package is
// back; saves written by this stand-in are not readable by real ACTk.
using System;
using System.Globalization;
using UnityEngine;

namespace CodeStage.AntiCheat.Storage
{
	public static class ObscuredPrefs
	{
		private const string Prefix = "ACTk.";

		public static bool HasKey(string key)
		{
			return PlayerPrefs.HasKey(Prefix + key);
		}

		public static void DeleteKey(string key)
		{
			PlayerPrefs.DeleteKey(Prefix + key);
		}

		public static int GetInt(string key, int defaultValue = 0)
		{
			return PlayerPrefs.GetInt(Prefix + key, defaultValue);
		}

		public static void SetInt(string key, int value)
		{
			PlayerPrefs.SetInt(Prefix + key, value);
		}

		public static string GetString(string key, string defaultValue = "")
		{
			return PlayerPrefs.GetString(Prefix + key, defaultValue);
		}

		public static void SetString(string key, string value)
		{
			PlayerPrefs.SetString(Prefix + key, value);
		}

		public static void Save()
		{
			PlayerPrefs.Save();
		}

		public static void Set<T>(string key, T value)
		{
			PlayerPrefs.SetString(Prefix + key, Serialize(value));
		}

		public static T Get<T>(string key, T defaultValue = default)
		{
			if (!PlayerPrefs.HasKey(Prefix + key))
			{
				return defaultValue;
			}
			try
			{
				return Deserialize<T>(PlayerPrefs.GetString(Prefix + key));
			}
			catch (Exception e)
			{
				Debug.LogWarning("ObscuredPrefs: could not read '" + key + "': " + e.Message);
				return defaultValue;
			}
		}

		private static string Serialize<T>(T value)
		{
			switch (value)
			{
				case null:
					return string.Empty;
				case string s:
					return s;
				case bool b:
					return b ? "1" : "0";
				case Enum e:
					return Convert.ToInt64(e, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
				case IConvertible c:
					return c.ToString(CultureInfo.InvariantCulture);
				default:
					return JsonUtility.ToJson(value);
			}
		}

		private static T Deserialize<T>(string text)
		{
			Type t = typeof(T);
			if (t == typeof(string))
			{
				return (T)(object)text;
			}
			if (t == typeof(bool))
			{
				return (T)(object)(text == "1" || text.Equals("true", StringComparison.OrdinalIgnoreCase));
			}
			if (t.IsEnum)
			{
				return (T)Enum.ToObject(t, long.Parse(text, CultureInfo.InvariantCulture));
			}
			if (typeof(IConvertible).IsAssignableFrom(t))
			{
				return (T)Convert.ChangeType(text, t, CultureInfo.InvariantCulture);
			}
			return JsonUtility.FromJson<T>(text);
		}
	}
}
