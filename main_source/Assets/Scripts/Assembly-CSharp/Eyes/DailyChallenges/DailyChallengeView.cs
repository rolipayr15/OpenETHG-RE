using System;

namespace Eyes.DailyChallenges
{
	public sealed class DailyChallengeView
	{
		internal static readonly DailyChallengeView Unavailable = new DailyChallengeView(null, 0, false, default(DateTime));

		public bool IsAvailable => Definition != null;

		public string QuestId => Definition?.Id;

		public DailyChallengeDefinition Definition { get; }

		public int Progress { get; }

		public int Goal => Definition != null ? Definition.Goal : 0;

		public bool IsCompleted { get; }

		public DateTime ExpiresAtUtc { get; }

		internal DailyChallengeView(DailyChallengeDefinition definition, int progress, bool completed, DateTime expiresAtUtc)
		{
			Definition = definition;
			Progress = progress;
			IsCompleted = completed;
			ExpiresAtUtc = expiresAtUtc;
		}
	}
}
