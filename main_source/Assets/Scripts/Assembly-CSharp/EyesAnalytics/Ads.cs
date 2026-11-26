using System;

namespace EyesAnalytics
{
	public static class Ads
	{
		private const string Platform = "AppLovin";

		private const string RevenueCurrency = "USD";

		public static AdOpportunityHandle Begin(AdFormat format, AdPlacement placement, AdTrigger trigger, AnalyticsContext.PlayMode? nextPlayMode = null)
		{
			return default;
		}

		public static void ReportSuppressed(AdFormat format, AdPlacement placement, AdTrigger trigger, string reason)
		{
		}

		public static void ReportNotReady(AdFormat format, AdPlacement placement, AdTrigger trigger)
		{
		}

		public static void SetNextInterstitialTrigger(AdTrigger trigger)
		{
		}

		public static AdTrigger TakeNextInterstitialTrigger()
		{
			return AdTrigger.Unknown;
		}

		public static AdPlacement PlacementOf(RewardedAdPlacement placement)
		{
			return AdPlacement.Unknown;
		}

		private static AdOpportunity BeginOpportunity(AdFormat format, AdPlacement placement, AdTrigger trigger)
		{
			return null;
		}

		internal static void Resolve(AdOpportunity opportunity, AdOutcome outcome, string suppressReason)
		{
		}

		public static AdLoadState ClassifyLoad(bool sdkInitialized, bool canRequestAds, bool loading, bool inBackoff, bool lastLoadFailed)
		{
			return AdLoadState.NotInitialized;
		}

		internal static void ReportMaxAdRevenue(string adUnitId, MaxSdkBase.AdInfo info)
		{
		}

		internal static void ReportAdRevenue(string unitId, string network, string providerFormat, double revenue)
		{
		}

		internal static void Warn(Exception exception)
		{
		}

		private static AdFormat? ParseFormat(string providerFormat)
		{
			return null;
		}

		private static long Milliseconds(double seconds)
		{
			return 0L;
		}
	}
}
