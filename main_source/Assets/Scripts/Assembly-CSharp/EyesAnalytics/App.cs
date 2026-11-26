namespace EyesAnalytics
{
	public static class App
	{
		public static void ConsentFlowStarted()
		{
		}

		public static void ConsentResolved()
		{
		}

		public static void Ready(Landing landing, string configSource = null)
		{
		}

		internal static void RetryReady()
		{
		}

		private static void SendReady()
		{
		}

		public static void AdsConsentRecovered(string failureStage, double secondsToRecovery)
		{
		}

		internal static void RetryAdsConsentRecovered()
		{
		}

		public static void Background(int attemptElapsedSeconds, AttemptPlace place = default(AttemptPlace))
		{
		}
	}
}
