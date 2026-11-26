public static class ReviewPromptPolicy
{
	public const int MinWins = 2;

	public const int MinSessions = 2;

	public const long CooldownSeconds = 7776000L;

	public static bool IsEligible(int wins, int sessions, bool legacyAllows, bool neverAsk, long nowUnix, long lastAttemptUnix, string currentVersion, string lastAttemptVersion)
	{
		if (sessions < 2 || wins < 2 || !legacyAllows || neverAsk)
		{
			return false;
		}
		if (string.IsNullOrEmpty(currentVersion) || currentVersion == lastAttemptVersion)
		{
			return false;
		}
		if (lastAttemptUnix == 0)
		{
			return true;
		}
		return lastAttemptUnix <= nowUnix && nowUnix - lastAttemptUnix > 7775999;
	}
}
