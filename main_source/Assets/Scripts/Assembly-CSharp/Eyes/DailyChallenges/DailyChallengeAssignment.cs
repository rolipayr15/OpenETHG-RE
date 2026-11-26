using System;

namespace Eyes.DailyChallenges
{
	public sealed class DailyChallengeAssignment
	{
		public string Id { get; }

		public DateTime ExpiresAtUtc { get; }

		public DailyChallengeAssignment(string id, DateTime expiresAtUtc)
		{
			Id = id;
			ExpiresAtUtc = expiresAtUtc;
		}
	}
}
