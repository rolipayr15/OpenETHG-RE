using System.Reflection;
using CodeStage.AntiCheat.Storage;
using EyesAnalytics;
using Prime31.MessageKit;

[Obfuscation(Exclude = false)]
public class EncryptedValuesManager : CustomSingleton<EncryptedValuesManager>
{
	private void Start()
	{
		if (!UserGameSettings.googleNBO)
		{
			if (ObscuredPrefs.GetInt("ENEMY_CHARLIE", 0) == 1)
			{
				UserGameSettings.unlockedCharlie = true;
			}
			if (ObscuredPrefs.GetInt("HOSPITAL_LEVEL", 0) == 1)
			{
				UserGameSettings.unlockedHospital = true;
			}
			if (ObscuredPrefs.GetInt("SCHOOL_LEVEL", 0) == 1)
			{
				UserGameSettings.unlockedSchool = true;
			}
			if (ObscuredPrefs.GetInt("MODE_ENDLESS", 0) == 1)
			{
				UserGameSettings.unlockedEndlessMode = true;
			}
			if (ObscuredPrefs.GetInt("PIXEL", 0) == 1)
			{
				UserGameSettings.unlockedPixelMode = true;
			}
			if (ObscuredPrefs.GetInt("MODE_DOUBLE_ENEMY", 0) == 1)
			{
				UserGameSettings.unlockedDoubleEnemyMode = true;
			}
			if (ObscuredPrefs.GetInt("MODE_TRICK_OR_TREAT", 0) == 1)
			{
				UserGameSettings.unlockedTrickOrTreatMode = true;
			}
			if (ObscuredPrefs.GetInt("DOG", 0) == 1)
			{
				UserGameSettings.unlockedDog = true;
			}
			if (ObscuredPrefs.GetInt("CHARLIE_HOSPITAL", 0) == 1)
			{
				UserGameSettings.unlockedCharlieHospital = true;
				UserGameSettings.unlockedCharlie = true;
				UserGameSettings.unlockedHospital = true;
			}
			if (ObscuredPrefs.GetInt("GOOD_BOY_SCHOOL", 0) == 1)
			{
				UserGameSettings.unlockedGoodBoySchool = true;
				UserGameSettings.unlockedDog = true;
				UserGameSettings.unlockedSchool = true;
			}
			if (ObscuredPrefs.GetInt("WITCH", 0) == 1)
			{
				UserGameSettings.unlockedWitch = true;
			}
			if (ObscuredPrefs.GetInt("IMMERSIVE", 0) == 1)
			{
				UserGameSettings.unlockedImmersive = true;
			}
		}
		else
		{
			UnlockAllContent();
		}
		UserGameSettings.playingForTheFirstTime = ObscuredPrefs.GetInt("PLAYING_FIRST_TIME", 1) == 1;
		PlayerProfile.SetHasRemoveAds(UserGameSettings.unlockedNoAds);
		FirebaseChapterOwnershipUserProperties.MarkEntitlementsHydrated();
		FirebaseWinbackRewardSequence.MarkPlayerStateHydrated();
	}

	public void SetNextVideoDate(string value)
	{
		ObscuredPrefs.SetString("NEXT_VIDEO", value);
	}

	private void UnlockAllContent()
	{
		UserGameSettings.unlockedCharlie = true;
		UserGameSettings.unlockedHospital = true;
		UserGameSettings.unlockedSchool = true;
		UserGameSettings.unlockedEndlessMode = true;
		UserGameSettings.unlockedTrickOrTreatMode = true;
		UserGameSettings.unlockedDoubleEnemyMode = true;
		UserGameSettings.unlockedDog = true;
		UserGameSettings.unlockedPixelMode = true;
		UserGameSettings.unlockedCharlieHospital = true;
		UserGameSettings.unlockedGoodBoySchool = true;
		UserGameSettings.unlockedWitch = true;
		UserGameSettings.unlockedImmersive = true;
	}

