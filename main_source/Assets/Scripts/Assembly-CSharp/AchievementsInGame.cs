using Eyes.GameplayConditions;
using Firebase.Crashlytics;
using Prime31.MessageKit;
using UnityEngine;
using VoxelBusters.CoreLibrary;
using VoxelBusters.EssentialKit;

public class AchievementsInGame : MonoBehaviour
{
	private const string ACHIEVEMENT_ALLEYES = "achievementCollector";

	private const string ACHIEVEMENT_PERSISTENT = "achievementPersistent";

	private const string ACHIEVEMENT_EXTRA_EYES = "achievementEyesMoreEyes";

	private const string ACHIEVEMENT_SPRINT_EFFECT = "achievementWithTheSpeedOfLight";

	private const string ACHIEVEMENT_CLOAK_EFFECT = "achievementWeMoveUnseen";

	private const string ACHIEVEMENT_MONEY_BAGS_VISION = "achievementThirdEyeSeer";

	private const string ACHIEVEMENT_ALL_POTIONS = "achievementMeticulous";

	private const string ACHIEVEMENT_10_POTIONS = "achievementPotionNovice";

	private const string ACHIEVEMENT_30_POTIONS = "achievementPotionApprentice";

	private const string ACHIEVEMENT_50_POTIONS = "achievementMasterChemist";

	private const string ACHIEVEMENT_GOOD_BOY = "achievementGoodBoy";

	private const string ACHIEVEMENT_SO_CLOSE = "achievementSoClose";

	private const string ACHIEVEMENT_EXPLORER = "achievementExplorer";

	private const string ACHIEVEMENT_SECRET_ROOM = "achievementFamilyMatters";

	private const string ACHIEVEMENT_RITUAL = "TheRitual";

	private const string ACHIEVEMENT_COOP_NIGHTMARE_MANSION = "achievement_coop_nightmare_mansion";

	private const string ACHIEVEMENT_COOP_NIGHTMARE_HOSPITAL = "achievement_coop_nightmare_hospital";

	private const string ACHIEVEMENT_COOP_NIGHTMARE_SCHOOL = "achievement_coop_nightmare_school";

	private const string ACHIEVEMENT_EXPLORER_SCHOOL = "achievement_map_school";

	private const string ACHIEVEMENT_EXPLORER_HOSPITAL = "achievement_map_hospital";

	private bool extraEyes;

	private bool sprint;

	private bool moneyBags;

	private bool cloak;

	private int potionsCounter;

	private string sceneName;

	private bool alreadyHasGoodBoy;

	private int counter;

	private void Start()
	{
		if ((int)UserGameSettings.currentEnemy == 4)
		{
			return;
		}
		MessageKit.addObserver(16, ExtraEyesEffectAchievement);
		MessageKit<bool>.addObserver(13, SprintEffectAchievement);
		MessageKit<bool>.addObserver(10, MoneyBagsVisionEffectAchievement);
		MessageKit<bool>.addObserver(14, CloakEffectAchievement);
		MessageKit.addObserver(17, AchievementPersistent);
		MessageKit.addObserver(24, AchievementAllEyes);
		MessageKit.addObserver(27, AchievementGoodBoy);
		MessageKit<bool, string>.addObserver(5, AchievementShhh);
		MessageKit<int>.addObserver(35, AchievementExplorer);
		MessageKit.addObserver(52, AchievementSecretRoom);
		MessageKit.addObserver(55, AchievementSoClose);
		MessageKit.addObserver(132, AchievementMultiplayerWin);
		MessageKit<int>.addObserver(15, CheckPotions);
		potionsCounter = PlayerPrefs.GetInt("POTIONS_COUNTER", 0);
		GameServices.OnAuthStatusChange += OnAuthStatusChange;
		CheckPotionCountAchievements();
	}

	private void OnAuthStatusChange(GameServicesAuthStatusChangeResult result, Error error)
	{
		if (result != null && error == null && (int)result.AuthStatus == 2)
		{
			CheckPotionCountAchievements();
		}
	}

	private void OnDestroy()
	{
		GameServices.OnAuthStatusChange -= OnAuthStatusChange;
		MessageKit.removeObserver(16, ExtraEyesEffectAchievement);
		MessageKit<bool>.removeObserver(13, SprintEffectAchievement);
		MessageKit<bool>.removeObserver(10, MoneyBagsVisionEffectAchievement);
		MessageKit<bool>.removeObserver(14, CloakEffectAchievement);
		MessageKit.removeObserver(17, AchievementPersistent);
		MessageKit.removeObserver(24, AchievementAllEyes);
		MessageKit.removeObserver(27, AchievementGoodBoy);
		MessageKit<bool, string>.removeObserver(5, AchievementShhh);
		MessageKit<int>.removeObserver(35, AchievementExplorer);
		MessageKit.removeObserver(52, AchievementSecretRoom);
		MessageKit.removeObserver(55, AchievementSoClose);
		MessageKit.removeObserver(132, AchievementMultiplayerWin);
		MessageKit<int>.removeObserver(15, CheckPotions);
	}

	private void UnlockAchievement(string achievementId)
	{
		if (achievementId == "")
		{
			return;
		}
		if (!GameServices.IsAuthenticated)
		{
			Debug.Log("Player must be authenticated");
			return;
		}
		Debug.Log(achievementId);
		IAchievement achievement = GameServices.CreateAchievement(achievementId);
		if (achievement == null)
		{
			Debug.Log("Achievement " + achievementId + " not found in settings");
			return;
		}
		achievement.PercentageCompleted = 100.0;
		achievement.ReportProgress(delegate(bool success, Error error)
		{
			Debug.Log(success ? "Achievement reported successfully" : "Achievement report failed: " + error.Description);
		});
	}

