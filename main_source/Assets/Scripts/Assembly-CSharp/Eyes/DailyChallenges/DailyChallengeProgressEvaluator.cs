using Eyes.GameplayConditions;

namespace Eyes.DailyChallenges
{
	internal static class DailyChallengeProgressEvaluator
	{
		private const int GooseEnemy = 4;

		public static int EarnedProgress(DailyChallengeDefinition definition, DailyChallengeAttempt attempt, DailyChallengeRunResult result, PositivePotionEffect persistedEffects = (PositivePotionEffect)0)
		{
			switch (definition.Kind)
			{
			case DailyChallengeKind.CollectBags:
				return attempt.BagsCollected;
			case DailyChallengeKind.MansionDoorLimit:
				return IsNormalWin(result) && (int)result.Level == 0 && result.DoorLimitRespected ? 1 : 0;
			case DailyChallengeKind.WinWithStepLimit:
				return IsNormalWin(result) && result.StepsTaken.HasValue && result.StepsTaken.Value >= 0 && result.StepsTaken.Value <= definition.Parameter ? 1 : 0;
			case DailyChallengeKind.HospitalPositivePotions:
				return CountPotionEffects((PositivePotionEffect)((int)attempt.PositivePotionEffects & ~(int)persistedEffects & 0xF));
			case DailyChallengeKind.CollectEyesBeforeBags:
				return IsNormalOrHigherWin(result) && !attempt.EyeOrderFailed && attempt.RemainingEyeIds != null && attempt.RemainingEyeIds.Count == 0 ? 1 : 0;
			case DailyChallengeKind.WinWithTimeLimit:
				return IsNormalOrHigherWin(result) && result.DurationSeconds.HasValue && result.DurationSeconds.Value >= 0f && GameplayConditionChecker.WonWithinTime(new GameplayRunFacts(result.WasWon, result.GameMode, false, (double?)result.DurationSeconds, null), definition.Parameter) ? 1 : 0;
			case DailyChallengeKind.FamilyMatters:
				return attempt.PuzzleSolved ? 1 : 0;
			case DailyChallengeKind.WinVersusRandom:
				return GetMultiplayerWin(result) == MultiplayerWinKind.VersusRandom ? 1 : 0;
			case DailyChallengeKind.SilentEscape:
				return result != null && result.WasWon && (int)result.GameMode == 0 && (int)result.Difficulty - 2 < 3U && !attempt.EyeUsed && !attempt.WasChased ? 1 : 0;
			case DailyChallengeKind.PumpkinParty:
				return result != null && (int)result.GameMode == 0 && (int)result.Mode == 2 && (int)result.Enemy != GooseEnemy && GameplayConditionChecker.WonWithPumpkinCatches(new GameplayRunFacts(result.WasWon, result.GameMode, false, null, attempt.PumpkinsCaught), definition.Parameter) ? 1 : 0;
			case DailyChallengeKind.FeedGoodBoy:
				return result != null && (int)result.GameMode == 0 && (int)result.Enemy != GooseEnemy && (int)result.Difficulty - 2 < 3U && attempt.GoodBoyFeedings >= definition.Parameter ? 1 : 0;
			case DailyChallengeKind.WinCoopRandom:
				return GetMultiplayerWin(result) == MultiplayerWinKind.CoopRandom ? 1 : 0;
			case DailyChallengeKind.WinCoopFriend:
				return GetMultiplayerWin(result) == MultiplayerWinKind.CoopFriend ? 1 : 0;
			case DailyChallengeKind.WinVersusFriend:
				return GetMultiplayerWin(result) == MultiplayerWinKind.VersusFriend && IsGameplayVictoryReason(attempt.EndReason) ? 1 : 0;
			case DailyChallengeKind.ExploreMansion:
			case DailyChallengeKind.ExploreSchool:
			case DailyChallengeKind.ExploreHospital:
				return GameplayConditionChecker.HasFullyExploredMap(attempt.DiscoveredMapFragmentIds.Count, definition.Parameter) ? 1 : 0;
			default:
				return 0;
			}
		}

		private static MultiplayerWinKind GetMultiplayerWin(DailyChallengeRunResult result)
		{
			return result == null ? MultiplayerWinKind.None : MultiplayerWinRules.Evaluate(result.WasWon, result.GameMode, result.PlayedWithRandom);
		}

		private static bool IsGameplayVictoryReason(string endReason)
		{
			return endReason == "HOST_WON" || endReason == "GUEST_WON" || endReason == "HOST_KILLED" || endReason == "GUEST_KILLED";
		}

		public static int CountPotionEffects(PositivePotionEffect effects)
		{
			int count = 0;
			for (uint bits = (uint)effects & 0xFU; bits != 0; bits &= bits - 1)
			{
				count++;
			}
			return count;
		}

		private static bool AllowedRun(DailyChallengeRunResult result)
		{
			return (int)result.GameMode == 0 && (int)result.Enemy != GooseEnemy && (int)result.Mode < 4 && ((0xD >> (int)result.Mode) & 1) != 0;
		}

		private static bool IsNormalOrHigherWin(DailyChallengeRunResult result)
		{
			return result != null && result.WasWon && (int)result.Difficulty - 2 < 3U && AllowedRun(result);
		}

		private static bool IsNormalWin(DailyChallengeRunResult result)
		{
			return result != null && result.WasWon && (int)result.Difficulty == 2 && AllowedRun(result);
		}
	}
}
