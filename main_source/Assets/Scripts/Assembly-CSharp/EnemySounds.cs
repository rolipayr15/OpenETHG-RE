using DarkTonic.MasterAudio;
using Prime31.MessageKit;
using Characters.States.Goose;
using UnityEngine;

public class EnemySounds : MonoBehaviour
{
	private const string GoosePatrolSound = "goose-patrol";

	private const float GoosePatrolInterval = 2.5f;

	private const float GooseChaseInterval = 1.8f;

	private GooseStateController gooseStateController;

	private string soundVariationName;

	private float interval;

	private bool ambientSilent;

	private Transform playerT;

	[SerializeField]
	private EnemyCustomSounds enemyCustomSounds;

	[SerializeField]
	private Transform enemyT;

	private bool isPlayerDead;

	private bool isGameWon;

	private float lastMonsterSoundStart;

	private float lastBreathSoundStart;

	private bool enemySoundStarted;

	private bool isEnemyVisionActive;

	private bool isChasing;

	private bool isGuestDead;

	public bool CanPlayAmbient => !isPlayerDead && !isGuestDead && !isGameWon;

	private void Start()
	{
		MessageKit<bool>.addObserver(7, EnemyVision);
		MessageKit.addObserver(17, PlayerIsDead);
		MessageKit.addObserver(53, PlayerIsAlive);
		MessageKit<bool, string>.addObserver(5, EnemyChase);
		MessageKit.addObserver(92, GuestIsDead);
		MessageKit.addObserver(26, GameIsWon);
		playerT = CustomSingleton<PlayerManager>.Instance.Heart.transform;
		gooseStateController = GetComponent<GooseStateController>();
		if (gooseStateController != null)
		{
			soundVariationName = string.Empty;
			interval = GoosePatrolInterval;
		}
		else if (name == "Enemy_Krasue")
		{
			soundVariationName = "mmm";
			interval = 5f;
		}
		else if (name == "Enemy_Witch")
		{
			soundVariationName = "Witch";
			interval = 2.8f;
		}
		else if (name == "Enemy_Charlie")
		{
			soundVariationName = "Charlie";
			interval = 12f;
		}
		else if (name == "Enemy_Dog")
		{
			soundVariationName = "Dog";
			interval = 4.2f;
		}
		else
		{
			bool isGoose = name == "Enemy_Goose";
			soundVariationName = "mmm";
			interval = 5f;
			if (isGoose)
			{
				ambientSilent = true;
			}
		}
		bool customReady = false;
		if ((int)UserGameSettings.currentEnemy == 3 && enemyCustomSounds != null)
		{
			customReady = enemyCustomSounds.TryInitialize();
		}
		if (enemyCustomSounds != null)
		{
			enemyCustomSounds.enabled = customReady;
		}
		if (customReady)
		{
			soundVariationName = "CustomKrasue";
			interval = 3f;
			enabled = false;
		}
	}

	private void PlayerIsDead()
	{
		MasterAudio.MuteGroup("breathe");
		SetEnemySoundsMuted(true);
		isPlayerDead = true;
	}

	private void GameIsWon()
	{
		isGameWon = true;
		MasterAudio.MuteGroup("breathe");
		SetEnemySoundsMuted(true);
	}

	private void PlayerIsAlive()
	{
		MasterAudio.UnmuteGroup("breathe");
		SetEnemySoundsMuted(false);
		isPlayerDead = false;
	}

	private void SetEnemySoundsMuted(bool muted)
	{
		SetGroupMuted(gooseStateController != null ? GoosePatrolSound : soundVariationName, muted);
	}

	private static void SetGroupMuted(string group, bool muted)
	{
		if (string.IsNullOrEmpty(group))
		{
			return;
		}
		if (muted)
		{
			MasterAudio.MuteGroup(group);
		}
		else
		{
			MasterAudio.UnmuteGroup(group);
		}
	}

