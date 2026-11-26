using System.Threading.Tasks;
using BestHTTP;
using ExternalApi.Models;

namespace Multiplayer.ExternalApi
{
	public interface IUserApi
	{
		Task<HTTPResponse> SetUsernameAsync(UsernameData username);

		Task<UsernameResponse?> GetUsernameAsync();

		Task<SyncUserInfoResponse> SyncUserInfoAsync();

		Task<HTTPResponse> SetDistanceAsync(DistanceData distance);
	}
}
