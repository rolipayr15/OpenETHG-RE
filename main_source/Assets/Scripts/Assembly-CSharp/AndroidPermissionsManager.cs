using UnityEngine;

public class AndroidPermissionsManager
{
	private static AndroidJavaObject m_Activity;

	private static AndroidJavaObject m_PermissionService;

	private static AndroidJavaObject GetActivity()
	{
		return null;
	}

	private static AndroidJavaObject GetPermissionsService()
	{
		return null;
	}

	public static bool IsPermissionGranted(string permissionName)
	{
		return false;
	}

	public static void RequestPermission(string permissionName, AndroidPermissionCallback callback)
	{
	}

	public static void RequestPermission(string[] permissionNames, AndroidPermissionCallback callback)
	{
	}
}
