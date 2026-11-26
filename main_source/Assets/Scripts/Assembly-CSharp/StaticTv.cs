using Prime31.MessageKit;
using DarkTonic.MasterAudio;
using UnityEngine;

public class StaticTv : MonoBehaviour
{
	[SerializeField]
	private GameObject staticScreen;

	private bool isStaticOn;

	private bool isPlayerDead;

	private bool isGuestDead;

	private float lastCheckTime;

	private float randomTime;

	private void Start()
	{
		MessageKit.addObserver(17, PlayerIsDead);
		MessageKit.addObserver(53, PlayerIsAlive);
		MessageKit.addObserver(92, GuestIsDead);
	}

	private void PlayerIsDead()
	{
		isPlayerDead = true;
	}

	private void PlayerIsAlive()
	{
		isPlayerDead = false;
	}

	private void GuestIsDead()
	{
		isGuestDead = true;
	}

	private void Update()
	{
		if (UserGameSettings.isEnemyOnTheSameFloor && !isPlayerDead && !isGuestDead && CustomSingleton<FloorManager>.Instance.GetPlayerFloor() == 1)
		{
			if (!isStaticOn)
			{
				MasterAudio.PlaySound3DAtTransform("fx", transform, 1f, null, 0f, "static");
				isStaticOn = true;
			}
			if (lastCheckTime + randomTime < Time.time)
			{
				staticScreen.SetActive(Random.Range(0, 2) == 1);
				lastCheckTime = Time.time;
				randomTime = Random.Range(0.02f, 0.03f);
			}
			return;
		}
		if (isStaticOn)
		{
			transform.StopSoundGroupOfTransform("fx");
			isStaticOn = false;
			staticScreen.SetActive(false);
		}
	}
}
