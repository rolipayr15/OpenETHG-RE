using System.Collections;
using System.Text;
using Bibloteka;
using DarkTonic.MasterAudio;
using Eyes.DailyChallenges;
using EyesAnalytics;
using Firebase.Crashlytics;
using UnityEngine;

public class MyGameSettings : MonoBehaviour
{
	private vp_FPInput playerInput;

	private void Awake()
	{
		DoorUseTracker.Reset();
	}

	private void OnDestroy()
	{
		PushNotificationPermissionPresenter.SceneDestroyed(this);
		DailyChallengeRuntime.SceneDestroyed(this);
	}

	private void Start()
	{
		PushNotificationPermissionPresenter.BeginGame(this);
		DailyChallengeRuntime.Start(this);
		StringBuilder builder = new StringBuilder();
		builder.Append(UserGameSettings.currentLevel.ToString());
		builder.Append("-");
		builder.Append(UserGameSettings.currentEnemy.ToString());
		builder.Append("-");
		builder.Append(UserGameSettings.currentMode.ToString());
		builder.Append("-");
		builder.Append(UserGameSettings.currentDifficulty.ToString());
		builder.Append("-");
		builder.Append(UserGameSettings.IsMultiplayer() ? "1" : "0");
		builder.Append("-");
		builder.Append(UserGameSettings.isMultiplayerHost ? "1" : "0");
		if (FirebaseState.IsInitialized)
		{
			Crashlytics.Log("start a new game " + builder.ToString());
			Crashlytics.SetCustomKey("map", UserGameSettings.currentLevel.ToString());
			Crashlytics.SetCustomKey("enemy", UserGameSettings.currentEnemy.ToString());
			Crashlytics.SetCustomKey("mode", UserGameSettings.currentMode.ToString());
			Crashlytics.SetCustomKey("difficulty", UserGameSettings.currentDifficulty.ToString());
			Crashlytics.SetCustomKey("multiplayer", UserGameSettings.IsMultiplayer() ? "1" : "0");
			Crashlytics.SetCustomKey("host", UserGameSettings.isMultiplayerHost ? "1" : "0");
		}
		CustomSingleton<MouseManager>.Instance.Synchronize();
		CustomSingleton<ScreenFader>.Instance.fadeIn = false;
		CustomSingleton<ScreenFader>.Instance.fadeIn = true;
		if (!UserGameSettings.volume.HasValue)
		{
			UserGameSettings.volume = PlayerPrefs.GetFloat("musicVolume", 1f);
		}
		MasterAudio.MasterVolumeLevel = UserGameSettings.volume.Value;
		MasterAudio.PlaylistMasterVolume = UserGameSettings.volume.Value;
		if (!UserGameSettings.sensitivity.HasValue)
		{
			UserGameSettings.sensitivity = PlayerPrefs.GetFloat("sensitivity", 5f);
		}
		UserGameSettings.wasPlayerSeenByMonster = false;
		playerInput = CustomSingleton<PlayerManager>.Instance.Player.GetComponent<vp_FPInput>();
		UserGameSettings.eyesUsed = false;
		UserGameSettings.lastGameDuration = 0f;
		StartCoroutine(SetSensitivity());
		UserGameSettings.eyesCollected = 0;
		UserGameSettings.wasLastGameWon = false;
		UserGameSettings.playCounter++;
		PlayerPrefs.SetInt("PLAY_COUNTER", UserGameSettings.playCounter);
		if (UserGameSettings.IsMultiplayer())
		{
			AnalyticsContext.SetMultiplayer((int)UserGameSettings.currentGameMode == 1, !UserGameSettings.isPlayingWithRandom);
		}
		else
		{
			AnalyticsContext.SetSingleplayer();
		}
		Game.Start(UserGameSettings.currentLevel, UserGameSettings.currentEnemy, UserGameSettings.currentDifficulty, UserGameSettings.currentMode, UserGameSettings.expertMode, base.gameObject.scene.handle);
		Screens.View((ScreenId)19);
		Breadcrumbs.Log("Level start");
		if (UserGameSettings.isOneTimeUnlocked)
		{
			if (UserGameSettings.currentPreset.HasValue)
			{
				AdTicketStore.Consume(UserGameSettings.currentPreset.Value);
			}
			else if (FirebaseState.IsInitialized)
			{
				Crashlytics.Log("MY_GAME_SETTINGS ERROR: isOneTimeUnlocked=true but currentPreset=NULL");
			}
		}
	}

	private IEnumerator SetSensitivity()
	{
		yield return Utils.GetDelay(0.5f);
		OptionsSensitivity.ApplySavedSensitivity(playerInput);
		OptionsFOV.ApplyFov(OptionsFOV.GetSavedFov());
	}

	private void OnApplicationQuit()
	{
		PushNotificationPermissionPresenter.CancelGame(this);
		DailyChallengeRuntime.ApplicationQuit(this);
		MultiplayerSettings settings = CustomSingleton<MultiplayerSettings>.Instance;
		if (settings != null)
		{
			settings.SendFinishGame((GameFinishedReason)7);
		}
	}

	private void OnApplicationFocus(bool isFocus)
	{
		MultiplayerSettings settings = CustomSingleton<MultiplayerSettings>.Instance;
		if (settings != null)
		{
			if (isFocus)
			{
				settings.SendGameResumed((GameResumedReason)0);
			}
			else
			{
				settings.SendGameInterrupted((GameInterruptedReason)0);
			}
		}
	}
}
