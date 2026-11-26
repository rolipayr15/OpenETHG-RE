using System;
using UnityEngine;
using UnityEngine.Scripting;

public sealed class AndroidResultImageExport : MonoBehaviour
{
	public enum Outcome
	{
		Saved = 0,
		Cancelled = 1,
		Error = 2
	}

	[Preserve]
	private sealed class SaveCallback : AndroidJavaProxy
	{
		private readonly AndroidResultImageExport owner;

		public SaveCallback(AndroidResultImageExport owner)
			: base("com.eyes.sharing.ResultImageExport$SaveCallback")
		{
			this.owner = owner;
		}

		[Preserve]
		public void onComplete(string outcome, string error)
		{
			owner.ReceiveCompletion(outcome, error);
		}
	}

	private const string NativeClass = "com.eyes.sharing.ResultImageExport";

	private readonly object completionLock = new object();

	private Action<Outcome, string> completion;

	private bool completionPending;

	private bool completed;

	private Outcome pendingOutcome;

	private string pendingError;

	private SaveCallback nativeCallback;

	public static bool CanShareImage()
	{
		using (AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
		using (AndroidJavaObject activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
		using (AndroidJavaClass native = new AndroidJavaClass(NativeClass))
		{
			return native.CallStatic<bool>("canShareImage", new object[] { activity });
		}
	}

	public static void Save(byte[] png, string fileName, Action<Outcome, string> callback)
	{
		if (png == null || png.Length == 0)
		{
			throw new ArgumentException("The PNG is empty.", "png");
		}
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		AndroidResultImageExport export = new GameObject("Result image export").AddComponent<AndroidResultImageExport>();
		DontDestroyOnLoad(export.gameObject);
		export.completion = callback;
		export.BeginSave(png, fileName);
	}

	private void BeginSave(byte[] png, string fileName)
	{
		nativeCallback = new SaveCallback(this);
		sbyte[] data = new sbyte[png.Length];
		Buffer.BlockCopy(png, 0, data, 0, png.Length);
		using (AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
		using (AndroidJavaObject activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
		using (AndroidJavaClass native = new AndroidJavaClass(NativeClass))
		{
			native.CallStatic("saveImage", new object[] { activity, data, fileName, nativeCallback });
		}
	}

	private void ReceiveCompletion(string outcome, string error)
	{
		lock (completionLock)
		{
			if (!completionPending && !completed)
			{
				pendingOutcome = outcome == "saved" ? Outcome.Saved : (outcome == "cancelled" ? Outcome.Cancelled : Outcome.Error);
				pendingError = error;
				completionPending = true;
			}
		}
	}

	private void Update()
	{
		Outcome outcome;
		string error;
		lock (completionLock)
		{
			if (!completionPending || completed)
			{
				return;
			}
			outcome = pendingOutcome;
			error = pendingError;
			completed = true;
		}
		try
		{
			completion?.Invoke(outcome, error);
		}
		finally
		{
			completion = null;
			nativeCallback = null;
			Destroy(gameObject);
		}
	}
}
