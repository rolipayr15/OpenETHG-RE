using System.Collections.Generic;

namespace EyesAnalytics
{
	internal sealed class AdOpportunityTracker
	{
		private readonly AdOpportunity[] open;

		internal AdTrigger NextInterstitialTrigger;

		private const int RecentCapacity = 8;

		private const double MaxRevenueDelaySeconds = 600.0;

		private readonly List<AdOpportunity> recent;

		internal AdOpportunity OnScreen => null;

		internal AdOpportunity Open(AdFormat format)
		{
			return null;
		}

		internal AdOpportunity Begin(AdFormat format, AdPlacement placement, AdTrigger trigger, AnalyticsContext.Snapshot context, double now)
		{
			return null;
		}

		internal void MarkShown(AdOpportunity opportunity, double now, AdUnitRole? unitRole)
		{
		}

		internal void MarkCovered(double now)
		{
		}

		internal AdOpportunity BeginDetached(AdFormat format, AdPlacement placement, AdTrigger trigger, AnalyticsContext.Snapshot context, double now)
		{
			return null;
		}

		internal void MarkRequested(AdOpportunity opportunity, string unitId)
		{
		}

		internal AdOpportunity FindForRevenue(AdFormat? format, string unitId, double now)
		{
			return null;
		}

		internal void Close(AdOpportunity opportunity)
		{
		}
	}
}
