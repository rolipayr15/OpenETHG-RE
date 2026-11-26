using Gameplay.Eggs;
using Eyes.DailyChallenges;
using Firebase.Crashlytics;
using I2.Loc;
using EyesAnalytics;
using Bibloteka;
using DarkTonic.MasterAudio;
using Prime31.MessageKit;
using System.Globalization;
using System;
using System.Collections.Generic;
using Multiplayer;
using UnityEngine;
using UnityEngine.UI;

public class MoneyBagsManager : CustomSingleton<MoneyBagsManager>
{
	private int bagsToCollect;

	private int bagsCollected;

	private int? displayedBagCount;

	private int? displayedBagTarget;

	private System.Random gooseCounterRandom = new System.Random();

	private int? gooseCounterValue;

	private string gooseCounterExpression;

	private int gooseCounterPattern = -1;

	private bool canEscape;

	private int[] bagsToActivate;

	[SerializeField]
	private MoneyBag tutorialMoneyBag;

	public List<string> activatedBags;

	[SerializeField]
	private Text vsOpponentCountLabel;

	public int guestBagsCollected;

	private void Start()
	{
		if (!UserGameSettings.IsMultiplayer())
		{
			ActivateBags();
			return;
		}
		CustomSingleton<MultiplayerSettings>.Instance.RegisterOnResult(OnResult);
		UserGameSettings.myBagsCollected = 0;
	}

	private void Init()
	{
		UserGameSettings.bagsCollected = 0;
		LevelConfig config = new LevelConfig(UserGameSettings.currentDifficulty, UserGameSettings.currentLevel, UserGameSettings.IsMultiplayer() ? (Platform)1 : (Platform)0, UserGameSettings.currentEnemy);
		bagsToCollect = config.GetBagsToCollect();
		bagsToActivate = config.GetBagsToActivate();
		UserGameSettings.bagsToCollect = bagsToCollect;
		HowManyBagsLeft();
	}

	public void ActivateBags()
	{
		Init();
		GameObject[] bags = GameObject.FindGameObjectsWithTag("MoneyBag");
		activatedBags = new List<string>();
		if (UserGameSettings.IsMultiplayer() && !UserGameSettings.IsMultiplayerCoop())
		{
			foreach (GameObject bag in bags)
			{
				activatedBags.Add(bag.transform.position.sqrMagnitude.ToString(CultureInfo.InvariantCulture));
			}
			HowManyBagsLeft();
			return;
		}
		bags.Shuffle();
		int[] perFloor = new int[3];
		int total = bagsToActivate[0] + bagsToActivate[1] + bagsToActivate[2];
		int used = 0;
		foreach (GameObject bag in bags)
		{
			if (TutorialManager.ShouldPlayTutorial() && bag.transform.position.sqrMagnitude == tutorialMoneyBag.transform.position.sqrMagnitude)
			{
				// The tutorial bag always stays and does not count towards the floor quotas.
				continue;
			}
			if (used < total)
			{
				int floor = CustomSingleton<FloorManager>.Instance.GetFloor(bag.transform.position);
				if (perFloor[floor] < bagsToActivate[floor])
				{
					perFloor[floor]++;
					used++;
					activatedBags.Add(bag.transform.position.sqrMagnitude.ToString(CultureInfo.InvariantCulture));
					continue;
				}
			}
			bag.SetActive(false);
		}
		HowManyBagsLeft();
	}

	public List<string> ActivateBagsAsHost()
	{
		ActivateBags();
		return activatedBags;
	}

	public void ActivateBagsFromHost(List<string> activatedBagIds)
	{
		Init();
		foreach (GameObject bag in GameObject.FindGameObjectsWithTag("MoneyBag"))
		{
			if (!activatedBagIds.Contains(bag.transform.position.sqrMagnitude.ToString(CultureInfo.InvariantCulture)))
			{
				bag.SetActive(false);
			}
		}
		HowManyBagsLeft();
	}

	public void CollectBag(string moneyBagId)
	{
		Game.CountBag((int)Time.timeSinceLevelLoad);
		MessageKit<string>.post(19, moneyBagId);
		MasterAudio.PlaySound3DAtTransform("fx", CustomSingleton<PlayerManager>.Instance.Player.transform, 1f, null, 0f, "takeIt");
		if (UserGameSettings.IsMultiplayer() && canEscape)
		{
			return;
		}
		bagsCollected++;
		UserGameSettings.bagsCollected++;
		DailyChallengeRuntime.BagCollected(this);
		if (bagsCollected == 3)
		{
			Game.NoteActivationCause((ActivationCause)3);
			MessageKit.post(25);
		}
		HowManyBagsLeft();
		if (FirebaseState.IsInitialized)
		{
			Crashlytics.Log("Bag collected " + UserGameSettings.bagsCollected);
		}
	}

	public void CollectBagByOtherPlayer(string bagId)
	{
		if (!UserGameSettings.IsMultiplayerVs())
		{
			Breadcrumbs.Log("bag_collected");
		}
		MasterAudio.PlaySound3DAtTransform("fx", CustomSingleton<PlayerManager>.Instance.Player.transform, 0.1f, null, 0f, "takeIt");
	}

	private void HowManyBagsLeft()
	{
		if ((int)UserGameSettings.currentMode == 1)
		{
			ShowBagCount(bagsCollected);
		}
		else if (!UserGameSettings.IsMultiplayer() && bagsToCollect <= bagsCollected)
		{
			canEscape = true;
			ShowObjectiveText(ScriptLocalization.FindTheExitAndEscape);
			Breadcrumbs.Log("all_bags_collected");
		}
		else
		{
			ShowBagCount(bagsCollected, bagsToCollect);
		}
		if (UserGameSettings.IsMultiplayer() && UserGameSettings.IsMultiplayerVs())
		{
			vsOpponentCountLabel.text = guestBagsCollected + "/" + bagsToCollect;
		}
	}

