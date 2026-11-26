using Firebase.Crashlytics;
using UnityEngine;

public class InternetConnectionMonitor : CustomSingleton<InternetConnectionMonitor>
{
	private bool isInternetAvailable = true;

	private float lastTimeChecked;

	private const float CheckInterval = 15f;

	private const float MinCheckInterval = 2f;

	private void Awake()
	{
		if (Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		DontDestroyOnLoad(gameObject);
		CheckInternetConnection(true);
	}

	public void CheckInternetConnection(bool force = false)
	{
		if (!force && Time.realtimeSinceStartup - lastTimeChecked < MinCheckInterval)
		{
			return;
		}
		lastTimeChecked = Time.realtimeSinceStartup;
		bool available = Application.internetReachability != NetworkReachability.NotReachable;
		if (isInternetAvailable == available)
		{
			return;
		}
		isInternetAvailable = available;
		if (FirebaseState.IsInitialized)
		{
			Crashlytics.Log("Internet connection changed: " + (isInternetAvailable ? "AVAILABLE" : "UNAVAILABLE") + ", " + string.Format("reachability={0}", Application.internetReachability));
		}
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (hasFocus)
		{
			CheckInternetConnection();
		}
	}

	private void Update()
	{
		if (Time.realtimeSinceStartup - lastTimeChecked >= CheckInterval)
		{
			CheckInternetConnection(true);
		}
	}

	public bool IsInternetConnectionAvailable()
	{
		return isInternetAvailable;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}
