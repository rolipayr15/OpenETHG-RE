using System;
using System.Globalization;
using System.Text;

namespace Multiplayer
{
	[Serializable]
	public class AllStats
	{
		private const string Row = "{0,-20} | {1,-12} | {2,-12} | {3,-12}\n";

		public Stats all_time;

		public Stats current_week;

		public Stats last_week;

		public AllStats(Stats _all_time, Stats _current_week, Stats _last_week)
		{
			all_time = _all_time;
			current_week = _current_week;
			last_week = _last_week;
		}

		private static int Average(int value, int count)
		{
			return count < 1 ? 0 : value / count;
		}

		public string GetFormattedTable()
		{
			if (all_time == null || current_week == null || last_week == null)
			{
				return "";
			}
			StringBuilder table = new StringBuilder();
			table.AppendFormat(Row, "Category", "All Time", "Current Week", "Last Week");
			table.Append(new string('-', 20)).Append("-+-").Append(new string('-', 12)).Append("-+-").Append(new string('-', 12)).Append("-+-").Append(new string('-', 12)).Append("\n");
			table.AppendFormat(Row, "Games (win/total)", all_time.won_games + "/" + all_time.total_games, Colorize(current_week.won_games, last_week.won_games, true) + "/" + Colorize(current_week.total_games, last_week.total_games, true), last_week.won_games + "/" + last_week.total_games);
			table.AppendFormat(Row, "Bags (win/total)", all_time.won_bag_events + "/" + all_time.total_bag_events, Colorize(current_week.won_bag_events, last_week.won_bag_events, true) + "/" + Colorize(current_week.total_bag_events, last_week.total_bag_events, true), last_week.won_bag_events + "/" + last_week.total_bag_events);
			table.AppendFormat(Row, "Avg time (win/total)", ConvertSecondsToMinutes(Average(all_time.won_duration_seconds, all_time.won_games)) + "/" + ConvertSecondsToMinutes(Average(all_time.total_duration_seconds, all_time.total_games)), ConvertSecondsToMinutes(Average(current_week.won_duration_seconds, current_week.won_games)) + "/" + ConvertSecondsToMinutes(Average(current_week.total_duration_seconds, current_week.total_games)), ConvertSecondsToMinutes(Average(last_week.won_duration_seconds, last_week.won_games)) + "/" + ConvertSecondsToMinutes(Average(last_week.total_duration_seconds, last_week.total_games)));
			table.AppendFormat(Row, "Bags/min (win/total)", all_time.won_events_per_minute.ToString("F1") + "/" + all_time.events_per_minute.ToString("F1"), Colorize(current_week.won_events_per_minute, last_week.won_events_per_minute, true, "F1") + "/" + Colorize(current_week.events_per_minute, last_week.events_per_minute, true, "F1"), last_week.won_events_per_minute.ToString("F1") + "/" + last_week.events_per_minute.ToString("F1"));
			table.AppendFormat(Row, "Distance (meters)", all_time.distance, current_week.distance, last_week.distance);
			return table.ToString();
		}

		public string ConvertSecondsToMinutes(int seconds)
		{
			return string.Format("{0}:{1:D2}", seconds / 60, seconds % 60);
		}

		private string Colorize(float current, float previous, bool higherIsBetter, string format = null)
		{
			string text = format == null ? current.ToString(CultureInfo.CurrentCulture) : current.ToString(format);
			if (current == previous)
			{
				return string.Format("{0,-12}", text);
			}
			bool better = higherIsBetter ? current > previous : current < previous;
			return string.Format("<color={0}>{1,-12}</color>", better ? "green" : "red", text);
		}

		private string Colorize(int current, int previous, bool higherIsBetter)
		{
			return Colorize((float)current, (float)previous, higherIsBetter);
		}
	}
}
