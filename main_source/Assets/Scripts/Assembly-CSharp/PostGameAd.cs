using Debug = UnityEngine.Debug;
using Bibloteka;
using Firebase.Crashlytics;
using I2.Loc;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using EyesAnalytics;
using UnityEngine;
using UnityEngine.UI;

public class PostGameAd : MonoBehaviour
{




	public Image image;

	private float lastProgress;

	private StringBuilder label;

	[SerializeField]
	private GameObject loadingPanel;

	[SerializeField]
	private Text pleaseWait;

	[SerializeField]
	private GameObject continueButton;

	[SerializeField]
	private GameObject adTimeoutContinueButton;

	private AdTimeoutContinuePress adTimeoutContinuePress;

	private float adTimeoutContinueShownAt;

	private int timedOutInterstitialAttemptId;

	private bool isAudioRestartHintShown;

	private const string AudioRestartHintTerm = "AudioRestartHint";

	private bool isLoading;

	[SerializeField]
	private bool isPlayerBeforeTheLevel = true;

	private AdPlacement adPlacement;

	private AdTrigger adTrigger;

	private bool adFinished;

	private Coroutine adTimeoutCoroutine;

	private bool destroyed;

	private void Start()
	{
		adPlacement = isPlayerBeforeTheLevel ? (AdPlacement)1 : (AdPlacement)2;
		adTrigger = Ads.TakeNextInterstitialTrigger();
		if (FirebaseState.IsInitialized && (image == null || loadingPanel == null || pleaseWait == null || continueButton == null || adTimeoutContinueButton == null))
		{
			string message = "POST_GAME_AD MISSING REFERENCES: " + string.Format("image={0}, ", image != null) + string.Format("loadingPanel={0}, ", loadingPanel != null) + string.Format("pleaseWait={0}, ", pleaseWait != null) + string.Format("continueButton={0}, ", continueButton != null) + string.Format("adTimeoutContinueButton={0}", adTimeoutContinueButton != null);
			Debug.LogError(message, this);
			Crashlytics.Log(message);
		}
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		continueButton.SetActive(false);
		if (adTimeoutContinueButton != null)
		{
			adTimeoutContinueButton.SetActive(false);
			adTimeoutContinuePress = adTimeoutContinueButton.GetComponent<AdTimeoutContinuePress>();
			if (adTimeoutContinuePress == null)
			{
				adTimeoutContinuePress = adTimeoutContinueButton.AddComponent<AdTimeoutContinuePress>();
			}
		}
		pleaseWait.text = ScriptLocalization.PleaseWait;
		label = new StringBuilder();
		AdsManager ads = CustomSingleton<AdsManager>.Instance;
		if (ads == null)
		{
			if (FirebaseState.IsInitialized)
			{
				Crashlytics.Log("POST_GAME_AD NULL: AdsManager.Instance");
			}
			Debug.LogError("PostGameAd: AdsManager.Instance is NULL", this);
			AdClosedContinue();
			return;
		}
		float capLeft = ads.InterstitialCapSecondsLeft;
		if (!ads.ShouldDisplayAds())
		{
			AdClosedContinue();
			return;
		}
		string reason = null;
		if (isPlayerBeforeTheLevel)
		{
			int config = UserGameSettings.preLevelInterstitial;
			if (config >= 0 && (config == 0 || UserGameSettings.playCounter < config))
			{
				LogAdTimeout(string.Format("pre-level interstitial held back by Remote Config ({0}, play {1}), continuing without an ad", config, UserGameSettings.playCounter));
				reason = "remote_config";
			}
		}
		if (reason == null && capLeft > 0f)
		{
			LogAdTimeout(string.Format("interstitial capped, {0}s left, continuing without an ad", (int)capLeft));
			reason = "frequency_cap";
		}
		if (reason != null)
		{
			Ads.ReportSuppressed((AdFormat)0, adPlacement, adTrigger, reason);
			AdClosedContinue();
		}
		else
		{
			StartCoroutine(ShowAd());
		}
	}

