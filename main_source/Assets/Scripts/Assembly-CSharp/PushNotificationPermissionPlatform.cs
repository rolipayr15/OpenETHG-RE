using System.Threading.Tasks;

public static class PushNotificationPermissionPlatform
{
	private const string AndroidPermission = "android.permission.POST_NOTIFICATIONS";

	public static bool IsSupported => false;

	public static bool IsReady => false;

	public static Task<PushPermissionStatus> QueryAsync()
	{
		return null;
	}

	public static Task RequestAsync()
	{
		return null;
	}

	private static PushPermissionStatus QueryAndroid()
	{
		return PushPermissionStatus.Unknown;
	}
}
