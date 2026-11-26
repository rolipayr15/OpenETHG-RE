using VoxelBusters.EssentialKit;
using System.Collections;
using System;
using System.Collections.Concurrent;
using UnityEngine;

public sealed class LegacySnapshotReader : ILegacySnapshotReader, IDisposable
{
	private sealed class AndroidTransport : ILegacySnapshotTransport, IDisposable
	{
		private sealed class Callback : AndroidJavaProxy
		{
			private readonly Action<string> done;

			public Callback(Action<string> done)
				: base("com.eyes.saves.LegacySaveReader$Listener")
			{
				this.done = done;
			}

			public void onResult(string json)
			{
				done?.Invoke(json);
			}
		}

		private AndroidJavaObject request;

		private Callback callback;

		public void Read(string expectedPlayerId, Action<string> done)
		{
			ReleaseRequest();
			callback = new Callback(done);
			using (AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			using (AndroidJavaObject activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
			using (AndroidJavaClass reader = new AndroidJavaClass("com.eyes.saves.LegacySaveReader"))
			{
				if (activity == null)
				{
					throw new InvalidOperationException("Android activity is unavailable.");
				}
				request = reader.CallStatic<AndroidJavaObject>("read", new object[] { activity, expectedPlayerId, callback });
			}
		}

		private void ReleaseRequest()
		{
			if (request != null)
			{
				try
				{
					request.Call("cancel");
				}
				finally
				{
					request.Dispose();
					request = null;
				}
			}
			callback = null;
		}

		public void Dispose()
		{
			ReleaseRequest();
		}
	}

	public const int MaxSnapshots = 32;

	public const int MaxSnapshotBytes = 1048576;

	public const int MaxTotalBytes = 4194304;

	public const double TimeoutSeconds = 125.0;

	private const string AccountPrefix = "playgames:";

	private const int MaxResponseCharacters = 6291456;

	private const int MaxEncodedCharacters = 1398104;

	private readonly ILegacySnapshotTransport transport;

	private readonly Func<string> currentAccount;

	private readonly Func<double> clock;

	private readonly ConcurrentQueue<Action> callbacks;

	private Action<LegacySnapshotReadResult> completion;

	private string requestedAccount;

	private double deadline;

	private int generation;

	private bool disposed;

	public LegacySnapshotReader()
		: this(CreateTransport(), CurrentAccount, () => Time.realtimeSinceStartupAsDouble)
	{
	}

	public LegacySnapshotReader(ILegacySnapshotTransport transport, Func<string> currentAccount, Func<double> clock)
	{
		callbacks = new ConcurrentQueue<Action>();
		this.transport = transport;
		this.currentAccount = currentAccount ?? throw new ArgumentNullException("currentAccount");
		this.clock = clock ?? throw new ArgumentNullException("clock");
	}

	public void Read(string account, Action<LegacySnapshotReadResult> done)
	{
		if (done == null)
		{
			throw new ArgumentNullException("done");
		}
		if (disposed)
		{
			return;
		}
		if (completion != null)
		{
			callbacks.Enqueue(delegate
			{
				done(Failure(account, "Legacy read already in progress."));
			});
			return;
		}
		if (transport == null)
		{
			// No reader on this platform: nothing legacy can exist, so the read is complete and empty.
			callbacks.Enqueue(delegate
			{
				done(new LegacySnapshotReadResult
				{
					account = account,
					complete = true,
					ownershipVerified = true
				});
			});
			return;
		}
		if (string.IsNullOrEmpty(account) || !account.StartsWith(AccountPrefix, StringComparison.Ordinal) || account.Length > 4096 || account.Length == AccountPrefix.Length || currentAccount() != account)
		{
			callbacks.Enqueue(delegate
			{
				done(Failure(account, "Cloud account is unavailable or changed."));
			});
			return;
		}
		requestedAccount = account;
		completion = done;
		deadline = clock() + TimeoutSeconds;
		int request = ++generation;
		try
		{
			transport.Read(account.Substring(AccountPrefix.Length), delegate(string json)
			{
				// The Java listener calls back on its own thread; results are handled in Tick.
				callbacks.Enqueue(delegate
				{
					Complete(request, json);
				});
			});
		}
		catch (Exception exception)
		{
			callbacks.Enqueue(delegate
			{
				Fail(request, "Legacy reader could not start: " + exception.GetType().Name);
			});
		}
	}

	public void Tick()
	{
		if (disposed)
		{
			return;
		}
		if (completion != null)
		{
			if (currentAccount() != requestedAccount)
			{
				Fail(generation, "Cloud account changed during legacy read.");
			}
			if (completion != null && deadline <= clock())
			{
				Fail(generation, "Legacy snapshot read timed out.");
			}
		}
		while (!disposed && callbacks.TryDequeue(out Action callback))
		{
			callback();
		}
	}

	private void Complete(int request, string json)
	{
		if (disposed || generation != request || completion == null)
		{
			return;
		}
		if (currentAccount() != requestedAccount)
		{
			Fail(request, "Cloud account changed during legacy read.");
		}
		else
		{
			Finish(ParseResponse(requestedAccount, json));
		}
	}

	private void Fail(int request, string error)
	{
		if (!disposed && generation == request && completion != null)
		{
			Finish(Failure(requestedAccount, error));
		}
	}

	private void Finish(LegacySnapshotReadResult result)
	{
		Action<LegacySnapshotReadResult> done = completion;
		completion = null;
		requestedAccount = null;
		generation++;
		done(result);
	}

	private static LegacySnapshotReadResult Failure(string account, string error)
	{
		return new LegacySnapshotReadResult
		{
			account = account,
			error = error
		};
	}

	private static LegacySnapshotReadResult ParseResponse(string account, string json)
	{
		if (!SaveGameProgressData.TryReadJsonObject(json, out IDictionary document, out string error, MaxResponseCharacters))
		{
			return Failure(account, error);
		}
		if (!(document["playerId"] is string player) || account != AccountPrefix + player || !(document["complete"] is bool complete) || !(document["ownershipVerified"] is bool verified))
		{
			return Failure(account, "Legacy response has no matching owner.");
		}
		LegacySnapshotReadResult result = new LegacySnapshotReadResult
		{
			account = account,
			ownershipVerified = verified,
			complete = verified && complete,
			error = document["error"] as string
		};
		if (!verified)
		{
			return result;
		}
		if (!(document["snapshots"] is IList snapshots) || snapshots.Count > MaxSnapshots)
		{
			return Failure(account, "Legacy snapshot list exceeds its limit or is missing.");
		}
		long total = 0L;
		foreach (object item in snapshots)
		{
			IDictionary entry = item as IDictionary;
			string name = entry?["name"] as string;
			string base64 = entry?["base64"] as string;
			if (string.IsNullOrWhiteSpace(name) || name.Length > 256 || name.StartsWith("cpnp-snapshot-", StringComparison.Ordinal) || base64 == null || base64.Length > MaxEncodedCharacters)
			{
				return Failure(account, "Invalid legacy snapshot entry.");
			}
			foreach (char c in name)
			{
				if (char.IsControl(c))
				{
					return Failure(account, "Invalid legacy snapshot name.");
				}
			}
			int length;
			try
			{
				length = Convert.FromBase64String(base64).Length;
			}
			catch (FormatException)
			{
				return Failure(account, "Invalid legacy snapshot entry.");
			}
			total += length;
			if (length > MaxSnapshotBytes || total > MaxTotalBytes)
			{
				return Failure(account, "Legacy snapshot data exceeds its size limit.");
			}
			result.snapshots.Add(new LegacySnapshotPayload
			{
				name = name,
				base64 = base64
			});
		}
		return result;
	}

	public void Dispose()
	{
		if (disposed)
		{
			return;
		}
		completion = null;
		disposed = true;
		generation++;
		while (callbacks.TryDequeue(out Action _))
		{
		}
		transport?.Dispose();
	}

	private static string CurrentAccount()
	{
		var user = CloudServices.ActiveUser;
		if (user == null || user.AccountStatus != CloudUserAccountStatus.Available || string.IsNullOrEmpty(user.UserId))
		{
			return null;
		}
		return AccountPrefix + user.UserId;
	}

	private static ILegacySnapshotTransport CreateTransport()
	{
		#if UNITY_ANDROID && !UNITY_EDITOR
		return new AndroidTransport();
		#else
		return null;
		#endif
	}
}
