using UnityEngine;
using VoxelBusters.EssentialKit;

public class NetworkStatusCheck : MonoBehaviour
{
	private bool isHostReachable;

	private bool isInternetActive;

	private bool isNotifierActive;

	public bool IsInternetActive => false;

	public bool IsHostReachable => false;

	public bool IsNotifierActive => false;

	private void Start()
	{
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	private void OnInternetConnectivityChange(NetworkServicesInternetConnectivityStatusChangeResult result)
	{
	}

	private void OnHostReachabilityChange(NetworkServicesHostReachabilityStatusChangeResult result)
	{
	}
}
