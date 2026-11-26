using System.Threading.Tasks;
using BestHTTP;
using ExternalApi.Models;

namespace Multiplayer.ExternalApi
{
	public interface IGameSingleApi
	{
		Task<HTTPResponse> StartGameAsync(StartGameData startGameData);

		Task<HTTPResponse> SendGameSummaryAsync(EndGameData endGameData);
	}
}
