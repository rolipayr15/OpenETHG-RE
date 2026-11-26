// Tracks transitions of "may we request ads" so that blocking and resuming are only handled once.
public sealed class AdsConsentGate
{
	public enum Transition
	{
		None = 0,
		Blocked = 1,
		Resumed = 2
	}

	public bool IsBlocked { get; private set; }

	public int BlockedPeriods { get; private set; }

	public Transition Evaluate(bool canRequestAds)
	{
		if (canRequestAds)
		{
			if (!IsBlocked)
			{
				return Transition.None;
			}
			IsBlocked = false;
			return Transition.Resumed;
		}
		if (IsBlocked)
		{
			return Transition.None;
		}
		IsBlocked = true;
		BlockedPeriods++;
		return Transition.Blocked;
	}
}
