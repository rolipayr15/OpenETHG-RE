using Firebase;
using Firebase.Crashlytics;
using Firebase.Extensions;
using UnityEngine;

public class CrashlyticsInit : MonoBehaviour
{
	public static FirebaseApp app;

	private void Awake()
	{
		FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
		{
			if (task.IsCanceled)
			{
				FirebaseState.IsInitialized = false;
				Debug.LogWarning("Firebase dependency check was canceled.");
				return;
			}
			if (task.IsFaulted)
			{
				FirebaseState.IsInitialized = false;
				Debug.LogError("Firebase dependency check failed: " + task.Exception.GetBaseException());
				return;
			}
			if (task.Result == DependencyStatus.Available)
			{
				app = FirebaseApp.DefaultInstance;
				Crashlytics.ReportUncaughtExceptionsAsFatal = true;
				FirebaseState.IsInitialized = true;
				RemoteConfigWrapper.InitializeAsync();
				FirebasePushNotifications.Initialize();
				Debug.Log("Firebase initialized");
			}
			else
			{
				FirebaseState.IsInitialized = false;
				Debug.LogError(string.Format("Firebase dependencies unavailable: {0}", task.Result));
			}
		});
	}
}
