using System;

namespace Eyes.DailyChallenges
{
	public interface IDailyChallengeAssignmentProvider
	{
		DailyChallengeAssignment GetAssignment(DateTime utcNow);
	}
}
