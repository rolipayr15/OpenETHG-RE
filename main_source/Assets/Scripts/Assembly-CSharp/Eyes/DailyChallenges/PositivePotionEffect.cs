using System;

namespace Eyes.DailyChallenges
{
	[Flags]
	public enum PositivePotionEffect
	{
		None = 0,
		ExtraEyes = 1,
		Invisibility = 2,
		Speed = 4,
		BagVision = 8,
		All = ExtraEyes | Invisibility | Speed | BagVision
	}
}
