namespace EyesAnalytics
{
	public sealed class LogSink : IAnalyticsSink
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
	}
}
