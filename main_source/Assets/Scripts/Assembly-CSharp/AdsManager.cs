using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using DarkTonic.MasterAudio;
using EyesAnalytics;
using Firebase.Crashlytics;
using GoogleMobileAds.Ump.Api;
using UnityEngine;
using MaxSdk = MaxSdkAndroid;

// AppLovin MAX interstitial and rewarded ads with primary and backup ad units, consent gating, load retries
// with backoff, and recovery from interstitials whose SDK callbacks never arrive.
public class AdsManager : CustomSingleton<AdsManager>
{
	internal interface IMaxAdsApi
	{
		bool IsInitialized();

		bool IsInterstitialReady(string adUnitId);

		void LoadInterstitial(string adUnitId);

		bool IsRewardedAdReady(string adUnitId);

		void LoadRewardedAd(string adUnitId);
	}

	private sealed class MaxSdkAdsApi : IMaxAdsApi
	{
		public bool IsInitialized()
		{
			return MaxSdk.IsInitialized();
		}

		public bool IsInterstitialReady(string adUnitId)
		{
			return MaxSdk.IsInterstitialReady(adUnitId);
		}

		public void LoadInterstitial(string adUnitId)
		{
			MaxSdk.LoadInterstitial(adUnitId);
		}

		public bool IsRewardedAdReady(string adUnitId)
		{
			return MaxSdk.IsRewardedAdReady(adUnitId);
		}

		public void LoadRewardedAd(string adUnitId)
		{
			MaxSdk.LoadRewardedAd(adUnitId);
		}
	}

	private sealed class MobileAudioOutput : FullscreenAdAudioOutput.IOutput
	{
		public bool IsStarted => AudioSettings.Mobile.audioOutputStarted;

		public void Stop()
		{
			AudioSettings.Mobile.StopAudioOutput();
		}

		public void Start()
		{
			AudioSettings.Mobile.StartAudioOutput();
		}
	}

	public enum InitializeOutcome
	{
		Requested = 0,
		AlreadyRequestedOrInitialized = 1,
		NotAllowed = 2
	}

	private sealed class LegacySessionDisableSnapshot
	{
		public int AttemptId;

		public string AdUnitId;

		public string Trigger;

		public bool LostForeground;

		public bool DisplayConfirmed;

		public float SecondsSinceBegin;
	}

	private const float AdLoadRequestTimeout = 60f;

	private const float UnconfirmedDisplayGraceSeconds = 5f;

	private readonly AdsSdkInitGate sdkInitGate = new AdsSdkInitGate();

	private readonly AdsConsentGate consentGate = new AdsConsentGate();

	private const float ConsentRecheckSeconds = 5f;

	private float nextConsentRecheckAt;

	private bool isAdLoadingResumePending;

	private static int mainThreadId;

	private readonly ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();

	private bool? canRequestAdsOverrideForTests;

	private bool? loadPathStubOverrideForTests;

	private string[] adUnitIdsOverrideForTests;

	private IMaxAdsApi maxAdsApi = new MaxSdkAdsApi();

	private bool didMuteAudioForInterstitial;

	private readonly FullscreenAdAudioOutput adAudioOutput = new FullscreenAdAudioOutput(new MobileAudioOutput());

	private bool? audioRestorePendingOverrideForTests;

	private bool isInterstitialAdShowing;

	private bool interstitialDisplayConfirmed;

	private AdOpportunityHandle interstitialOpportunity;

	private bool interstitialCallbackTimedOut;

	private bool interstitialLostForeground;

	private bool isApplicationPaused;

	private int interstitialAttemptId;

	private int legacySessionDisableReportedAttemptId;

	private float interstitialBeganAt;

	private readonly Dictionary<string, int> quarantinedInterstitialAttempts = new Dictionary<string, int>();

	private bool isUnconfirmedDisplayGraceRunning;

	private int unconfirmedDisplayGraceAttemptId;

	private float unconfirmedDisplayGraceEndsAt;

	private int audioRestoreFailureReportedAttemptId;

	private bool? focusOverrideForTests;

	private string activeInterstitialAdUnitId;

	private bool isPrimaryInterstitialLoading;

	private bool isBackupInterstitialLoading;

	private float lastPrimaryInterstitialLoadRequestTime = -1f;

	private float lastBackupInterstitialLoadRequestTime = -1f;

	private string lastPrimaryInterstitialLoadError = "none";

	private string lastBackupInterstitialLoadError = "none";

	private bool isPrimaryRewardedLoading;

	private bool isBackupRewardedLoading;

	private bool areMaxSdkCallbacksRegistered;

	private float lastPrimaryRewardedLoadRequestTime = -1f;

	private float lastBackupRewardedLoadRequestTime = -1f;

	private string lastPrimaryRewardedLoadError = "none";

	private string lastBackupRewardedLoadError = "none";

	private float nextPrimaryInterstitialRetryAt;

	private float nextBackupInterstitialRetryAt;

	private float nextPrimaryRewardedRetryAt;

	private float nextBackupRewardedRetryAt;

	private Action<bool> _interstitialCallback;

	private RewardedAdRequest rewardedRequest;

#if UNITY_ANDROID || UNITY_IOS
	private static readonly bool IsAdsStubPlatform = false;
#else
	private static readonly bool IsAdsStubPlatform = true;
#endif

	private const float RetryTickSeconds = 1f;

	private float nextRetryTickAt;

	private readonly InterstitialFrequencyCap interstitialFrequencyCap = new InterstitialFrequencyCap();

	private const float RewardedAdWaitTimeout = 3f;

	private const float RewardedAdCheckInterval = 0.25f;

	private RewardedAdPlacement? _rewardedAdPlacement;

	private AdOpportunityHandle rewardedOpportunity;

	private bool isRewardedAdShowing;

	private int rewardedRetryAttempt;

	private int interstitialRetryAttempt;

	private bool isMaxSdkInitialized => sdkInitGate.IsInitialized;

	private bool IsLoadPathStubbed => loadPathStubOverrideForTests ?? IsAdsStubPlatform;

	private bool IsUnityAudioRestorePending => audioRestorePendingOverrideForTests ?? (adAudioOutput != null && adAudioOutput.IsRestorePending);

	private bool IsApplicationFocused => focusOverrideForTests ?? Application.isFocused;

	private string maxInterstitialId => AdUnitId(0, "7472d0e732e11ef2", "1692d1c72dfa0118");

	private string maxRewardedAdId => AdUnitId(2, "88a1e38a1b0d8d5c", "468e2db0fd716f06");

	private string maxInterstitialBackupId => AdUnitId(1, "92497e83d4280728", "54ea78f33e3f71c2");

	private string maxRewardedAdBackupId => AdUnitId(3, "4be4d0a5e78bedf5", "c9e1cc2600d50541");

	private bool HasInterstitialBackupAdUnit => !string.IsNullOrEmpty(maxInterstitialBackupId);

	private bool HasRewardedBackupAdUnit => !string.IsNullOrEmpty(maxRewardedAdBackupId);

	public bool IsUnityAudioOutputStoppedForFullscreenAd => adAudioOutput != null && adAudioOutput.IsStoppedForAd;

	public bool HaveUnityAudioRestoreRetriesFailed => adAudioOutput != null && adAudioOutput.RetriesExhausted;

	public bool IsFullscreenAdBusy => IsFullscreenAdLifecycleBusy();

	public bool IsFullscreenAdShowing => isInterstitialAdShowing || isRewardedAdShowing;

	public float InterstitialCapSecondsLeft => interstitialFrequencyCap.SecondsLeft(UserGameSettings.interstitialMinIntervalSeconds, Time.realtimeSinceStartup);

	public bool IsGameInFront => IsApplicationFocused && !isApplicationPaused;

	private static bool IsOnMainThread => mainThreadId == 0 || Thread.CurrentThread.ManagedThreadId == mainThreadId;

