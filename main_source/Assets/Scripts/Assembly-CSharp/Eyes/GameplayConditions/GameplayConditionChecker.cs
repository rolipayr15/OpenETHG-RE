namespace Eyes.GameplayConditions
{
	public static class GameplayConditionChecker
	{
		public static bool HasFullyExploredMap(int? discoveredCount, int requiredCount)
		{
			return requiredCount > 0 && discoveredCount.HasValue && requiredCount <= discoveredCount.Value;
		}

		public static MultiplayerWinKind GetMultiplayerWin(GameplayRunFacts facts)
		{
			return MultiplayerWinRules.Evaluate(facts.WasWon, facts.GameMode, facts.PlayedWithRandom);
		}

		public static bool WonWithPumpkinCatches(GameplayRunFacts facts, int requiredCatches)
		{
			return facts.WasWon && facts.PumpkinsCaught.HasValue && requiredCatches <= facts.PumpkinsCaught.Value;
		}

		public static bool WonWithinTime(GameplayRunFacts facts, double limitSeconds)
		{
			return facts.WasWon && facts.DurationSeconds.HasValue && facts.DurationSeconds.Value < limitSeconds;
		}
	}
}
