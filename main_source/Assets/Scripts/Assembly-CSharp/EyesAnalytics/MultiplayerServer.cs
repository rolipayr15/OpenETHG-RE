namespace EyesAnalytics
{
	public static class MultiplayerServer
	{
		public static void ResponseErrorEvent(int statusCode)
		{
		}

		public static void SocketErrorEvent(int failedRetries = 0)
		{
		}

		public static void ApiError(string errorKind, int? statusCode)
		{
		}

		public static void LobbyOpened(bool random)
		{
		}

		public static void SearchStarted()
		{
		}

		public static void RoomCreated()
		{
		}

		public static void CodeEntered()
		{
		}

		public static void CodeShared()
		{
		}

		public static void Matched()
		{
		}

		public static void MatchmakingCancelled()
		{
		}

		public static void LobbyClosed()
		{
		}

		public static void Blocked(BlockReason reason)
		{
		}

		private static void BeginWait(MatchmakingStage stage, MpEntry entry, bool settings)
		{
		}

		private static void EndWait(MatchmakingStage stage, bool settings)
		{
		}

		private static void ResetWait()
		{
		}

		private static void SendStage(MatchmakingStage stage, bool settings, long? waitSec = null, BlockReason? reason = null)
		{
		}
	}
}
