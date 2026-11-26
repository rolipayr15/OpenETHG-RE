using UnityEngine;

public class AndroidPermissionsUsageExample : MonoBehaviour
{
	private const string STORAGE_PERMISSION = "android.permission.READ_EXTERNAL_STORAGE";

	public void OnBrowseGalleryButtonPress()
	{
	}

	private bool CheckPermissions()
	{
		return false;
	}

	public void OnGrantButtonPress()
	{
	}
}
