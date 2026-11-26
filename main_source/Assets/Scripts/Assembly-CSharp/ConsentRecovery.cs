using System;
using System.Threading.Tasks;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

public sealed class ConsentRecovery : MonoBehaviour, ConsentRecoveryFlow.IHost
{
	private sealed class UmpConsentApi : ConsentRecoveryFlow.IConsentApi
	{
		private readonly ConsentRequestParameters request;

		public ConsentRecoveryFlow.ConsentState Status => ConsentRecoveryFlow.ConsentState.Unknown;

		public UmpConsentApi(ConsentRequestParameters request)
		{
		}

		public void RequestUpdate(Action<string> onCompleted)
		{
		}

		public bool CanRequestAds()
		{
			return false;
		}

		public void LoadForm(Action<object, string> onLoaded)
		{
		}

		public void ShowForm(object form, Action<string> onDismissed)
		{
		}

		public static string Describe(FormError error)
		{
			return null;
		}

		public static string Describe(Task<FormError> task)
		{
			return null;
		}
	}

	private const float MenuSettleSeconds = 1f;

	private const float TickIntervalSeconds = 0.25f;

	private static ConsentRecovery instance;

	private ConsentRecoveryFlow flow;

	private MenuInputScope formInputScope;

	private bool paused;

	private bool focused;

	private int menuHandle;

	private float menuShownSince;

	private float nextTickAt;

	public static bool IsConsentFormVisible { get; private set; }

	public bool IsMenuShown => false;

	public bool CanPresent => false;

	public bool IsNetworkReachableHint => false;

	public static void Begin(ConsentRequestParameters request, Task<FormError> updateInFlight, Task<(ConsentForm form, FormError error)> formLoadInFlight)
	{
	}

	private void Awake()
	{
	}

	private void Update()
	{
	}

	private void OnApplicationPause(bool isPaused)
	{
	}

	private void OnApplicationFocus(bool hasFocus)
	{
	}

	private void OnDestroy()
	{
	}

	private void TrackMenu()
	{
	}

	private static bool HasInputThisFrame()
	{
		return false;
	}

	public void OnEligibilityMayHaveChanged()
	{
	}

	public bool InitializeAds()
	{
		return false;
	}

	public void RefreshAnalyticsConsent()
	{
	}

	public void SetFormVisible(bool visible)
	{
	}

	private void EndFormInputScope()
	{
	}

	public void Log(string message)
	{
	}
}
