using System;

namespace DarkTonic.MasterAudio
{
	[Serializable]
	public class BusSingleDuckInfo
	{
		public GroupBus Bus;

		public float StartingVolume;

		public float InitialDuckVolume;

		public float DuckRange;
	}
}
