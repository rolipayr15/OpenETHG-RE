public sealed class InterstitialFocusWait
{
	public enum Outcome
	{
		Waiting = 0,
		InFront = 1,
		GaveUp = 2
	}

	public const float Seconds = 5f;

	private readonly float deadline;

	public InterstitialFocusWait(float now)
	{
		deadline = now + Seconds;
	}

	public Outcome Evaluate(bool gameInFront, float now)
	{
		if (deadline <= now)
		{
			return Outcome.GaveUp;
		}
		return gameInFront ? Outcome.InFront : Outcome.Waiting;
	}
}
