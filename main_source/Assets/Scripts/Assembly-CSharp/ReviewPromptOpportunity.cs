public sealed class ReviewPromptOpportunity
{
	public const double QuietSeconds = 3.0;

	private double? readySince;

	public bool IsCancelled { get; private set; }

	public void Cancel()
	{
	}

	public bool Advance(double now, bool resultsReady, bool canContinue)
	{
		return false;
	}
}
