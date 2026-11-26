using System;

namespace SkinSystem
{
	public sealed class MaterialSwapStrategy : ISkinApplicationStrategy
	{
		public Type VisualType => null;

		public void Apply(SlotAnchor anchor, ISkinVisual visual)
		{
		}

		public void Clear(SlotAnchor anchor)
		{
		}
	}
}
