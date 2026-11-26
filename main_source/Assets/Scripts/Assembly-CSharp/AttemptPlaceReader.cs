using EyesAnalytics;
using UnityEngine;

public static class AttemptPlaceReader
{
	public static AttemptPlace Read(Transform monster = null)
	{
		FloorManager floorManager;
		Vector3 playerPosition;
		if (!FloorManager.TryGetInitializedSceneInstance(out floorManager) || floorManager == null || !floorManager.TryGetPlayerPosition(out playerPosition))
		{
			return default(AttemptPlace);
		}
		int? playerFloor = floorManager.GetFloor(playerPosition);
		int? monsterFloor = null;
		if (monster != null)
		{
			monsterFloor = floorManager.GetFloor(monster.position);
		}
		return new AttemptPlace(playerFloor, monsterFloor, AttemptPlace.CellOf(playerPosition));
	}
}