	private void ShowBagCount(int count, int? target = null)
	{
		displayedBagCount = count;
		displayedBagTarget = target;
		RefreshBagCounter();
	}

	private void ShowObjectiveText(string text)
	{
		gooseCounterExpression = null;
		displayedBagTarget = null;
		displayedBagCount = null;
		gooseCounterValue = null;
		MessageKit<string>.post(22, text);
	}

	public void RefreshBagCounter()
	{
		if (!displayedBagCount.HasValue)
		{
			return;
		}
		string plain = displayedBagCount.Value.ToString(CultureInfo.InvariantCulture);
		string text = HasLocalArithmeticPrank() ? GooseCounterExpression(displayedBagCount.Value) : plain;
		if (displayedBagTarget.HasValue && text != plain)
		{
			text = "(" + text + ")";
		}
		if (displayedBagTarget.HasValue)
		{
			text = text + "/" + displayedBagTarget.Value.ToString(CultureInfo.InvariantCulture);
		}
		MessageKit<string>.post(22, text);
	}

	private static bool HasLocalArithmeticPrank()
	{
		PlayerManager playerManager = CustomSingleton<PlayerManager>.HasInstance ? CustomSingleton<PlayerManager>.Instance : FindFirstObjectByType<PlayerManager>();
		GameObject player = playerManager != null ? playerManager.Player : null;
		EggEffects effects = player != null ? player.GetComponent<EggEffects>() : null;
		return effects != null && effects.IsLocalPlayer && effects.HasArithmeticPrank;
	}

	private string GooseCounterExpression(int count)
	{
		if (gooseCounterValue.GetValueOrDefault() == count && gooseCounterValue.HasValue && gooseCounterExpression != null)
		{
			return gooseCounterExpression;
		}
		int upper = count > 1 ? 4 : 2;
		int lower = count < 2 ? 1 : 0;
		int pattern = gooseCounterRandom.Next(lower, upper | lower);
		if (pattern == gooseCounterPattern)
		{
			pattern = (pattern - lower + gooseCounterRandom.Next(1, upper)) % upper + lower;
		}
		long value = count;
		string expression;
		if (pattern == 2)
		{
			int a = gooseCounterRandom.Next(1, 10);
			int b = gooseCounterRandom.Next(1, 10);
			expression = string.Format(CultureInfo.InvariantCulture, "{0} + {1} − {2}", value - a + (b + a), a, b + a);
		}
		else if (pattern == 1)
		{
			int a = gooseCounterRandom.Next(1, 10);
			expression = string.Format(CultureInfo.InvariantCulture, "{0} − {1}", value + a, a);
		}
		else if (pattern == 0)
		{
			int a = gooseCounterRandom.Next(1, Math.Min(count - 1, 9) + 1);
			expression = string.Format(CultureInfo.InvariantCulture, "{0} + {1}", value - a, a);
		}
		else
		{
			int a = gooseCounterRandom.Next(1, Math.Min(count - 1, 9) + 1);
			int b = gooseCounterRandom.Next(1, 9);
			if (a <= b)
			{
				b++;
			}
			expression = string.Format(CultureInfo.InvariantCulture, "{0} − {1} + {2}", value - a + b, b, a);
		}
		gooseCounterExpression = expression;
		gooseCounterPattern = pattern;
		gooseCounterValue = count;
		return gooseCounterExpression;
	}

	private void OnResult(Results results)
	{
		int first = results.player0;
		int second = results.player1;
		if (!UserGameSettings.IsMultiplayerVs())
		{
			bagsCollected = second + first;
			UserGameSettings.bagsCollected = bagsCollected;
			if (bagsCollected == 3)
			{
				Game.NoteActivationCause((ActivationCause)3);
				MessageKit.post(25);
			}
			if (bagsToCollect <= bagsCollected)
			{
				canEscape = true;
				Breadcrumbs.Log("all_bags_collected");
				ShowObjectiveText(ScriptLocalization.FindTheExitAndEscapeMulti);
			}
			else
			{
				ShowBagCount(bagsCollected, bagsToCollect);
			}
		}
		else
		{
			guestBagsCollected = second;
			if (second == 3 || first == 3)
			{
				Game.NoteActivationCause((ActivationCause)3);
				MessageKit.post(25);
			}
			if (UserGameSettings.isMultiplayerHost)
			{
				if (first < bagsToCollect)
				{
					ShowBagCount(first, UserGameSettings.bagsToCollect);
				}
				else
				{
					Breadcrumbs.Log("all_bags_collected");
					canEscape = true;
					ShowObjectiveText(ScriptLocalization.FindTheExitAndEscape);
				}
				vsOpponentCountLabel.text = second + "/" + UserGameSettings.bagsToCollect;
			}
			else
			{
				vsOpponentCountLabel.text = first + "/" + UserGameSettings.bagsToCollect;
				if (bagsToCollect <= second)
				{
					canEscape = true;
					ShowObjectiveText(ScriptLocalization.FindTheExitAndEscape);
					Breadcrumbs.Log("all_bags_collected");
				}
				else
				{
					ShowBagCount(second, UserGameSettings.bagsToCollect);
				}
			}
		}
		if (UserGameSettings.isMultiplayerHost)
		{
			UserGameSettings.myBagsCollected = first;
			UserGameSettings.secondPlayerBagsCollected = second;
		}
		else
		{
			UserGameSettings.myBagsCollected = second;
			UserGameSettings.secondPlayerBagsCollected = first;
		}
	}

	public bool CanEscape()
	{
		return canEscape;
	}

	public bool CanPlayerEscape()
	{
		return bagsToCollect <= guestBagsCollected;
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
