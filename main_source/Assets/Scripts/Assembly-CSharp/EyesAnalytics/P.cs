using Eyes.DailyChallenges;

namespace EyesAnalytics
{
	public static class P
	{
		public static readonly LongParam SchemaVersion;

		public static readonly EnumParam<AnalyticsContext.PlayMode> PlayMode;

		public static readonly LongParam MpToken;

		public static readonly StringParam SessionBucket;

		public static readonly LongParam SessionIndex;

		public static readonly ParamDef[] Globals;

		public static readonly StringParam Stage;

		public static readonly StringParam Reason;

		public static readonly LongParam ElapsedSec;

		public static readonly StringParam ConfigSource;

		public static readonly LongParam LaunchMs;

		public static readonly LongParam ConsentMs;

		public static readonly EnumParam<Landing> Landing;

		public static readonly EnumParam<ScreenId> ScreenName;

		public static readonly LongParam AttemptElapsedSec;

		public static readonly EnumParam<AdPlacement> AdPlacementOnScreen;

		public static readonly StringParam FailureStage;

		public static readonly LongParam RecoverySec;

		public static readonly StringParam AttemptId;

		public static readonly LongParam AttemptSeq;

		public static readonly LongParam AttemptNumber;

		public static readonly EnumParam<Level> LevelName;

		public static readonly EnumParam<Enemy> Opponent;

		public static readonly EnumParam<Mode> GameVariant;

		public static readonly EnumParam<Difficulty> Difficulty;

		public static readonly LongParam ExpertMode;

		public static readonly StringParam MpRole;

		public static readonly StringParam MatchId;

		public static readonly EnumParam<AttemptResult> Result;

		public static readonly StringParam EndReason;

		public static readonly LongParam DurationSec;

		public static readonly LongParam BagsCount;

		public static readonly LongParam BagsTarget;

		public static readonly LongParam MyBagsCount;

		public static readonly LongParam EyesCount;

		public static readonly LongParam Deaths;

		public static readonly LongParam Revives;

		public static readonly LongParam DeathIndex;

		public static readonly LongParam ReviveCount;

		public static readonly StringParam Method;

		public static readonly LongParam StageElapsedMs;

		public static readonly LongParam SlowFramePct;

		public static readonly LongParam Interactions;

		public static readonly LongParam EyeUses;

		public static readonly LongParam EyeOutOfCharges;

		public static readonly LongParam SprintUses;

		public static readonly LongParam SprintOutOfCharges;

		public static readonly LongParam MapUses;

		public static readonly LongParam MeatUses;

		public static readonly LongParam MeatOutOfCharges;

		public static readonly LongParam JournalOpens;

		public static readonly LongParam JournalUnlocks;

		public static readonly LongParam MonsterActivations;

		public static readonly LongParam FirstMonsterSec;

		public static readonly LongParam FirstBagSec;

		public static readonly LongParam ChasesStarted;

		public static readonly LongParam ChasesEscapedLost;

		public static readonly LongParam ChasesEscapedDoor;

		public static readonly LongParam FirstChaseSec;

		public static readonly EnumParam<ActivationCause> ActivationCause;

		public static readonly LongParam PlayerFloor;

		public static readonly LongParam MonsterFloor;

		public static readonly LongParam DoorPassageSec;

		public static readonly StringParam PosCell;

		public static readonly EnumParam<Tutorial.Step> StepName;

		public static readonly LongParam StepIndex;

		public static readonly LongParam TutorialVersion;

		public static readonly LongParam AttemptsTaken;

		public static readonly StringParam OpportunityId;

		public static readonly EnumParam<AdFormat> AdFormat;

		public static readonly EnumParam<AdPlacement> Placement;

		public static readonly EnumParam<AdTrigger> Trigger;

		public static readonly EnumParam<AdOutcome> Outcome;

		public static readonly StringParam SuppressReason;

		public static readonly EnumParam<AnalyticsContext.PlayMode> NextPlayMode;

		public static readonly EnumParam<AdLoadState> LoadState;

		public static readonly LongParam LoadAgeMs;

		public static readonly LongParam Rewarded;

		public static readonly LongParam Clicked;

		public static readonly LongParam WaitMs;

		public static readonly LongParam WatchMs;

		public static readonly EnumParam<AdUnitRole> UnitRole;

		public static readonly StringParam LegacyDisable;

		public static readonly StringParam AdPlatform;

		public static readonly StringParam AdSource;

		public static readonly StringParam AdUnitName;

		public static readonly StringParam CurrencyCode;

		public static readonly DoubleParam Value;

		public static readonly EnumParam<CurrencyType> VirtualCurrencyName;

		public static readonly EnumParam<CurrencyItemType> ItemType;

		public static readonly EnumParam<CurrencyItemId> ItemId;

		public static readonly LongParam BalanceBefore;

		public static readonly LongParam BalanceAfter;

		public static readonly StringParam ProductId;

		public static readonly StringParam ContentId;

		public static readonly EnumParam<ItemType> ProductType;

		public static readonly EnumParam<CartType> Shop;

		public static readonly LongParam IsUpsell;

		public static readonly StringParam PurchaseOrigin;

		public static readonly LongParam Level;

		public static readonly LongParam LevelsGained;

		public static readonly StringParam ChallengeId;

		public static readonly EnumParam<DailyChallengeKind> ChallengeKind;

		public static readonly StringParam ChallengeExpiresAt;

		public static readonly EnumParam<MpErrorSource> ErrorSource;

		public static readonly StringParam ErrorKind;

		public static readonly LongParam StatusCode;

		public static readonly EnumParam<GameMode> MpType;

		public static readonly EnumParam<MpEntry> MpEntry;

		public static readonly LongParam WaitSec;

		public static readonly EnumParam<BlockReason> BlockReason;

		public static readonly StringParam ContentType;

		public static readonly StringParam Error;

		public static readonly StringParam RewardType;

		public static readonly StringParam RewardId;

		public static readonly LongParam RewardAmount;

		public static readonly EnumParam<LightingSettingsSource> SettingsSource;

		public static readonly EnumParam<LightingMode> LightingModeFrom;

		public static readonly EnumParam<LightingMode> LightingModeTo;

		public static readonly DoubleParam BrightnessFrom;

		public static readonly DoubleParam BrightnessTo;

		public static readonly DoubleParam ScreenBrightness;

		public static readonly LongParam SymbolsVisible;

		public static readonly LongParam SliderMoved;

		public static readonly LongParam OpenMs;

		public static readonly LongParam CalibratorVersion;

		public static readonly EnumParam<PumpkinKind> PumpkinKind;

		public static readonly EnumParam<PumpkinOutcome> PumpkinOutcome;
	}
}
