using System;
using System.Collections.Generic;

namespace Eyes.DailyChallenges
{
	public sealed class DailyChallengeAttempt
	{
		internal readonly HashSet<int> DiscoveredMapFragmentIds = new HashSet<int>();

		internal HashSet<int> RemainingEyeIds;

		internal bool EyeOrderFailed;

		public string QuestId { get; }

		public DateTime ExpiresAtUtc { get; }

		public string PlayerContext { get; }

		public long Sequence { get; }

		public int SceneHandle { get; }

		public int BagsCollected { get; internal set; }

		public int PumpkinsCaught { get; internal set; }

		public int GoodBoyFeedings { get; internal set; }

		public PositivePotionEffect PositivePotionEffects { get; internal set; }

		public bool PuzzleSolved { get; internal set; }

		public bool EyeUsed { get; internal set; }

		public bool WasChased { get; internal set; }

		public bool IsClosed { get; internal set; }

		public string EndReason { get; internal set; }

		internal DailyChallengeAttempt(string questId, DateTime expiresAtUtc, string playerContext, long sequence, int sceneHandle)
		{
			QuestId = questId;
			ExpiresAtUtc = expiresAtUtc;
			PlayerContext = playerContext;
			Sequence = sequence;
			SceneHandle = sceneHandle;
		}
	}
}
