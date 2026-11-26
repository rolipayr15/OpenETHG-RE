// Minimum time between interstitials, measured from the moment the previous one closed.
public sealed class InterstitialFrequencyCap
{
	private bool hasClosed;

	private float lastClosedAt;

	public void RecordClosed(float now)
	{
		lastClosedAt = now;
		hasClosed = true;
	}

	public float SecondsLeft(int minIntervalSeconds, float now)
	{
		if (minIntervalSeconds <= 0 || !hasClosed)
		{
			return 0f;
		}
		return lastClosedAt + minIntervalSeconds - now;
	}
}
