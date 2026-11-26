using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SkinSystem
{
	public sealed class SkinWardrobe : ISkinWardrobe
	{
		private const string OwnedKey = "skins.owned";

		private readonly ISkinCatalog catalog;

		private readonly IKeyValueStore store;

		private readonly ISkinStateSerializer serializer;

		private readonly Func<DateTime> utcNow;

		[CompilerGenerated]
		private Action<string> m_SkinGranted;

		[CompilerGenerated]
		private Action<string, string, string> m_LoadoutChanged;

		public IReadOnlyCollection<OwnedSkin> Owned => null;

		public event Action<string> SkinGranted
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<string, string, string> LoadoutChanged
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		private static string LoadoutKey(string targetId)
		{
			return null;
		}

		public SkinWardrobe(ISkinCatalog catalog, IKeyValueStore store, ISkinStateSerializer serializer, Func<DateTime> utcNow)
		{
		}

		public bool IsOwned(string skinId)
		{
			return false;
		}

		public GrantResult TryGrant(string skinId)
		{
			return GrantResult.Granted;
		}

		public void MarkSeen(string skinId)
		{
		}

		private Dictionary<string, OwnedSkin> ReadOwned()
		{
			return null;
		}

		private Dictionary<string, OwnedSkin> ReadStoredOwned()
		{
			return null;
		}

		private void WriteOwned(Dictionary<string, OwnedSkin> owned)
		{
		}

		private bool IsDefaultOwned(string skinId)
		{
			return false;
		}

		public SkinLoadout GetLoadout(string targetId)
		{
			return null;
		}

		public EquipResult Equip(string skinId)
		{
			return EquipResult.Equipped;
		}

		public void Unequip(string targetId, string slotId)
		{
		}

		private void WriteLoadout(SkinLoadout loadout)
		{
		}
	}
}
