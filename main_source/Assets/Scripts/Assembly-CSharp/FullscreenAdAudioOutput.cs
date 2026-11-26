using UnityEngine;

// Stops the Unity audio output while a fullscreen ad plays and restarts it afterwards, with backoff retries.
public sealed class FullscreenAdAudioOutput
{
	public interface IOutput
	{
		bool IsStarted { get; }

		void Stop();

		void Start();
	}

	public enum StopResult
	{
		NotNeeded = 0,
		Stopped = 1,
		Failed = 2
	}

	public enum RestoreResult
	{
		NotAttempted = 0,
		Restarted = 1,
		Failed = 2,
		Exhausted = 3
	}

	public const float RetryInterval = 0.25f;

	public const int MaxAttempts = 7;

	private readonly IOutput output;

	private int earliestFrame;

	private float nextAttemptTime;

	public bool DidStop { get; private set; }

	public bool IsRestorePending { get; private set; }

	public bool WaitsForForeground { get; private set; }

	public bool RetriesExhausted { get; private set; }

	public int AttemptCount { get; private set; }

	public bool IsStoppedForAd => DidStop && !output.IsStarted;

	public FullscreenAdAudioOutput(IOutput output)
	{
		this.output = output;
	}

	public StopResult StopForAd()
	{
		if (DidStop || !output.IsStarted)
		{
			return StopResult.NotNeeded;
		}
		output.Stop();
		if (output.IsStarted)
		{
			return StopResult.Failed;
		}
		nextAttemptTime = 0f;
		DidStop = true;
		IsRestorePending = false;
		WaitsForForeground = false;
		RetriesExhausted = false;
		AttemptCount = 0;
		return StopResult.Stopped;
	}

	public void RequestRestore(bool waitForForeground, int frame)
	{
		if (!DidStop)
		{
			return;
		}
		RetriesExhausted = false;
		IsRestorePending = true;
		WaitsForForeground = waitForForeground;
		AttemptCount = 0;
		earliestFrame = frame + 1;
		nextAttemptTime = 0f;
	}

	public void OnReturnedToForeground(int frame)
	{
		if (!IsRestorePending)
		{
			return;
		}
		WaitsForForeground = false;
		RetriesExhausted = false;
		AttemptCount = 0;
		earliestFrame = frame + 1;
		nextAttemptTime = 0f;
	}

	public RestoreResult TryRestore(bool canRestore, int frame, float now)
	{
		if (!IsRestorePending || WaitsForForeground || !canRestore || frame < earliestFrame || now < nextAttemptTime)
		{
			return RestoreResult.NotAttempted;
		}
		if (!output.IsStarted)
		{
			output.Start();
		}
		if (output.IsStarted)
		{
			DidStop = false;
			IsRestorePending = false;
			WaitsForForeground = false;
			RetriesExhausted = false;
			return RestoreResult.Restarted;
		}
		AttemptCount++;
		if (AttemptCount < MaxAttempts)
		{
			nextAttemptTime = RetryDelay(AttemptCount) + now;
			return RestoreResult.Failed;
		}
		WaitsForForeground = true;
		RetriesExhausted = true;
		return RestoreResult.Exhausted;
	}

	public static float RetryDelay(int failedAttempts)
	{
		int exponent = Mathf.Max(0, Mathf.Min(failedAttempts - 1, 10));
		return (1 << exponent) * RetryInterval;
	}

	public static float RetryCycleSeconds()
	{
		float total = 0f;
		for (int attempt = 1; attempt < MaxAttempts; attempt++)
		{
			total += RetryDelay(attempt);
		}
		return total;
	}
}