	private string AdUnitId(int overrideIndex, string android, string ios)
	{
		if (adUnitIdsOverrideForTests != null)
		{
			return adUnitIdsOverrideForTests[overrideIndex];
		}
		if (Application.platform == RuntimePlatform.Android)
		{
			return android;
		}
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return ios;
		}
		if (!IsAdsStubPlatform)
		{
			throw new Exception("Configured only for Android and iOS");
		}
		return string.Empty;
	}

	private static void CrashLog(string message)
	{
		if (FirebaseState.IsInitialized)
		{
			Crashlytics.Log(message);
		}
	}

	public bool ShouldDisplayAds()
	{
		return !UserGameSettings.googleNBO && !UserGameSettings.unlockedNoAds && !UserGameSettings.unlockedBundle3 && !UserGameSettings.adsDisabledViaRemoteConfig && !UserGameSettings.isOneTimeUnlocked;
	}

	private string GetReadyInterstitialAdUnitId()
	{
		if (!IsInterstitialQuarantined(maxInterstitialId) && maxAdsApi.IsInterstitialReady(maxInterstitialId))
		{
			return maxInterstitialId;
		}
		if (HasInterstitialBackupAdUnit && !IsInterstitialQuarantined(maxInterstitialBackupId) && maxAdsApi.IsInterstitialReady(maxInterstitialBackupId))
		{
			return maxInterstitialBackupId;
		}
		return null;
	}

	private string GetReadyRewardedAdUnitId()
	{
		if (maxAdsApi.IsRewardedAdReady(maxRewardedAdId))
		{
			return maxRewardedAdId;
		}
		if (HasRewardedBackupAdUnit && maxAdsApi.IsRewardedAdReady(maxRewardedAdBackupId))
		{
			return maxRewardedAdBackupId;
		}
		return null;
	}

	private AdUnitRole RewardedUnitRole(string adUnitId)
	{
		return adUnitId == maxRewardedAdBackupId ? AdUnitRole.Backup : AdUnitRole.Primary;
	}

	private AdUnitRole InterstitialUnitRole(string adUnitId)
	{
		return adUnitId == maxInterstitialBackupId ? AdUnitRole.Backup : AdUnitRole.Primary;
	}

	private string GetReadyInterstitialAdUnitIdIfAllowed(string context)
	{
		return CanRequestAdsNow(context) ? GetReadyInterstitialAdUnitId() : null;
	}

	private string GetReadyRewardedAdUnitIdIfAllowed(string context)
	{
		return CanRequestAdsNow(context) ? GetReadyRewardedAdUnitId() : null;
	}

	public bool IsInterstitialAdAvailable()
	{
		return !IsAdsStubPlatform && ShouldDisplayAds() && CanRequestAdsNow("interstitial_availability") && GetReadyInterstitialAdUnitId() != null;
	}

	private void InvokeInterstitialCallback(bool success)
	{
		Action<bool> callback = _interstitialCallback;
		if (callback != null)
		{
			callback(success);
			_interstitialCallback = null;
		}
	}

	private void ResolveInterstitialNotReady(AdOpportunityHandle opportunity, Action reload)
	{
		AdLoadState state = InterstitialLoadState();
		long age = InterstitialLoadAgeMs();
		reload?.Invoke();
		opportunity.ResolveNotReady(state, age);
		InvokeInterstitialCallback(false);
	}

	private void ResolveRewardedNotReady(AdOpportunityHandle opportunity)
	{
		AdLoadState state = RewardedLoadState();
		long age = RewardedLoadAgeMs();
		EnsureRewardedAdsLoading();
		opportunity.ResolveNotReady(state, age);
	}

	public bool IsRewardedAdAvailable()
	{
		return !UserGameSettings.adsDisabledViaRemoteConfig && CanRequestAdsNow("rewarded_availability") && GetReadyRewardedAdUnitId() != null;
	}

	public void LogRewardedAdAvailability(string context)
	{
		bool primaryReady = MaxSdk.IsRewardedAdReady(maxRewardedAdId);
		bool backupReady = HasRewardedBackupAdUnit && MaxSdk.IsRewardedAdReady(maxRewardedAdBackupId);
		CrashLog("RewardedAdAvailability_" + context + "_" + AvailabilityDetails(primaryReady, isPrimaryRewardedLoading, lastPrimaryRewardedLoadRequestTime, lastPrimaryRewardedLoadError, backupReady, isBackupRewardedLoading, lastBackupRewardedLoadRequestTime, lastBackupRewardedLoadError, rewardedRetryAttempt, HasRewardedBackupAdUnit));
	}

	private void LogInterstitialAdAvailability(string context)
	{
		bool primaryReady = MaxSdk.IsInterstitialReady(maxInterstitialId);
		bool backupReady = HasInterstitialBackupAdUnit && MaxSdk.IsInterstitialReady(maxInterstitialBackupId);
		CrashLog("InterstitialAdAvailability_" + context + "_" + AvailabilityDetails(primaryReady, isPrimaryInterstitialLoading, lastPrimaryInterstitialLoadRequestTime, lastPrimaryInterstitialLoadError, backupReady, isBackupInterstitialLoading, lastBackupInterstitialLoadRequestTime, lastBackupInterstitialLoadError, interstitialRetryAttempt, HasInterstitialBackupAdUnit));
	}

	private string AvailabilityDetails(bool primaryReady, bool primaryLoading, float primaryRequest, string primaryError, bool backupReady, bool backupLoading, float backupRequest, string backupError, int retryAttempt, bool hasBackup)
	{
		return string.Format("sdkInitialized={0}_", sdkInitGate.IsInitialized)
			+ string.Format("primaryReady={0}_", primaryReady)
			+ string.Format("primaryLoading={0}_", primaryLoading)
			+ string.Format("primarySecondsSinceLoadRequest={0:F1}_", SecondsSince(primaryRequest))
			+ "primaryLastError=" + primaryError + "_"
			+ string.Format("backupReady={0}_", backupReady)
			+ string.Format("backupLoading={0}_", backupLoading)
			+ string.Format("backupSecondsSinceLoadRequest={0:F1}_", SecondsSince(backupRequest))
			+ "backupLastError=" + backupError + "_"
			+ string.Format("retryAttempt={0}_", retryAttempt)
			+ string.Format("hasBackup={0}", hasBackup);
	}

	private float SecondsSince(float timestamp)
	{
		return timestamp >= 0f ? Time.realtimeSinceStartup - timestamp : -1f;
	}

	private bool IsLoadRequestInProgress(bool isLoading, float requestTime)
	{
		return isLoading && requestTime >= 0f && Time.realtimeSinceStartup - requestTime < AdLoadRequestTimeout;
	}

	private bool IsRetryAllowed(float nextRetryAt)
	{
		return nextRetryAt <= 0f || nextRetryAt <= Time.realtimeSinceStartup;
	}

	private AdLoadState InterstitialLoadState()
	{
		return Ads.ClassifyLoad(sdkInitGate.IsInitialized, CanRequestAdsNow("interstitial_not_ready"), isPrimaryInterstitialLoading || isBackupInterstitialLoading, nextPrimaryInterstitialRetryAt > 0f && Time.realtimeSinceStartup < nextPrimaryInterstitialRetryAt, lastPrimaryInterstitialLoadError != "none");
	}

	private long InterstitialLoadAgeMs()
	{
		return LoadAgeMs(lastPrimaryInterstitialLoadRequestTime);
	}

	private AdLoadState RewardedLoadState()
	{
		return Ads.ClassifyLoad(sdkInitGate.IsInitialized, CanRequestAdsNow("rewarded_not_ready"), isPrimaryRewardedLoading || isBackupRewardedLoading, nextPrimaryRewardedRetryAt > 0f && Time.realtimeSinceStartup < nextPrimaryRewardedRetryAt, lastPrimaryRewardedLoadError != "none");
	}

	private long RewardedLoadAgeMs()
	{
		return LoadAgeMs(lastPrimaryRewardedLoadRequestTime);
	}

	private static long LoadAgeMs(float lastRequestTime)
	{
		if (lastRequestTime < 0f)
		{
			return -1L;
		}
		return (long)((Time.realtimeSinceStartup - lastRequestTime) * 1000f);
	}

	private float RetryDelaySeconds(int retryAttempt)
	{
		return (float)Math.Pow(2.0, Math.Min(6, retryAttempt));
	}

	private bool StopUnityAudioForFullscreenAd()
	{
		FullscreenAdAudioOutput.StopResult result = adAudioOutput.StopForAd();
		if (result == FullscreenAdAudioOutput.StopResult.Failed)
		{
			Debug.Log("AdLifecycle: Unity audio output stop failed before fullscreen ad");
			Crashlytics.Log("FullscreenAdUnityAudioStopFailed");
		}
		else
		{
			Debug.Log("AdLifecycle: Unity audio output stopped for fullscreen ad");
			Crashlytics.Log("FullscreenAdUnityAudioStopped");
		}
		return result != FullscreenAdAudioOutput.StopResult.Failed;
	}

	private void RequestUnityAudioRestoreAfterFullscreenAd(bool waitForForeground)
	{
		adAudioOutput.RequestRestore(waitForForeground, Time.frameCount);
	}

	private void TryRestoreUnityAudioAfterFullscreenAd()
	{
		bool canRestore = !isInterstitialAdShowing && Application.isFocused && !isApplicationPaused;
		switch (adAudioOutput.TryRestore(canRestore, Time.frameCount, Time.realtimeSinceStartup))
		{
		case FullscreenAdAudioOutput.RestoreResult.Restarted:
			Debug.Log("AdLifecycle: Unity audio output restarted after fullscreen ad");
			CrashLog("FullscreenAdUnityAudioStarted");
			break;
		case FullscreenAdAudioOutput.RestoreResult.Exhausted:
			Debug.Log(string.Format("AdLifecycle: Unity audio output restart failed, attempt {0}/{1}", adAudioOutput.AttemptCount, FullscreenAdAudioOutput.MaxAttempts));
			CrashLog("FullscreenAdUnityAudioStartFailed");
			ReportUnityAudioRestoreStillFailing();
			break;
		case FullscreenAdAudioOutput.RestoreResult.Failed:
			Debug.Log(string.Format("AdLifecycle: Unity audio output restart failed, attempt {0}/{1}", adAudioOutput.AttemptCount, FullscreenAdAudioOutput.MaxAttempts));
			break;
		}
	}

	private void Update()
	{
		RunMainThreadActions();
		UpdateUnconfirmedDisplayGrace();
		TryRestoreUnityAudioAfterFullscreenAd();
		UpdateConsentGate();
		RetryDueAdLoads();
	}

	private void RetryDueAdLoads()
	{
		if (Time.realtimeSinceStartup < nextRetryTickAt)
		{
			return;
		}
		nextRetryTickAt = Time.realtimeSinceStartup + RetryTickSeconds;
		if (RetryDue(nextPrimaryInterstitialRetryAt) || RetryDue(nextBackupInterstitialRetryAt))
		{
			EnsureInterstitialAdsLoading();
		}
		if (RetryDue(nextPrimaryRewardedRetryAt) || RetryDue(nextBackupRewardedRetryAt))
		{
			EnsureRewardedAdsLoading();
		}
	}

	private static bool RetryDue(float nextRetryAt)
	{
		return nextRetryAt > 0f && nextRetryAt <= Time.realtimeSinceStartup;
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (hasFocus)
		{
			HandleApplicationReturnedToForeground();
		}
		else if (isInterstitialAdShowing)
		{
			interstitialLostForeground = true;
			LogAdLifecycle(string.Format("focus lost during attempt={0}", interstitialAttemptId));
			CancelUnconfirmedDisplayGrace("focus lost");
		}
	}

	private void OnApplicationPause(bool isPaused)
	{
		isApplicationPaused = isPaused;
		if (!isPaused)
		{
			HandleApplicationReturnedToForeground();
		}
		else if (isInterstitialAdShowing)
		{
			interstitialLostForeground = true;
			LogAdLifecycle(string.Format("app paused during attempt={0}", interstitialAttemptId));
			CancelUnconfirmedDisplayGrace("app paused");
		}
	}

	private void HandleApplicationReturnedToForeground()
	{
		if (!IsApplicationFocused || isApplicationPaused)
		{
			return;
		}
		if (isInterstitialAdShowing)
		{
			LogAdLifecycle(string.Format("back in front during attempt={0} timedOut={1} ", interstitialAttemptId, interstitialCallbackTimedOut) + string.Format("displayConfirmed={0}", interstitialDisplayConfirmed));
		}
		LegacySessionDisableSnapshot snapshot = CaptureLegacySessionDisable("foreground");
		EvaluateTimedOutInterstitial();
		ReportLegacySessionDisable(snapshot);
		adAudioOutput?.OnReturnedToForeground(Time.frameCount);
	}

	private void ReportFullscreenAdState()
	{
		FullscreenAdSessions.Report(isInterstitialAdShowing || isRewardedAdShowing);
	}

	public bool IsInterstitialReadyOnceInFront()
	{
		if (IsAdsStubPlatform || !ShouldDisplayAds() || InterstitialCapSecondsLeft > 0f || IsFullscreenAdLifecycleBusy())
		{
			return false;
		}
		return IsInterstitialAdAvailable();
	}

	private bool IsFullscreenAdLifecycleBusy()
	{
		return isInterstitialAdShowing || isRewardedAdShowing || IsUnityAudioRestorePending;
	}

	public void EnsureRewardedAdsLoading()
	{
		if (!IsLoadPathStubbed && sdkInitGate.IsInitialized && !UserGameSettings.adsDisabledViaRemoteConfig)
		{
			LoadRewardedAd();
			LoadRewardedAdBackup();
		}
	}

	public Coroutine WhenRewardedAdAvailable(MonoBehaviour host, Action onAvailable, Action onUnavailable)
	{
		return WhenRewardedAdAvailable(host, null, onAvailable, onUnavailable);
	}

	public Coroutine WhenRewardedAdAvailable(MonoBehaviour host, RewardedAdPlacement? placement, Action onAvailable, Action onUnavailable)
	{
		if (onAvailable == null)
		{
			return null;
		}
		if (IsRewardedAdAvailable())
		{
			onAvailable();
			return null;
		}
		if (host != null)
		{
			return host.StartCoroutine(WaitForRewardedAdAvailabilityThen(host, placement, onAvailable, onUnavailable));
		}
		ReportRewardedOfferGotNoAd(placement);
		onUnavailable?.Invoke();
		return null;
	}

	private void ReportRewardedOfferGotNoAd(RewardedAdPlacement? placement)
	{
		LogAdLifecycle(string.Format("rewarded offer got no ad: placement={0}", placement));
		if (placement.HasValue)
		{
			Ads.ReportNotReady(AdFormat.Rewarded, Ads.PlacementOf(placement.Value), AdTrigger.Button);
		}
	}

	private IEnumerator WaitForRewardedAdAvailabilityThen(MonoBehaviour host, RewardedAdPlacement? placement, Action onAvailable, Action onUnavailable)
	{
		EnsureRewardedAdsLoading();
		float elapsed = 0f;
		WaitForSecondsRealtime checkDelay = new WaitForSecondsRealtime(RewardedAdCheckInterval);
		while (elapsed < RewardedAdWaitTimeout)
		{
			yield return checkDelay;
			elapsed += RewardedAdCheckInterval;
			if (host == null)
			{
				yield break;
			}
			if (IsRewardedAdAvailable())
			{
				onAvailable?.Invoke();
				yield break;
			}
		}
		if (host != null)
		{
			ReportRewardedOfferGotNoAd(placement);
			onUnavailable?.Invoke();
		}
	}

	private void EnsureInterstitialAdsLoading()
	{
		if (!IsLoadPathStubbed && sdkInitGate.IsInitialized && ShouldDisplayAds())
		{
			LoadInterstitial();
			LoadInterstitialBackup();
		}
	}

	public void ShowRewardedAd(RewardedAdPlacement placement, Action<bool> callback)
	{
		if (IsFullscreenAdLifecycleBusy())
		{
			LogAdLifecycle(string.Format("rewarded show ignored, a fullscreen ad is busy: placement={0} ", placement) + string.Format("interstitialShowing={0} rewardedShowing={1} ", isInterstitialAdShowing, isRewardedAdShowing) + string.Format("audioRestorePending={0}", IsUnityAudioRestorePending));
			Ads.ReportSuppressed(AdFormat.Rewarded, Ads.PlacementOf(placement), AdTrigger.Button, "ad_busy");
			callback?.Invoke(false);
			return;
		}
		rewardedOpportunity = Ads.Begin(AdFormat.Rewarded, Ads.PlacementOf(placement), AdTrigger.Button);
		RewardedAdRequest request = new RewardedAdRequest(callback);
		rewardedRequest = request;
		_rewardedAdPlacement = placement;
		string adUnitId = CanRequestAdsNow("rewarded_show") ? GetReadyRewardedAdUnitId() : null;
		if (string.IsNullOrEmpty(adUnitId))
		{
			ResolveRewardedNotReady(rewardedOpportunity);
			request.Fail();
			return;
		}
		isRewardedAdShowing = true;
		FullscreenAdSessions.Report(true);
		if (CustomSingleton<MouseManager>.HasInstance)
		{
			CustomSingleton<MouseManager>.Instance.Synchronize();
		}
		MaxSdk.ShowRewardedAd(adUnitId, RewardedAdEnumPlacementToString(placement));
		rewardedOpportunity.MarkRequested(adUnitId);
	}

	private void Awake()
	{
		mainThreadId = Thread.CurrentThread.ManagedThreadId;
		if (Application.isPlaying)
		{
			DontDestroyOnLoad(gameObject);
		}
	}

	public void Initialize()
	{
		Initialize(AdsSdkInitGate.Origin.ConsentGathered);
	}

	public InitializeOutcome InitializeAfterConsentGathered()
	{
		return Initialize(AdsSdkInitGate.Origin.ConsentGathered);
	}

	public void InitializeFromStoredEligibility()
	{
		Initialize(AdsSdkInitGate.Origin.StoredEligibility);
	}

	private InitializeOutcome Initialize(AdsSdkInitGate.Origin origin)
	{
		if (!CanRequestAdsNow("initialize"))
		{
			LogAdsInit(string.Format("initialisation not requested ({0}): CanRequestAds() is false", origin));
			return InitializeOutcome.NotAllowed;
		}
		bool unreadable = false;
		AdsSdkInitGate.Decision decision = sdkInitGate.TryBeginRequest(origin, () => SdkReportsInitialized(out unreadable));
		if (unreadable)
		{
			LogAdsInit(string.Format("initialisation request not decided ({0}): SDK state unreadable", origin));
			throw new InvalidOperationException("MAX SDK state unreadable; initialisation not requested");
		}
		if (decision > AdsSdkInitGate.Decision.RepeatAfterStoredEligibility)
		{
			LogAdsInit(string.Format("initialisation request ignored ({0}): {1}", origin, decision));
			return InitializeOutcome.AlreadyRequestedOrInitialized;
		}
		LogAdsInit(string.Format("initialisation requested ({0}, {1}, request {2})", origin, decision, sdkInitGate.RequestCount));
		try
		{
			InitializeAds();
		}
		catch (Exception)
		{
			sdkInitGate.RequestFailed();
			throw;
		}
		return InitializeOutcome.Requested;
	}

	private bool SdkReportsInitialized(out bool unreadable)
	{
		unreadable = false;
		try
		{
			return maxAdsApi.IsInitialized();
		}
		catch (Exception)
		{
			unreadable = true;
			return false;
		}
	}

	private void InitializeAds()
	{
		MaxSdk.SetDoNotSell(PlayerPrefz.GetBool("DO_NOT_SELL_MY_PERSONAL_INFORMATION", false));
		RegisterMaxSdkCallbacks();
		string[] adUnitIds = null;
		if (!IsAdsStubPlatform)
		{
			List<string> ids = new List<string> { maxInterstitialId, maxRewardedAdId };
			if (HasInterstitialBackupAdUnit)
			{
				ids.Add(maxInterstitialBackupId);
			}
			if (HasRewardedBackupAdUnit)
			{
				ids.Add(maxRewardedAdBackupId);
			}
			adUnitIds = ids.ToArray();
		}
		MaxSdk.InitializeSdk(adUnitIds);
	}

	private void RegisterMaxSdkCallbacks()
	{
		if (areMaxSdkCallbacksRegistered)
		{
			return;
		}
		MaxSdkCallbacks.OnSdkInitializedEvent += OnMaxSdkInitialized;
		MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
		MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
		MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
		MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent;
		MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialAdClickedEvent;
		MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnAdLoadFailedEvent;
		MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
		MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
		MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
		MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
		MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
		MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
		MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
		MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
		MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
		areMaxSdkCallbacksRegistered = true;
	}

	private void OnMaxSdkInitialized(MaxSdkBase.SdkConfiguration sdkConfiguration)
	{
		if (!IsOnMainThread)
		{
			RunOnMainThread(() => OnMaxSdkInitialized(sdkConfiguration));
			return;
		}
		bool wasInitialized = sdkInitGate.IsInitialized;
		bool successful = sdkConfiguration == null || sdkConfiguration.IsSuccessfullyInitialized;
		sdkInitGate.MarkInitialized(successful);
		LogAdsInit(string.Format("initialised (callback) success={0} origin={1} first={2}", successful, sdkInitGate.RequestOrigin, !wasInitialized));
		ConsentRecoveryReporting.AdsInitialized(successful, sdkInitGate.RequestOrigin.ToString());
		LoadInterstitial();
		LoadRewardedAd();
	}

	private void UnregisterMaxSdkCallbacks()
	{
		if (!areMaxSdkCallbacksRegistered)
		{
			return;
		}
		MaxSdkCallbacks.OnSdkInitializedEvent -= OnMaxSdkInitialized;
		MaxSdkCallbacks.Interstitial.OnAdLoadedEvent -= OnInterstitialLoadedEvent;
		MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent -= OnInterstitialDisplayedEvent;
		MaxSdkCallbacks.Interstitial.OnAdHiddenEvent -= OnInterstitialHiddenEvent;
		MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent -= OnInterstitialAdFailedToDisplayEvent;
		MaxSdkCallbacks.Interstitial.OnAdClickedEvent -= OnInterstitialAdClickedEvent;
		MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent -= OnAdLoadFailedEvent;
		MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent -= OnAdRevenuePaidEvent;
		MaxSdkCallbacks.Rewarded.OnAdLoadedEvent -= OnRewardedAdLoadedEvent;
		MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent -= OnRewardedAdReceivedRewardEvent;
		MaxSdkCallbacks.Rewarded.OnAdHiddenEvent -= OnRewardedAdHiddenEvent;
		MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent -= OnRewardedAdFailedToDisplayEvent;
		MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent -= OnRewardedAdLoadFailedEvent;
		MaxSdkCallbacks.Rewarded.OnAdClickedEvent -= OnRewardedAdClickedEvent;
		MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent -= OnRewardedAdDisplayedEvent;
		MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent -= OnAdRevenuePaidEvent;
		areMaxSdkCallbacksRegistered = false;
	}

	private void OnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo info)
	{
		Ads.ReportMaxAdRevenue(adUnitId, info);
	}

	private void OnRewardedAdDisplayedEvent(string arg1, MaxSdkBase.AdInfo info)
	{
		if (isRewardedAdShowing)
		{
			rewardedOpportunity.MarkShown(RewardedUnitRole(arg1));
		}
		CrashLog("RewardedAdDisplayedEvent" + info);
	}

	private void OnRewardedAdClickedEvent(string arg1, MaxSdkBase.AdInfo info)
	{
		if (isRewardedAdShowing)
		{
			rewardedOpportunity.MarkClicked();
		}
		CrashLog("RewardedAdClickedEvent" + info);
	}

	private void OnAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo info)
	{
		if (!IsOnMainThread)
		{
			RunOnMainThread(() => OnAdLoadFailedEvent(adUnitId, info));
			return;
		}
		SetInterstitialLoading(adUnitId, false);
		SetInterstitialLoadError(adUnitId, string.Format("code={0},message={1}", info.Code, info.Message));
		CrashLog("AdLoadFailedEvent" + adUnitId + "_" + info);
		ScheduleInterstitialRetry(adUnitId);
	}

	// A failed primary tries the backup first; only when no unit is left does the shared attempt count grow.
	private void ScheduleInterstitialRetry(string adUnitId)
	{
		if (adUnitId == maxInterstitialId && HasInterstitialBackupAdUnit)
		{
			nextPrimaryInterstitialRetryAt = Time.realtimeSinceStartup + RetryDelaySeconds(Math.Max(1, interstitialRetryAttempt + 1));
			LoadInterstitialBackup();
			return;
		}
		interstitialRetryAttempt++;
		nextPrimaryInterstitialRetryAt = Time.realtimeSinceStartup + RetryDelaySeconds(interstitialRetryAttempt);
		if (HasInterstitialBackupAdUnit)
		{
			nextBackupInterstitialRetryAt = nextPrimaryInterstitialRetryAt;
		}
	}

	private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
	{
		SetRewardedLoading(adUnitId, false);
		ClearRewardedLoadError(adUnitId);
		ClearRewardedRetry(adUnitId);
		CrashLog("RewardedAdLoaded_" + adInfo);
		rewardedRetryAttempt = 0;
	}

	private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
	{
		if (!IsOnMainThread)
		{
			RunOnMainThread(() => OnRewardedAdLoadFailedEvent(adUnitId, errorInfo));
			return;
		}
		SetRewardedLoading(adUnitId, false);
		SetRewardedLoadError(adUnitId, string.Format("code={0},message={1}", errorInfo.Code, errorInfo.Message));
		CrashLog("RewardedAdLoadFailedEvent_" + adUnitId + "_" + errorInfo);
		ScheduleRewardedRetry(adUnitId);
	}

	private void ScheduleRewardedRetry(string adUnitId)
	{
		if (adUnitId == maxRewardedAdId && HasRewardedBackupAdUnit)
		{
			nextPrimaryRewardedRetryAt = Time.realtimeSinceStartup + RetryDelaySeconds(Math.Max(1, rewardedRetryAttempt + 1));
			LoadRewardedAdBackup();
			return;
		}
		rewardedRetryAttempt++;
		nextPrimaryRewardedRetryAt = Time.realtimeSinceStartup + RetryDelaySeconds(rewardedRetryAttempt);
		if (HasRewardedBackupAdUnit)
		{
			nextBackupRewardedRetryAt = nextPrimaryRewardedRetryAt;
		}
	}

	private void OnInterstitialDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
	{
		if (!IsActiveInterstitial(adUnitId))
		{
			LogAdLifecycle("late displayed unit=" + adUnitId + " " + (IsInterstitialQuarantined(adUnitId) ? "(quarantined, ignored)" : "(no matching active attempt)"));
			return;
		}
		interstitialOpportunity.MarkShown(InterstitialUnitRole(adUnitId));
		interstitialDisplayConfirmed = true;
		LogAdLifecycle(string.Format("displayed attempt={0} unit={1} afterTimeout={2}", interstitialAttemptId, adUnitId, interstitialCallbackTimedOut));
		CancelUnconfirmedDisplayGrace("display confirmed");
		MasterAudio.MuteEverything();
		didMuteAudioForInterstitial = true;
	}

	private void OnInterstitialAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
	{
		if (IsActiveInterstitial(adUnitId))
		{
			interstitialOpportunity.MarkClicked();
		}
		CrashLog("AdClicked_" + adUnitId + "_" + adInfo);
	}

	private void EndRewardedAd(bool closed, Action restoreAudio)
	{
		AdOutcome outcome = AdOutcome.Failed;
		if (closed)
		{
			if (isRewardedAdShowing)
			{
				interstitialFrequencyCap.RecordClosed(Time.realtimeSinceStartup);
			}
			outcome = AdOutcome.Displayed;
		}
		rewardedOpportunity.Resolve(outcome);
		isRewardedAdShowing = false;
		FullscreenAdSessions.Report(isInterstitialAdShowing);
		try
		{
			restoreAudio?.Invoke();
		}
		finally
		{
			RewardedAdRequest request = rewardedRequest;
			rewardedRequest = null;
			if (closed)
			{
				request?.Complete();
			}
			else
			{
				request?.Fail();
			}
		}
	}

	private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
	{
		EndRewardedAd(true, MasterAudio.UnmuteEverything);
		ReloadRewardedAd(adUnitId);
		CrashLog("RewardedAdHidden " + adInfo);
	}

	private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo adInfo)
	{
		rewardedOpportunity.MarkRewarded();
		CrashLog("RewardedAdReceivedReward " + adInfo);
		if (isRewardedAdShowing && rewardedRequest != null)
		{
			rewardedRequest.ReportReward(Time.realtimeSinceStartup);
		}
	}

	private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
	{
		EndRewardedAd(false, MasterAudio.UnmuteEverything);
		CrashLog("RewardedAdFailedToDisplay_" + adUnitId + "_" + errorInfo.Code + "_" + errorInfo.Message);
		ReloadAllRewardedAds();
	}

	private void ReloadRewardedAd(string adUnitId)
	{
		if (HasRewardedBackupAdUnit && adUnitId == maxRewardedAdBackupId)
		{
			LoadRewardedAdBackup();
		}
		else
		{
			LoadRewardedAd();
		}
	}

	private void ReloadAllRewardedAds()
	{
		LoadRewardedAdBackup();
		LoadRewardedAd();
	}

	private void LoadInterstitial()
	{
		if (!IsOnMainThread)
		{
			RunOnMainThread(LoadInterstitial);
		}
		else if (!IsLoadPathStubbed && !maxAdsApi.IsInterstitialReady(maxInterstitialId) && !IsLoadRequestInProgress(isPrimaryInterstitialLoading, lastPrimaryInterstitialLoadRequestTime) && IsRetryAllowed(nextPrimaryInterstitialRetryAt) && CanRequestAdsNow("interstitial_load"))
		{
			if (isPrimaryInterstitialLoading)
			{
				lastPrimaryInterstitialLoadError = "load_timeout";
			}
			isPrimaryInterstitialLoading = true;
			lastPrimaryInterstitialLoadRequestTime = Time.realtimeSinceStartup;
			nextPrimaryInterstitialRetryAt = 0f;
			maxAdsApi.LoadInterstitial(maxInterstitialId);
			CrashLog("LoadInterstitial");
		}
	}

	private void LoadInterstitialBackup()
	{
		if (!IsOnMainThread)
		{
			RunOnMainThread(LoadInterstitialBackup);
		}
		else if (!IsLoadPathStubbed && HasInterstitialBackupAdUnit && !maxAdsApi.IsInterstitialReady(maxInterstitialBackupId) && !IsLoadRequestInProgress(isBackupInterstitialLoading, lastBackupInterstitialLoadRequestTime) && IsRetryAllowed(nextBackupInterstitialRetryAt) && CanRequestAdsNow("interstitial_backup_load"))
		{
			if (isBackupInterstitialLoading)
			{
				lastBackupInterstitialLoadError = "load_timeout";
			}
			isBackupInterstitialLoading = true;
			lastBackupInterstitialLoadRequestTime = Time.realtimeSinceStartup;
			nextBackupInterstitialRetryAt = 0f;
			maxAdsApi.LoadInterstitial(maxInterstitialBackupId);
			CrashLog("LoadInterstitialBackup");
		}
	}

	private void ReloadInterstitial(string adUnitId)
	{
		if (HasInterstitialBackupAdUnit && adUnitId == maxInterstitialBackupId)
		{
			LoadInterstitialBackup();
		}
		else
		{
			LoadInterstitial();
		}
	}

	private void ReloadAllInterstitialAds()
	{
		LoadInterstitialBackup();
		LoadInterstitial();
	}

	private void SetInterstitialLoading(string adUnitId, bool isLoading)
	{
		if (adUnitId == maxInterstitialId)
		{
			isPrimaryInterstitialLoading = isLoading;
		}
		else if (HasInterstitialBackupAdUnit && adUnitId == maxInterstitialBackupId)
		{
			isBackupInterstitialLoading = isLoading;
		}
	}

	private void SetInterstitialLoadError(string adUnitId, string error)
	{
		if (adUnitId == maxInterstitialId)
		{
			lastPrimaryInterstitialLoadError = error;
		}
		else if (HasInterstitialBackupAdUnit && adUnitId == maxInterstitialBackupId)
		{
			lastBackupInterstitialLoadError = error;
		}
	}

	private void ClearInterstitialLoadError(string adUnitId)
	{
		SetInterstitialLoadError(adUnitId, "none");
	}

	private void ClearInterstitialRetry(string adUnitId)
	{
		if (adUnitId == maxInterstitialId)
		{
			nextPrimaryInterstitialRetryAt = 0f;
		}
		else if (HasInterstitialBackupAdUnit && adUnitId == maxInterstitialBackupId)
		{
			nextBackupInterstitialRetryAt = 0f;
		}
	}

	private void LoadRewardedAd()
	{
		if (!IsOnMainThread)
		{
			RunOnMainThread(LoadRewardedAd);
		}
		else if (!IsLoadPathStubbed && !maxAdsApi.IsRewardedAdReady(maxRewardedAdId) && !IsLoadRequestInProgress(isPrimaryRewardedLoading, lastPrimaryRewardedLoadRequestTime) && IsRetryAllowed(nextPrimaryRewardedRetryAt) && CanRequestAdsNow("rewarded_load"))
		{
			if (isPrimaryRewardedLoading)
			{
				lastPrimaryRewardedLoadError = "load_timeout";
			}
			isPrimaryRewardedLoading = true;
			lastPrimaryRewardedLoadRequestTime = Time.realtimeSinceStartup;
			nextPrimaryRewardedRetryAt = 0f;
			CrashLog("LoadRewardedAd");
			maxAdsApi.LoadRewardedAd(maxRewardedAdId);
		}
	}

	private void LoadRewardedAdBackup()
	{
		if (!IsOnMainThread)
		{
			RunOnMainThread(LoadRewardedAdBackup);
		}
		else if (!IsLoadPathStubbed && HasRewardedBackupAdUnit && !maxAdsApi.IsRewardedAdReady(maxRewardedAdBackupId) && !IsLoadRequestInProgress(isBackupRewardedLoading, lastBackupRewardedLoadRequestTime) && IsRetryAllowed(nextBackupRewardedRetryAt) && CanRequestAdsNow("rewarded_backup_load"))
		{
			if (isBackupRewardedLoading)
			{
				lastBackupRewardedLoadError = "load_timeout";
			}
			isBackupRewardedLoading = true;
			lastBackupRewardedLoadRequestTime = Time.realtimeSinceStartup;
			nextBackupRewardedRetryAt = 0f;
			CrashLog("LoadRewardedAdBackup");
			maxAdsApi.LoadRewardedAd(maxRewardedAdBackupId);
		}
	}

	private void SetRewardedLoading(string adUnitId, bool isLoading)
	{
		if (adUnitId == maxRewardedAdId)
		{
			isPrimaryRewardedLoading = isLoading;
		}
		else if (HasRewardedBackupAdUnit && adUnitId == maxRewardedAdBackupId)
		{
			isBackupRewardedLoading = isLoading;
		}
	}

	private void SetRewardedLoadError(string adUnitId, string error)
	{
		if (adUnitId == maxRewardedAdId)
		{
			lastPrimaryRewardedLoadError = error;
		}
		else if (HasRewardedBackupAdUnit && adUnitId == maxRewardedAdBackupId)
		{
			lastBackupRewardedLoadError = error;
		}
	}

	private void ClearRewardedLoadError(string adUnitId)
	{
		SetRewardedLoadError(adUnitId, "none");
	}

	private void ClearRewardedRetry(string adUnitId)
	{
		if (adUnitId == maxRewardedAdId)
		{
			nextPrimaryRewardedRetryAt = 0f;
		}
		else if (HasRewardedBackupAdUnit && adUnitId == maxRewardedAdBackupId)
		{
			nextBackupRewardedRetryAt = 0f;
		}
	}

	private bool IsActiveInterstitial(string adUnitId)
	{
		return isInterstitialAdShowing && activeInterstitialAdUnitId == adUnitId;
	}

	private void BeginInterstitial(string adUnitId)
	{
		isInterstitialAdShowing = true;
		FullscreenAdSessions.Report(true);
		if (CustomSingleton<MouseManager>.HasInstance)
		{
			CustomSingleton<MouseManager>.Instance.Synchronize();
		}
		activeInterstitialAdUnitId = adUnitId;
		interstitialDisplayConfirmed = false;
		interstitialCallbackTimedOut = false;
		interstitialLostForeground = false;
		interstitialAttemptId++;
		interstitialBeganAt = Time.realtimeSinceStartup;
		LogAdLifecycle(string.Format("begin attempt={0} unit={1}", interstitialAttemptId, adUnitId));
	}

	private bool CompleteInterstitial(string adUnitId)
	{
		if (!IsActiveInterstitial(adUnitId))
		{
			return false;
		}
		isUnconfirmedDisplayGraceRunning = false;
		isInterstitialAdShowing = false;
		FullscreenAdSessions.Report(isRewardedAdShowing);
		interstitialDisplayConfirmed = false;
		interstitialCallbackTimedOut = false;
		activeInterstitialAdUnitId = null;
		RestoreAudioAfterInterstitial();
		return true;
	}

	// The callback timed out with no display confirmation while we are in front: allow a short grace period.
	private void EvaluateTimedOutInterstitial()
	{
		if (!isInterstitialAdShowing || !interstitialCallbackTimedOut || interstitialDisplayConfirmed || isUnconfirmedDisplayGraceRunning || !IsApplicationFocused || isApplicationPaused)
		{
			return;
		}
		isUnconfirmedDisplayGraceRunning = true;
		unconfirmedDisplayGraceAttemptId = interstitialAttemptId;
		unconfirmedDisplayGraceEndsAt = Time.realtimeSinceStartup + UnconfirmedDisplayGraceSeconds;
		LogAdLifecycle(string.Format("grace started attempt={0} unit={1} ", interstitialAttemptId, activeInterstitialAdUnitId) + string.Format("seconds={0:F0}", UnconfirmedDisplayGraceSeconds));
	}

	private void CancelUnconfirmedDisplayGrace(string reason)
	{
		if (isUnconfirmedDisplayGraceRunning)
		{
			isUnconfirmedDisplayGraceRunning = false;
			if (reason != null)
			{
				LogAdLifecycle(string.Format("grace cancelled attempt={0}: {1}", unconfirmedDisplayGraceAttemptId, reason));
			}
		}
	}

	private void UpdateUnconfirmedDisplayGrace()
	{
		if (!isUnconfirmedDisplayGraceRunning || Time.realtimeSinceStartup < unconfirmedDisplayGraceEndsAt)
		{
			return;
		}
		isUnconfirmedDisplayGraceRunning = false;
		if (!isInterstitialAdShowing || interstitialAttemptId != unconfirmedDisplayGraceAttemptId || !interstitialCallbackTimedOut || interstitialDisplayConfirmed || !IsApplicationFocused || isApplicationPaused)
		{
			LogAdLifecycle(string.Format("grace ended attempt={0}, no longer applies", unconfirmedDisplayGraceAttemptId));
			return;
		}
		LogAdLifecycle(string.Format("grace ended attempt={0}, writing the ad off", unconfirmedDisplayGraceAttemptId));
		WriteOffTimedOutInterstitial("unconfirmed_display_grace");
	}

	public bool IsTimedOutInterstitialWaitingInFront(int attemptId)
	{
		return attemptId != 0 && isInterstitialAdShowing && interstitialCallbackTimedOut && interstitialAttemptId == attemptId && IsApplicationFocused && !isApplicationPaused;
	}

	public bool TryWriteOffTimedOutInterstitialOnPlayerRequest(int attemptId)
	{
		if (!IsTimedOutInterstitialWaitingInFront(attemptId))
		{
			LogAdLifecycle(string.Format("continue request ignored attempt={0}: activeAttempt={1} ", attemptId, interstitialAttemptId) + string.Format("showing={0} timedOut={1} ", isInterstitialAdShowing, interstitialCallbackTimedOut) + string.Format("focused={0} paused={1}", IsApplicationFocused, isApplicationPaused));
			return false;
		}
		WriteOffTimedOutInterstitial("continue");
		return true;
	}

	// Gives up on the attempt; its ad unit stays quarantined until a late terminal callback or an app restart.
	private void WriteOffTimedOutInterstitial(string reason)
	{
		int attemptId = interstitialAttemptId;
		string adUnitId = activeInterstitialAdUnitId;
		bool lostForeground = interstitialLostForeground;
		bool displayConfirmed = interstitialDisplayConfirmed;
		float sinceBegin = Time.realtimeSinceStartup - interstitialBeganAt;
		LogAdLifecycle(string.Format("written off after timeout: attempt={0} unit={1} reason={2} ", attemptId, adUnitId, reason) + string.Format("lostForeground={0} displayConfirmed={1} sinceBegin={2:F1}s", lostForeground, displayConfirmed, sinceBegin));
		if (!string.IsNullOrEmpty(adUnitId))
		{
			quarantinedInterstitialAttempts[adUnitId] = attemptId;
			LogAdLifecycle(string.Format("quarantine unit={0} attempt={1} until its terminal callback or an app restart", adUnitId, attemptId));
		}
		interstitialOpportunity.Resolve(AdOutcome.TimedOut);
		CompleteInterstitial(adUnitId);
		interstitialFrequencyCap.RecordClosed(Time.realtimeSinceStartup);
		ReportInterstitialWrittenOff(attemptId, adUnitId, reason, lostForeground, displayConfirmed, sinceBegin);
		InvokeInterstitialCallback(false);
	}

	private bool IsInterstitialQuarantined(string adUnitId)
	{
		return adUnitId != null && quarantinedInterstitialAttempts.ContainsKey(adUnitId);
	}

	private bool TryReleaseInterstitialQuarantine(string adUnitId, string callback)
	{
		if (adUnitId != null && quarantinedInterstitialAttempts.TryGetValue(adUnitId, out int attemptId))
		{
			quarantinedInterstitialAttempts.Remove(adUnitId);
			LogAdLifecycle(string.Format("late {0} unit={1} (likely written-off attempt={2}), quarantine released", callback, adUnitId, attemptId));
			return true;
		}
		LogAdLifecycle("late " + callback + " unit=" + adUnitId + " (no matching attempt)");
		return false;
	}

	private void ReportInterstitialWrittenOff(int attemptId, string adUnitId, string reason, bool lostForeground, bool displayConfirmed, float secondsSinceBegin)
	{
		if (!FirebaseState.IsInitialized)
		{
			return;
		}
		Crashlytics.Log("InterstitialRecoveredAfterCallbackTimeout");
		Crashlytics.SetCustomKey("ad_attempt", attemptId.ToString());
		Crashlytics.SetCustomKey("ad_unit", adUnitId ?? string.Empty);
		Crashlytics.SetCustomKey("ad_writeoff_reason", reason);
		Crashlytics.SetCustomKey("ad_lost_foreground", lostForeground.ToString());
		Crashlytics.SetCustomKey("ad_display_confirmed", displayConfirmed.ToString());
		Crashlytics.SetCustomKey("ad_seconds_since_begin", secondsSinceBegin.ToString("F1"));
		Crashlytics.LogException(new InterstitialWrittenOffAfterTimeout());
	}

	private void ReportUnityAudioRestoreStillFailing()
	{
		int attempts = adAudioOutput != null ? adAudioOutput.AttemptCount : 0;
		LogAdLifecycle(string.Format("audio output still stopped after {0} restart attempts, attempt={1}", attempts, interstitialAttemptId));
		if (audioRestoreFailureReportedAttemptId == interstitialAttemptId)
		{
			return;
		}
		audioRestoreFailureReportedAttemptId = interstitialAttemptId;
		if (FirebaseState.IsInitialized)
		{
			Crashlytics.SetCustomKey("ad_attempt", interstitialAttemptId.ToString());
			Crashlytics.SetCustomKey("ad_audio_restart_attempts", attempts.ToString());
			Crashlytics.LogException(new UnityAudioRestoreStillFailing());
		}
	}

	public static bool LegacySessionDisableCondition(bool showing, bool timedOut, bool focused, bool paused, bool lostForeground, bool displayConfirmed)
	{
		return showing && timedOut && focused && !paused && (lostForeground || !displayConfirmed);
	}

	private LegacySessionDisableSnapshot CaptureLegacySessionDisable(string trigger)
	{
		if (!LegacySessionDisableCondition(isInterstitialAdShowing, interstitialCallbackTimedOut, IsApplicationFocused, isApplicationPaused, interstitialLostForeground, interstitialDisplayConfirmed) || legacySessionDisableReportedAttemptId == interstitialAttemptId)
		{
			return null;
		}
		legacySessionDisableReportedAttemptId = interstitialAttemptId;
		interstitialOpportunity.MarkLegacyDisable(trigger);
		return new LegacySessionDisableSnapshot
		{
			AttemptId = interstitialAttemptId,
			AdUnitId = activeInterstitialAdUnitId,
			Trigger = trigger,
			LostForeground = interstitialLostForeground,
			DisplayConfirmed = interstitialDisplayConfirmed,
			SecondsSinceBegin = Time.realtimeSinceStartup - interstitialBeganAt
		};
	}

	private void ReportLegacySessionDisable(LegacySessionDisableSnapshot snapshot)
	{
		if (snapshot == null)
		{
			return;
		}
		LogAdLifecycle(string.Format("legacy session-disable condition met: attempt={0} unit={1} ", snapshot.AttemptId, snapshot.AdUnitId) + string.Format("trigger={0} lostForeground={1} displayConfirmed={2} ", snapshot.Trigger, snapshot.LostForeground, snapshot.DisplayConfirmed) + string.Format("sinceBegin={0:F1}s", snapshot.SecondsSinceBegin));
		if (FirebaseState.IsInitialized)
		{
			Crashlytics.SetCustomKey("ad_attempt", snapshot.AttemptId.ToString());
			Crashlytics.SetCustomKey("ad_unit", snapshot.AdUnitId ?? string.Empty);
			Crashlytics.SetCustomKey("ad_trigger", snapshot.Trigger);
			Crashlytics.SetCustomKey("ad_lost_foreground", snapshot.LostForeground.ToString());
			Crashlytics.SetCustomKey("ad_display_confirmed", snapshot.DisplayConfirmed.ToString());
			Crashlytics.SetCustomKey("ad_seconds_since_begin", snapshot.SecondsSinceBegin.ToString("F1"));
			Crashlytics.LogException(new InterstitialWouldHaveBeenDisabledForSession());
		}
	}

	private void RunOnMainThread(Action action)
	{
		if (!IsOnMainThread)
		{
			mainThreadActions.Enqueue(action);
		}
		else
		{
			action?.Invoke();
		}
	}

	private void RunMainThreadActions()
	{
		while (mainThreadActions.TryDequeue(out Action action))
		{
			action?.Invoke();
		}
	}

	private bool CanRequestAdsNow(string context)
	{
		bool canRequest;
		if (canRequestAdsOverrideForTests.HasValue)
		{
			canRequest = canRequestAdsOverrideForTests.Value;
		}
		else
		{
			if (IsAdsStubPlatform)
			{
				return true;
			}
			if (!IsOnMainThread)
			{
				Debug.LogWarning("AdsConsent: eligibility checked off the main thread, treated as not allowed: " + context);
				return false;
			}
			canRequest = ConsentInformation.CanRequestAds();
		}
		switch (consentGate.Evaluate(canRequest))
		{
		case AdsConsentGate.Transition.Blocked:
			LogAdsConsent(string.Format("ad requests blocked, CanRequestAds() is false: context={0} sdkInitialized={1}", context, sdkInitGate.IsInitialized));
			break;
		case AdsConsentGate.Transition.Resumed:
			LogAdsConsent(string.Format("ad requests allowed again: context={0} sdkInitialized={1}", context, sdkInitGate.IsInitialized));
			isAdLoadingResumePending = true;
			break;
		}
		return canRequest;
	}

	private void UpdateConsentGate()
	{
		if (consentGate.IsBlocked && sdkInitGate.IsInitialized && nextConsentRecheckAt <= Time.realtimeSinceStartup)
		{
			nextConsentRecheckAt = Time.realtimeSinceStartup + ConsentRecheckSeconds;
			CanRequestAdsNow("recheck");
		}
		if (isAdLoadingResumePending && sdkInitGate.IsInitialized)
		{
			isAdLoadingResumePending = false;
			EnsureInterstitialAdsLoading();
			EnsureRewardedAdsLoading();
		}
	}

	public void OnConsentMayHaveChanged(string reason)
	{
		RunOnMainThread(delegate
		{
			CanRequestAdsNow(reason);
		});
	}

	private void LogAdsConsent(string message)
	{
		Debug.Log("AdsConsent: " + message);
		CrashLog("AdsConsent: " + message);
	}

	private void LogAdsInit(string message)
	{
		Debug.Log("AdsInit: " + message);
		CrashLog("AdsInit: " + message);
	}

	private void LogAdLifecycle(string message)
	{
		Debug.Log("AdLifecycle: " + message);
		CrashLog("AdLifecycle: " + message);
	}

	private bool TryShowInterstitial(string adUnitId, string placement, out Exception exception)
	{
		BeginInterstitial(adUnitId);
		try
		{
			if (!StopUnityAudioForFullscreenAd())
			{
				throw new InvalidOperationException("Could not stop Unity audio before interstitial");
			}
			MaxSdk.ShowInterstitial(adUnitId, placement);
			exception = null;
			return true;
		}
		catch (Exception caught)
		{
			exception = caught;
			CompleteInterstitial(adUnitId);
			return false;
		}
	}

	public void ShowInterstitialAd(Action<bool> callback)
	{
		ShowInterstitialAd(callback, AdPlacement.Unknown, AdTrigger.Unknown);
	}

	public void ShowInterstitialAd(Action<bool> callback, AdPlacement adPlacement, AdTrigger adTrigger)
	{
		bool display = ShouldDisplayAds();
		if (IsFullscreenAdLifecycleBusy())
		{
			callback?.Invoke(false);
			return;
		}
		string suppressed = null;
		if (InterstitialCapSecondsLeft > 0f)
		{
			LogAdLifecycle(string.Format("interstitial capped, {0}s left (min interval {1}s)", (int)InterstitialCapSecondsLeft, UserGameSettings.interstitialMinIntervalSeconds));
			suppressed = "frequency_cap";
		}
		else if (!Application.isFocused || isApplicationPaused)
		{
			suppressed = "not_in_front";
		}
		if (suppressed != null)
		{
			if (display)
			{
				Ads.ReportSuppressed(AdFormat.Interstitial, adPlacement, adTrigger, suppressed);
			}
			callback?.Invoke(false);
			return;
		}
		AdOpportunityHandle opportunity = default;
		if (display)
		{
			AnalyticsContext.PlayMode? nextPlayMode = null;
			if (adPlacement == AdPlacement.PreLevel)
			{
				nextPlayMode = AnalyticsContext.PlayModeOf(UserGameSettings.currentGameMode);
			}
			opportunity = Ads.Begin(AdFormat.Interstitial, adPlacement, adTrigger, nextPlayMode);
		}
		_interstitialCallback = callback;
		if (IsAdsStubPlatform || !display)
		{
			ResolveInterstitialNotReady(opportunity, null);
			return;
		}
		if (!IsInterstitialAdAvailable())
		{
			LogInterstitialAdAvailability("show_unavailable");
			ResolveInterstitialNotReady(opportunity, EnsureInterstitialAdsLoading);
			return;
		}
		string adUnitId = GetReadyInterstitialAdUnitId();
		if (string.IsNullOrEmpty(adUnitId))
		{
			LogInterstitialAdAvailability("ready_unit_missing");
			CrashLog("InterstitialAdUnitIdNullOrEmpty_" + adUnitId);
		}
		else if (maxAdsApi.IsInterstitialReady(adUnitId))
		{
			string placement = UserGameSettings.IsMultiplayer() ? "multiplayer" : "default";
			if (TryShowInterstitial(adUnitId, placement, out Exception exception))
			{
				interstitialOpportunity = opportunity;
				interstitialOpportunity.MarkRequested(adUnitId);
				CrashLog("InterstitialShown_" + adUnitId + "_" + placement);
				return;
			}
			LoadInterstitialBackup();
			LoadInterstitial();
			CrashLog("Exception while showing interstitial" + exception.Message);
			opportunity.Resolve(AdOutcome.Failed);
			InvokeInterstitialCallback(false);
			return;
		}
		else
		{
			CrashLog("Interstitial not ready");
		}
		ResolveInterstitialNotReady(opportunity, ReloadAllInterstitialAds);
	}

	private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
	{
		SetInterstitialLoading(adUnitId, false);
		ClearInterstitialLoadError(adUnitId);
		ClearInterstitialRetry(adUnitId);
		interstitialRetryAttempt = 0;
		CrashLog("InterstitialLoaded_" + adInfo);
	}

	private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
	{
		int attemptId = interstitialAttemptId;
		bool timedOut = interstitialCallbackTimedOut;
		if (!CompleteInterstitial(adUnitId))
		{
			if (TryReleaseInterstitialQuarantine(adUnitId, "display failure"))
			{
				ReloadAllInterstitialAds();
			}
			return;
		}
		interstitialOpportunity.Resolve(AdOutcome.Failed);
		LogAdLifecycle(string.Format("display failed attempt={0} unit={1} afterTimeout={2}", attemptId, adUnitId, timedOut));
		CrashLog("InterstitialAdFailedToDisplay" + errorInfo + adInfo);
		InvokeInterstitialCallback(false);
		ReloadAllInterstitialAds();
	}

	private void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
	{
		int attemptId = interstitialAttemptId;
		bool timedOut = interstitialCallbackTimedOut;
		if (!CompleteInterstitial(adUnitId))
		{
			if (TryReleaseInterstitialQuarantine(adUnitId, "hidden"))
			{
				ReloadInterstitial(adUnitId);
			}
			return;
		}
		interstitialOpportunity.Resolve(AdOutcome.Displayed);
		interstitialFrequencyCap.RecordClosed(Time.realtimeSinceStartup);
		LogAdLifecycle(string.Format("closed attempt={0} unit={1} afterTimeout={2}", attemptId, adUnitId, timedOut));
		InvokeInterstitialCallback(true);
		ReloadInterstitial(adUnitId);
	}

	private void RestoreAudioAfterInterstitial()
	{
		if (didMuteAudioForInterstitial)
		{
			didMuteAudioForInterstitial = false;
			MasterAudio.UnmuteEverything();
		}
		RequestUnityAudioRestoreAfterFullscreenAd(false);
	}

	public bool HandleInterstitialCallbackTimeout(out int attemptId)
	{
		if (!isInterstitialAdShowing)
		{
			attemptId = 0;
			LogAdLifecycle("timeout with no active attempt");
			return false;
		}
		attemptId = interstitialAttemptId;
		interstitialCallbackTimedOut = true;
		LogAdLifecycle(string.Format("timeout attempt={0} unit={1} ", attemptId, activeInterstitialAdUnitId) + string.Format("focused={0} paused={1} ", IsApplicationFocused, isApplicationPaused) + string.Format("displayConfirmed={0} lostForeground={1}", interstitialDisplayConfirmed, interstitialLostForeground));
		LegacySessionDisableSnapshot snapshot = CaptureLegacySessionDisable("timeout");
		EvaluateTimedOutInterstitial();
		ReportLegacySessionDisable(snapshot);
		return true;
	}

	private string RewardedAdEnumPlacementToString(RewardedAdPlacement adPlacement)
	{
		return adPlacement.ToString().ToLowerInvariant();
	}

	private string InterstitialAdEnumPlacementToString(InterstitialPlacement adPlacement)
	{
		return (int)adPlacement == 1 ? "Multiplayer" : string.Empty;
	}

	private string InterstitialAdPlacementToIronSourcePlacement(InterstitialPlacement adPlacement)
	{
		return InterstitialAdEnumPlacementToString(adPlacement);
	}

	private string RewardedAdPlacementToIronSourcePlacement(RewardedAdPlacement adPlacement)
	{
		return RewardedAdEnumPlacementToString(adPlacement);
	}

	protected override void OnDestroy()
	{
		UnregisterMaxSdkCallbacks();
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}