	private void UpdateGooseSound()
	{
		isChasing = false;
		string state = gooseStateController.DiagnosticCurrentStateName;
		string sound;
		if (state == "Windup" || state == "Charge")
		{
			sound = GoosePatrolSound;
			isChasing = true;
		}
		else if (state == "Patrol" || state == "CheckLastLocation")
		{
			sound = GoosePatrolSound;
		}
		else
		{
			sound = string.Empty;
		}
		interval = isChasing ? GooseChaseInterval : GoosePatrolInterval;
		if (sound != soundVariationName)
		{
			if (!string.IsNullOrEmpty(soundVariationName))
			{
				MasterAudio.FadeOutAllOfSound(soundVariationName, 0.2f);
			}
			soundVariationName = sound;
			enemySoundStarted = false;
			lastMonsterSoundStart = float.NegativeInfinity;
		}
	}

	private void Update()
	{
		if (isPlayerDead || isGuestDead || isGameWon)
		{
			return;
		}
		if (gooseStateController != null)
		{
			UpdateGooseSound();
			if (string.IsNullOrEmpty(soundVariationName))
			{
				return;
			}
		}
		if (isEnemyVisionActive && lastMonsterSoundStart + interval < Time.time)
		{
			PlayVisionSound();
		}
		FloorManager floorManager = CustomSingleton<FloorManager>.Instance;
		if (!floorManager.IsEnemyOnTheFloor() && !isChasing)
		{
			if (!isEnemyVisionActive)
			{
				FadeOutEnemySoundIfNeeded();
			}
			return;
		}
		if ((int)UserGameSettings.currentEnemy == 5 && floorManager.GetFloor(transform.position) != floorManager.GetPlayerFloor())
		{
			return;
		}
		if (!ambientSilent && !isEnemyVisionActive && lastMonsterSoundStart + interval < Time.time)
		{
			MasterAudio.PlaySound3DFollowTransform(soundVariationName, enemyT);
			enemySoundStarted = true;
			lastMonsterSoundStart = Time.time;
		}
		if (lastBreathSoundStart + 2f < Time.time && floorManager.IsEnemyClose())
		{
			lastBreathSoundStart = Time.time;
			MasterAudio.PlaySound3DAtTransform("breathe", playerT);
		}
	}

	private void FadeOutEnemySoundIfNeeded()
	{
		if (enemySoundStarted)
		{
			MasterAudio.FadeOutAllSoundsOfTransform(enemyT, 1f);
			enemySoundStarted = false;
		}
	}

	private void EnemyVision(bool _isEnemyVisionActive)
	{
		if (isEnemyVisionActive == _isEnemyVisionActive)
		{
			return;
		}
		if (isEnemyVisionActive && !string.IsNullOrEmpty(soundVariationName))
		{
			MasterAudio.FadeOutSoundGroupOfTransform(playerT, soundVariationName, 0.2f);
		}
		isEnemyVisionActive = _isEnemyVisionActive;
		if (gooseStateController != null)
		{
			UpdateGooseSound();
		}
		if (!CanPlayAmbient || string.IsNullOrEmpty(soundVariationName))
		{
			return;
		}
		if (isEnemyVisionActive)
		{
			FadeOutEnemySoundIfNeeded();
			PlayVisionSound();
			return;
		}
		if (!CustomSingleton<FloorManager>.Instance.IsEnemyOnTheFloor())
		{
			FadeOutEnemySoundIfNeeded();
		}
	}

	private void PlayVisionSound()
	{
		MasterAudio.PlaySound3DAtTransform(soundVariationName, playerT);
		lastMonsterSoundStart = Time.time;
	}

	private void EnemyChase(bool _isChasing, string enemyName)
	{
		if (name == enemyName)
		{
			isChasing = _isChasing;
		}
	}

	private void GuestIsDead()
	{
		if (UserGameSettings.IsMultiplayer() && UserGameSettings.isMultiplayerHost)
		{
			isGuestDead = true;
			MasterAudio.MuteGroup("breathe");
			SetEnemySoundsMuted(true);
		}
	}
}
