using Newtonsoft.Json;
using System.Threading.Tasks;
using BestHTTP;
using ExternalApi.Models;

namespace Multiplayer.ExternalApi
{
	public class GameSingleApi : IGameSingleApi
	{
		private IExperienceManager experienceManager;

		public Task<HTTPResponse> StartGameAsync(StartGameData startGameData)
		{
			return CiekawostkomierzApiClient.PostJsonAsync("/game-single/play/start", JsonConvert.SerializeObject(startGameData));
		}

		public Task<HTTPResponse> SendGameSummaryAsync(EndGameData endGameData)
		{
			return CiekawostkomierzApiClient.PostJsonAsync("/game-single/play/end", JsonConvert.SerializeObject(endGameData));
		}
	}
}