	public void UnlockProduct(string productId, bool isUnlocked)
	{
		if (!isUnlocked)
		{
			return;
		}
		switch (productId)
		{
		case "bundle_1":
			UserGameSettings.unlockedCharlieHospital = true;
			UserGameSettings.unlockedDog = true;
			UserGameSettings.unlockedGoodBoySchool = true;
			UserGameSettings.unlockedWitch = true;
			UserGameSettings.unlockedBundle1 = true;
			UserGameSettings.unlockedCharlie = true;
			UserGameSettings.unlockedHospital = true;
			UserGameSettings.unlockedSchool = true;
			break;
		case "bundle_2":
			UnlockAllContent();
			UserGameSettings.unlockedBundle2 = true;
			break;
		case "bundle_3":
			UnlockAllContent();
			UserGameSettings.unlockedNoAds = true;
			UserGameSettings.unlockedBirthdayDiamond = true;
			UserGameSettings.unlockedMultiplayerUnlimited = true;
			UserGameSettings.unlockedBundle3 = true;
			break;
		case "multiplayer_week":
			UserGameSettings.unlockedMultiplayerSub = true;
			UnlockAllContent();
			UserGameSettings.unlockedNoAds = true;
			UserGameSettings.googleNBO = true;
			break;
		case "multiplayer_month":
			UnlockAllContent();
			UserGameSettings.unlockedNoAds = true;
			UserGameSettings.googleNBO = true;
			UserGameSettings.unlockedMultiplayerSub = true;
			break;
		case "multiplayer_keys_unlimited":
			UserGameSettings.unlockedMultiplayerUnlimited = true;
			break;
		case "multiplayer_keys":
			Currency.SendCurrencyEvent(CurrencyFlowType.Source, CurrencyType.MultiplayerKeys, UserGameSettings.purchaseTickets, CurrencyItemType.InAppSource, CurrencyItemId.InApp_StandardPack);
			KeysManager.AddMultiplayerKeys(UserGameSettings.purchaseTickets);
			break;
		case "remove_all_ads":
			UnlockAllContent();
			UserGameSettings.unlockedNoAds = true;
			UserGameSettings.googleNBO = true;
			UserGameSettings.unlockedMultiplayerSub = true;
			Breadcrumbs.Log("unlocked_nbo");
			break;
		case "birthday_gold":
			UnlockAllContent();
			UserGameSettings.unlockedBirthdayGold = true;
			break;
		case "birthday_platinium":
			UnlockAllContent();
			UserGameSettings.unlockedNoAds = true;
			UserGameSettings.unlockedBirthdayPlatinium = true;
			break;
		case "birthday_diamond":
			UnlockAllContent();
			UserGameSettings.unlockedNoAds = true;
			UserGameSettings.unlockedBirthdayDiamond = true;
			break;
		case "eyes.noads":
			UserGameSettings.unlockedNoAds = true;
			break;
		case "charliehospital":
		case "charlie_hospital":
			UserGameSettings.unlockedCharlieHospital = true;
			UserGameSettings.unlockedCharlie = true;
			UserGameSettings.unlockedHospital = true;
			break;
		case "goodboyschool":
		case "good_boy_school":
			UserGameSettings.unlockedGoodBoySchool = true;
			UserGameSettings.unlockedSchool = true;
			UserGameSettings.unlockedDog = true;
			break;
		case "trickortreat":
			UserGameSettings.unlockedTrickOrTreatMode = true;
			break;
		case "witch":
			UserGameSettings.unlockedWitch = true;
			break;
		case "immersive":
			UserGameSettings.unlockedImmersive = true;
			break;
		case "doubleenemy":
			UserGameSettings.unlockedDoubleEnemyMode = true;
			break;
		case "eyes.hospital":
			UserGameSettings.unlockedHospital = true;
			break;
		case "endless":
			UserGameSettings.unlockedEndlessMode = true;
			break;
		case "pixel":
			UserGameSettings.unlockedPixelMode = true;
			break;
		case "school":
			UserGameSettings.unlockedSchool = true;
			break;
		case "charlie":
			UserGameSettings.unlockedCharlie = true;
			break;
		case "dog":
			UserGameSettings.unlockedDog = true;
			break;
		}
		OnEntitlementsChanged(productId);
	}

