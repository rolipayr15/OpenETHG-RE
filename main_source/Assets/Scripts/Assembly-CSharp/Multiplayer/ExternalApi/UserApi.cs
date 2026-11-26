using System.Threading.Tasks;
using BestHTTP;
using ExternalApi.Models;
using Newtonsoft.Json;
using UnityEngine;

namespace Multiplayer.ExternalApi
{
	public class UserApi : IUserApi
	{
		public static int LocalExp;

		public Task<HTTPResponse> SetUsernameAsync(UsernameData username)
		{
			return CiekawostkomierzApiClient.PostJsonAsync("/user/name", JsonConvert.SerializeObject(username));
		}

		public async Task<UsernameResponse?> GetUsernameAsync()
		{
			HTTPResponse response = await CiekawostkomierzApiClient.GetAsync("/user/name");
			if (response == null || !response.IsSuccess)
			{
				return null;
			}
			return JsonConvert.DeserializeObject<UsernameResponse>(response.DataAsText);
		}

		public async Task<SyncUserInfoResponse> SyncUserInfoAsync()
		{
			int localDistance = PlayerPrefs.GetInt("WalkedDistance", 0);
			string localName = UserGameSettings.multiplayerUsername;
			if (string.IsNullOrEmpty(localName))
			{
				Debug.Log("pusty username, ale glupio");
			}
			SyncUserInfoData data = new SyncUserInfoData
			{
				localExp = LocalExp,
				username = localName,
				localDistance = localDistance
			};
			SyncUserInfoResponse fallback = new SyncUserInfoResponse
			{
				username = UserGameSettings.multiplayerUsername,
				previousLocalExp = LocalExp,
				newLocalExp = LocalExp,
				previousLocalDistance = localDistance,
				newLocalDistance = localDistance
			};
			HTTPResponse response = await CiekawostkomierzApiClient.PostJsonAsync("/user/sync-info", JsonConvert.SerializeObject(data));
			if (response == null || !response.IsSuccess)
			{
				return fallback;
			}
			SyncUserInfoResponse result = JsonConvert.DeserializeObject<SyncUserInfoResponse>(response.DataAsText);
			if (!string.IsNullOrEmpty(result.username))
			{
				OnlineConfig.SavePlayerName(result.username);
			}
			PlayerPrefs.SetInt("WalkedDistance", result.newLocalDistance);
			UserGameSettings.totalDistance = result.newLocalDistance;
			return result;
		}

		public Task<HTTPResponse> SetDistanceAsync(DistanceData distanceData)
		{
			return CiekawostkomierzApiClient.PostJsonAsync("/user/stats/game-distance", JsonConvert.SerializeObject(distanceData));
		}
	}
}
