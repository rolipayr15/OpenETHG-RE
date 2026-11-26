using UnityEngine;

namespace Characters.Strategies
{
	public static class DogPeriodicHuntScheduler
	{
		public static bool ShouldQueuePeriodicHunt(bool isMonsterActive, bool isPatrolling, float patrolTimerStartedAt, float periodicHuntIntervalSeconds, float nowSeconds)
		{
			return isMonsterActive && isPatrolling && periodicHuntIntervalSeconds > 0f && patrolTimerStartedAt + periodicHuntIntervalSeconds <= nowSeconds;
		}

		public static Vector3 CapturePeriodicHuntDestination(Vector3 targetPositionAtHunt)
		{
			return targetPositionAtHunt;
		}
	}
}
