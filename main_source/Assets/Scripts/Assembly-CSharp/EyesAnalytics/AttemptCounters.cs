using System;

namespace EyesAnalytics
{
	[Serializable]
	internal sealed class AttemptCounters
	{
		public int interactions;

		public int eyeUses;

		public int eyeOutOfCharges;

		public int sprintUses;

		public int sprintOutOfCharges;

		public int mapUses;

		public int meatUses;

		public int meatOutOfCharges;

		public int journalOpens;

		public int journalUnlocks;

		public int monsterActivations;

		public int firstMonsterSec;

		public int firstBagSec;

		public int chasesStarted;

		public int chasesEscapedLost;

		public int chasesEscapedDoor;

		public int firstChaseSec;

		public int frames;

		public int slowFrames;

		public int SlowFramePct => 0;
	}
}
