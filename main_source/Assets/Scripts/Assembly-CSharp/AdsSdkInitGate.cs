using System;

// Makes sure the ad SDK is initialized once, but allows one repeat when consent arrives after a start
// from stored eligibility did not produce a working SDK.
public sealed class AdsSdkInitGate
{
	public enum State
	{
		NotRequested = 0,
		Requested = 1,
		Initialized = 2
	}

	public enum Origin
	{
		ConsentGathered = 0,
		StoredEligibility = 1
	}

	public enum Decision
	{
		Start = 0,
		RepeatAfterStoredEligibility = 1,
		IgnoredPending = 2,
		IgnoredInitialized = 3
	}

	private bool repeatedAfterStoredEligibility;

	private bool storedEligibilityStartFailed;

	private bool isRepeatInFlight;

	private State stateBeforeRepeat;

	private bool storedEligibilityStartFailedBeforeRepeat;

	public State Current { get; private set; }

	public int RequestCount { get; private set; }

	public Origin RequestOrigin { get; private set; }

	public bool IsInitialized => Current == State.Initialized;

	public bool LastCallbackSuccessful { get; private set; }

	public Decision TryBeginRequest(Origin origin, Func<bool> sdkReportsInitialized)
	{
		if (Current == State.Requested)
		{
			if (origin != Origin.ConsentGathered || RequestOrigin != Origin.StoredEligibility || sdkReportsInitialized == null || repeatedAfterStoredEligibility || sdkReportsInitialized())
			{
				return Decision.IgnoredPending;
			}
			return Repeat(origin);
		}
		if (Current == State.Initialized)
		{
			if (origin != Origin.ConsentGathered || !storedEligibilityStartFailed || repeatedAfterStoredEligibility)
			{
				return Decision.IgnoredInitialized;
			}
			return Repeat(origin);
		}
		RequestOrigin = origin;
		Current = State.Requested;
		RequestCount++;
		return Decision.Start;
	}

	private Decision Repeat(Origin origin)
	{
		isRepeatInFlight = true;
		repeatedAfterStoredEligibility = true;
		storedEligibilityStartFailedBeforeRepeat = storedEligibilityStartFailed;
		storedEligibilityStartFailed = false;
		RequestOrigin = origin;
		stateBeforeRepeat = Current;
		RequestCount++;
		return Decision.RepeatAfterStoredEligibility;
	}

	public void RequestFailed()
	{
		if (isRepeatInFlight)
		{
			isRepeatInFlight = false;
			repeatedAfterStoredEligibility = false;
			Current = stateBeforeRepeat;
			storedEligibilityStartFailed = storedEligibilityStartFailedBeforeRepeat;
			RequestOrigin = Origin.StoredEligibility;
		}
		else if (Current == State.Requested)
		{
			Current = State.NotRequested;
		}
	}

	public void MarkInitialized(bool successful)
	{
		isRepeatInFlight = false;
		Current = State.Initialized;
		LastCallbackSuccessful = successful;
		storedEligibilityStartFailed = !successful && RequestOrigin == Origin.StoredEligibility;
	}
}
