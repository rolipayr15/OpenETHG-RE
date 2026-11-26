using System;

// One rewarded ad offer: the callback fires exactly once, with whether the reward was earned.
public sealed class RewardedAdRequest
{
	private Action<bool> callback;

	public bool RewardEarned { get; private set; }

	public float RewardTime { get; private set; }

	public bool IsCompleted => callback == null;

	public RewardedAdRequest(Action<bool> callback)
	{
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		this.callback = callback;
	}

	public void ReportReward(float time)
	{
		if (callback == null || RewardEarned)
		{
			return;
		}
		RewardTime = time;
		RewardEarned = true;
	}

	public void Complete()
	{
		Finish(RewardEarned);
	}

	public void Fail()
	{
		Finish(false);
	}

	private void Finish(bool rewarded)
	{
		Action<bool> finished = callback;
		if (finished != null)
		{
			callback = null;
			finished(rewarded);
		}
	}
}
