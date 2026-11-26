using UnityEngine;
namespace I2.Loc
{
	public static class PersistentStorage
	{
		public enum eFileType
		{
			Raw = 0,
			Persistent = 1,
			Temporal = 2,
			Streaming = 3
		}

		private static I2CustomPersistentStorage mStorage;

		public static void SetSetting_String(string key, string value)
		{
			PlayerPrefs.SetString(key, value);
		}

		public static string GetSetting_String(string key, string defaultValue)
		{
			return PlayerPrefs.GetString(key, defaultValue);
		}

		public static void DeleteSetting(string key)
		{
			PlayerPrefs.DeleteKey(key);
		}

		public static bool HasSetting(string key)
		{
			return PlayerPrefs.HasKey(key);
		}

		public static void ForceSaveSettings()
		{
			PlayerPrefs.Save();
		}

		public static bool CanAccessFiles()
		{
			return false;
		}

		public static bool SaveFile(eFileType fileType, string fileName, string data, bool logExceptions = true)
		{
			return false;
		}

		public static string LoadFile(eFileType fileType, string fileName, bool logExceptions = true)
		{
			return null;
		}

		public static bool DeleteFile(eFileType fileType, string fileName, bool logExceptions = true)
		{
			return false;
		}

		public static bool HasFile(eFileType fileType, string fileName, bool logExceptions = true)
		{
			return false;
		}
	}
}
