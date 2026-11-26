using System;
using System.Collections;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using BestHTTP;
using ExternalApi.Models;
using Firebase.Crashlytics;
using Multiplayer.ExternalApi;
using Prime31.MessageKit;
using Reflex.Attributes;
using UnityEngine;

public class Exp : MonoBehaviour
{
	[Inject]
	private IExperienceManager experienceManager;

	[Inject]
	private IGameSingleApi gameSingleApi;

	[Inject]
	private IExpApi expApi;

	private string gameId = GenerateGameId();

	private bool expGranted;

	public ExpCalculator expCalculator;

	private void Awake()
	{
		if (Application.exitCancellationToken.IsCancellationRequested)
		{
			base.enabled = false;
		}
		else
		{
			ExpReporter.EnsureInitialized(experienceManager, expApi);
		}
	}

	private IEnumerator Start()
	{
		expCalculator = new ExpCalculator();
		PlayerPrefs.GetInt("EXP_POINTS");
		Debug.Log("EXP: Starting Exp.cs for" + (UserGameSettings.IsMultiplayer() ? "multiplayer" : "singleplayer"));
		UserGameSettings.ExpPoints.sum = 0;
		MessageKit.addObserver(20, delegate
		{
			expCalculator.RegisterEvent(ExpSourceType.SawFound);
		});
		MessageKit.addObserver(52, delegate
		{
			expCalculator.RegisterEvent(ExpSourceType.PortraitsMinigameSolved);
		});
		MessageKit<int>.addObserver(127, delegate(int n)
		{
			expCalculator.RegisterEvent(ExpSourceType.EyesCollected, n);
		});
		MessageKit.addObserver(91, delegate
		{
			expCalculator.RegisterEvent(ExpSourceType.SafeUnlocked);
		});
		if (UserGameSettings.IsMultiplayer())
		{
			MessageKit<GameFinishedReason>.addObserver(105, MultiplayerGameFinished);
			yield break;
		}
		MessageKit.addObserver(90, OnDeath);
		MessageKit.addObserver(26, OnWin);
		yield return SendSingleGameStartInfo(gameId).ToCoroutine(this, delegate(HTTPResponse response)
		{
			if (response == null || !response.IsSuccess)
			{
				Debug.Log("Failed to send game start info");
			}
		});
	}

	private async Task<HTTPResponse> SendSingleGameStartInfo(string gameId)
	{
		return await gameSingleApi.StartGameAsync(new StartGameData
		{
			gameId = gameId,
			diff = UserGameSettings.currentDifficulty,
			enemy = UserGameSettings.currentEnemy,
			level = UserGameSettings.currentLevel,
			mapMode = UserGameSettings.currentMode
		});
	}

	private void MultiplayerGameFinished(GameFinishedReason reason)
	{
		if (IsPlayerWin(reason))
		{
			OnWin();
		}
		else
		{
			OnDeath();
		}
	}

	private bool IsPlayerWin(GameFinishedReason reason)
	{
		int value = (int)reason;
		if (UserGameSettings.IsMultiplayerCoop())
		{
			return value == 4 || value == 5;
		}
		if (value > 8)
		{
			return false;
		}
		int mask = (UserGameSettings.isMultiplayerHost ? 0x118 : 0x124);
		return ((mask >> value) & 1) != 0;
	}

	private void OnDeath()
	{
		if (!expGranted)
		{
			GameEnded("Death");
			expGranted = true;
		}
	}

	private void OnWin()
	{
		if (!expGranted)
		{
			ReviewPromptState.RecordWin();
			UserGameSettings.wasLastGameWon = true;
			GameEnded("Win");
			expGranted = true;
		}
	}

	private async void GameEnded(string finishReason)
	{
		Debug.Log("EXP: Game ended with reason: " + finishReason);
		CalculateGamePointsAndSetGameEvents();
		int exp = expCalculator.Calculate().FinalTotal;
		GrantExpData grantExpData;
		if (UserGameSettings.IsMultiplayer())
		{
			grantExpData = new GrantExpData
			{
				exp = exp,
				eventType = "GameMultiFinished",
				gameMultiId = Uri.UnescapeDataString(UserGameSettings.multiplayerGameId),
				eventData = expCalculator.ExportGameEvents()
			};
		}
		else
		{
			grantExpData = new GrantExpData
			{
				exp = exp,
				eventType = "GameSingleFinished",
				gameSingleId = gameId,
				eventData = expCalculator.ExportGameEvents()
			};
			await gameSingleApi.SendGameSummaryAsync(new EndGameData
			{
				gameId = gameId,
				finishReason = finishReason
			});
		}
		Debug.Log("EXP: Granting exp: " + grantExpData.exp);
		experienceManager.GrantExp(grantExpData);
	}

	private void CalculateGamePointsAndSetGameEvents()
	{
		if (UserGameSettings.wasLastGameWon)
		{
			expCalculator.RegisterEvent(ExpSourceType.GameWon);
		}
		expCalculator.RegisterEvent(ExpSourceType.BagsCollected, UserGameSettings.IsMultiplayer() ? UserGameSettings.myBagsCollected : UserGameSettings.bagsCollected);
		CalcResult result = expCalculator.Calculate();
		UserGameSettings.ExpPoints.sum = result.FinalTotal;
		UserGameSettings.calcResult = result;
		if (FirebaseState.IsInitialized)
		{
			Crashlytics.Log("Exp " + result.FinalTotal);
		}
	}

	public static string GenerateGameId()
	{
		byte[] bytes = new byte[6];
		using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
		{
			generator.GetBytes(bytes);
		}
		string id = Convert.ToBase64String(bytes).Substring(0, 8);
		UserGameSettings.singleGameId = id;
		return id;
	}
}
