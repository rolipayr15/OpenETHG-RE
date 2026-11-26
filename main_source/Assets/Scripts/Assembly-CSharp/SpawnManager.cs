using UnityEngine;

public class SpawnManager : CustomSingleton<SpawnManager>
{
	[SerializeField]
	private GameObject host;

	[SerializeField]
	private GameObject guest;

	[SerializeField]
	private Location chosenLocation;

	public void Awake()
	{
		if (!UserGameSettings.IsMultiplayer())
		{
			return;
		}
		if (!UserGameSettings.isMultiplayerHost)
		{
			GameObject swap = guest;
			guest = host;
			host = swap;
		}
		host.transform.position = chosenLocation.hostPosition.transform.position;
		host.transform.rotation = chosenLocation.hostPosition.transform.rotation;
		guest.transform.position = chosenLocation.guestPosition.transform.position;
		guest.transform.rotation = chosenLocation.guestPosition.transform.rotation;
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
