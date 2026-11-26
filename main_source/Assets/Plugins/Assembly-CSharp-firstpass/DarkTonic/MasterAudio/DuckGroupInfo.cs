using System;

namespace DarkTonic.MasterAudio
{
	[Serializable]
	public class DuckGroupInfo
	{
		public MasterAudio.DuckMode duckMode;

		public bool enableDistanceDuckRatio;

		public string soundType;

		public float riseVolStart;

		public float unduckTime;

		public float duckedVolumeCut;

		public bool isTemporary;

		public bool isExpanded;
	}
}