	private IEnumerator ShowAd()
	{
		if (FirebaseState.IsInitialized)
		{
			Crashlytics.Log("Start ShowAd coroutine");
		}
		for (int i = 1; i >= 1; i--)
		{
			label.Length = 0;
			label.Append(ScriptLocalization.PleaseWait);
			label.Append(" ");
			label.Append(i);
			pleaseWait.text = label.ToString();
			yield return Utils.GetDelay(1f);
		}
		adFinished = false;
		AdsManager adsManager = CustomSingleton<AdsManager>.Instance;
		if (adsManager == null)
		{
			if (FirebaseState.IsInitialized)
			{
				Crashlytics.Log("POST_GAME_AD NULL: AdsManager.Instance in ShowAd");
			}
			adFinished = true;
			AdClosedContinue();
			yield break;
		}
		if (!adsManager.IsGameInFront && adsManager.IsInterstitialReadyOnceInFront())
		{
			LogAdTimeout(string.Format("not in front, waiting up to {0:F0}s before requesting the ad", 5f));
			InterstitialFocusWait focusWait = new InterstitialFocusWait(Time.realtimeSinceStartup);
			float nextReadinessCheckAt = Time.realtimeSinceStartup + 0.25f;
			while (true)
			{
				yield return null;
				int state = (int)focusWait.Evaluate(adsManager.IsGameInFront, Time.realtimeSinceStartup);
				if (state == 1)
				{
					LogAdTimeout("back in front, requesting the ad");
					break;
				}
				bool suppress = state == 2;
				if (!suppress && Time.realtimeSinceStartup >= nextReadinessCheckAt)
				{
					nextReadinessCheckAt = Time.realtimeSinceStartup + 0.25f;
					if (!adsManager.IsInterstitialReadyOnceInFront())
					{
						LogAdTimeout("ad no longer available while waiting");
						suppress = true;
					}
				}
				if (suppress)
				{
					LogAdTimeout("not requesting the ad, continuing without it");
					Ads.ReportSuppressed((AdFormat)0, adPlacement, adTrigger, "not_in_front");
					adFinished = true;
					AdClosedContinue();
					yield break;
				}
			}
		}
		adTimeoutCoroutine = StartCoroutine(AdTimeoutWatchdog());
		adsManager.ShowInterstitialAd(delegate
		{
			if (!destroyed && !adFinished)
			{
				adFinished = true;
				if (adTimeoutCoroutine != null)
				{
					StopCoroutine(adTimeoutCoroutine);
				}
				AdClosedContinue();
			}
		}, adPlacement, adTrigger);
	}

	public void AdClosedContinue()
	{
		if (FirebaseState.IsInitialized)
		{
			Crashlytics.Log("AdClosedContinue");
		}
		if (isLoading)
		{
			return;
		}
		if (continueButton == null || pleaseWait == null)
		{
			string message = "POST_GAME_AD NULL in AdClosedContinue: " + string.Format("continueButton={0}, ", continueButton != null) + string.Format("pleaseWait={0}", pleaseWait != null);
			if (FirebaseState.IsInitialized)
			{
				Crashlytics.Log(message);
			}
			return;
		}
		continueButton.SetActive(false);
		SetAdTimeoutContinueVisible(false, "continuing");
		isLoading = true;
		pleaseWait.text = "0%";
		StartCoroutine(LoadLevel());
	}

	public void RequestContinueAfterAdTimeout()
	{
		string ignored = null;
		if (adTimeoutContinueButton == null || !adTimeoutContinueButton.activeInHierarchy)
		{
			ignored = "button not visible";
		}
		else if (adTimeoutContinuePress == null || !adTimeoutContinuePress.IsFreshPointerRelease(adTimeoutContinueShownAt))
		{
			ignored = "not a fresh pointer press";
		}
		else if (adFinished || isLoading)
		{
			ignored = "already continuing";
		}
		else if (!Application.isFocused)
		{
			ignored = "game not focused";
		}
		else if (!CustomSingleton<AdsManager>.HasInstance)
		{
			ignored = "no AdsManager";
		}
		if (ignored != null)
		{
			LogAdTimeout("Continue press ignored: " + ignored);
			return;
		}
		LogAdTimeout(string.Format("Continue pressed attempt={0}", timedOutInterstitialAttemptId));
		if (!CustomSingleton<AdsManager>.Instance.TryWriteOffTimedOutInterstitialOnPlayerRequest(timedOutInterstitialAttemptId))
		{
			LogAdTimeout("Continue press ignored: the ad attempt no longer waits");
		}
	}

	private void Update()
	{
		UpdateAdTimeoutContinue();
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (!hasFocus)
		{
			SetAdTimeoutContinueVisible(false, "focus lost");
		}
	}

	private void OnApplicationPause(bool isPaused)
	{
		if (isPaused)
		{
			SetAdTimeoutContinueVisible(false, "app paused");
		}
	}

	private void UpdateAdTimeoutContinue()
	{
		if (adTimeoutContinueButton == null || timedOutInterstitialAttemptId == 0)
		{
			return;
		}
		bool visible = !adFinished && !isLoading && CustomSingleton<AdsManager>.HasInstance && CustomSingleton<AdsManager>.Instance.IsTimedOutInterstitialWaitingInFront(timedOutInterstitialAttemptId);
		if (visible != adTimeoutContinueButton.activeSelf)
		{
			SetAdTimeoutContinueVisible(visible, visible ? "ad timed out, game in front" : "game not in front or ad finished");
		}
	}

	private void SetAdTimeoutContinueVisible(bool visible, string reason)
	{
		if (adTimeoutContinueButton == null || adTimeoutContinueButton.activeSelf == visible)
		{
			return;
		}
		adTimeoutContinueButton.SetActive(visible);
		if (adTimeoutContinuePress != null)
		{
			adTimeoutContinuePress.ResetPresses();
		}
		if (visible)
		{
			adTimeoutContinueShownAt = Time.realtimeSinceStartup;
		}
		LogAdTimeout("Continue " + (visible ? "shown" : "hidden") + ": " + reason);
	}

