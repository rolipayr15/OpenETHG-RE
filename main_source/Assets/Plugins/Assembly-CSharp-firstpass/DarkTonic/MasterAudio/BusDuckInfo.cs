using System;
using System.Collections.Generic;

namespace DarkTonic.MasterAudio
{
	[Serializable]
	public class BusDuckInfo
	{
		public List<BusSingleDuckInfo> BusesToDuck;

		public PlaylistController.AudioDuckingMode DuckingMode;

		public string TriggeredBySoundGroup;

		public float TimeToStartUnducking;

		public float DuckFinishTime;

		public float DuckVolumeCut;

		public bool IsActive;
	}
}
