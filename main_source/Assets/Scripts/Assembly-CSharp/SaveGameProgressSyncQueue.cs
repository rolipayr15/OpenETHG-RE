using System;

public sealed class SaveGameProgressSyncQueue : IDisposable
{
	private sealed class Request
	{
		public string account;

		public Action<bool> completed;

		public bool drained;

		public int generation;
	}

	private readonly Func<string> currentAccount;

	private readonly Action<Action<bool>> synchronizeNative;

	private Request pending;

	private bool inFlight;

	private bool delivering;

	private bool disposed;

	private bool verified;

	private string verifiedAccount;

	private int accountGeneration;

	public SaveGameProgressSyncQueue(Func<string> currentAccount, Action<Action<bool>> synchronizeNative)
	{
		if (currentAccount == null)
		{
			throw new ArgumentNullException("currentAccount");
		}
		if (synchronizeNative == null)
		{
			throw new ArgumentNullException("synchronizeNative");
		}
		this.currentAccount = currentAccount;
		this.synchronizeNative = synchronizeNative;
	}

	public void Synchronize(Action<bool> completed)
	{
		if (disposed)
		{
			completed?.Invoke(false);
			return;
		}
		pending = new Request
		{
			account = currentAccount(),
			completed = completed,
			generation = accountGeneration
		};
	}

	public void InvalidateAccount()
	{
		verified = false;
		accountGeneration++;
	}

	public void Tick()
	{
		if (disposed || inFlight || delivering || pending == null)
		{
			return;
		}
		Request request = pending;
		pending = null;
		if (request.account != currentAccount() || request.generation != accountGeneration)
		{
			Deliver(request, false);
			return;
		}
		inFlight = true;
		bool consumed = false;
		synchronizeNative(delegate(bool success)
		{
			if (consumed)
			{
				return;
			}
			consumed = true;
			inFlight = false;
			if (disposed)
			{
				return;
			}
			if (!success || request.account != currentAccount() || request.generation != accountGeneration)
			{
				Deliver(request, false);
				return;
			}
			// The first sync for an account only drains the native queue: it runs once more before it counts.
			if ((!verified || verifiedAccount != request.account) && !request.drained)
			{
				request.drained = true;
				if (pending == null)
				{
					pending = request;
				}
				return;
			}
			verified = true;
			verifiedAccount = request.account;
			Deliver(request, true);
		});
	}

	private void Deliver(Request request, bool success)
	{
		delivering = true;
		try
		{
			request.completed?.Invoke(success);
		}
		finally
		{
			delivering = false;
		}
	}

	public void Dispose()
	{
		pending = null;
		disposed = true;
	}
}
