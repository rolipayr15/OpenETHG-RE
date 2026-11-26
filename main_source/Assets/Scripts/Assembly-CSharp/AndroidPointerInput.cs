using System;
using UnityEngine;

[DefaultExecutionOrder(-32100)]
public sealed class AndroidPointerInput : MonoBehaviour
{
	private sealed class JavaTransport : AndroidPointerBackend.ITransport, IDisposable
	{
		private AndroidJavaObject peer;

		public double[] Poll()
		{
			return null;
		}

		public void SetActive(bool active)
		{
		}

		public void RequestCapture(int generation)
		{
		}

		public void ReleaseCapture(int generation)
		{
		}

		public void ResetInput()
		{
		}

		public void Dispose()
		{
		}
	}

	private static AndroidPointerInput instance;

	private AndroidPointerBackend backend;

	private NativePointerInput bridge;

	private ControlModeManager controls;

	private int lossVersion;

	private bool warned;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void ResetStatic()
	{
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Bootstrap()
	{
	}

	private void Awake()
	{
	}

	private void OnEnable()
	{
	}

	private void Update()
	{
	}

	private void OnModeChanged(ControlMode mode)
	{
	}

	private void OnRewiredInitialized()
	{
	}

	private void UnbindRewired()
	{
	}

	private void Pump()
	{
	}

	private void OnApplicationFocus(bool focus)
	{
	}

	private void OnApplicationPause(bool paused)
	{
	}

	private void WarnUnavailable()
	{
	}

	private void Cleanup()
	{
	}

	private void OnDisable()
	{
	}

	private void OnDestroy()
	{
	}
}
