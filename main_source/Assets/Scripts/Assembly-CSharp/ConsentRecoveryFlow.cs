using System;
using System.Threading.Tasks;

public sealed class ConsentRecoveryFlow
{
	public enum ConsentState
	{
		Unknown = 0,
		Required = 1,
		NotRequired = 2,
		Obtained = 3
	}

	public interface IConsentApi
	{
		ConsentState Status { get; }

		void RequestUpdate(Action<string> onCompleted);

		bool CanRequestAds();

		void LoadForm(Action<object, string> onLoaded);

		void ShowForm(object form, Action<string> onDismissed);
	}

	public interface IHost
	{
		bool IsMenuShown { get; }

		bool CanPresent { get; }

		bool IsNetworkReachableHint { get; }

		void OnEligibilityMayHaveChanged();

		bool InitializeAds();

		void RefreshAnalyticsConsent();

		void SetFormVisible(bool visible);

		void Log(string message);
	}

	public enum Phase
	{
		NeedUpdate = 0,
		Updating = 1,
		NeedForm = 2,
		LoadingForm = 3,
		FormVisible = 4,
		NeedFinalize = 5,
		Done = 6,
		GaveUp = 7
	}

	public const float OperationTimeoutSeconds = 10f;

	public const float LoadedFormReuseSeconds = 60f;

	private readonly IConsentApi api;

	private readonly IHost host;

	private readonly ConsentRetrySchedule schedule;

	private Task<string> update;

	private Task<(object form, string error)> formLoad;

	private Task<string> formDismissal;

	private float deadline;

	private bool formLoadFromStartup;

	private object loadedForm;

	private float loadedFormAt;

	private bool formCompleted;

	private bool analyticsRefreshed;

	public Phase Current { get; private set; }

	public ConsentRetrySchedule Schedule => null;

	public bool IsFinished => false;

	public ConsentRecoveryFlow(IConsentApi api, IHost host, Task<string> updateInFlight = null, Task<(object, string)> formLoadInFlight = null)
	{
	}

	public void Tick(float now)
	{
	}

	private void TickNeedUpdate(float now)
	{
	}

	private void TickUpdating(float now)
	{
	}

	private void HandleUpdateResult(float now)
	{
	}

	private void TickNeedForm(float now)
	{
	}

	private void TickLoadingForm(float now)
	{
	}

	private void ShowLoadedForm(float now)
	{
	}

	private void TickFormVisible(float now)
	{
	}

	private void TickNeedFinalize(float now)
	{
	}

	private void Fail(float now, Phase next, string reason)
	{
	}

	private bool GiveUpIfExhausted()
	{
		return false;
	}

	private ConsentState SafeStatus()
	{
		return ConsentState.Unknown;
	}

	private static string ErrorOf(Task<string> task)
	{
		return null;
	}

	private static string Describe(AggregateException exception)
	{
		return null;
	}

	private static Task<T> Start<T>(Action<Action<T>> begin)
	{
		return null;
	}
}
