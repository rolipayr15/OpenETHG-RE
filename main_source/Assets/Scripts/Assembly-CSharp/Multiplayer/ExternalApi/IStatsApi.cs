using System.Threading.Tasks;

namespace Multiplayer.ExternalApi
{
	public interface IStatsApi
	{
		Task<AllStats> GetStatsAsync();

		void InvalidateStatsCache();
	}
}
