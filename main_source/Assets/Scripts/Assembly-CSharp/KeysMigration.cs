using CodeStage.AntiCheat.Storage;
using UnityEngine;

public class KeysMigration : MonoBehaviour
{
	private void Start()
	{
		if (!ObscuredPrefs.HasKey("KEYS_UPDATED"))
		{
			ObscuredPrefs.Set("KEYS_UPDATED", true);
			if (ObscuredPrefs.HasKey("TICKETS"))
			{
				KeysManager.OverwriteMultiplayerKeys(KeysManager.GetMultiplayerKeys() * 10);
			}
		}
	}
}
