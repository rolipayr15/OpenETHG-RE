using System;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

// Firebase initialisation flag plus the analytics-collection decision derived from the UMP/TCF consent.
// Analytics is "ready" only when Firebase is up, the build is not a debug build and consent allows collection.
public static class FirebaseState
{
	private static bool isInitialized;

	private static bool analyticsConsent;

	private static bool isAnalyticsReady;

	private static int analyticsRevision;

	internal const int StoredDecisionMaxAgeDays = 30;

	private const string ConsentRefreshedKey = "CONSENT_REFRESHED_UTC_TICKS";

	// TCF purposes 1 (store/access information), 9 (market research) and 10 (develop and improve products).
	private static readonly int[] AnalyticsPurposes = new int[3] { 1, 9, 10 };

	public static bool IsAnalyticsReady => isAnalyticsReady;

	internal static int AnalyticsRevision => analyticsRevision;

	internal static bool SupportsAnalytics => true;

	public static bool IsInitialized
	{
		get
		{
			return isInitialized;
		}
		set
		{
			isInitialized = value;
			ApplyAnalyticsPolicy();
		}
	}

	internal static bool ResolveCollectionAllowed(bool platformSupported, bool debugBuild)
	{
		return platformSupported && !debugBuild;
	}

	public static void SetAnalyticsConsent(bool allowed)
	{
		analyticsConsent = allowed;
		ApplyAnalyticsPolicy();
	}

	public static void RefreshAnalyticsConsent(bool consentFlowFailed = false)
	{
		SetAnalyticsConsent(!consentFlowFailed && ReadAnalyticsDecision());
	}

	// Used when the UMP refresh failed: a decision stored within the last 30 days still applies.
	public static void RefreshAnalyticsConsentFromStoredDecision()
	{
		bool allowed = ResolveStoredDecisionFresh(ReadConsentRefreshedUtcTicks(), DateTime.UtcNow.Ticks) && ReadAnalyticsDecision();
		Debug.Log("[Analytics] Consent refresh failed; stored decision " + (allowed ? "allows" : "does not allow") + " analytics.");
		SetAnalyticsConsent(allowed);
	}

	public static void MarkConsentRefreshed()
	{
		PlayerPrefs.SetString(ConsentRefreshedKey, DateTime.UtcNow.Ticks.ToString());
	}

	private static long ReadConsentRefreshedUtcTicks()
	{
		return long.TryParse(PlayerPrefs.GetString(ConsentRefreshedKey, ""), out long ticks) ? ticks : 0L;
	}

	private static bool ReadAnalyticsDecision()
	{
		ConsentStatus status = ConsentInformation.ConsentStatus;
		bool notRequired = status == ConsentStatus.NotRequired;
		bool allPurposes = status != ConsentStatus.NotRequired && status != ConsentStatus.Unknown && GDPRConsents.HasAllConsents(AnalyticsPurposes);
		return ResolveAnalyticsConsent(status == ConsentStatus.Unknown, notRequired, allPurposes);
	}

	internal static bool ResolveStoredDecisionFresh(long refreshedUtcTicks, long nowUtcTicks)
	{
		return refreshedUtcTicks > 0 && refreshedUtcTicks <= nowUtcTicks && nowUtcTicks - refreshedUtcTicks <= TimeSpan.FromDays(StoredDecisionMaxAgeDays).Ticks;
	}

	internal static bool ResolveAnalyticsConsent(bool unknown, bool notRequired, bool allPurposes)
	{
		return !unknown && (notRequired || allPurposes);
	}

	private static void ApplyAnalyticsPolicy()
	{
		bool wasReady = IsAnalyticsReady;
		isAnalyticsReady = false;
		if (isInitialized && Debug.isDebugBuild)
		{
			EyesAnalytics.FirebaseSink.DisableCollection();
		}
		if (isInitialized && !Debug.isDebugBuild)
		{
			EyesAnalytics.FirebaseSink.ApplyConsent(analyticsConsent);
			isAnalyticsReady = analyticsConsent;
		}
		if (!IsAnalyticsReady)
		{
			analyticsRevision++;
			EyesAnalytics.Game.InvalidateAnalyticsAttempt(wasReady);
		}
		// On becoming ready: publish user properties and retry events that were waiting for consent.
		if (wasReady || !IsAnalyticsReady)
		{
			return;
		}
		FirebaseChapterOwnershipUserProperties.Sync();
		FirebaseWinbackRewardSequence.Refresh();
		EyesAnalytics.UserProperties.Republish();
		EyesAnalytics.AnalyticsDispatcher.RunWhenReady(EyesAnalytics.App.RetryReady);
		EyesAnalytics.AnalyticsDispatcher.RunWhenReady(EyesAnalytics.App.RetryAdsConsentRecovered);
		EyesAnalytics.AnalyticsDispatcher.RunWhenReady(EyesAnalytics.Screens.RetryCurrent);
	}
}
