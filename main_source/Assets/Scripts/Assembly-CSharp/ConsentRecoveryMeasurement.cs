public sealed class ConsentRecoveryMeasurement
{
	public sealed class Report
	{
		public string FailureStage;

		public float SecondsToRecovery;

		public string InitOrigin;
	}

	private string failureStage;

	private float failedAt;

	private bool reported;

	public bool HasStartupFailure => false;

	public void StartupConsentFailed(string stage, float now)
	{
	}

	public Report AdsInitialized(bool successful, string initOrigin, float now)
	{
		return null;
	}
}
