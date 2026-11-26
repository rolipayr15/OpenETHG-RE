using System;

namespace Multiplayer
{
	[Serializable]
	public class Stats
	{
		public int won_games;

		public int total_games;

		public int won_bag_events;

		public int total_bag_events;

		public int won_duration_seconds;

		public int total_duration_seconds;

		public float won_events_per_minute;

		public float events_per_minute;

		public uint distance;

		public Stats(int _won_games, int _total_games, int _won_bag_events, int _total_bag_events, int _won_duration_seconds, int _total_duration_seconds, float _won_events_per_minute, float _events_per_minute, uint _distance)
		{
		}
	}
}
