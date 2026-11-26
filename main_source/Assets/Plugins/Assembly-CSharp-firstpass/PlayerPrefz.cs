using UnityEngine;

public class PlayerPrefz : PlayerPrefs
{
	public static bool GetBool(string playerPrefsKey, bool defaultValue)
	{
		return PlayerPrefs.GetInt(playerPrefsKey, defaultValue ? 1 : 0) == 1;
	}

	public static void SetBool(string playerPrefsKey, bool value)
	{
		PlayerPrefs.SetInt(playerPrefsKey, value ? 1 : 0);
	}
}
