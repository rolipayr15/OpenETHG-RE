using System;

namespace EyesAnalytics
{
	public static class Game
	{
		internal enum ReviveStage
		{
			WaitingAd = 0,
			Offered = 1,
			Selected = 2,
			Revived = 3,
			Declined = 4,
			Unavailable = 5,
			Failed = 6
		}

		internal readonly struct ReviveToken
		{
			internal readonly string AttemptId;

			internal readonly int DeathIndex;

			internal ReviveToken(string id, int deathIndex)
			{
				AttemptId = null;
				DeathIndex = 0;
			}
		}

		private const string AbandonedReason = "app_closed";

		internal static ReviveToken CaptureReviveToken()
		{
			return default;
		}

		internal static void InvalidateAnalyticsAttempt(bool clearSavedAttempt = true)
		{
		}

		internal static void Start(Level level, Enemy enemy, Difficulty difficulty, Mode mode, bool expertMode, int? sceneHandle = null)
		{
		}

		internal static void End(GameFinishedResult result, int bags, int eyes, int elapsedSeconds, GameFinishedReason reason, int? myBags = null)
		{
		}

		internal static void End(GameFinishedResult result, int bags, int eyes, int elapsedSeconds, string endReason = null, int? myBags = null)
		{
		}

		internal static void PlayerDeath(int bags, int bagsTarget, int elapsedSeconds, int? sceneHandle = null, int? myBags = null, AttemptPlace place = default(AttemptPlace))
		{
		}

		internal static void ReviveFlow(ReviveStage stage, string method = null, string reason = null, ReviveToken? token = null, int? sceneHandle = null)
		{
		}

		private static void RefreshSavedAttempt(string attemptId, int reviveCount)
		{
		}

		public static void PumpkinCollected(PumpkinKind kind, PumpkinOutcome outcome, int elapsedSeconds, double? value = null, int? playerFloor = null)
		{
		}

		public static void CountInteraction()
		{
		}

		public static void CountFrame(float deltaSeconds)
		{
		}

		public static void CountJournalUnlock()
		{
		}

		public static void CountBag(int elapsedSeconds)
		{
		}

		public static void CountMonsterActivation(int elapsedSeconds)
		{
		}

		public static void CountChaseStarted(int elapsedSeconds)
		{
		}

		public static void CountChaseEscapedLost()
		{
		}

		public static void CountChaseEscapedDoor()
		{
		}

		public static void NoteActivationCause(ActivationCause cause)
		{
		}

		public static void NoteDoorPassage(int elapsedSeconds)
		{
		}

		internal static void SaveAttemptOnBackground(int elapsedSeconds)
		{
		}

		internal static void ReportAbandonedAttempt()
		{
		}

		private static void ClearSavedAttempt()
		{
		}

		private static bool SendAttemptEvent(EventDef def, AttemptState attempt, Action<EventBuilder> more)
		{
			return false;
		}

		private static void AddAttempt(EventBuilder b, string id, long sequence, int attemptNumber, Level level, Enemy opponent, Mode variant, Difficulty difficulty, bool expert, string mpRole, string matchId)
		{
		}

		private static string MatchIdOrNull()
		{
			return null;
		}

		private static void SendSummary(AnalyticsContext.Snapshot context, string id, long sequence, int attemptNumber, Level level, Mode variant, AttemptCounters c)
		{
		}
	}
}
