// Switches for the recovery test release, the baseline for further development.
// Set FreeEverything to false once map purchases and paid multiplayer are wired to the new server.
public static class RecoveryTestBuild
{
	public const bool FreeEverything = true;

	// Content the player owns, or everything while the test release is free.
	public static bool Unlocked(bool owned)
	{
		return FreeEverything || owned;
	}

	public static bool LevelUnlocked(Level level)
	{
		if (FreeEverything)
		{
			return true;
		}
		switch (level)
		{
		case Level.School:
			return UserGameSettings.unlockedSchool;
		case Level.Hospital:
			return UserGameSettings.unlockedHospital;
		default:
			return true;
		}
	}
}
