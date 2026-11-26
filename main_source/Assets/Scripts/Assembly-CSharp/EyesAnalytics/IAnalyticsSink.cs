namespace EyesAnalytics
{
	public interface IAnalyticsSink
	{
		SinkLimits Limits { get; }

		void Send(AnalyticsEvent analyticsEvent);

		void SetUserProperty(string name, string value);

		void ReportStoreTransaction(string transactionId);
	}
}
