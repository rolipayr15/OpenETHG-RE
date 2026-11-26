using UnityEngine;

public sealed class SaveGameProgressPump : MonoBehaviour
{
	private static SaveGameProgressPump instance;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Reset()
	{
		instance = null;
	}

	public static void EnsureRunning()
	{
		if (instance != null)
		{
			return;
		}
		GameObject pump = new GameObject("SaveGameProgressPump");
		instance = pump.AddComponent<SaveGameProgressPump>();
		DontDestroyOnLoad(pump);
	}

	private void Update()
	{
		SaveGameProgress.Tick();
	}

	private void OnApplicationPause(bool paused)
	{
		if (paused)
		{
			SaveGameProgress.FlushLocal();
		}
	}

	private void OnApplicationQuit()
	{
		SaveGameProgress.FlushLocal();
		SaveGameProgress.Shutdown();
	}

	private void OnDestroy()
	{
		if (instance == this)
		{
			instance = null;
		}
	}
}
