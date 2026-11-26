using System;
using System.Collections.Generic;
using EyesAnalytics;
using UnityEngine.Purchasing;

public abstract class PurchaserBase : CustomSingleton<Purchaser>
{
	public const string GOOD_BOY = "dog";

	public const string CHARLIE = "charlie";

	public const string SCHOOL = "school";

	public const string DOUBLE_TROUBLE = "doubleenemy";

	public const string ENDLESS = "endless";

	public const string TRICK_OR_TREAT = "trickortreat";

	public const string HOSPITAL = "eyes.hospital";

	public const string REMOVE_ADS = "eyes.noads";

	public const string GOOGLE_NBO = "remove_all_ads";

	public const string CHARLIE_HOSPITAL = "charlie_hospital";

	public const string CHARLIE_HOSPITAL_OLD = "charliehospital";

	public const string GOOD_BOY_SCHOOL = "good_boy_school";

	public const string GOOD_BOY_SCHOOL_OLD = "goodboyschool";

	public const string MULTIPLAYER_MONTH = "multiplayer_month";

	public const string MULTIPLAYER_WEEK = "multiplayer_week";

	public const string MULTIPLAYER_KEYS = "multiplayer_keys";

	public const string BIRTHDAY_GOLD = "birthday_gold";

	public const string BIRTHDAY_PLATINIUM = "birthday_platinium";

	public const string BIRTHDAY_DIAMOND = "birthday_diamond";

	public const string MULTIPLAYER_KEYS_UNLIMITED = "multiplayer_keys_unlimited";

	public SubscriptionInfo subInfo;

	public const string KRASUE = "KRASUE";

	public const string CUSTOM_KRASUE = "CUSTOM_KRASUE";

	public const string SANDBOX = "SANDBOX";

	public const string MODE_STANDARD = "MODE_STANDARD";

	public const string PIXEL = "pixel";

	public const string MANSION = "MANSION";

	public const string KRASUE_MANSION = "KRASUE_MANSION";

	public const string WITCH = "witch";

	public const string IMMERSIVE = "immersive";

	public const string BUNDLE_1 = "bundle_1";

	public const string BUNDLE_2 = "bundle_2";

	public const string BUNDLE_3 = "bundle_3";

	public Dictionary<StoreItems, string> storeItems;

	public string GetInAppName(StoreItems storeItem)
	{
		return null;
	}

	public abstract void Init();

	public abstract bool IsInitialized();

	public abstract void RestorePurchasesButton();

	public abstract string GetProductPrice(string productId);

	public abstract void BuyProductID(string productId, Action<Product, Order> onSuccess = null, Action<string, string> onFailure = null);

	public virtual void BuyProductID(string productId, IAP.PurchaseContext analyticsContext, Action<Product, Order> onSuccess = null, Action<string, string> onFailure = null)
	{
	}

	protected override void OnDestroy()
	{
	}

	protected override void OnApplicationQuit()
	{
	}
}
