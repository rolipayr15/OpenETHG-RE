public static class FewestStepsLeaderboard
{
	public const string MansionId = "leaderboardMansionFewestSteps";

	public const string HospitalId = "leaderboardHospitalFewestSteps";

	public const string SchoolId = "leaderboardSchoolFewestSteps";

	public static bool TryGetLeaderboardId(Level level, out string leaderboardId)
	{
		leaderboardId = null;
		return false;
	}

	public static bool TryGetScore(int stepCount, out long score)
	{
		score = default;
		return false;
	}

	public static void ReportWonGame(Level level, int stepCount)
	{
	}

	private static bool IsConfiguredForActivePlatform(string leaderboardId)
	{
		return false;
	}
}
