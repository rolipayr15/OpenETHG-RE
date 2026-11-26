using System.Collections.Generic;

namespace SkinSystem
{
	public sealed class ScriptableObjectSkinCatalog : ISkinCatalog, ISkinVisualResolver
	{
		private readonly Dictionary<string, SkinDefinition> definitions;

		private readonly Dictionary<string, ISkinVisual> visuals;

		private readonly List<SkinDefinition> all;

		private readonly List<string> defaultOwned;

		public IReadOnlyList<SkinDefinition> All => null;

		public IReadOnlyList<string> DefaultOwnedSkinIds => null;

		public ScriptableObjectSkinCatalog(SkinCatalogAsset asset)
		{
		}

		public bool TryGet(string skinId, out SkinDefinition definition)
		{
			definition = null;
			return false;
		}

		public bool TryResolve(string skinId, out ISkinVisual visual)
		{
			visual = null;
			return false;
		}
	}
}
