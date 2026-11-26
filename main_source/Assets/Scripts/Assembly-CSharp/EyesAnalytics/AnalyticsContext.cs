namespace EyesAnalytics
{
	public static class AnalyticsContext
	{
		public enum PlayMode
		{
			Menu = 0,
			Single = 1,
			Coop = 2,
			Vs = 3
		}

		public readonly struct Snapshot
		{
			public readonly PlayMode? Mode;

			public readonly bool? MpToken;

			public readonly int SessionIndex;

			internal Snapshot(PlayMode? mode, bool? mpToken, int sessionIndex)
			{
				Mode = null;
				MpToken = null;
				SessionIndex = 0;
			}
		}

		private static PlayMode? mode;

		private static bool? mpToken;

		private static int sessionIndex;

		public static void SetMenu()
		{
		}

		public static void SetSingleplayer()
		{
		}

		public static void SetMultiplayer(bool coop, bool withToken)
		{
		}

		public static void SetSession(int index)
		{
		}

		public static PlayMode PlayModeOf(GameMode gameMode)
		{
			return PlayMode.Menu;
		}

		public static Snapshot Capture()
		{
			return default;
		}

		public static string SessionBucket(int index)
		{
			return null;
		}

		internal static void Reset()
		{
		}
	}
}
