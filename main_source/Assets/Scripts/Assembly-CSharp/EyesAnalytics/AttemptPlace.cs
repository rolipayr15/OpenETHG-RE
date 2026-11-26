using UnityEngine;

namespace EyesAnalytics
{
	public readonly struct AttemptPlace
	{
		public const float CellSize = 2f;

		public readonly int? PlayerFloor;

		public readonly int? MonsterFloor;

		public readonly string Cell;

		public AttemptPlace(int? playerFloor, int? monsterFloor, string cell)
		{
			PlayerFloor = null;
			MonsterFloor = null;
			Cell = null;
		}

		public static string CellOf(Vector3 position)
		{
			return null;
		}
	}
}
