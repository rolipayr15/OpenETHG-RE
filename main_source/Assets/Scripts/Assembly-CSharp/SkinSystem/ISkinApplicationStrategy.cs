using System;

namespace SkinSystem
{
	public interface ISkinApplicationStrategy
	{
		Type VisualType { get; }

		void Apply(SlotAnchor anchor, ISkinVisual visual);

		void Clear(SlotAnchor anchor);
	}
}
