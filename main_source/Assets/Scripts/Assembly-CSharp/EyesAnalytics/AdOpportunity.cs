namespace EyesAnalytics
{
	internal sealed class AdOpportunity
	{
		internal string Id;

		internal AdFormat Format;

		internal AdPlacement Placement;

		internal AdTrigger Trigger;

		internal AnalyticsContext.Snapshot Context;

		internal double StartedAt;

		internal double RequestedAt;

		internal double ShownAt;

		internal double CoveredAt;

		internal AdUnitRole? UnitRole;

		internal bool Clicked;

		internal bool Rewarded;

		internal string LegacyDisable;

		internal bool Resolved;

		internal string UnitId;

		internal AdOutcome? Outcome;

		internal bool RevenueAttributed;

		internal AnalyticsContext.PlayMode? NextPlayMode;

		internal AdLoadState? LoadState;

		internal long LoadAgeMs;

		internal bool IsShown => false;

		internal bool IsRequested => false;
	}
}
