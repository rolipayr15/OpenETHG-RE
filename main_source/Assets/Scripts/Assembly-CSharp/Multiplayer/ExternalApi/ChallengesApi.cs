using Newtonsoft.Json;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BestHTTP;
using ExternalApi.Models;

namespace Multiplayer.ExternalApi
{
	public class ChallengesApi : IChallengesApi
	{
		public async Task<CurrentDailyChallengeResponse?> GetCurrentDailyChallengeAsync()
		{
			HTTPResponse response = await CiekawostkomierzApiClient.GetAsyncUnauthenticated("/challenges/daily/current");
			if (response == null || !response.IsSuccess)
			{
				return null;
			}
			return JsonConvert.DeserializeObject<CurrentDailyChallengeResponse>(response.DataAsText);
		}
	}
}
