using Bibloteka;
using UnityEngine;

public class RandomExit : CustomSingleton<RandomExit>
{
	private void Start()
	{
		if (!UserGameSettings.IsMultiplayer())
		{
			ActivateExitDoor();
		}
	}

	public string ActivateExitDoor()
	{
		GameObject[] doors = GameObject.FindGameObjectsWithTag("ExitDoor");
		GameObject chosen = doors[Random.Range(0, doors.Length)];
		foreach (GameObject door in doors)
		{
			door.GetComponent<SetRandomExit>().SetExit(door == chosen);
		}
		return Utils.GetId(chosen);
	}

	public void ActivateExitDoorFromHost(string activeExitDoor)
	{
		foreach (GameObject door in GameObject.FindGameObjectsWithTag("ExitDoor"))
		{
			door.GetComponent<SetRandomExit>().SetExit(activeExitDoor == Utils.GetId(door));
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
