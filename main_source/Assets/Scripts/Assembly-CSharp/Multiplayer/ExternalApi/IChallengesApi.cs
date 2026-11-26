using System.Threading.Tasks;
using ExternalApi.Models;

namespace Multiplayer.ExternalApi
{
	public interface IChallengesApi
	{
		Task<CurrentDailyChallengeResponse?> GetCurrentDailyChallengeAsync();
	}
}
