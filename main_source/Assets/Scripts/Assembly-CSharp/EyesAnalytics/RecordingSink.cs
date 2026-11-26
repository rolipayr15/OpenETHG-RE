using System.Collections.Generic;

namespace EyesAnalytics
{
	public sealed class RecordingSink : IAnalyticsSink
	{
		public readonly List<AnalyticsEvent> Events;

		public readonly Dictionary<string, string> UserProperties;

		public readonly List<string> Transactions;

		public bool ThrowOnSend;

		public int UserPropertySets;

		public SinkLimits Limits { get; set; }

		public void Send(AnalyticsEvent analyticsEvent)
		{
		}

		public void SetUserProperty(string name, string value)
		{
		}

		public void ReportStoreTransaction(string transactionId)
		{
		}

		public AnalyticsEvent Last(string eventName)
		{
			return null;
		}

		public int Count(string eventName)
		{
			return 0;
		}
	}
}
