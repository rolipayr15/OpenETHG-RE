using System.Collections.Generic;

namespace EyesAnalytics
{
	internal sealed class AppSessionState
	{
		internal bool ReadySent;

		internal long LaunchMs;

		internal Landing? PendingLanding;

		internal string PendingConfigSource;

		internal double ConsentStartedAt;

		internal long ConsentMs;

		internal string PendingRecoveryStage;

		internal long PendingRecoverySec;

		internal bool RecoverySent;

		internal bool BalanceSnapshotSent;

		internal ScreenId? CurrentScreen;

		internal bool ScreenViewSent;

		internal ScreenId? BaseScreen;

		internal readonly List<ScreenId> Overlays;

		internal double ScreenViewedAt;
	}
}
