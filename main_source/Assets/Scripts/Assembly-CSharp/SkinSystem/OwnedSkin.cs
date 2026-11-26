using System;

namespace SkinSystem
{
	public readonly struct OwnedSkin
	{
		public string SkinId { get; }

		public DateTime AcquiredAtUtc { get; }

		public bool Seen { get; }

		public OwnedSkin(string skinId, DateTime acquiredAtUtc, bool seen)
		{
			SkinId = null;
			AcquiredAtUtc = default;
			Seen = false;
		}

		public OwnedSkin AsSeen()
		{
			return default;
		}
	}
}
