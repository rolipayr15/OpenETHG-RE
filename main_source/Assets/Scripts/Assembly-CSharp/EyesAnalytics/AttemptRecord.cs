using System;

namespace EyesAnalytics
{
	[Serializable]
	internal sealed class AttemptRecord
	{
		public const int CurrentVersion = 1;

		public const long MaxAgeSeconds = 2592000L;

		public int version;

		public string id;

		public long seq;

		public int attemptNumber;

		public string level;

		public string opponent;

		public string variant;

		public string difficulty;

		public bool expert;

		public string mpRole;

		public string matchId;

		public string playMode;

		public int mpToken;

		public int sessionIndex;

		public int durationSec;

		public int bagsCount;

		public int bagsTarget;

		public int eyesCount;

		public int deaths;

		public int revives;

		public long savedAtUnixSec;

		public AttemptCounters counters;
	}
}
