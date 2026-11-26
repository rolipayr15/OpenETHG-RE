using UnityEngine.SceneManagement;
using Eyes.DailyChallenges;
using Firebase.Crashlytics;
using I2.Loc;
using Bibloteka;
using DarkTonic.MasterAudio;
using Prime31.MessageKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ExitDoor : Interactable
{

	private float closeDistance = 6f;

	private bool playerLeftTheGame;

	public override void Start()
	{
		base.Start();
		if (UserGameSettings.IsMultiplayer())
		{
			MessageKit.addObserver(106, PlayerleftGame);
		}
	}

	private void PlayerleftGame()
	{
		MessageKit.removeObserver(106, PlayerleftGame);
		playerLeftTheGame = true;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (FirebaseState.IsInitialized)
		{
			Crashlytics.Log("EXIT_DOOR_ENTER tag=" + other.tag + " name=" + other.name + " " + string.Format("multiplayer={0} ", UserGameSettings.IsMultiplayer()) + string.Format("isHost={0} ", UserGameSettings.isMultiplayerHost) + string.Format("gameMode={0} ", UserGameSettings.currentGameMode) + string.Format("bagsCollected={0}", UserGameSettings.bagsCollected));
		}
		if (!UserGameSettings.IsMultiplayer())
		{
			if (other.CompareTag("Player"))
			{
				CheckWinConditions();
			}
			return;
		}
		int gameMode = (int)UserGameSettings.currentGameMode;
		MoneyBagsManager bags = CustomSingleton<MoneyBagsManager>.Instance;
		if (gameMode == 1)
		{
			if (!other.CompareTag("Player") && !other.CompareTag("Avatar"))
			{
				return;
			}
			if (!bags.CanEscape())
			{
				MessageKit<string>.post(3, ScriptLocalization.QuitCondition);
				return;
			}
			if (!playerLeftTheGame)
			{
				Vector3 player = CustomSingleton<PlayerManager>.Instance.Player.transform.position;
				Vector3 guest = CustomSingleton<GuestManager>.Instance.Avatar.transform.position;
				if (closeDistance * closeDistance <= (player - guest).sqrMagnitude || !UserGameSettings.isMultiplayerHost)
				{
					return;
				}
			}
			CustomSingleton<MultiplayerSettings>.Instance.SendFinishGame((GameFinishedReason)4);
			MessageKit.post(96);
			StartCoroutine(FadeOutMultiplayer());
			MessageKit.post(26);
		}
		else if (gameMode == 2)
		{
			bool isPlayer = other.CompareTag("Player");
			if (!UserGameSettings.isMultiplayerHost)
			{
				if (!isPlayer)
				{
					return;
				}
			}
			else
			{
				if (!isPlayer)
				{
					if (!other.CompareTag("Avatar"))
					{
						return;
					}
					if (bags.CanPlayerEscape())
					{
						CustomSingleton<MultiplayerSettings>.Instance.SendFinishGame((GameFinishedReason)5);
						MessageKit.post(99);
						StartCoroutine(FadeOutMultiplayer());
						MessageKit.post(26);
					}
					else if (FirebaseState.IsInitialized)
					{
						Crashlytics.Log("EXIT_VS_GUEST_BLOCKED: canEscape=false, bags collected: " + bags.guestBagsCollected);
					}
					return;
				}
				if (bags.CanEscape())
				{
					CustomSingleton<MultiplayerSettings>.Instance.SendFinishGame((GameFinishedReason)4);
					MessageKit.post(98);
					StartCoroutine(FadeOutMultiplayer());
					MessageKit.post(26);
					return;
				}
			}
			if (!bags.CanEscape())
			{
				MessageKit<string>.post(3, ScriptLocalization.QuitCondition);
			}
		}
	}

	private void CheckWinConditions()
	{
		if ((int)UserGameSettings.currentMode == 1)
		{
			MessageKit<string>.post(3, ScriptLocalization.ExitDoorEndless);
			return;
		}
		if (!CustomSingleton<MoneyBagsManager>.Instance.CanEscape())
		{
			MessageKit<string>.post(3, (int)UserGameSettings.currentMode == 2 ? ScriptLocalization.QuitConditionHalloween : ScriptLocalization.QuitCondition);
			return;
		}
		DailyChallengeRuntime.Win(this);
		PushNotificationPermissionPresenter.FinishGame(this);
		CustomEvents.FirebaseEventLevelFinished(UserGameSettings.currentLevel, UserGameSettings.currentEnemy, UserGameSettings.currentMode, UserGameSettings.currentDifficulty, (GameFinishedResult)0, UserGameSettings.expertMode, UserGameSettings.bagsCollected, UserGameSettings.eyesCollected);
		UserGameSettings.lastGameDuration = Time.timeSinceLevelLoad;
		UserGameSettings.wasLastGameWon = true;
		CustomSingleton<ScreenFader>.Instance.fadeIn = false;
		MessageKit.post(26);
		FadeOutMobile();
	}

	private void FadeOutMobile()
	{
		int mode = (int)UserGameSettings.currentMode;
		int gameMode = (int)UserGameSettings.currentGameMode;
		int enemy = (int)UserGameSettings.currentEnemy;
		int level = (int)UserGameSettings.currentLevel;
		bool outro = false;
		if (mode == 0 && gameMode == 0)
		{
			if (enemy == 0)
			{
				outro = level == 0 && !PlayerPrefz.GetBool("SKIP_OUTRO_MANSION", false);
			}
			else if (enemy == 1)
			{
				outro = level == 1 && !PlayerPrefz.GetBool("SKIP_OUTRO_HOSPITAL", false);
			}
			else if (enemy == 2)
			{
				outro = level == 3 && !PlayerPrefz.GetBool("SKIP_OUTRO_SCHOOL", false);
			}
		}
		SceneManager.LoadScene(outro ? "Outro" : "Win");
	}

	private IEnumerator FadeOutMultiplayer()
	{
		MessageKit.post(40);
		CustomSingleton<PlayerManager>.Instance.playerHandler.SetState("Freeze", true, true, false);
		yield return Utils.GetDelay(2f);
		CustomSingleton<ScreenFader>.Instance.fadeIn = false;
		yield return Utils.GetDelay(1f);
		MasterAudio.PlaySound3DAtTransform("music", CustomSingleton<PlayerManager>.Instance.Player.transform);
	}

	protected override void OnDestroy()
	{
		MessageKit.removeObserver(106, PlayerleftGame);
		base.OnDestroy();
	}
}
