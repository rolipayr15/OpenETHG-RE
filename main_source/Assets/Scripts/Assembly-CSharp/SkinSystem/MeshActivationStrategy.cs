using System;

namespace SkinSystem
{
	public sealed class MeshActivationStrategy : ISkinApplicationStrategy
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
