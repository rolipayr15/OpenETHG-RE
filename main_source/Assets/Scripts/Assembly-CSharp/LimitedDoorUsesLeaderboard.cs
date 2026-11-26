public static class LimitedDoorUsesLeaderboard
{
	public const string MansionId = "leaderboardMansionLimitedDoorUses";

	public const string HospitalId = "leaderboardHospitalLimitedDoorUses";

	public const string SchoolId = "leaderboardSchoolLimitedDoorUses";

	public static bool TryGetLeaderboardId(Level level, out string leaderboardId)
	{
		leaderboardId = null;
		return false;
	}

	public static bool IsQualifyingGame(bool wasWon, Level level, Difficulty difficulty, Mode mode, Enemy enemy, GameMode gameMode, bool doorLimitRespected)
	{
		return false;
	}

	public static bool TryGetScore(float durationSeconds, bool reportMilliseconds, out long score)
	{
		score = default;
		return false;
	}

	public static void ReportWonGame(bool wasWon, Level level, Difficulty difficulty, Mode mode, Enemy enemy, GameMode gameMode, bool doorLimitRespected, float durationSeconds)
	{
	}

	private static bool IsConfiguredForActivePlatform(string leaderboardId)
	{
		return false;
	}
}
