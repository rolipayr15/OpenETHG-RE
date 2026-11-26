using Bibloteka;
using Prime31.MessageKit;
using System.Globalization;
using System.Collections.Generic;
using Characters;
using UnityEngine;

public class WitchTotemManager : CustomSingleton<WitchTotemManager>
{
	private GameObject[] totems;

	private int[] maxTotemsPerFloor;

	private int[] activeTotemsPerFloor = new int[3];

	private bool wasInitialized;

	public EnemyTarget WitchTarget { get; set; }

	private void Start()
	{
		InitTotems();
		if (!UserGameSettings.IsMultiplayer())
		{
			ActivateTotems();
		}
	}

	private void InitTotems()
	{
		if (wasInitialized)
		{
			return;
		}
		wasInitialized = true;
		if (totems == null)
		{
			totems = GameObject.FindGameObjectsWithTag("WitchTotem");
		}
		foreach (GameObject totem in totems)
		{
			totem.SetActive(false);
		}
	}

	public List<string> ActivateTotems()
	{
		InitTotems();
		List<string> activated = new List<string>();
		if ((int)UserGameSettings.currentEnemy != 6)
		{
			return activated;
		}
		switch ((int)UserGameSettings.currentDifficulty)
		{
		case 0:
		case 1:
			maxTotemsPerFloor = new int[] { 4, 1, 4 };
			break;
		case 2:
			maxTotemsPerFloor = new int[] { 6, 2, 6 };
			break;
		case 3:
			maxTotemsPerFloor = new int[] { 7, 2, 8 };
			break;
		case 4:
			maxTotemsPerFloor = new int[] { 8, 3, 10 };
			break;
		}
		totems.Shuffle();
		foreach (GameObject totem in totems)
		{
			int floor = CustomSingleton<FloorManager>.Instance.GetFloor(totem.transform.position);
			if (activeTotemsPerFloor[floor] < maxTotemsPerFloor[floor])
			{
				totem.SetActive(true);
				activeTotemsPerFloor[floor]++;
				activated.Add(totem.transform.position.sqrMagnitude.ToString(CultureInfo.InvariantCulture));
			}
		}
		return activated;
	}

	public void ActivateTotemsFromHost(List<string> totemsToActivate)
	{
		InitTotems();
		foreach (GameObject totem in totems)
		{
			if (totemsToActivate.Contains(totem.transform.position.sqrMagnitude.ToString(CultureInfo.InvariantCulture)))
			{
				totem.SetActive(true);
			}
		}
	}

	private int ChooseTotemFloor(int floor1, int floor2)
	{
		return TotemsAvailableToActivate(floor1) <= TotemsAvailableToActivate(floor2) ? floor2 : floor1;
	}

	private void ActivateNewRandomTotem(int currentPlayersFloor)
	{
		switch (currentPlayersFloor)
		{
		case 0:
			ActivateTotemOnFloor(TotemsAvailableToActivate(1) <= TotemsAvailableToActivate(2) ? 2 : 1);
			break;
		case 1:
			ActivateTotemOnFloor(TotemsAvailableToActivate(0) <= TotemsAvailableToActivate(2) ? 2 : 0);
			break;
		case 2:
			ActivateTotemOnFloor(TotemsAvailableToActivate(0) <= TotemsAvailableToActivate(1) ? 1 : 0);
			break;
		}
	}

	private void ActivateNewTotem()
	{
		int hostFloor = CustomSingleton<FloorManager>.Instance.GetPlayerFloor();
		if (!UserGameSettings.IsMultiplayer())
		{
			ActivateNewRandomTotem(hostFloor);
			return;
		}
		if (!UserGameSettings.isMultiplayerHost)
		{
			return;
		}
		int guestFloor = CustomSingleton<FloorManager>.Instance.GetFloor(CustomSingleton<GuestManager>.Instance.Avatar.transform.position);
		if (hostFloor == guestFloor)
		{
			ActivateNewRandomTotem(hostFloor);
			return;
		}
		int third = 3 - hostFloor - guestFloor;
		ActivateTotemOnFloor(TotemsAvailableToActivate(third) < 1 ? ChooseTotemFloor(hostFloor, guestFloor) : third);
	}

	private void ActivateTotemOnFloor(int floor)
	{
		foreach (GameObject totem in totems)
		{
			if (!totem.activeInHierarchy && CustomSingleton<FloorManager>.Instance.GetFloor(totem.transform.position) == floor)
			{
				totem.SetActive(true);
				activeTotemsPerFloor[floor]++;
				MessageKit<string>.post(113, totem.transform.position.sqrMagnitude.ToString(CultureInfo.InvariantCulture));
				break;
			}
		}
	}

	private int TotemsAvailableToActivate(int floor)
	{
		return maxTotemsPerFloor[floor] - activeTotemsPerFloor[floor];
	}

	public void NotifyOfTotemDeactivation(GameObject totem)
	{
		activeTotemsPerFloor[CustomSingleton<FloorManager>.Instance.GetFloor(totem.transform.position)]--;
		ActivateNewTotem();
	}

	public void ActivateNewTotem(string id, bool status)
	{
		foreach (GameObject totem in totems)
		{
			if (totem.transform.position.sqrMagnitude.ToString(CultureInfo.InvariantCulture) == id)
			{
				totem.SetActive(status);
				activeTotemsPerFloor[CustomSingleton<FloorManager>.Instance.GetFloor(totem.transform.position)]--;
				return;
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