	private void AchievementExplorer(int i)
	{
		int level = (int)UserGameSettings.currentLevel;
		string id;
		if (level == 3)
		{
			if (++counter < 25)
			{
				return;
			}
			id = ACHIEVEMENT_EXPLORER_SCHOOL;
		}
		else if (level == 1)
		{
			if (++counter < 17)
			{
				return;
			}
			id = ACHIEVEMENT_EXPLORER_HOSPITAL;
		}
		else
		{
			if (level != 0 || ++counter < 24)
			{
				return;
			}
			id = ACHIEVEMENT_EXPLORER;
		}
		UnlockAchievement(id);
	}

	private void AchievementShhh(bool isChased, string enemyName)
	{
		UserGameSettings.wasPlayerSeenByMonster = true;
	}

	private void AchievementGoodBoy()
	{
		if (!alreadyHasGoodBoy)
		{
			UnlockAchievement(ACHIEVEMENT_GOOD_BOY);
			alreadyHasGoodBoy = true;
		}
	}

	private void CloakEffectAchievement(bool isCloakOn)
	{
		cloak = true;
		UnlockAchievement(ACHIEVEMENT_CLOAK_EFFECT);
	}

	private void MoneyBagsVisionEffectAchievement(bool isOn)
	{
		moneyBags = true;
		UnlockAchievement(ACHIEVEMENT_MONEY_BAGS_VISION);
	}

	private void SprintEffectAchievement(bool isOn)
	{
		sprint = true;
		UnlockAchievement(ACHIEVEMENT_SPRINT_EFFECT);
	}

	private void ExtraEyesEffectAchievement()
	{
		extraEyes = true;
		UnlockAchievement(ACHIEVEMENT_EXTRA_EYES);
	}

	private void CheckIfMadeAllPotions()
	{
		if (extraEyes && sprint && moneyBags && cloak)
		{
			UnlockAchievement(ACHIEVEMENT_ALL_POTIONS);
		}
		potionsCounter++;
		PlayerPrefs.SetInt("POTIONS_COUNTER", potionsCounter);
		CheckPotionCountAchievements();
	}

	private void CheckPotionCountAchievements()
	{
		if (potionsCounter < 10)
		{
			return;
		}
		UnlockAchievement(ACHIEVEMENT_10_POTIONS);
		if (potionsCounter < 30)
		{
			return;
		}
		UnlockAchievement(ACHIEVEMENT_30_POTIONS);
		if (potionsCounter >= 50)
		{
			UnlockAchievement(ACHIEVEMENT_50_POTIONS);
		}
	}

	private void CheckPotions(int x)
	{
		CheckIfMadeAllPotions();
	}

	private void AchievementSoClose()
	{
		UnlockAchievement(ACHIEVEMENT_SO_CLOSE);
	}

	private void AchievementPersistent()
	{
		int value = PlayerPrefs.GetInt("Persistent", 0) + 10;
		PlayerPrefs.SetInt("Persistent", value);
		if (value >= 100)
		{
			UnlockAchievement(ACHIEVEMENT_PERSISTENT);
		}
	}

	private void AchievementAllEyes()
	{
		UnlockAchievement(ACHIEVEMENT_ALLEYES);
	}

	private void AchievementSecretRoom()
	{
		UnlockAchievement(ACHIEVEMENT_SECRET_ROOM);
	}

	private void AchievementMultiplayerWin()
	{
		MultiplayerWinKind kind = GameplayConditionChecker.GetMultiplayerWin(new GameplayRunFacts(true, UserGameSettings.currentGameMode, UserGameSettings.isPlayingWithRandom));
		int wins = PlayerPrefs.GetInt("MULTIPLAYER_WINS_COUNTER", 0) + 1;
		PlayerPrefs.SetInt("MULTIPLAYER_WINS_COUNTER", wins);
		UpdateAchievementProgress(ACHIEVEMENT_RITUAL, wins, 25);
		int kindValue = (int)kind;
		if (kindValue >= 1 && kindValue < 5)
		{
			UnlockAchievement(new string[] { null, "WinCoopRandom", "WinCoopFriend", "WinVersusRandom", "WinVersusFriend" }[kindValue]);
		}
		if (!UserGameSettings.IsMultiplayerCoop() || (int)UserGameSettings.currentDifficulty != 4 || UserGameSettings.myBagsCollected != UserGameSettings.secondPlayerBagsCollected)
		{
			return;
		}
		switch ((int)UserGameSettings.currentLevel)
		{
		case 0:
			UnlockAchievement(ACHIEVEMENT_COOP_NIGHTMARE_MANSION);
			break;
		case 1:
			UnlockAchievement(ACHIEVEMENT_COOP_NIGHTMARE_HOSPITAL);
			break;
		case 3:
			UnlockAchievement(ACHIEVEMENT_COOP_NIGHTMARE_SCHOOL);
			break;
		}
	}

	private void UpdateAchievementProgress(string achievementId, int currentProgress, int maxProgress)
	{
		if (!GameServices.IsAuthenticated)
		{
			Debug.Log("Player must be authenticated");
			return;
		}
		if (maxProgress < currentProgress || achievementId == "")
		{
			return;
		}
		Debug.Log(achievementId);
		IAchievement achievement = GameServices.CreateAchievement(achievementId);
		if (achievement == null)
		{
			Debug.Log("Achievement " + achievementId + " not found in settings");
			return;
		}
		achievement.PercentageCompleted = (double)currentProgress / maxProgress * 100.0;
		achievement.ReportProgress(delegate(bool success, Error error)
		{
			Debug.Log(success ? "Achievement reported successfully" : "Achievement report failed: " + error.Description);
		});
	}
}
