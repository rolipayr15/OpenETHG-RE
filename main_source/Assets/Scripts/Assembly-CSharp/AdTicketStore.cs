using UnityEngine;

// "Watch an ad, play this chapter once" tickets, at most one per store item.
public static class AdTicketStore
{
	private const string KeyPrefix = "ad_ticket_storeitem_";

	private static string Key(StoreItems item)
	{
		return KeyPrefix + item;
	}

	public static bool HasTicket(StoreItems item)
	{
		return PlayerPrefs.GetInt(Key(item), 0) > 0;
	}

	public static void Grant(StoreItems item)
	{
		string key = Key(item);
		PlayerPrefs.SetInt(key, Mathf.Min(PlayerPrefs.GetInt(key, 0) + 1, 1));
		PlayerPrefs.Save();
		FirebaseWinbackRewardSequence.Refresh();
		Debug.Log(string.Format("[AdTicket] Granted for {0}", item));
	}

	public static bool Consume(StoreItems item)
	{
		string key = Key(item);
		int count = PlayerPrefs.GetInt(key, 0);
		if (count > 0)
		{
			PlayerPrefs.SetInt(key, count - 1);
			PlayerPrefs.Save();
			Debug.Log(string.Format("[AdTicket] Consumed for {0}", item));
		}
		return count > 0;
	}
}
