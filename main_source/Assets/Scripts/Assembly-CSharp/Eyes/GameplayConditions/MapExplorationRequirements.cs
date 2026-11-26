using System;
using System.Collections.Generic;

namespace Eyes.GameplayConditions
{
	public sealed class MapExplorationRequirements
	{
		private readonly HashSet<int> validIds;

		public static MapExplorationRequirements Mansion { get; }

		public static MapExplorationRequirements School { get; }

		public static MapExplorationRequirements Hospital { get; }

		public Level Level { get; }

		public int RequiredCount { get; }

		public IReadOnlyList<int> ValidFragmentIds { get; }

		static MapExplorationRequirements()
		{
			Mansion = new MapExplorationRequirements((Level)0, 24, new int[25] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 });
			School = new MapExplorationRequirements((Level)3, 25, new int[25] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 });
			Hospital = new MapExplorationRequirements((Level)1, 17, new int[17] { 1, 2, 3, 4, 5, 6, 7, 8, 10, 11, 12, 13, 14, 16, 17, 18, 19 });
		}

		private MapExplorationRequirements(Level level, int requiredCount, int[] fragmentIds)
		{
			Level = level;
			RequiredCount = requiredCount;
			ValidFragmentIds = Array.AsReadOnly(fragmentIds);
			validIds = new HashSet<int>(fragmentIds);
		}

		public bool Contains(int fragmentId)
		{
			return validIds.Contains(fragmentId);
		}

		public static bool TryGet(Level level, out MapExplorationRequirements requirements)
		{
			switch ((int)level)
			{
			case 0:
				requirements = Mansion;
				return true;
			case 3:
				requirements = School;
				return true;
			case 1:
				requirements = Hospital;
				return true;
			default:
				requirements = null;
				return false;
			}
		}
	}
}
