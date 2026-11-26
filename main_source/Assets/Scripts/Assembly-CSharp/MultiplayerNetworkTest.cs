using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;

// The ARM build pinged every backend region over websockets through WsBestHttpRouteTester; the restored
// server has a single address, so the gate is one health request. The public surface is the original one.
public static class MultiplayerNetworkTest
{
	public const int PingCount = 11;

	public const int AutomaticPingCount = 5;

	private const int PassedLifetimeSeconds = 3600;

	private const int FailedLifetimeSeconds = 5;

	private static long lastRun;

	private static bool passed;

	public static bool IsRunning { get; private set; }

	public static float Progress { get; private set; }

	public static bool HasFreshResult
	{
		get
		{
			long age = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - lastRun;
			return lastRun > 0 && age >= 0 && age < (passed ? PassedLifetimeSeconds : FailedLifetimeSeconds);
		}
	}

	public static bool CanPlay => !IsRunning && HasFreshResult && passed;

	public static event Action Changed;

	public static void Invalidate()
	{
		lastRun = 0L;
		passed = false;
		Changed?.Invoke();
	}

	public static bool IsFresh(string timestamp, DateTimeOffset now)
	{
		if (long.TryParse(timestamp, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out long seconds) && seconds > 0)
		{
			return (ulong)(now.ToUnixTimeSeconds() - seconds) < 3601UL;
		}
		return false;
	}

	public static WsTestResult FindHealthyRoute(IReadOnlyList<WsTestResult> rankedResults, int pingCount = 11)
	{
		return null;
	}

	public static async Task EnsureFreshAsync(WsBestHttpRouteTester tester, CancellationToken ct)
	{
		if (HasFreshResult || IsRunning)
		{
			return;
		}
		IsRunning = true;
		Progress = 0f;
		Changed?.Invoke();
		bool ok = false;
		using (UnityWebRequest request = UnityWebRequest.Get(ApiConfiguration.GetBackendHttpUrl("/api/v1/ping")))
		{
			request.timeout = 8;
			if (BestHTTP.OfficialServerPin.Applies(request.uri))
			{
				request.certificateHandler = new BestHTTP.OfficialServerPinHandler();
			}
			UnityWebRequestAsyncOperation operation = request.SendWebRequest();
			while (!operation.isDone)
			{
				if (ct.IsCancellationRequested)
				{
					request.Abort();
					IsRunning = false;
					throw new OperationCanceledException(ct);
				}
				Progress = operation.progress;
				await Task.Yield();
			}
			ok = request.result == UnityWebRequest.Result.Success || request.result == UnityWebRequest.Result.ProtocolError;
			if (ok && OnlineConfig.IsCustomServer)
			{
				OnlineConfig.ApplyServerRules(request.downloadHandler.text);
			}
			if (!ok)
			{
				UnityEngine.Debug.LogWarning("[Online] server check failed: " + request.error);
			}
		}
		passed = ok;
		lastRun = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		IsRunning = false;
		Progress = 1f;
		Changed?.Invoke();
	}

	public static async Task<List<WsTestResult>> RunAsync(WsBestHttpRouteTester tester, CancellationToken ct, int pingCount = 11, bool extended = false)
	{
		lastRun = 0L;
		await EnsureFreshAsync(tester, ct);
		return new List<WsTestResult>();
	}
}
