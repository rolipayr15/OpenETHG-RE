using System.Collections.Generic;
using UnityEngine;

namespace Characters.Strategies.Goose
{
	public static class GooseChargeRules
	{
		public readonly struct Profile
		{
			public readonly float WindupSeconds;

			public readonly float ChargeSpeed;

			public readonly float DazeSeconds;

			public Profile(float windupSeconds, float chargeSpeed, float dazeSeconds)
			{
				WindupSeconds = windupSeconds;
				ChargeSpeed = chargeSpeed;
				DazeSeconds = dazeSeconds;
			}
		}

		public enum ChargeOutcome
		{
			Continue = 0,
			Hit = 1,
			Finished = 2
		}

		public enum WindupOutcome
		{
			Continue = 0,
			Attack = 1,
			Charge = 2
		}

		public enum RecoveryOutcome
		{
			Continue = 0,
			Windup = 1,
			CheckLastLocation = 2
		}

		public const float ChargeTimeoutSeconds = 3f;

		public const float MinimumChargeDistance = 1.5f;

		public const float DetourRatio = 1.5f;

		public const float StalkDistance = 3f;

		public const float StalkSeconds = 4f;

		public static Difficulty EffectiveDifficulty(Difficulty stored, Mode mode)
		{
			return (int)mode == 1 || (int)mode == 2 ? (Difficulty)2 : stored;
		}

		public static Profile ProfileFor(Difficulty difficulty)
		{
			switch ((int)difficulty)
			{
			case 0:
				return new Profile(1.8f, 5f, 2.5f);
			case 1:
				return new Profile(1.5f, 6f, 2f);
			case 2:
				return new Profile(1.2f, 8f, 1.5f);
			case 3:
				return new Profile(1f, 10f, 1.2f);
			case 4:
				return new Profile(0.8f, 12f, 1f);
			default:
				return new Profile(1.2f, 8f, 1.5f);
			}
		}

		public static WindupOutcome EvaluateWindup(float elapsedSeconds, float windupSeconds, float distanceToTarget, float minimumChargeDistance)
		{
			if (distanceToTarget < minimumChargeDistance)
			{
				return WindupOutcome.Attack;
			}
			return elapsedSeconds >= windupSeconds ? WindupOutcome.Charge : WindupOutcome.Continue;
		}

		public static Vector3 ChargeDestination(bool targetVisible, Vector3 livePosition, Vector3 lastSeenPosition)
		{
			return targetVisible ? livePosition : lastSeenPosition;
		}

		public static float PathLength(IReadOnlyList<Vector3> corners)
		{
			if (corners == null)
			{
				return 0f;
			}
			float length = 0f;
			for (int i = 1; i < corners.Count; i++)
			{
				length += Vector3.Distance(corners[i - 1], corners[i]);
			}
			return length;
		}

		public static bool IsDetour(float pathLength, float straightDistance, float ratio)
		{
			return straightDistance > 0f && pathLength > straightDistance * ratio;
		}

		public static bool IsStalkDone(float movedDistance, float elapsedSeconds, float stalkDistance, float stalkSeconds)
		{
			return elapsedSeconds >= stalkSeconds || movedDistance >= stalkDistance;
		}

		public static ChargeOutcome EvaluateCharge(bool targetHit, bool pathPending, bool pathComplete, float remainingDistance, float stopDistance, float elapsedSeconds, float timeoutSeconds, bool doorDetected)
		{
			if (targetHit)
			{
				return ChargeOutcome.Hit;
			}
			if (doorDetected || elapsedSeconds >= timeoutSeconds)
			{
				return ChargeOutcome.Finished;
			}
			if (pathPending)
			{
				return ChargeOutcome.Continue;
			}
			return pathComplete && remainingDistance >= stopDistance ? ChargeOutcome.Continue : ChargeOutcome.Finished;
		}

		public static RecoveryOutcome EvaluateRecovery(float elapsedSeconds, float dazeSeconds, bool targetVisible)
		{
			if (elapsedSeconds < dazeSeconds)
			{
				return RecoveryOutcome.Continue;
			}
			return targetVisible ? RecoveryOutcome.Windup : RecoveryOutcome.CheckLastLocation;
		}
	}
}