	private void LogAdTimeout(string message)
	{
		Debug.Log("PostGameAd: " + message);
		if (FirebaseState.IsInitialized)
		{
			Crashlytics.Log("PostGameAd: " + message);
		}
	}

	private IEnumerator AdTimeoutWatchdog()
	{
		yield return new WaitForSecondsRealtime(60f);
		if (adFinished)
		{
			yield break;
		}
		AdsManager ads = CustomSingleton<AdsManager>.Instance;
		if (ads != null && ads.HandleInterstitialCallbackTimeout(out int attemptId))
		{
			timedOutInterstitialAttemptId = attemptId;
			LogAdTimeout("Ad callback timeout — waiting for the ad");
			yield break;
		}
		adFinished = true;
		LogAdTimeout("Ad callback timeout — no active ad, continuing");
		AdClosedContinue();
	}

	private IEnumerator LoadLevel()
	{
		if (FirebaseState.IsInitialized)
		{
			Crashlytics.Log("LoadLevel");
		}
		yield return null;
		if (CustomSingleton<AdsManager>.HasInstance && CustomSingleton<AdsManager>.Instance.IsUnityAudioOutputStoppedForFullscreenAd)
		{
			LogAdTimeout("scene load waiting for the audio output");
			yield return WaitWhileAudioOutputStopped(() => CustomSingleton<AdsManager>.HasInstance && CustomSingleton<AdsManager>.Instance.IsUnityAudioOutputStoppedForFullscreenAd, () => CustomSingleton<AdsManager>.HasInstance && CustomSingleton<AdsManager>.Instance.HaveUnityAudioRestoreRetriesFailed, SetAudioRestartHintVisible);
			LogAdTimeout("audio output running, loading the scene");
		}
		string scene = "Loading";
		if (!isPlayerBeforeTheLevel)
		{
			if (UserGameSettings.IsMultiplayer() && UserGameSettings.multiplayerPlayAgain)
			{
				UserGameSettings.multiplayerPlayAgain = false;
			}
			else
			{
				scene = "MenuLoader";
			}
		}
		else if ((int)UserGameSettings.currentGameMode == 0 && (int)UserGameSettings.currentMode == 0)
		{
			int enemy = (int)UserGameSettings.currentEnemy;
			int level = (int)UserGameSettings.currentLevel;
			if ((enemy == 0 && level == 0 && !PlayerPrefz.GetBool("SKIP_INTRO_MANSION", false)) || (enemy == 1 && level == 1 && !PlayerPrefz.GetBool("SKIP_INTRO_HOSPITAL", false)) || (enemy == 2 && level == 3 && !PlayerPrefz.GetBool("SKIP_INTRO_SCHOOL", false)))
			{
				scene = "Intro";
			}
		}
		AsyncOperation async = SceneManager.LoadSceneAsync(scene);
		if (async == null)
		{
			if (FirebaseState.IsInitialized)
			{
				Crashlytics.Log("POST_GAME_AD ERROR: LoadSceneAsync returned null scene=" + scene);
			}
			Debug.LogError("Could not load scene: " + scene);
			yield break;
		}
		while (!async.isDone)
		{
			UpdateLabel(async.progress);
			yield return null;
		}
	}

	public static IEnumerator WaitWhileAudioOutputStopped(Func<bool> isOutputStopped, Func<bool> haveRetriesFailed, Action<bool> setRestartHintVisible)
	{
		while (isOutputStopped())
		{
			setRestartHintVisible(haveRetriesFailed());
			yield return null;
		}
		setRestartHintVisible(false);
	}

	private void SetAudioRestartHintVisible(bool visible)
	{
		if (isAudioRestartHintShown == visible || pleaseWait == null)
		{
			return;
		}
		isAudioRestartHintShown = visible;
		pleaseWait.resizeTextForBestFit = true;
		if (visible)
		{
			string hint = LocalizationManager.GetTranslation(AudioRestartHintTerm);
			pleaseWait.resizeTextMinSize = 20;
			pleaseWait.resizeTextMaxSize = pleaseWait.fontSize;
			pleaseWait.text = string.IsNullOrEmpty(hint) ? "Sound could not be restarted. Please restart the game." : hint;
		}
		else
		{
			pleaseWait.resizeTextMinSize = 0;
			pleaseWait.text = "0%";
		}
		LogAdTimeout("audio restart hint " + (visible ? "shown" : "hidden"));
	}

	private void UpdateLabel(float progress)
	{
		if (lastProgress >= progress)
		{
			return;
		}
		if (image == null || pleaseWait == null || label == null)
		{
			string message = "POST_GAME_AD NULL in UpdateLabel: " + string.Format("image={0}, ", image != null) + string.Format("pleaseWait={0}, ", pleaseWait != null) + string.Format("label={0}", label != null);
			if (FirebaseState.IsInitialized)
			{
				Crashlytics.Log(message);
			}
			return;
		}
		image.fillAmount = progress;
		label.Length = 0;
		label.AppendFormat("{0}%", (int)(progress * 100f));
		pleaseWait.text = label.ToString();
		lastProgress = progress;
	}

	private void OnDestroy()
	{
		destroyed = true;
	}
}
