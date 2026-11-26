using System.Collections.Generic;
using Eyes.DailyChallenges;

namespace EyesAnalytics
{
	public static class EnumMaps
	{
		public static readonly IReadOnlyDictionary<AnalyticsContext.PlayMode, string> PlayMode;

		public static readonly IReadOnlyDictionary<Level, string> Level;

		public static readonly IReadOnlyDictionary<Enemy, string> Enemy;

		public static readonly IReadOnlyDictionary<Mode, string> Mode;

		public static readonly IReadOnlyDictionary<Difficulty, string> Difficulty;

		public static readonly IReadOnlyDictionary<GameMode, string> GameMode;

		public static readonly IReadOnlyDictionary<GameFinishedReason, string> FinishedReason;

		public static readonly IReadOnlyDictionary<AttemptResult, string> Result;

		internal static readonly IReadOnlyDictionary<Game.ReviveStage, string> ReviveStage;

		public static readonly IReadOnlyDictionary<Landing, string> Landing;

		public static readonly IReadOnlyDictionary<ScreenId, string> Screen;

		public static readonly IReadOnlyDictionary<Tutorial.Step, string> TutorialStep;

		public static readonly IReadOnlyDictionary<AdFormat, string> AdFormat;

		public static readonly IReadOnlyDictionary<AdPlacement, string> AdPlacement;

		public static readonly IReadOnlyDictionary<AdTrigger, string> AdTrigger;

		public static readonly IReadOnlyDictionary<AdOutcome, string> AdOutcome;

		public static readonly IReadOnlyDictionary<AdLoadState, string> AdLoadState;

		public static readonly IReadOnlyDictionary<AdUnitRole, string> AdUnitRole;

		public static readonly IReadOnlyDictionary<CurrencyType, string> Currency;

		public static readonly IReadOnlyDictionary<CurrencyItemType, string> CurrencyItemType;

		public static readonly IReadOnlyDictionary<CurrencyItemId, string> CurrencyItemId;

		public static readonly IReadOnlyDictionary<ItemType, string> ProductType;

		public static readonly IReadOnlyDictionary<CartType, string> Shop;

		public static readonly IReadOnlyDictionary<DailyChallengeKind, string> ChallengeKind;

		public static readonly IReadOnlyDictionary<MpErrorSource, string> MpErrorSource;

		public static readonly IReadOnlyDictionary<MatchmakingStage, string> MatchmakingStage;

		public static readonly IReadOnlyDictionary<MpEntry, string> MpEntry;

		public static readonly IReadOnlyDictionary<BlockReason, string> BlockReason;

		public static readonly IReadOnlyDictionary<ActivationCause, string> ActivationCause;

		public static readonly IReadOnlyDictionary<ShareStage, string> ShareStage;

		public static readonly IReadOnlyDictionary<ReviewStage, string> ReviewStage;

		public static readonly IReadOnlyDictionary<PushRewardStage, string> PushRewardStage;

		public static readonly IReadOnlyDictionary<BalanceReason, string> BalanceReason;

		public static readonly IReadOnlyDictionary<LightingMode, string> LightingMode;

		public static readonly IReadOnlyDictionary<LightingSettingsSource, string> LightingSettingsSource;

		public static readonly IReadOnlyDictionary<PumpkinKind, string> PumpkinKind;

		public static readonly IReadOnlyDictionary<PumpkinOutcome, string> PumpkinOutcome;
	}
}
