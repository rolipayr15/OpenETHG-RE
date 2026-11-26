using System;

namespace Eyes.DailyChallenges
{
	public sealed class DailyChallengeCompletion
	{
		public DailyChallengeView Challenge { get; }

		public string PlayerContext { get; }

		public long AttemptSequence { get; }

		public DateTime CompletedAtUtc { get; }

		public DateTime NextChallengeAtUtc { get; }

		internal DailyChallengeCompletion(DailyChallengeView challenge, string playerContext, long attemptSequence, DateTime completedAtUtc)
		{
			Challenge = challenge;
			PlayerContext = playerContext;
			AttemptSequence = attemptSequence;
			CompletedAtUtc = completedAtUtc;
			NextChallengeAtUtc = challenge.ExpiresAtUtc;
		}
	}
}
