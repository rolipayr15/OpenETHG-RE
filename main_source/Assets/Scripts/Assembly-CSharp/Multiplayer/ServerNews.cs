using System.Collections;
using BestHTTP;
using UnityEngine;
using UnityEngine.Networking;

// Recovery addition: news published from the server's admin panel. Read state stays in the local news archive.
public static class ServerNews
{
	// The public client key of the Firebase project, same value as in the shipped google-services file.
	private const string OfficialApiKey = "AIzaSyDYez_R9uaD7ANoyboSjmtWtj9WDK_inkg";

	public static IEnumerator Fetch()
	{
		if (!OnlineConfig.IsCustomServer)
		{
			string json = null;
			yield return FirebaseRemoteConfigRest.FetchNewsJson(OfficialApiKey, delegate(string result)
			{
				json = result;
			});
			if (!string.IsNullOrEmpty(json) && !OnlineConfig.IsCustomServer)
			{
				GameNewsConfig official = JsonUtility.FromJson<GameNewsConfig>(json);
				if (official != null && official.items != null)
				{
					GameNewsService.Synchronize(official.items);
				}
			}
			yield break;
		}
		using (UnityWebRequest request = UnityWebRequest.Get(ApiConfiguration.GetBackendHttpUrl("/api/v1/news")))
		{
			request.timeout = 8;
			yield return request.SendWebRequest();
			if (request.result != UnityWebRequest.Result.Success)
			{
				yield break;
			}
			GameNewsConfig config = JsonUtility.FromJson<GameNewsConfig>(request.downloadHandler.text);
			if (config == null || config.items == null)
			{
				yield break;
			}
			GameNewsService.Synchronize(config.items);
			foreach (GameNewsItem item in config.items)
			{
				GameNewsItem local = GameNewsService.GetById(item.id);
				if (item.showAsPopup && local != null && !local.isRead)
				{
					JournalEntryBox.Show(item.title, item.body, null);
					GameNewsService.MarkAsRead(item.id);
					break;
				}
			}
		}
	}
}
