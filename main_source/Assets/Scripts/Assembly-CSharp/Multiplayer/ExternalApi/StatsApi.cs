using Newtonsoft.Json;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BestHTTP;

namespace Multiplayer.ExternalApi
{
	public class StatsApi : IStatsApi
	{
		private bool statsCalled;

		private HTTPResponse cachedResponse;

		private async void StartTimer()
		{
			await Task.Delay(10000);
			statsCalled = false;
		}

		public void InvalidateStatsCache()
		{
			cachedResponse = null;
			statsCalled = false;
		}

		public async Task<AllStats> GetStatsAsync()
		{
			if (!statsCalled || cachedResponse == null)
			{
				statsCalled = true;
				StartTimer();
				cachedResponse = await CiekawostkomierzApiClient.GetAsync("/games/stats/bag-collection");
				if (cachedResponse == null || !cachedResponse.IsSuccess)
				{
					return null;
				}
			}
			return JsonConvert.DeserializeObject<AllStats>(cachedResponse.DataAsText);
		}
	}
}
