using System;
using UnityEngine;

// Numbers fullscreen ad sessions and notifies listeners; one faulty listener cannot block the others.
public static class FullscreenAdSessions
{
	public static bool IsShowing { get; private set; }

	public static int Current { get; private set; }

	public static event Action<int> Started;

	public static event Action<int> Ended;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void ResetStatic()
	{
		IsShowing = false;
		Current = 0;
		Started = null;
		Ended = null;
	}

	public static void Report(bool showing)
	{
		if (IsShowing == showing)
		{
			return;
		}
		IsShowing = showing;
		if (showing)
		{
			Current++;
			Raise(Started, Current);
		}
		else
		{
			Raise(Ended, Current);
		}
	}

	private static void Raise(Action<int> handlers, int session)
	{
		if (handlers == null)
		{
			return;
		}
		foreach (Delegate handler in handlers.GetInvocationList())
		{
			try
			{
				((Action<int>)handler)(session);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}
}
