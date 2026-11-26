using System.Collections.Generic;

namespace SkinSystem
{
	public sealed class SkinLoadout
	{
		private readonly Dictionary<string, string> equipped;

		public string TargetId { get; }

		public IReadOnlyDictionary<string, string> Equipped => null;

		public SkinLoadout(string targetId, IDictionary<string, string> entries = null)
		{
		}

		public string GetEquipped(string slotId)
		{
			return null;
		}

		public SkinLoadout With(string slotId, string skinId)
		{
			return null;
		}

		public SkinLoadout Without(string slotId)
		{
			return null;
		}
	}
}
