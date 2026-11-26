using System.Collections.Generic;

namespace SkinSystem
{
	public interface ISkinCatalog
	{
		IReadOnlyList<SkinDefinition> All { get; }

		IReadOnlyList<string> DefaultOwnedSkinIds { get; }

		bool TryGet(string skinId, out SkinDefinition definition);
	}
}