	public void UnlockProductWithCoins(string productId)
	{
		switch (productId)
		{
		case "trickortreat":
			UserGameSettings.unlockedTrickOrTreatMode = true;
			ObscuredPrefs.SetInt("MODE_TRICK_OR_TREAT", 1);
			break;
		case "witch":
			UserGameSettings.unlockedWitch = true;
			ObscuredPrefs.SetInt("WITCH", 1);
			break;
		case "immersive":
			UserGameSettings.unlockedImmersive = true;
			ObscuredPrefs.SetInt("IMMERSIVE", 1);
			break;
		case "doubleenemy":
			UserGameSettings.unlockedDoubleEnemyMode = true;
			UserGameSettings.unlockedSchool = true;
			ObscuredPrefs.SetInt("MODE_DOUBLE_ENEMY", 1);
			ObscuredPrefs.SetInt("SCHOOL_LEVEL", 1);
			break;
		case "good_boy_school":
			UserGameSettings.unlockedGoodBoySchool = true;
			UserGameSettings.unlockedSchool = true;
			UserGameSettings.unlockedDog = true;
			ObscuredPrefs.SetInt("DOG", 1);
			ObscuredPrefs.SetInt("GOOD_BOY_SCHOOL", 1);
			ObscuredPrefs.SetInt("SCHOOL_LEVEL", 1);
			break;
		case "eyes.hospital":
			UserGameSettings.unlockedHospital = true;
			ObscuredPrefs.SetInt("HOSPITAL_LEVEL", 1);
			break;
		case "school":
			UserGameSettings.unlockedSchool = true;
			ObscuredPrefs.SetInt("SCHOOL_LEVEL", 1);
			break;
		case "endless":
			UserGameSettings.unlockedEndlessMode = true;
			ObscuredPrefs.SetInt("MODE_ENDLESS", 1);
			break;
		case "pixel":
			UserGameSettings.unlockedPixelMode = true;
			ObscuredPrefs.SetInt("PIXEL", 1);
			break;
		case "charlie_hospital":
			UserGameSettings.unlockedCharlie = true;
			UserGameSettings.unlockedHospital = true;
			UserGameSettings.unlockedCharlieHospital = true;
			ObscuredPrefs.SetInt("CHARLIE_HOSPITAL", 1);
			ObscuredPrefs.SetInt("HOSPITAL_LEVEL", 1);
			ObscuredPrefs.SetInt("ENEMY_CHARLIE", 1);
			break;
		case "dog":
			UserGameSettings.unlockedDog = true;
			ObscuredPrefs.SetInt("DOG", 1);
			break;
		case "charlie":
			UserGameSettings.unlockedCharlie = true;
			ObscuredPrefs.SetInt("ENEMY_CHARLIE", 1);
			break;
		}
		OnEntitlementsChanged(productId);
	}

	private static void OnEntitlementsChanged(string productId)
	{
		PlayerProfile.SetHasRemoveAds(UserGameSettings.unlockedNoAds);
		FirebaseChapterOwnershipUserProperties.Sync();
		FirebaseWinbackRewardSequence.Refresh();
		MessageKit<string>.post(34, productId);
	}

