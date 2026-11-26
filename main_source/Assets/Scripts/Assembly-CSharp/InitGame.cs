using System.Collections;
using System.Threading.Tasks;
using EyesAnalytics;
using Firebase.Crashlytics;
using GoogleMobileAds.Ump.Api;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitGame : CustomSingleton<InitGame>
{
	private const float ConsentNetworkTimeoutSeconds = 10f;

	[Header("Subsystems that need to be inited when GDPR/ATT flow decides")]
	[SerializeField]
	private InitializeUnityServices unityServices;

	[SerializeField]
	private AdsManager adsManager;

	[SerializeField]
	private Purchaser purchaser;

	[SerializeField]
	private EpilepsyWarning epilepsyWarning;

	private SpinnerRadialFillingEmptying spinner;

	private bool ump_obtained_decision;

	private bool ump_encountered_error;

	private ConsentRequestParameters consentRequest;

	private Task<FormError> startupUpdateInFlight;

	private Task<(ConsentForm form, FormError error)> startupFormLoadInFlight;

	private string consentFailureStage;

	private void Awake()
	{
		AnalyticsDispatcher.InitializeMainThread();
		FirebaseState.SetAnalyticsConsent(false);
		spinner = FindObjectOfType<SpinnerRadialFillingEmptying>();
	}

	private void Start()
	{
		App.ConsentFlowStarted();
		StartCoroutine(StartupFlow());
	}

	private IEnumerator StartupFlow()
	{
		consentRequest = new ConsentRequestParameters();
		yield return GatherConsent(consentRequest);
		yield return WaitForUMPDecision();
	}

	private IEnumerator GatherConsent(ConsentRequestParameters request)
	{
		TaskCompletionSource<FormError> update = new TaskCompletionSource<FormError>();
		Debug.Log("[UMP] Updating consent information...");
		ConsentInformation.Update(request, error => update.TrySetResult(error));
		if (ump_encountered_error)
		{
			yield break;
		}
		yield return WaitForConsentNetworkOperation(update.Task, "update", "Consent info update");
		if (ump_encountered_error)
		{
			// Timed out: hand the still-running request over to the recovery flow.
			if (!update.Task.IsCompleted)
			{
				startupUpdateInFlight = update.Task;
			}
			yield break;
		}
		if (update.Task.Result != null)
		{
			FailConsent("update", "Consent info update error: " + update.Task.Result);
			yield break;
		}

		ConsentStatus status = ConsentInformation.ConsentStatus;
		Debug.Log(string.Format("[UMP] Consent info updated. Status={0}", status));
		if (status == ConsentStatus.Required)
		{
			TaskCompletionSource<(ConsentForm form, FormError error)> load = new TaskCompletionSource<(ConsentForm form, FormError error)>();
			ConsentForm.Load((form, error) => load.TrySetResult((form, error)));
			if (ump_encountered_error)
			{
				yield break;
			}
			yield return WaitForConsentNetworkOperation(load.Task, "form_load", "Consent form load");
			if (ump_encountered_error)
			{
				if (!load.Task.IsCompleted)
				{
					startupFormLoadInFlight = load.Task;
				}
				yield break;
			}
			if (load.Task.Result.error != null || load.Task.Result.form == null)
			{
				FailConsent("form_load", "Consent form load error: " + load.Task.Result.error);
				yield break;
			}

			TaskCompletionSource<FormError> dismissal = new TaskCompletionSource<FormError>();
			load.Task.Result.form.Show(error => dismissal.TrySetResult(error));
			if (ump_encountered_error)
			{
				yield break;
			}
			while (!dismissal.Task.IsCompleted)
			{
				yield return null;
			}
			if (dismissal.Task.Result != null)
			{
				FailConsent("form_show", "Consent form error: " + dismissal.Task.Result);
				yield break;
			}
			ump_obtained_decision = true;
			yield break;
		}
		if (status == ConsentStatus.Unknown)
		{
			FailConsent("update", "Consent status is still unknown after updating");
			yield break;
		}
		ump_obtained_decision = true;
	}

	private IEnumerator WaitForConsentNetworkOperation(Task operation, string stage, string description)
	{
		float deadline = Time.realtimeSinceStartup + ConsentNetworkTimeoutSeconds;
		while (!operation.IsCompleted && Time.realtimeSinceStartup < deadline)
		{
			yield return null;
		}
		if (!operation.IsCompleted)
		{
			FailConsent(stage, string.Format("{0} timed out after {1}s (network={2})", description, ConsentNetworkTimeoutSeconds, Application.internetReachability));
		}
	}

	private void FailConsent(string stage, string message)
	{
		consentFailureStage = stage;
		ump_encountered_error = true;
		Debug.LogWarning("[UMP] " + message);
		LogConsentBreadcrumb("consent refresh failed stage=" + stage + ": " + message);
	}

	private static void LogConsentBreadcrumb(string message)
	{
		if (FirebaseState.IsInitialized)
		{
			Crashlytics.Log("[UMP] " + message);
		}
	}

	private IEnumerator WaitForUMPDecision()
	{
		if (!ump_obtained_decision)
		{
			ump_encountered_error = true;
			Debug.LogWarning("[UMP] Error path — consent status unknown. Initialising consent-exempt subsystems only.");
		}
		else
		{
			Debug.Log(string.Format("[UMP] Decision obtained. Status={0}, CanRequestAds={1}", ConsentInformation.ConsentStatus, ConsentInformation.CanRequestAds()));
		}
		unityServices.Initialize();
		if (!ump_obtained_decision)
		{
			ConsentRecoveryReporting.StartupConsentFailed(consentFailureStage);
		}
		if (ump_obtained_decision && ConsentInformation.CanRequestAds())
		{
			adsManager.Initialize();
		}
		else if (!ump_obtained_decision)
		{
			InitializeAdsFromStoredEligibility();
		}
		if (!ump_obtained_decision)
		{
			BeginConsentRecovery();
		}
		if (!ump_obtained_decision)
		{
			FirebaseState.RefreshAnalyticsConsentFromStoredDecision();
		}
		else
		{
			FirebaseState.MarkConsentRefreshed();
			FirebaseState.RefreshAnalyticsConsent();
		}
		App.ConsentResolved();
		purchaser.Init();
		yield return PrepareRemoteConfig();
		spinner.TurnOff();
		yield return epilepsyWarning.Show();
		LoadGame();
	}

	private void InitializeAdsFromStoredEligibility()
	{
		if (!ConsentInformation.CanRequestAds())
		{
			Debug.Log("[UMP] No stored ad eligibility after the consent error — ads stay off.");
			return;
		}
		Debug.Log("[UMP] Stored ad eligibility after the consent error (stage=" + consentFailureStage + ") — initialising ads.");
		LogConsentBreadcrumb("ads initialisation requested from stored eligibility after consent failure stage=" + consentFailureStage);
		adsManager.InitializeFromStoredEligibility();
	}

	private void BeginConsentRecovery()
	{
		ConsentRecovery.Begin(consentRequest, startupUpdateInFlight, startupFormLoadInFlight);
	}

	private static IEnumerator PrepareRemoteConfig()
	{
		float deadline = Time.realtimeSinceStartup + 3f;
		while (!RemoteConfigWrapper.IsReady && !RemoteConfigWrapper.IsFetchComplete && Time.realtimeSinceStartup < deadline)
		{
			yield return null;
		}
		RemoteConfigWrapper.CompleteStartup();
		RemoteConfigsHandler.ApplyConfigs();
	}

	public void LoadGame()
	{
		int session = PlayerPrefs.GetInt("GAME_SESSION_COUNTER", 0) + 1;
		PlayerPrefs.SetInt("GAME_SESSION_COUNTER", session);
		AnalyticsContext.SetSession(session);
		if (!PlayerPrefs.HasKey("PLAY_COUNTER"))
		{
			UserGameSettings.playCounter = 0;
			PlayerPrefs.SetInt("PLAY_COUNTER", 0);
			UserGameSettings.playingForTheFirstTime = true;
		}
		else
		{
			UserGameSettings.playCounter = PlayerPrefs.GetInt("PLAY_COUNTER", 0);
			UserGameSettings.playingForTheFirstTime = false;
		}
		Breadcrumbs.Log("init_started");
		SceneManager.LoadScene("MenuLoader");
	}

	private void LoadNextSceneIfNintendoSwitch()
	{
	}

	private void LoadNextSceneIfNBO()
	{
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}
