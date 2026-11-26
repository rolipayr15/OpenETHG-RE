using UnityEngine;

public class GuestManager : CustomSingleton<GuestManager>
{
	public GameObject Heart;

	public GameObject Avatar;

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}
