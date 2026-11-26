using Eyes.DailyChallenges;
using Firebase.Crashlytics;
using EyesAnalytics;
using Bibloteka;
using DarkTonic.MasterAudio;
using Prime31.MessageKit;
using System.Collections.Generic;
using UnityEngine;

public class EyesManager : CustomSingleton<EyesManager>
{
	private int eyesAvailable;

	private GameObject[][] eyes;

	private int eyesToCollect = 12;

	private int eyesCollected;

	private int potionReward = 5;

	private int adReward = 3;

	private bool firstUseOfEye = true;

	private int eyesCounter;

	public IReadOnlyCollection<int> ActivatedEyeIds { get; private set; }

	private void Awake()
	{
		MessageKit.addObserver(1, AddEye);
		MessageKit.addObserver(16, ExtraEyesEffect);
		if (UserGameSettings.IsMultiplayer() && UserGameSettings.IsMultiplayerCoop())
		{
			eyesToCollect = 6;
		}
	}

	private void Start()
	{
		GameObject[] allEyes = GameObject.FindGameObjectsWithTag("eyes");
		allEyes.Shuffle();
		List<int> activated = new List<int>();
		int[] perFloor = new int[3];
		int used = 0;
		foreach (GameObject eye in allEyes)
		{
			if (used < eyesToCollect)
			{
				int floor = CustomSingleton<FloorManager>.Instance.GetFloor(eye.transform.position);
				if (perFloor[floor] >= eyesToCollect / 3)
				{
					eye.SetActive(false);
					continue;
				}
				perFloor[floor]++;
				used++;
				if (eye.scene.handle == gameObject.scene.handle && eye.GetComponent<Eye>() != null)
				{
					activated.Add(eye.GetInstanceID());
				}
			}
			else
			{
				eye.SetActive(false);
			}
		}
		ActivatedEyeIds = activated.AsReadOnly();
		eyesAvailable = UserGameSettings.initialEyesNumber;
		MessageKit.post(2);
	}

	private void AddEye()
	{
		if (FirebaseState.IsInitialized)
		{
			Crashlytics.Log("Eye collected");
		}
		eyesAvailable++;
		MasterAudio.PlaySound3DAtTransform("fx", CustomSingleton<PlayerManager>.Instance.Player.transform, 0.45f, null, 0f, "eye");
		MessageKit.post(2);
		MessageKit<int>.post(127, 1);
		eyesCollected++;
		UserGameSettings.eyesCollected = eyesCollected;
		Breadcrumbs.Log("eye_collected");
		if (eyesCollected == eyesToCollect)
		{
			MessageKit.post(24);
		}
	}

	public void ExtraEyesEffect()
	{
		eyesAvailable += potionReward;
		MasterAudio.PlaySound3DAtTransform("fx", CustomSingleton<PlayerManager>.Instance.Player.transform, 1f, null, 0f, "eye");
		MessageKit.post(2);
		MessageKit<int>.post(127, potionReward);
		MessageKit<System.Type, int>.post(137, typeof(Eye), potionReward);
	}

	public void UseEye()
	{
		eyesAvailable--;
		DailyChallengeRuntime.EyeUsed(this);
		MessageKit.post(2);
		MessageKit.post(125);
		Breadcrumbs.Log("eye_used");
		if (firstUseOfEye)
		{
			firstUseOfEye = false;
			UserGameSettings.eyesUsed = true;
			Breadcrumbs.Log("First eye used");
		}
		eyesCounter++;
		if (FirebaseState.IsInitialized)
		{
			Crashlytics.Log("Eye used");
		}
	}

	public int GetAvailableEyes()
	{
		return eyesAvailable;
	}

	public void AddRewardedEyes()
	{
		eyesAvailable += adReward;
		MessageKit<int>.post(127, adReward);
		if (FirebaseState.IsInitialized)
		{
			Crashlytics.Log("Eye rewarded");
		}
	}

	protected override void OnDestroy()
	{
		MessageKit.removeObserver(1, AddEye);
		MessageKit.removeObserver(16, ExtraEyesEffect);
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}
