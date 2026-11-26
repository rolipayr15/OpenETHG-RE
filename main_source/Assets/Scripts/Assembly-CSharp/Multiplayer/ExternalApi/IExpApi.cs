using System.Threading.Tasks;
using BestHTTP;
using ExternalApi.Models;

namespace Multiplayer.ExternalApi
{
	public interface IExpApi
	{
		Task<HTTPResponse> SendGrantExpAsync(GrantExpData grantExpData);

		Task<LocalExpInfoResponse?> GetOpponentLocalExpInfo(string gameId);
	}
}
