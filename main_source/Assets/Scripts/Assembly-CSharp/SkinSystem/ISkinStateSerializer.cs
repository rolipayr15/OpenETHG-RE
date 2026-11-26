using System.Collections.Generic;

namespace SkinSystem
{
	public interface ISkinStateSerializer
	{
		string SerializeOwned(IReadOnlyCollection<OwnedSkin> owned);

		IReadOnlyList<OwnedSkin> DeserializeOwned(string json);

		string SerializeLoadout(SkinLoadout loadout);

		SkinLoadout DeserializeLoadout(string targetId, string json);
	}
}