	public bool IsProductUnlocked(string productId)
	{
		switch (productId)
		{
		case "trickortreat":
			return UserGameSettings.unlockedTrickOrTreatMode;
		case "eyes.noads":
			return UserGameSettings.unlockedNoAds;
		case "witch":
			return UserGameSettings.unlockedWitch;
		case "immersive":
			return UserGameSettings.unlockedImmersive;
		case "doubleenemy":
			return UserGameSettings.unlockedDoubleEnemyMode;
		case "good_boy_school":
			return UserGameSettings.unlockedGoodBoySchool;
		case "bundle_1":
			return UserGameSettings.unlockedBundle1;
		case "bundle_2":
			return UserGameSettings.unlockedBundle2;
		case "bundle_3":
			return UserGameSettings.unlockedBundle3;
		case "multiplayer_keys_unlimited":
			return UserGameSettings.unlockedMultiplayerUnlimited;
		case "multiplayer_week":
		case "multiplayer_month":
			return UserGameSettings.unlockedMultiplayerSub;
		case "birthday_gold":
			return UserGameSettings.unlockedBirthdayGold;
		case "birthday_platinium":
			return UserGameSettings.unlockedBirthdayPlatinium;
		case "birthday_diamond":
			return UserGameSettings.unlockedBirthdayDiamond;
		case "remove_all_ads":
			return UserGameSettings.googleNBO;
		case "charlie_hospital":
			return UserGameSettings.unlockedCharlieHospital;
		case "dog":
			return UserGameSettings.unlockedDog;
		case "charlie":
			return UserGameSettings.unlockedCharlie;
		case "eyes.hospital":
			return UserGameSettings.unlockedHospital;
		case "endless":
			return UserGameSettings.unlockedEndlessMode;
		case "pixel":
			return UserGameSettings.unlockedPixelMode;
		case "school":
			return UserGameSettings.unlockedSchool;
		default:
			return false;
		}
	}

	public bool IsProductUnlocked(StoreItems storeItem)
	{
		switch (storeItem)
		{
		case StoreItems.HOSPITAL:
			return UserGameSettings.unlockedHospital;
		case StoreItems.DOUBLE_TROUBLE:
			return UserGameSettings.unlockedDoubleEnemyMode;
		case StoreItems.ENDLESS:
			return UserGameSettings.unlockedEndlessMode;
		case StoreItems.TRICK_OR_TREAT:
			return UserGameSettings.unlockedTrickOrTreatMode;
		case StoreItems.REMOVE_ADS:
			return UserGameSettings.unlockedNoAds;
		case StoreItems.SCHOOL:
			return UserGameSettings.unlockedSchool;
		case StoreItems.KRASUE_MANSION:
		case StoreItems.GOOSE_MANSION:
			return true;
		case StoreItems.CHARLIE_HOSPITAL:
			return UserGameSettings.unlockedCharlieHospital;
		case StoreItems.GOOD_BOY_SCHOOL:
			return UserGameSettings.unlockedGoodBoySchool;
		case StoreItems.PIXEL:
			return UserGameSettings.unlockedPixelMode;
		case StoreItems.WITCH:
			return UserGameSettings.unlockedWitch;
		case StoreItems.IMMERSIVE:
			return UserGameSettings.unlockedImmersive;
		case StoreItems.MULTIPLAYER_MONTH:
		case StoreItems.MULTIPLAYER_WEEK:
			return UserGameSettings.unlockedMultiplayerSub;
		case StoreItems.BIRTHDAY_GOLD:
			return UserGameSettings.unlockedBirthdayGold;
		case StoreItems.BIRTHDAY_PLATINIUM:
			return UserGameSettings.unlockedBirthdayPlatinium;
		case StoreItems.BIRTHDAY_DIAMOND:
			return UserGameSettings.unlockedBirthdayDiamond;
		case StoreItems.BUNDLE_1:
			return UserGameSettings.unlockedBundle1;
		case StoreItems.BUNDLE_2:
			return UserGameSettings.unlockedBundle2;
		case StoreItems.BUNDLE_3:
			return UserGameSettings.unlockedBundle3;
		case StoreItems.MULTIPLAYER_KEYS_UNLIMITED:
			return UserGameSettings.unlockedMultiplayerUnlimited;
		default:
			return false;
		}
	}

	private void SetWelcomeBundle()
	{
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}
