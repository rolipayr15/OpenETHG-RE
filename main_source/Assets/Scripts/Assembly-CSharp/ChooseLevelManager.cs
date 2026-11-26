using System.Collections.Generic;
using EyesAnalytics;
using Prime31.MessageKit;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class ChooseLevelManager : CustomSingleton<ChooseLevelManager>
{
	[SerializeField]
	private GameObject chooseLevelWindow;

	[SerializeField]
	private Button playButton;

	[SerializeField]
	private HorizontalScrollSnap difficultyScroll;

	[SerializeField]
	private ScrollPositionController enemyScroll;

	[SerializeField]
	private ScrollPositionController chineseEnemyScroll;

	[SerializeField]
	private ScrollPositionController modeScroll;

	[SerializeField]
	private ScrollPositionController levelScroll;

	// Scroll positions in the expert menu do not follow the enum order, hence the two-way maps.
	private Dictionary<int, Enemy> enemiesList = new Dictionary<int, Enemy>
	{
		{ 0, Enemy.Krasue },
		{ 1, Enemy.Charlie },
		{ 2, Enemy.Dog },
		{ 3, Enemy.Sandbox },
		{ 4, Enemy.Double },
		{ 5, Enemy.Custom },
		{ 6, Enemy.Witch },
		{ 7, Enemy.Goose }
	};

	private Dictionary<Enemy, int> enemiesListReverted = new Dictionary<Enemy, int>
	{
		{ Enemy.Krasue, 0 },
		{ Enemy.Charlie, 1 },
		{ Enemy.Dog, 2 },
		{ Enemy.Sandbox, 3 },
		{ Enemy.Double, 4 },
		{ Enemy.Custom, 5 },
		{ Enemy.Witch, 6 },
		{ Enemy.Goose, 7 }
	};

	private Dictionary<int, Mode> modesList = new Dictionary<int, Mode>
	{
		{ 0, Mode.STANDARD },
		{ 1, Mode.ENDLESS },
		{ 2, Mode.HALLOWEEN },
		{ 3, Mode.PIXEL }
	};

	private Dictionary<Mode, int> modesListReverted = new Dictionary<Mode, int>
	{
		{ Mode.STANDARD, 0 },
		{ Mode.ENDLESS, 1 },
		{ Mode.HALLOWEEN, 2 },
		{ Mode.PIXEL, 3 }
	};

	private Dictionary<int, Level> levelsList = new Dictionary<int, Level>
	{
		{ 0, Level.Mansion },
		{ 1, Level.Hospital },
		{ 2, Level.School }
	};

	private Dictionary<Level, int> levelsListReverted = new Dictionary<Level, int>
	{
		{ Level.Mansion, 0 },
		{ Level.Hospital, 1 },
		{ Level.School, 2 }
	};

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private Image playButtonBackground;

	[SerializeField]
	private GameObject difficultySelect;

	private Mode lastChosenMode;

	private Level lastChosenLevel;

	private void Start()
	{
		MessageKit<string>.addObserver(34, UnlockProduct);
		MessageKit<StoreItems>.addObserver(133, UnlockOneTime);
	}

	private void UnlockProduct(string productId)
	{
		if (chooseLevelWindow.activeInHierarchy)
		{
			SetLevel(lastChosenLevel);
			SetMode(lastChosenMode);
		}
		SetPlayButton();
	}

	private void UnlockOneTime(StoreItems _storeItem)
	{
		SetPlayButton();
	}

	public void ShowChooseLevel()
	{
		lastChosenLevel = UserGameSettings.currentLevel;
		lastChosenMode = UserGameSettings.currentMode;
		difficultyScroll.StartingScreen = 2;
		SetDifficulty(2);
		levelScroll.ScrollTo(levelsListReverted[UserGameSettings.currentLevel], 1f);
		SetLevel(UserGameSettings.currentLevel);
		modeScroll.ScrollTo(modesListReverted[UserGameSettings.currentMode], 1f);
		SetMode(UserGameSettings.currentMode);
		enemyScroll.ScrollTo(enemiesListReverted[UserGameSettings.currentEnemy], 1f);
		UserGameSettings.currentGameMode = GameMode.SINGLE;
		UserGameSettings.platform = Platform.Singleplayer;
		SetPlayButton();
		chooseLevelWindow.SetActive(true);
	}

	private void SetPlayButton()
	{
		// Sandbox only exists in the standard mode.
		if (IsLevelUnlocked(lastChosenLevel) && IsModeUnlocked(lastChosenMode) && IsEnemyUnlocked(UserGameSettings.currentEnemy) && (UserGameSettings.currentEnemy != Enemy.Sandbox || lastChosenMode == Mode.STANDARD))
		{
			PlayAvailable();
		}
		else
		{
			PlayUnavailable();
		}
	}

	private void PlayAvailable()
	{
		playButton.interactable = true;
		playButtonBackground.color = Color.white;
		animator.enabled = true;
	}

	private void PlayUnavailable()
	{
		playButton.interactable = false;
		animator.enabled = false;
		playButtonBackground.color = Color.gray;
	}

	public bool IsModeUnlocked(Mode mode)
	{
		switch (mode)
		{
		case Mode.PIXEL:
			return RecoveryTestBuild.Unlocked(UserGameSettings.unlockedPixelMode);
		case Mode.HALLOWEEN:
			return RecoveryTestBuild.Unlocked(UserGameSettings.unlockedTrickOrTreatMode);
		case Mode.ENDLESS:
			return RecoveryTestBuild.Unlocked(UserGameSettings.unlockedEndlessMode);
		default:
			return true;
		}
	}

	public void SetMode(Mode mode)
	{
		if (IsModeUnlocked(mode))
		{
			UserGameSettings.currentMode = mode;
		}
		switch (mode)
		{
		case Mode.STANDARD:
		case Mode.HALLOWEEN:
		case Mode.PIXEL:
			if (FirstPlayDifficulty.Applies(UserGameSettings.playCounter))
			{
				UserGameSettings.currentDifficulty = UserGameSettings.firstPlayDifficulty;
			}
			difficultySelect.SetActive(true);
			break;
		case Mode.ENDLESS:
			UserGameSettings.currentDifficulty = Difficulty.Normal;
			difficultySelect.SetActive(false);
			break;
		}
		SetPlayButton();
	}

	public void SetMode(int modeID)
	{
		lastChosenMode = modesList[modeID];
		SetMode(lastChosenMode);
	}

	public bool IsLevelUnlocked(Level level)
	{
		if (RecoveryTestBuild.FreeEverything)
		{
			return true;
		}
		switch (level)
		{
		case Level.School:
			return UserGameSettings.unlockedSchool;
		case Level.Hospital:
			return UserGameSettings.unlockedHospital;
		default:
			return true;
		}
	}

	public void SetLevel(int levelID)
	{
		lastChosenLevel = levelsList[levelID];
		SetLevel(lastChosenLevel);
	}

	private void SetLevel(Level level)
	{
		if (IsLevelUnlocked(level))
		{
			UserGameSettings.currentLevel = level;
		}
		SetPlayButton();
	}

	public bool IsEnemyUnlocked(Enemy enemy)
	{
		switch (enemy)
		{
		case Enemy.Charlie:
			return RecoveryTestBuild.Unlocked(UserGameSettings.unlockedCharlie);
		case Enemy.Dog:
			return RecoveryTestBuild.Unlocked(UserGameSettings.unlockedDog);
		case Enemy.Double:
			return RecoveryTestBuild.Unlocked(UserGameSettings.unlockedDoubleEnemyMode);
		case Enemy.Witch:
			return RecoveryTestBuild.Unlocked(UserGameSettings.unlockedWitch);
		default:
			return true;
		}
	}

	public void SetEnemy(int enemyID)
	{
		UserGameSettings.currentEnemy = enemiesList[enemyID];
		SetEnemy(UserGameSettings.currentEnemy);
	}

	public void SetEnemy(Enemy enemy)
	{
		if (IsEnemyUnlocked(enemy))
		{
			UserGameSettings.currentEnemy = enemy;
		}
		SetPlayButton();
	}

	public void SetDifficulty(int index)
	{
		if (index >= 0 && index <= 4)
		{
			UserGameSettings.currentDifficulty = (Difficulty)index;
		}
	}

	public void Play()
	{
		if (UserGameSettings.isImmersiveMode && UserGameSettings.unlockedImmersiveOneTime)
		{
			UserGameSettings.unlockedImmersiveOneTime = false;
		}
		if (UserGameSettings.currentEnemy == Enemy.Custom)
		{
			MenuContentWindow.Instance.HideAllWindows();
			CustomKrasue.Instance.ShowCustomKrasue();
			Screens.View(ScreenId.CustomKrasue);
			return;
		}
		// An expert-menu combination that matches a chapter counts as that preset.
		UserGameSettings.currentPreset = null;
		Enemy enemy = UserGameSettings.currentEnemy;
		Level level = UserGameSettings.currentLevel;
		Mode mode = UserGameSettings.currentMode;
		if (enemy == Enemy.Krasue && level == Level.Mansion)
		{
			UserGameSettings.currentPreset = StoreItems.KRASUE_MANSION;
		}
		else if (enemy == Enemy.Charlie && level == Level.Hospital)
		{
			UserGameSettings.currentPreset = StoreItems.CHARLIE_HOSPITAL;
		}
		else if (enemy == Enemy.Dog && level == Level.School)
		{
			UserGameSettings.currentPreset = StoreItems.GOOD_BOY_SCHOOL;
		}
		else if (enemy == Enemy.Witch && level == Level.Mansion)
		{
			UserGameSettings.currentPreset = StoreItems.WITCH;
		}
		else if (enemy == Enemy.Goose && level == Level.Mansion)
		{
			UserGameSettings.currentPreset = StoreItems.GOOSE_MANSION;
		}
		else if (enemy == Enemy.Krasue && level == Level.Hospital && mode == Mode.HALLOWEEN)
		{
			UserGameSettings.currentPreset = StoreItems.TRICK_OR_TREAT;
		}
		else if (enemy == Enemy.Krasue && level == Level.Mansion && mode == Mode.PIXEL)
		{
			UserGameSettings.currentPreset = StoreItems.PIXEL;
		}
		UserGameSettings.expertMode = true;
		if (!UserGameSettings.IsMultiplayer())
		{
			Ads.SetNextInterstitialTrigger(AdTrigger.Play);
#if RECOVERY_PC_MENU
			if (PcMenuInput.BlockMissingScene("PreGameAd"))
			{
				return;
			}
#endif
			SceneManager.LoadScene("PreGameAd");
		}
		else
		{
			SceneManager.LoadScene("Loading");
		}
	}

	protected override void OnDestroy()
	{
		MessageKit<string>.removeObserver(34, UnlockProduct);
		MessageKit<StoreItems>.removeObserver(133, UnlockOneTime);
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}
