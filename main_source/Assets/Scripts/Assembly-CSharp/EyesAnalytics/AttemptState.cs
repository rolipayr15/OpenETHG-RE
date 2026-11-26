using System;

namespace EyesAnalytics
{
	internal sealed class AttemptState
	{
		private int? sceneHandle;

		private bool ended;

		private bool startSent;

		private bool dead;

		private Game.ReviveStage? reviveStage;

		private double stageStartedAt;

		private int lastDoorSec;

		internal string Id { get; private set; }

		internal long EventSequence { get; private set; }

		internal int DeathIndex { get; private set; }

		internal int ReviveCount { get; private set; }

		internal string ReviveMethod { get; private set; }

		internal int AttemptNumber { get; private set; }

		internal Level Level { get; private set; }

		internal Enemy Opponent { get; private set; }

		internal Mode Variant { get; private set; }

		internal Difficulty Difficulty { get; private set; }

		internal bool Expert { get; private set; }

		internal string MpRole { get; private set; }

		internal string MatchId { get; private set; }

		internal AnalyticsContext.Snapshot Context { get; private set; }

		internal AttemptCounters Counters { get; private set; }

		internal ActivationCause? FirstActivationCause { get; private set; }

		internal bool CanReport => false;

		internal bool IsScene(int handle)
		{
			return false;
		}

		internal bool Begin(int handle)
		{
			return false;
		}

		internal void Describe(int attemptNumber, Level level, Enemy opponent, Mode variant, Difficulty difficulty, bool expert, string mpRole, string matchId, AnalyticsContext.Snapshot context)
		{
		}

		internal long NextEventSequence()
		{
			return 0L;
		}

		internal void MarkStartSent(bool sent)
		{
		}

		internal void Invalidate()
		{
		}

		internal bool Death(double now)
		{
			return false;
		}

		internal bool Revive(Game.ReviveStage stage, string method, double now, out double stageElapsed)
		{
			stageElapsed = default;
			return false;
		}

		internal bool Finish()
		{
			return false;
		}

		internal void CountMonsterActivation(int elapsedSeconds)
		{
		}

		internal void NoteActivationCause(ActivationCause cause)
		{
		}

		internal void NoteDoorPassage(int elapsedSeconds)
		{
		}

		internal int? DoorPassageSec(int elapsedSeconds)
		{
			return null;
		}

		internal void CountBag(int elapsedSeconds)
		{
		}

		internal void CountChaseStarted(int elapsedSeconds)
		{
		}

		internal AttemptRecord ToRecord(int durationSec, int bags, int bagsTarget, int eyes, long nowUnixSec)
		{
			return null;
		}

		private static bool IsDefined<T>(string name) where T : struct, Enum
		{
			return false;
		}

		private static void Clamp(AttemptRecord r)
		{
		}

		internal static AttemptRecord Parse(string json, long nowUnixSec)
		{
			return null;
		}
	}
}
