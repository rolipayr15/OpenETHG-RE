namespace EyesAnalytics
{
	internal sealed class FirebaseSink : IAnalyticsSink
	{
		public SinkLimits Limits => default;

		public void Send(AnalyticsEvent analyticsEvent)
		{
		}

		public void SetUserProperty(string name, string value)
		{
		}

		public void ReportStoreTransaction(string transactionId)
		{
		}

		internal static void ApplyConsent(bool analyticsAllowed)
		{
		}

		internal static void DisableCollection()
		{
		}
	}
}
