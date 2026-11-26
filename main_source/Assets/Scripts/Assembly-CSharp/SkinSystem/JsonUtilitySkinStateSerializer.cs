using System;
using System.Collections.Generic;

namespace SkinSystem
{
	public sealed class JsonUtilitySkinStateSerializer : ISkinStateSerializer
	{
		[Serializable]
		private sealed class OwnedSkinDto
		{
			public string skinId;

			public long acquiredAtUtcTicks;

			public bool seen;
		}

		[Serializable]
		private sealed class OwnedSkinsDto
		{
			public List<OwnedSkinDto> items;
		}

		[Serializable]
		private sealed class SlotEntryDto
		{
			public string slotId;

			public string skinId;
		}

		[Serializable]
		private sealed class LoadoutDto
		{
			public List<SlotEntryDto> slots;
		}

		public string SerializeOwned(IReadOnlyCollection<OwnedSkin> owned)
		{
			return null;
		}

		public IReadOnlyList<OwnedSkin> DeserializeOwned(string json)
		{
			return null;
		}

		public string SerializeLoadout(SkinLoadout loadout)
		{
			return null;
		}

		public SkinLoadout DeserializeLoadout(string targetId, string json)
		{
			return null;
		}

		private static T TryParse<T>(string json) where T : class
		{
			return null;
		}
	}
}
