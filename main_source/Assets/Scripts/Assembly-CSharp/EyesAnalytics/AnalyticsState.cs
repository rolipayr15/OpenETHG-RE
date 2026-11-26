using System;
using System.Collections.Generic;

namespace EyesAnalytics
{
	internal static class AnalyticsState
	{
		internal static readonly Dictionary<string, string> UserProperties;

		internal static AttemptState Attempt;

		internal static AppSessionState App;

		internal static AdOpportunityTracker Ads;

		internal static LobbyState Lobby;

		internal static IAttemptStore AttemptStore;

		internal static Func<double> Clock;

		internal static Func<long> UnixClock;

		private static double RealClock()
		{
			return 0.0;
		}

		private static long RealUnixClock()
		{
			return 0L;
		}

		internal static void Reset()
		{
		}
	}
}
