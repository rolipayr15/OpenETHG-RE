using Debug = UnityEngine.Debug;
using UnityEngine;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BestHTTP;
using ExternalApi.Models;

namespace Multiplayer.ExternalApi
{
	public class ExpApi : IExpApi
	{
		private IExperienceManager experienceManager;

		public ExpApi(IExperienceManager experienceManager)
		{
			this.experienceManager = experienceManager;
		}

		public Task<HTTPResponse> SendGrantExpAsync(GrantExpData grantExpData)
		{
			return CiekawostkomierzApiClient.PostJsonAsync("/user/exp/grant", JsonConvert.SerializeObject(grantExpData));
		}

		public async Task<LocalExpInfoResponse?> GetOpponentLocalExpInfo(string gameId)
		{
			HTTPResponse response = await CiekawostkomierzApiClient.GetAsync("/user/exp/game-opponent-info?gameId=" + gameId);
			if (response != null && response.IsSuccess)
			{
				Debug.Log(response.DataAsText);
				return JsonConvert.DeserializeObject<LocalExpInfoResponse>(response.DataAsText);
			}
			Debug.Log("Failed to get opponent local exp info: " + response?.DataAsText);
			return null;
		}
	}
}
