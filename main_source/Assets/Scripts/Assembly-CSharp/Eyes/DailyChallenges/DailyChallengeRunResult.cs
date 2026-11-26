namespace Eyes.DailyChallenges
{
	public sealed class DailyChallengeRunResult
	{
		public bool WasWon { get; }

		public Level Level { get; }

		public Difficulty Difficulty { get; }

		public Mode Mode { get; }

		public Enemy Enemy { get; }

		public GameMode GameMode { get; }

		public bool DoorLimitRespected { get; }

		public int? StepsTaken { get; }

		public float? DurationSeconds { get; }

		public bool PlayedWithRandom { get; }

		public DailyChallengeRunResult(bool wasWon, Level level, Difficulty difficulty, Mode mode, Enemy enemy, GameMode gameMode, bool doorLimitRespected = false, int? stepsTaken = null, float? durationSeconds = null, bool playedWithRandom = false)
		{
			WasWon = wasWon;
			Level = level;
			Difficulty = difficulty;
			Mode = mode;
			Enemy = enemy;
			GameMode = gameMode;
			DoorLimitRespected = doorLimitRespected;
			StepsTaken = stepsTaken;
			DurationSeconds = durationSeconds;
			PlayedWithRandom = playedWithRandom;
		}
	}
}
