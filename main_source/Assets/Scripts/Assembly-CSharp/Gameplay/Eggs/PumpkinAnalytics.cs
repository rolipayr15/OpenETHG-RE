using EyesAnalytics;
using UnityEngine;

namespace Gameplay.Eggs
{
	public static class PumpkinAnalytics
	{
		public static bool TryOutcomeOf(EggEffectId effect, out PumpkinOutcome outcome)
		{
			switch ((int)effect)
			{
			case 2:
				outcome = (PumpkinOutcome)1;
				return true;
			case 3:
				outcome = (PumpkinOutcome)2;
				return true;
			case 5:
				outcome = (PumpkinOutcome)3;
				return true;
			case 6:
				outcome = (PumpkinOutcome)4;
				return true;
			case 9:
				outcome = (PumpkinOutcome)5;
				return true;
			case 11:
				outcome = (PumpkinOutcome)6;
				return true;
			default:
				outcome = (PumpkinOutcome)0;
				return false;
			}
		}

		public static bool IsPrank(EggEffectId effect)
		{
			int value = (int)effect;
			return value >= 0 && value < 12 && ((0xA60 >> value) & 1) != 0;
		}

		public static int? FloorOf(Vector3 position)
		{
			if (FloorManager.TryGetInitializedSceneInstance(out FloorManager floorManager))
			{
				return floorManager.GetFloor(position);
			}
			return null;
		}

		public static void ReportLantern(EggEffectId applied, int amount, Vector3 position)
		{
			if (!TryOutcomeOf(applied, out PumpkinOutcome outcome))
			{
				return;
			}
			double? value = (int)applied == 2 || (int)applied == 3 ? amount : (double?)null;
			Game.PumpkinCollected((PumpkinKind)1, outcome, (int)Time.timeSinceLevelLoad, value, FloorOf(position));
		}
	}
}
