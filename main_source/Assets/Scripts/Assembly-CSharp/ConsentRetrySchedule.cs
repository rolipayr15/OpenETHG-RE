public sealed class ConsentRetrySchedule
{
	private static readonly float[] DelaysAfterFailure;

	public const int MaxAttempts = 5;

	private float nextAttemptAt;

	public int AttemptsStarted { get; private set; }

	public int AttemptsFailed { get; private set; }

	public bool IsExhausted => false;

	public bool IsDue(float now)
	{
		return false;
	}

	public void AttemptStarted()
	{
	}

	public void AttemptFailed(float now)
	{
	}

	public void OperationSucceeded()
	{
	}

	public float SecondsUntilDue(float now)
	{
		return 0f;
	}
}
