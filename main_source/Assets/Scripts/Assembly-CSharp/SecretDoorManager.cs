using Bibloteka;
using UnityEngine;

public class SecretDoorManager : CustomSingleton<SecretDoorManager>
{
	private GameObject activeSaw;

	private void Start()
	{
		if (!UserGameSettings.IsMultiplayer())
		{
			ActivateSaw();
		}
	}

	public string ActivateSaw()
	{
		GameObject[] saws = GameObject.FindGameObjectsWithTag("Saw");
		foreach (GameObject saw in saws)
		{
			saw.SetActive(false);
		}
		Utils.Shuffle(saws);
		activeSaw = saws[0];
		activeSaw.SetActive(true);
		return Utils.GetId(activeSaw);
	}

	public void ActivateSawFromHost(string activeSaw)
	{
		foreach (GameObject saw in GameObject.FindGameObjectsWithTag("Saw"))
		{
			if (activeSaw != Utils.GetId(saw))
			{
				saw.SetActive(false);
			}
		}
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
