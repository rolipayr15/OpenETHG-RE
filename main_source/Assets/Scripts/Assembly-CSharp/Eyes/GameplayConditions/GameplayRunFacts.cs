namespace Eyes.GameplayConditions
{
	public readonly struct GameplayRunFacts
	{
		public bool WasWon { get; }

		public GameMode GameMode { get; }

		public bool PlayedWithRandom { get; }

		public double? DurationSeconds { get; }

		public long? PumpkinsCaught { get; }

		public GameplayRunFacts(bool wasWon, GameMode gameMode, bool playedWithRandom = false, double? durationSeconds = null, long? pumpkinsCaught = null)
		{
			WasWon = wasWon;
			GameMode = gameMode;
			PlayedWithRandom = playedWithRandom;
			DurationSeconds = durationSeconds;
			PumpkinsCaught = pumpkinsCaught;
		}
	}
}
