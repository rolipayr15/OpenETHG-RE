using System;

namespace Eyes.DailyChallenges
{
	public sealed class DeterministicDailyChallengeAssignmentProvider : IDailyChallengeAssignmentProvider
	{
		public DailyChallengeAssignment GetAssignment(DateTime utcNow)
		{
			if (utcNow.Kind != DateTimeKind.Utc || utcNow.Date == default(DateTime).Date)
			{
				return null;
			}
			return new DailyChallengeAssignment(DailyChallengeCatalog.Current.Id, utcNow.Date.AddDays(1.0));
		}
	}
}
