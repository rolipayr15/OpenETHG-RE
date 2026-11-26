using System;
using System.Collections.Generic;

namespace SkinSystem
{
	public interface ISkinWardrobe
	{
		IReadOnlyCollection<OwnedSkin> Owned { get; }

		event Action<string> SkinGranted;

		event Action<string, string, string> LoadoutChanged;

		bool IsOwned(string skinId);

		GrantResult TryGrant(string skinId);

		void MarkSeen(string skinId);

		SkinLoadout GetLoadout(string targetId);

		EquipResult Equip(string skinId);

		void Unequip(string targetId, string slotId);
	}
}
