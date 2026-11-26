using DarkTonic.MasterAudio;
using Prime31.MessageKit;
using UnityEngine;

public class ShakingItem : MonoBehaviour
{
	private bool isInInitialPosition = true;

	private Quaternion initialRotation;

	private Vector3 initialPosition;

	private FloorManager floorManager;

	private int itemFloor;

	private bool isFloorDataInitialized;

	private static MasterAudioGroup closeShakingGroup;

	private static MasterAudioGroup farShakingGroup;

	private static int closeShakingTotalVoices;

	private static int farShakingTotalVoices;

	private bool isPlayerDead;

	private bool isGuestDead;

	private float randomTime;

	private float lastCheckTime;

	private void Start()
	{
		iTween.Init(base.gameObject);
		initialRotation = base.gameObject.transform.rotation;
		initialPosition = base.gameObject.transform.position;
		if (closeShakingGroup == null)
		{
			closeShakingGroup = MasterAudio.GrabGroup("shakingClose");
			closeShakingTotalVoices = (closeShakingGroup != null) ? closeShakingGroup.TotalVoices : 0;
		}
		if (farShakingGroup == null)
		{
			farShakingGroup = MasterAudio.GrabGroup("shakingFar");
			farShakingTotalVoices = (farShakingGroup != null) ? farShakingGroup.TotalVoices : 0;
		}
		MessageKit.addObserver(17, PlayerIsDead);
		MessageKit.addObserver(53, PlayerIsAlive);
		MessageKit.addObserver(92, GuestIsDead);
	}

	private void PlayerIsDead()
	{
		isPlayerDead = true;
		MasterAudio.MuteGroup("shakingClose", false);
		MasterAudio.MuteGroup("shakingFar", false);
	}

	private void PlayerIsAlive()
	{
		isPlayerDead = false;
		MasterAudio.UnmuteGroup("shakingClose", false);
		MasterAudio.UnmuteGroup("shakingFar", false);
	}

	private void GuestIsDead()
	{
		isGuestDead = true;
	}

	private bool TryInitializeFloorData()
	{
		if (isFloorDataInitialized)
		{
			return true;
		}
		if (!FloorManager.TryGetInitializedSceneInstance(out floorManager))
		{
			return false;
		}
		itemFloor = floorManager.GetFloor(initialPosition);
		isFloorDataInitialized = true;
		return true;
	}

	private void Update()
	{
		if (!TryInitializeFloorData())
		{
			return;
		}
		if (UserGameSettings.isEnemyOnTheSameFloor && !isPlayerDead && !isGuestDead && floorManager.GetPlayerFloor() == itemFloor)
		{
			isInInitialPosition = false;
			if (Time.time > lastCheckTime + randomTime)
			{
				randomTime = (float)Random.Range(15, 25) / 10f;
				iTween.ShakeRotation(base.gameObject, iTween.Hash("x", 1f, "y", 10f, "z", 1f, "time", randomTime, "space", Space.World));
				MasterAudio.PlaySound3DAtTransform(floorManager.IsEnemyClose() ? "shakingClose" : "shakingFar", base.transform);
				lastCheckTime = Time.time;
			}
		}
		else if (!isInInitialPosition && Time.time > lastCheckTime + randomTime)
		{
			base.gameObject.transform.rotation = initialRotation;
			base.gameObject.transform.position = initialPosition;
			isInInitialPosition = true;
		}
	}

	private void PlayShakingCloseIfVoicesAvailable()
	{
		if (closeShakingGroup.ActiveVoices < closeShakingTotalVoices)
		{
			MasterAudio.PlaySound3DAtTransform("shakingClose", base.transform);
		}
	}

	private void PlayShakingFarIfVoicesAvailable()
	{
		if (farShakingGroup.ActiveVoices < farShakingTotalVoices)
		{
			MasterAudio.PlaySound3DAtTransform("shakingFar", base.transform);
		}
	}
}
