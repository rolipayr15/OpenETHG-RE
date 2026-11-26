using EyesAnalytics;
using UnityEngine;

public class EyesAnalytics_AppLifecycle : MonoBehaviour
{
	private static bool ownerExists;

	private bool isOwner;

	private void Awake()
	{
		DontDestroyOnLoad(this);
		if (!ownerExists)
		{
			ownerExists = true;
			isOwner = true;
		}
	}

	private void OnDestroy()
	{
		if (isOwner)
		{
			ownerExists = false;
		}
	}

	private void Update()
	{
		if (isOwner)
		{
			Game.CountFrame(Time.unscaledDeltaTime);
		}
	}

	private void OnApplicationPause(bool paused)
	{
		if (isOwner && paused)
		{
			App.Background((int)Time.timeSinceLevelLoad, AttemptPlaceReader.Read());
		}
	}
}
