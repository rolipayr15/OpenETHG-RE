using EyesAnalytics;
using System.Collections.Generic;
using UnityEngine;

public static class DoorUseTracker
{
	public const int MaxPassagesPerDoor = 2;

	private static readonly Dictionary<int, int> passageCounts = new Dictionary<int, int>();

	public static bool IsTrackingInitialized { get; private set; }

	public static bool IsLimitRespected { get; private set; }

	public static bool IsRunEligible => IsTrackingInitialized && IsLimitRespected;

	public static void Reset()
	{
		passageCounts.Clear();
		IsTrackingInitialized = true;
		IsLimitRespected = true;
	}

	public static void RecordPassage(int doorInstanceId, Object doorContext = null)
	{
		Game.NoteDoorPassage((int)Time.timeSinceLevelLoad);
		if (!IsTrackingInitialized)
		{
			return;
		}
		passageCounts.TryGetValue(doorInstanceId, out int count);
		if (count != int.MaxValue)
		{
			count++;
		}
		passageCounts[doorInstanceId] = count;
		if (count >= 3)
		{
			IsLimitRespected = false;
		}
	}

	public static int GetPassageCount(int doorInstanceId)
	{
		return passageCounts.TryGetValue(doorInstanceId, out int count) ? count : 0;
	}

	public static float GetLocalPassageOffset(BoxCollider passageTrigger, Vector3 worldPosition, bool useLocalX)
	{
		Vector3 local = passageTrigger.transform.InverseTransformPoint(worldPosition) - passageTrigger.center;
		return useLocalX ? local.x : local.z;
	}

	public static bool DidCrossDoor(float entryOffset, float exitOffset)
	{
		return entryOffset * exitOffset < 0f;
	}
}
