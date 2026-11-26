using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

// The Firebase SDK is not part of the recovered player, so official news are read over the public REST API
// with the same project values the shipped game uses.
public static class FirebaseRemoteConfigRest
{
	private const string ProjectNumber = "534717826144";

	private const string ProjectId = "api-project-534717826144";

	private const string AppId = "1:534717826144:android:d137be79ab4e25faf9ef05";

	private const string InstallationKey = "RC_REST_FID";

	[Serializable]
	private class InstallationToken
	{
		public string token;
	}

	[Serializable]
	private class InstallationResponse
	{
		public InstallationToken authToken;
	}

	public static IEnumerator FetchNewsJson(string apiKey, Action<string> done)
	{
		string fid = PlayerPrefs.GetString(InstallationKey, "");
		if (fid.Length != 22)
		{
			byte[] bytes = Guid.NewGuid().ToByteArray();
			bytes[0] = (byte)(0x70 | (bytes[0] & 0x0F));
			fid = Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').Substring(0, 22);
			PlayerPrefs.SetString(InstallationKey, fid);
		}
		string token = null;
		string installBody = "{\"fid\":\"" + fid + "\",\"appId\":\"" + AppId + "\",\"authVersion\":\"FIS_v2\",\"sdkVersion\":\"a:17.0.0\"}";
		using (UnityWebRequest request = Post("https://firebaseinstallations.googleapis.com/v1/projects/" + ProjectId + "/installations", installBody, apiKey))
		{
			yield return request.SendWebRequest();
			if (request.result == UnityWebRequest.Result.Success)
			{
				token = JsonUtility.FromJson<InstallationResponse>(request.downloadHandler.text)?.authToken?.token;
			}
		}
		if (string.IsNullOrEmpty(token))
		{
			done(null);
			yield break;
		}
		string fetchBody = "{\"appInstanceId\":\"" + fid + "\",\"appInstanceIdToken\":\"" + token + "\",\"appId\":\"" + AppId + "\",\"sdkVersion\":\"21.0.0\"}";
		using (UnityWebRequest request = Post("https://firebaseremoteconfig.googleapis.com/v1/projects/" + ProjectNumber + "/namespaces/firebase:fetch", fetchBody, apiKey))
		{
			yield return request.SendWebRequest();
			string json = null;
			if (request.result == UnityWebRequest.Result.Success)
			{
				try
				{
					Newtonsoft.Json.Linq.JObject root = Newtonsoft.Json.Linq.JObject.Parse(request.downloadHandler.text);
					json = (string)root["entries"]?["gameNewsRich"];
				}
				catch (Newtonsoft.Json.JsonException exception)
				{
					Debug.LogWarning("[GameNews] " + exception.Message);
				}
			}
			done(json);
		}
	}

	private static UnityWebRequest Post(string url, string body, string apiKey)
	{
		UnityWebRequest request = new UnityWebRequest(url, "POST");
		request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");
		request.SetRequestHeader("x-goog-api-key", apiKey);
		request.timeout = 15;
		return request;
	}
}
