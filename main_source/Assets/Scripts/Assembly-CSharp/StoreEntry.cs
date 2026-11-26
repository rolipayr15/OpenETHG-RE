using System.Collections.Generic;
using EyesAnalytics;
using Prime31.MessageKit;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class StoreEntry : MonoBehaviour
{
	[SerializeField]
	private StoreItems storeItem;

	private string inAppName;

	[SerializeField]
	private GameObject buttonPlay;

	[SerializeField]
	private GameObject buttonUnlockNow;

	[SerializeField]
	private HorizontalScrollSnap levelScroll;

	[SerializeField]
	private RawImage texturePlaceholder;

	[SerializeField]
	private GameObject difficulty;

	[SerializeField]
	private StarsForLevelCompletion starsForLevelCompletion;

	[SerializeField]
	private GameObject journal;

	[SerializeField]
	private Image padlock;

	private bool isOneTimeUnlocked;

	private Dictionary<int, Difficulty> difficultyDict = new Dictionary<int, Difficulty>
	{
		{ 1, Difficulty.Casual },
		{ 2, Difficulty.Easy },
		{ 3, Difficulty.Normal },
		{ 4, Difficulty.Hard },
		{ 5, Difficulty.Nightmare }
	};

	public static readonly StoreItems[] PlayablePresets = new StoreItems[9]
	{
		StoreItems.KRASUE_MANSION,
		StoreItems.CHARLIE_HOSPITAL,
		StoreItems.GOOD_BOY_SCHOOL,
		StoreItems.TRICK_OR_TREAT,
		StoreItems.DOUBLE_TROUBLE,
		StoreItems.PIXEL,
		StoreItems.ENDLESS,
		StoreItems.WITCH,
		StoreItems.GOOSE_MANSION
	};

	public StoreItems GetStoreItem()
	{
		return storeItem;
	}

	public GameObject GetButtonPlay()
	{
		return buttonPlay;
	}

	public void UnlockNow()
	{
		MenuPurchase.Show(storeItem, CartType.MainMenu);
	}

	private bool ShouldBeUnlockInGoldBundle(StoreItems storeI)
	{
		switch (storeI)
		{
		case StoreItems.CHARLIE:
		case StoreItems.HOSPITAL:
		case StoreItems.GOOD_BOY:
		case StoreItems.SCHOOL:
		case StoreItems.CHARLIE_HOSPITAL:
		case StoreItems.GOOD_BOY_SCHOOL:
		case StoreItems.WITCH:
			return true;
		default:
			return false;
		}
	}

	private static bool IsFree(StoreItems item)
	{
		return RecoveryTestBuild.FreeEverything || item == StoreItems.KRASUE_MANSION || item == StoreItems.GOOSE_MANSION;
	}

	private void Start()
	{
		if (IsFree(storeItem))
		{
			ShowUnlockedState();
		}
		else
		{
			inAppName = Purchaser.Instance.GetInAppName(storeItem);
			MessageKit<string>.addObserver(34, UnlockProduct);
			MessageKit<StoreItems>.addObserver(133, UnlockOneTime);
			CheckIfInAppIsUnlocked();
		}
		texturePlaceholder.texture = storeItem == StoreItems.PIXEL
			? MonsterLevelPresentationManager.Instance.GetSmallRenderTexture()
			: MonsterLevelPresentationManager.Instance.GetStandardRenderTexture();
		GetPlayerProgress();
	}

	private void OnDestroy()
	{
		MessageKit<string>.removeObserver(34, UnlockProduct);
		MessageKit<StoreItems>.removeObserver(133, UnlockOneTime);
	}

	private void ShowUnlockedState()
	{
		isOneTimeUnlocked = false;
		if (buttonUnlockNow != null)
		{
			buttonUnlockNow.SetActive(false);
		}
		if (buttonPlay != null)
		{
			buttonPlay.SetActive(true);
		}
		if (difficulty != null)
		{
			difficulty.SetActive(true);
		}
		if (starsForLevelCompletion != null)
		{
			starsForLevelCompletion.gameObject.SetActive(true);
		}
		if (journal != null)
		{
			journal.SetActive(true);
		}
		if (padlock != null)
		{
			padlock.enabled = false;
		}
	}

	private void UnlockProduct(string productId)
	{
		if (productId == inAppName || productId == "birthday_diamond" || productId == "birthday_gold" || productId == "birthday_platinium" || productId == "bundle_2" || productId == "bundle_3" || (productId == "bundle_1" && ShouldBeUnlockInGoldBundle(storeItem)))
		{
			ShowUnlockedState();
		}
	}

	private void UnlockOneTime(StoreItems _storeItem)
	{
		if (storeItem != _storeItem)
		{
			return;
		}
		isOneTimeUnlocked = true;
		buttonUnlockNow.SetActive(false);
		buttonPlay.SetActive(true);
		if (difficulty != null)
		{
			difficulty.SetActive(true);
		}
		if (padlock != null)
		{
			padlock.enabled = false;
		}
		if (starsForLevelCompletion != null)
		{
			starsForLevelCompletion.gameObject.SetActive(false);
		}
		if (journal != null)
		{
			journal.SetActive(false);
		}
	}

	private void CheckIfInAppIsUnlocked()
	{
		if (EncryptedValuesManager.Instance.IsProductUnlocked(storeItem))
		{
			UnlockProduct(inAppName);
			return;
		}
		if (AdTicketStore.HasTicket(storeItem))
		{
			UnlockOneTime(storeItem);
			return;
		}
		if (difficulty != null)
		{
			difficulty.SetActive(false);
		}
		if (starsForLevelCompletion != null)
		{
			starsForLevelCompletion.gameObject.SetActive(false);
		}
		if (journal != null)
		{
			journal.SetActive(false);
		}
		if (padlock != null)
		{
			padlock.enabled = true;
		}
		buttonPlay.SetActive(false);
		buttonUnlockNow.SetActive(true);
	}

	public void Play()
	{
		UserGameSettings.currentPreset = storeItem;
		if (AdTicketStore.HasTicket(storeItem))
		{
			UserGameSettings.isOneTimeUnlocked = isOneTimeUnlocked;
		}
		else if (!RecoveryTestBuild.FreeEverything && !EncryptedValuesManager.Instance.IsProductUnlocked(storeItem))
		{
			return;
		}
		switch (storeItem)
		{
		case StoreItems.KRASUE_MANSION:
			SetPreset(Mode.STANDARD, Enemy.Krasue, Level.Mansion);
			break;
		case StoreItems.CHARLIE_HOSPITAL:
			SetPreset(Mode.STANDARD, Enemy.Charlie, Level.Hospital);
			break;
		case StoreItems.GOOD_BOY_SCHOOL:
			SetPreset(Mode.STANDARD, Enemy.Dog, Level.School);
			break;
		case StoreItems.TRICK_OR_TREAT:
			SetPreset(Mode.HALLOWEEN, Enemy.Krasue, Level.Hospital);
			break;
		case StoreItems.DOUBLE_TROUBLE:
			SetPreset(Mode.STANDARD, Enemy.Double, Level.School);
			break;
		case StoreItems.ENDLESS:
			SetPreset(Mode.ENDLESS, Enemy.Dog, Level.Hospital);
			UserGameSettings.currentDifficulty = Difficulty.Normal;
			break;
		case StoreItems.PIXEL:
			SetPreset(Mode.PIXEL, Enemy.Krasue, Level.Mansion);
			break;
		case StoreItems.WITCH:
			SetPreset(Mode.STANDARD, Enemy.Witch, Level.Mansion);
			break;
		case StoreItems.GOOSE_MANSION:
			SetPreset(Mode.STANDARD, Enemy.Goose, Level.Mansion);
			break;
		}
		// Endless always runs on Normal; everything else uses the difficulty picked on the scroll.
		UserGameSettings.currentDifficulty = storeItem == StoreItems.ENDLESS ? Difficulty.Normal : difficultyDict[levelScroll.CurrentPage + 1];
		UserGameSettings.currentGameMode = GameMode.SINGLE;
		UserGameSettings.platform = Platform.Singleplayer;
		UserGameSettings.isImmersiveMode = false;
		UserGameSettings.expertMode = false;
		Ads.SetNextInterstitialTrigger(AdTrigger.Play);
#if RECOVERY_PC_MENU
		if (PcMenuInput.BlockMissingScene("PreGameAd"))
		{
			return;
		}
#endif
		SceneManager.LoadScene("PreGameAd");
	}

	private static void SetPreset(Mode mode, Enemy enemy, Level level)
	{
		UserGameSettings.currentMode = mode;
		UserGameSettings.currentEnemy = enemy;
		UserGameSettings.currentLevel = level;
	}

	private void GetPlayerProgress()
	{
		string key;
		switch (storeItem)
		{
		case StoreItems.ENDLESS:
			return;
		case StoreItems.TRICK_OR_TREAT:
			key = "DIFFICULTY_LEVEL_WON_TRICK_OR_TREAT";
			break;
		case StoreItems.KRASUE_MANSION:
			key = "DIFFICULTY_LEVEL_WON_KRASUE_MANSION";
			break;
		case StoreItems.DOUBLE_TROUBLE:
			key = "DIFFICULTY_LEVEL_WON_DOUBLE_TROUBLE";
			break;
		case StoreItems.CHARLIE_HOSPITAL:
			key = "DIFFICULTY_LEVEL_WON_CHARLIE_HOSPITAL";
			break;
		case StoreItems.GOOD_BOY_SCHOOL:
			key = "DIFFICULTY_LEVEL_WON_GOOD_BOY_SCHOOL";
			break;
		case StoreItems.PIXEL:
			key = "DIFFICULTY_LEVEL_WON_PIXEL";
			break;
		case StoreItems.WITCH:
			key = "DIFFICULTY_LEVEL_WON_WITCH";
			break;
		case StoreItems.GOOSE_MANSION:
			key = "DIFFICULTY_LEVEL_WON_GOOSE_MANSION";
			break;
		default:
			starsForLevelCompletion.SetLevelNotCompleted();
			return;
		}
		int highestWon = PlayerPrefs.GetInt(key, 0);
		if (highestWon != 0)
		{
			starsForLevelCompletion.SetHighestDifficultyCompleted(difficultyDict[highestWon]);
		}
		else
		{
			starsForLevelCompletion.SetLevelNotCompleted();
		}
	}
}
