using System.Collections.Generic;
using UnityEngine;

namespace SkinSystem
{
	[CreateAssetMenu(fileName = "SkinCatalog", menuName = "Skins/Catalog")]
	public sealed class SkinCatalogAsset : ScriptableObject
	{
		[Tooltip("Explicit shipping manifest. Not folder-scanned: AssetDatabase does not exist in builds.")]
		[SerializeField]
		private List<SkinAsset> skins;

		[Tooltip("Starter skins, owned implicitly with no save data written.")]
		[SerializeField]
		private List<string> defaultOwnedSkinIds;

		public IReadOnlyList<SkinAsset> Skins => null;

		public IReadOnlyList<string> DefaultOwnedSkinIds => null;
	}
}
