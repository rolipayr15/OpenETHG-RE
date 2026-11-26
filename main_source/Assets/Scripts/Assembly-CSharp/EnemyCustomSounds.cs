using DarkTonic.MasterAudio;
using Characters.States;
using UnityEngine;

public class EnemyCustomSounds : MonoBehaviour
{
	private float interval = 3.3f;

	private float lastMonsterSoundStart;

	private float lastBreathSoundStart;

	private Transform enemyT;

	private Transform playerT;

	private EnemyStateController stateController;

	private EnemySounds enemySounds;

	private bool initialized;

	private void Start()
	{
		if (!initialized)
		{
			enabled = false;
		}
	}

	public bool TryInitialize()
	{
		if (initialized)
		{
			return true;
		}
		if ((int)UserGameSettings.currentEnemy == 3)
		{
			stateController = GetComponent<Characters.States.EnemyStateController>();
			enemySounds = GetComponent<EnemySounds>();
			Characters.Vision.EnemyVision vision = GetComponent<Characters.Vision.EnemyVision>();
			if (stateController != null && enemySounds != null && vision != null && vision.RaycastOrigin != null && CustomSingleton<PlayerManager>.Instance.Heart != null)
			{
				if (!CustomKrasueAudioBinding.TryRegister())
				{
					return false;
				}
				playerT = CustomSingleton<PlayerManager>.Instance.Heart.transform;
				enemyT = vision.RaycastOrigin.transform;
				initialized = true;
				return true;
			}
			Debug.LogWarning("[CustomKrasue] Missing gameplay audio dependencies; using the default voice.", this);
		}
		return false;
	}

	private void Update()
	{
		if (!initialized || !stateController.IsActive || !enemySounds.CanPlayAmbient)
		{
			return;
		}
		FloorManager floors = CustomSingleton<FloorManager>.Instance;
		if (floors.GetFloor(enemyT.position) != floors.GetPlayerFloor())
		{
			if (MasterAudio.IsTransformPlayingSoundGroup("CustomKrasue", enemyT))
			{
				MasterAudio.FadeOutAllSoundsOfTransform(enemyT, 1f);
			}
			return;
		}
		if (lastMonsterSoundStart + interval < Time.time)
		{
			MasterAudio.PlaySound3DFollowTransform("CustomKrasue", enemyT);
			lastMonsterSoundStart = Time.time;
		}
		if (Time.time > lastBreathSoundStart + 2f && floors.IsEnemyClose())
		{
			lastBreathSoundStart = Time.time;
			MasterAudio.PlaySound3DAtTransform("breathe", playerT);
		}
	}
}
