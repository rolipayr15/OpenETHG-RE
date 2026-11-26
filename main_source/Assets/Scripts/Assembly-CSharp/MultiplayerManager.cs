using System.Collections;
using System.Collections.Generic;
using Bibloteka;
using EyesAnalytics;
using Multiplayer;
using Prime31.MessageKit;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerManager : MonoBehaviour
{
	public GameObject avatar;

	[SerializeField]
	private Animator anim;

	public float speed = 1f;

	private bool arePlayersReady;

	private Transform guestT;

	private readonly PursuitTracker guestPursuit = new PursuitTracker();

	private readonly HostActivationTracker guestActivation = new HostActivationTracker();

	[SerializeField]
	private PortraitsMinigame portraitsMinigame;

	private GameObject enemyAvatar;

	[SerializeField]
	private MultiplayerGui multiplayerGui;

	private bool showWithDelay;

	private bool isGameFinished;

	private float timeSinceLastCheck;

	private Vector3 startPoint;

	private Quaternion startRotationPoint;

	private Vector3 endPoint;

	private Quaternion endRotationPoint;

	private float dist;

	private float angle;

	private const float avatarTeleportSnapDistance = 10f;

	private float updateInterval = 0.5f;

	private Vector3 enemyDestination;

	private Vector3 enemyStartPoint;

	private Vector3 enemyEndPoint;

	private float enemyDist;

	private const float updatePositionWaitingTime = 2.3f;

	private bool wasDisconnected;

	private float tmpUpdateInterval;

	private Quaternion avatarDestinationRotation;

	private Vector3 avatarDestination;

	private float lastTimeUpdatePositions;

	private static MultiplayerSettings Net => CustomSingleton<MultiplayerSettings>.Instance;

	public void Awake()
	{
		if (!UserGameSettings.IsMultiplayer())
		{
			if (avatar != null)
			{
				avatar.SetActive(false);
			}
			return;
		}
		avatarDestination = avatar.transform.position;
		avatarDestinationRotation = avatar.transform.rotation;
		CustomSingleton<PlayerManager>.Instance.playerHandler.SetState("Freeze", true, true, false);
		Net.RegisterOnStart(OnGameStart);
		Net.RegisterOnInteraction(OnInteraction);
		Net.RegisterOnFinish(OnFinish);
		Net.RegisterOnInterrupted(OnInterrupted);
		Net.RegisterOnResumed(OnResumed);
		Net.RegisterOnPortraitsMinigameSolved(OnPortraitsMinigameSolved);
		Net.RegisterOnTotemMinigameFinished(OnTotemMinigameFinished);
		Net.RegisterOnGameObjectActiveStateUpdateById(OnGameObjectActiveStateUpdate);
		Net.RegisterOnPotionEffectUpdate(OnPotionEffectStateUpdate);
		Net.RegisterOnGameObjectPlaced(OnGameObjectPlaced);
		Net.RegisterOnPortalTravel(OnPortalTravel);
		MessageKit<string>.addObserver(19, BagFound);
		if (UserGameSettings.currentLevel == 0)
		{
			MessageKit.addObserver(52, PortraitsMinigameSolved);
		}
		MessageKit<int>.addObserver(94, OnChatMessage);
		MessageKit<string>.addObserver(87, OnError);
		MessageKit<string>.addObserver(28, SendMeatCollected);
		MessageKit<string>.addObserver(113, SendTotemActivated);
		MessageKit<string>.addObserver(29, SendMeatLeft);
		MessageKit<string>.addObserver(27, SendDogEatsMeat);
		MessageKit<bool, float>.addObserver(117, SendCharliePortalTravel);
		MessageKit<bool>.addObserver(14, OnCloakEffectUpdate);
		MessageKit<bool>.addObserver(11, OnLimitedVisionEffectUpdate);
		MessageKit<bool>.addObserver(12, OnDrunkEffectUpdate);
		MessageKit<bool>.addObserver(10, OnBagsVisionEffectUpdate);
		MessageKit<bool>.addObserver(13, OnSprintEffectUpdate);
		MessageKit.addObserver(125, SendEyeUsed);
		MessageKit.addObserver(126, SendSprintUsed);
		MessageKit.addObserver(29, SendMeatThrown);
		if (!UserGameSettings.isMultiplayerHost)
		{
			Net.RegisterOnHostMove(UpdateHostPositions);
			MessageKit.addObserver(25, SendEnemyActivated);
			Net.RegisterOnEnemyChase(OnEnemyChase);
			Net.RegisterOnGameObjectMessage(OnGameObjectMessage);
			Net.RegisterOnEnemyStateChanged(OnEnemyStateChanged);
			Net.RegisterOnGameObjectEnemyInteraction(OnGameObjectEnemyInteraction);
			Net.SendReady();
			guestT = CustomSingleton<PlayerManager>.Instance.Player.transform;
		}
		else
		{
			Net.RegisterOnGuestMove(UpdateGuestPositions);
			Net.RegisterOnEnemyActivated(OnEnemyActivated);
			MessageKit<bool, string>.addObserver(93, SendEnemyChase);
			MoneyBagsManager bags = CustomSingleton<MoneyBagsManager>.Instance;
			bags.ActivateBags();
			GameData data = new GameData
			{
				bags = bags.activatedBags,
				saw = "",
				safeCodes = new List<string> { "" },
				portraits = new List<int>(),
				totems = new List<string>(),
				potions = new List<string>()
			};
			int level = (int)UserGameSettings.currentLevel;
			if (level == 0)
			{
				data.saw = CustomSingleton<SecretDoorManager>.Instance.ActivateSaw();
				data.safeCodes = CustomSingleton<SafeManager>.Instance.ActivateCodes();
				data.portraits = portraitsMinigame.GetPortraitsOrder();
			}
			else if (level == 1)
			{
				PotionInitialData potions = CustomSingleton<PotionEffectManager>.Instance.ActivateInitialPotions();
				data.potionGreenId = potions.GreenId;
				data.potions = potions.Potions;
			}
			else if (level == 3)
			{
				data.saw = CustomSingleton<RandomExit>.Instance.ActivateExitDoor();
			}
			if ((int)UserGameSettings.currentEnemy == 6)
			{
				data.totems = CustomSingleton<WitchTotemManager>.Instance.ActivateTotems();
				Debug.Log("Number of totems to activate: " + data.totems.Count);
			}
			Net.SendReady(data);
			MessageKit.addObserver(92, GuestKilledByEnemy);
			MessageKit.addObserver(95, ShowSummaryForHostAfterHostDeath);
			MessageKit.addObserver(96, ShowSummaryAfterCoopWin);
			MessageKit.addObserver(99, ShowSummaryAfterVsLose);
			MessageKit.addObserver(98, ShowSummaryAfterVsWin);
			guestT = avatar.transform;
		}
		StartCoroutine(UpdateTransformsCoroutine());
		timeSinceLastCheck = Time.time - 2f;
	}

	private void OnEnable()
	{
		if (UserGameSettings.IsMultiplayer() && !UserGameSettings.isMultiplayerHost)
		{
			enemyAvatar = CustomSingleton<EnemyAvatarManager>.Instance.GetCurrentEnemy();
			enemyDestination = enemyAvatar.transform.position;
			enemyStartPoint = enemyAvatar.transform.position;
		}
	}

	private IEnumerator WaitFewSecondsAndWave()
	{
		yield return Utils.GetDelay(1.5f);
		anim.SetTrigger("Wave");
	}

	private void OnGameStart(GameData gameData)
	{
		if (!UserGameSettings.isInvitedPlayer)
		{
			Currency.SendCurrencyEvent((CurrencyFlowType)0, (CurrencyType)1, UserGameSettings.gamePriceInKeys, (CurrencyItemType)9, (CurrencyItemId)9);
			KeysManager.TryDeductMultiplayerKeys(UserGameSettings.gamePriceInKeys);
		}
		if (!UserGameSettings.isMultiplayerHost)
		{
			CustomSingleton<MoneyBagsManager>.Instance.ActivateBagsFromHost(gameData.bags);
			int level = (int)UserGameSettings.currentLevel;
			if (level == 0)
			{
				CustomSingleton<SecretDoorManager>.Instance.ActivateSawFromHost(gameData.saw);
				CustomSingleton<SafeManager>.Instance.ActivateCodesFromHost(gameData.safeCodes);
				portraitsMinigame.SetPortraitsOrder(gameData.portraits);
			}
			else if (level == 3)
			{
				CustomSingleton<RandomExit>.Instance.ActivateExitDoorFromHost(gameData.saw);
			}
			else if (level == 1)
			{
				CustomSingleton<PotionEffectManager>.Instance.ActivatePotionsFromHost(gameData.potionGreenId, gameData.potions);
			}
			if ((int)UserGameSettings.currentEnemy == 6)
			{
				Debug.Log("Activating totems " + gameData.totems);
				CustomSingleton<WitchTotemManager>.Instance.ActivateTotemsFromHost(gameData.totems);
			}
		}
		arePlayersReady = true;
		MessageKit.post(102);
		StartCoroutine(WaitFewSecondsAndWave());
		CustomSingleton<PlayerManager>.Instance.playerHandler.SetState("Freeze", false, true, false);
	}

	private void OnChatMessage(int msg)
	{
		Net.SendChatMessage(msg);
	}

	private void OnEnemyChase(bool status, string enemyName)
	{
		if (status)
		{
			guestPursuit.ChaseTargetsLocalPlayer((int)Time.timeSinceLevelLoad);
		}
		MessageKit<bool, string>.post(5, status, enemyName);
	}

	private void OnGameObjectMessage(string gameObjectMessage)
	{
		Debug.Log(gameObjectMessage.Substring(0, 1));
		Debug.Log(gameObjectMessage);
	}

	private void OnEnemyStateChanged(byte enemySymbol, byte enemyStateSymbol)
	{
		guestActivation.StateChanged(enemyStateSymbol, (int)Time.timeSinceLevelLoad);
		guestPursuit.StateChanged(enemyStateSymbol);
		Debug.Log("Got enemy state changed message: enemySymbol = '" + (char)enemySymbol + "', enemyStateSymbol: '" + (char)enemyStateSymbol + "'");
		MessageKit<byte, byte>.post(116, enemySymbol, enemyStateSymbol);
	}

	private void OnCloakEffectUpdate(bool state)
	{
		Net.SendPotionEffectUpdate(67, state);
	}

	private void OnLimitedVisionEffectUpdate(bool state)
	{
		Net.SendPotionEffectUpdate(76, state);
	}

	private void OnDrunkEffectUpdate(bool state)
	{
		Net.SendPotionEffectUpdate(68, state);
	}

	private void OnBagsVisionEffectUpdate(bool state)
	{
		Net.SendPotionEffectUpdate(66, state);
	}

	private void OnSprintEffectUpdate(bool state)
	{
		Net.SendPotionEffectUpdate(83, state);
	}

	private void SendMeatCollected(string meatId)
	{
		Debug.Log("Send message - player collected meat " + meatId);
		Net.SendGameObjectActiveStateUpdateById(77, false, meatId);
	}

	private void SendTotemActivated(string totemId)
	{
		Net.SendGameObjectActiveStateUpdateById(84, true, totemId);
	}

	private void SendMeatLeft(string meatPositionString)
	{
		Debug.Log("Send message - meat left at position " + meatPositionString);
		Net.SendGameObjectPlaced(77, meatPositionString);
	}

	private void SendDogEatsMeat(string meatPositionString)
	{
		Debug.Log("Send message - meat consumed at position " + meatPositionString);
		Net.SendGameObjectEnemyInteraction(77, meatPositionString);
	}

	private void SendEnemyChase(bool status, string enemyName)
	{
		Net.SendEnemyChase(status);
	}

	private void SendCharliePortalTravel(bool travelIntoPortal, float duration)
	{
		Net.SendPortalTravel(travelIntoPortal, duration);
	}

	private void PortraitsMinigameSolved()
	{
		Net.SendPortraitsMinigameSolved();
	}

	private void OnPortraitsMinigameSolved()
	{
		if ((int)UserGameSettings.currentLevel == 0 && portraitsMinigame != null)
		{
			MessageKit.removeObserver(52, PortraitsMinigameSolved);
			portraitsMinigame.MinigameSolved();
		}
	}

	private void OnTotemMinigameFinished(bool result, string totemId)
	{
		MessageKit<string, bool>.post(112, totemId, result);
	}

	private void OnPotionEffectStateUpdate(byte potionEffectSymbol, bool state)
	{
		Debug.Log("Received PotionEffect with symbol '" + (char)potionEffectSymbol + "' state update: " + state);
		switch (potionEffectSymbol)
		{
		case 66:
			MessageKit<bool>.post(120, state);
			break;
		case 67:
			MessageKit<bool>.post(119, state);
			break;
		case 68:
			MessageKit<bool>.post(121, state);
			break;
		case 76:
			MessageKit<bool>.post(122, state);
			break;
		case 83:
			MessageKit<bool>.post(123, state);
			break;
		}
	}

	private void OnGameObjectActiveStateUpdate(byte gameObjectSymbol, bool state, string gameObjectId)
	{
		Debug.Log("Received GameObject state update '" + (char)gameObjectSymbol + "', " + state + ", id: " + gameObjectId);
		if (gameObjectSymbol == 77)
		{
			if (!state)
			{
				MessageKit<string>.post(115, gameObjectId);
			}
		}
		else if (gameObjectSymbol == 84)
		{
			CustomSingleton<WitchTotemManager>.Instance.ActivateNewTotem(gameObjectId, state);
		}
		else
		{
			Debug.Log("Unsupported GameObjectSymbol '" + (char)gameObjectSymbol + "'");
		}
	}

	private void OnGameObjectPlaced(byte gameObjectSymbol, string positionString)
	{
		Debug.Log("Received GameObject placed: " + (char)gameObjectSymbol + " at position " + positionString);
		if (gameObjectSymbol == 77)
		{
			MessageKit<string>.post(114, positionString);
		}
	}

	private void OnPortalTravel(bool travelIntoPortal, float duration)
	{
		Debug.Log("Received portal travel. Travel into portal: " + travelIntoPortal);
		MessageKit<bool, float>.post(118, travelIntoPortal, duration);
	}

	private void OnGameObjectEnemyInteraction(byte gameObjectSymbol, string positionString)
	{
		Debug.Log("Received GameObject enemy interaction: '" + (char)gameObjectSymbol + "' at postion " + positionString);
		if (gameObjectSymbol == 77)
		{
			Debug.Log("Received meat consumed at position " + positionString);
			MessageKit<string>.post(27, positionString);
		}
	}

	private void OnInteraction(string id)
	{
		MessageKit<string>.post(81, id);
		anim.SetTrigger("Pickup");
	}

	private void OnEnemyActivated()
	{
		Game.NoteActivationCause(ActivationCause.Remote);
		MessageKit.post(25);
	}

	private void BagFound(string bagId)
	{
		Net.SendBagCollected(bagId);
	}

	public void SendEyeUsed()
	{
		Net.SendItemUsed(69);
	}

	public void SendSprintUsed()
	{
		Net.SendItemUsed(83);
	}

	public void SendMeatThrown()
	{
		Net.SendItemUsed(77);
	}

	public void SendEnemyActivated()
	{
		Net.SendEnemyActivated();
	}

	public void OnFinish(bool isWon, GameFinishedReason reason)
	{
		if (isGameFinished)
		{
			return;
		}
		isGameFinished = true;
		PushNotificationPermissionPresenter.FinishMultiplayerGame(this, reason);
		Eyes.DailyChallenges.DailyChallengeRuntime.MultiplayerFinished(this, isWon, reason.ToString());
		Game.End(isWon ? (GameFinishedResult)0 : (GameFinishedResult)1, UserGameSettings.bagsCollected, UserGameSettings.eyesCollected, (int)Time.timeSinceLevelLoad, reason, UserGameSettings.myBagsCollected);
		bool host = UserGameSettings.isMultiplayerHost;
		bool iWasKilled = false;
		bool otherWasKilled = false;
		if ((reason == GameFinishedReason.HOST_KILLED && host) || (reason == GameFinishedReason.GUEST_KILLED && !host))
		{
			iWasKilled = true;
			showWithDelay = true;
		}
		else
		{
			otherWasKilled = (reason == GameFinishedReason.GUEST_KILLED && host) || (reason == GameFinishedReason.HOST_KILLED && !host);
		}
		bool exitToMenu = reason == GameFinishedReason.EXIT_TO_MENU;
		bool networkError = reason == GameFinishedReason.NETWORK_ERROR;
		bool abuser = reason == GameFinishedReason.REMOVE_ABUSER;
		if (showWithDelay)
		{
			StartCoroutine(ShowSummaryWithDelay(isWon, iWasKilled, otherWasKilled, exitToMenu, networkError, abuser));
		}
		else
		{
			multiplayerGui.ShowSummary(isWon, iWasKilled, otherWasKilled, exitToMenu, networkError, abuser);
		}
	}

	public void OnInterrupted(GameInterruptedReason reason)
	{
		Debug.Log("Game interrupted " + reason);
	}

	public void OnResumed(GameResumedReason reason)
	{
		Debug.Log("Game resumed " + reason);
	}

	private IEnumerator ShowSummaryWithDelay(bool isWon, bool _wasIKilled, bool _wasSecondPlayerKilled, bool _secondPlayerExitToMenu, bool _networkError, bool _abuserRemoved)
	{
		yield return Utils.GetDelay(3f);
		multiplayerGui.ShowSummary(isWon, _wasIKilled, _wasSecondPlayerKilled, _secondPlayerExitToMenu, _networkError, _abuserRemoved);
	}

	private void GuestKilledByEnemy()
	{
		Net.SendFinishGame(GameFinishedReason.GUEST_KILLED);
	}

	private void ShowSummaryForHostAfterHostDeath()
	{
		OnFinish(false, GameFinishedReason.HOST_KILLED);
	}

	private void ShowSummaryAfterCoopWin()
	{
		OnFinish(true, GameFinishedReason.HOST_WON);
	}

	private void ShowSummaryAfterVsWin()
	{
		OnFinish(true, GameFinishedReason.HOST_WON);
	}

	private void ShowSummaryAfterVsLose()
	{
		OnFinish(false, GameFinishedReason.GUEST_WON);
	}

	private void OnError(string msg)
	{
		OnFinish(false, GameFinishedReason.NETWORK_ERROR);
		MessageKit<string>.post(100, msg);
	}

	public void ExitToMenu()
	{
		CustomSingleton<PauseManager>.Instance.QuitToMenu();
	}

	public void KickOtherPlayer()
	{
		Net.SendFinishGame(GameFinishedReason.REMOVE_ABUSER);
	}

	public void BackToMenu()
	{
		Net.SendPlayAgainReady(false);
		CustomSingleton<PauseManager>.Instance.PauseOff();
		UserGameSettings.lastGameDuration = Time.timeSinceLevelLoad;
		Ads.SetNextInterstitialTrigger(AdTrigger.MpRound);
		SceneManager.LoadSceneAsync("PostGameAd");
		Net.Clear();
		Net.CloseWebsocket();
	}

	private void Update()
	{
		if (!UserGameSettings.IsMultiplayer() || !arePlayersReady || isGameFinished)
		{
			return;
		}
		bool host = UserGameSettings.isMultiplayerHost;
		if (timeSinceLastCheck + updateInterval < Time.time)
		{
			Transform avatarT = avatar.transform;
			startPoint = avatarT.position;
			endPoint = avatarDestination;
			dist = Vector3.Distance(startPoint, endPoint);
			startRotationPoint = avatarT.rotation;
			endRotationPoint = avatarDestinationRotation;
			angle = Quaternion.Angle(startRotationPoint, endRotationPoint);
			if (dist > avatarTeleportSnapDistance)
			{
				avatarT.SetPositionAndRotation(endPoint, endRotationPoint);
				dist = 0f;
				startPoint = endPoint;
				startRotationPoint = endRotationPoint;
			}
			float moveSpeed = dist / updateInterval;
			if (Vector3.Angle(endPoint - startPoint, avatarT.forward) > 100f)
			{
				moveSpeed = -moveSpeed;
			}
			anim.SetFloat("MoveSpeed", moveSpeed);
			timeSinceLastCheck = Time.time;
			if (!host)
			{
				enemyStartPoint = enemyAvatar.transform.position;
				enemyEndPoint = enemyDestination;
				enemyDist = Vector3.Distance(enemyStartPoint, enemyEndPoint);
			}
		}
		float elapsed = Time.time - timeSinceLastCheck;
		if (dist > 0f)
		{
			avatar.transform.position = Vector3.MoveTowards(startPoint, endPoint, dist / updateInterval * elapsed);
		}
		if (angle != 0f)
		{
			avatar.transform.rotation = Quaternion.RotateTowards(startRotationPoint, endRotationPoint, angle / updateInterval * elapsed);
		}
		if (!host)
		{
			if (enemyDist > 0f)
			{
				enemyAvatar.transform.position = Vector3.MoveTowards(enemyStartPoint, enemyEndPoint, enemyDist / updateInterval * elapsed);
			}
			Vector3 direction = (enemyEndPoint - enemyStartPoint).normalized;
			if (direction.sqrMagnitude >= 1E-10f)
			{
				Quaternion look = Quaternion.LookRotation(direction, Vector3.up);
				enemyAvatar.transform.rotation = Quaternion.Slerp(enemyAvatar.transform.rotation, look, Time.deltaTime * 5f);
			}
		}
		Transform guest = host ? guestT : CustomSingleton<PlayerManager>.Instance.Player.transform;
		if (guest != null)
		{
			updateInterval = CustomSingleton<FloorManager>.Instance.AreEnemyAndGuestClose(guest.position) ? 0.2f : 0.5f;
		}
		CheckOtherPlayerConnectionState();
	}

	private void CheckOtherPlayerConnectionState()
	{
		if (Time.time - lastTimeUpdatePositions <= updatePositionWaitingTime)
		{
			if (wasDisconnected)
			{
				MessageKit<bool>.post(107, false);
				wasDisconnected = false;
			}
		}
		else if (!wasDisconnected)
		{
			MessageKit<bool>.post(107, true);
			wasDisconnected = true;
		}
	}

	private void UpdateGuestPositions(ObjectTransform guestTransform)
	{
		avatarDestination = guestTransform.position;
		avatarDestinationRotation.y = guestTransform.rotationY;
		avatarDestinationRotation.w = guestTransform.rotationW;
		lastTimeUpdatePositions = Time.time;
	}

	private void UpdateHostPositions(HostTransforms hostTransforms)
	{
		avatarDestination = hostTransforms.host.position;
		avatarDestinationRotation.y = hostTransforms.host.rotationY;
		avatarDestinationRotation.w = hostTransforms.host.rotationW;
		enemyDestination = hostTransforms.enemy;
		lastTimeUpdatePositions = Time.time;
	}

	private IEnumerator UpdateTransformsCoroutine()
	{
		while (!isGameFinished)
		{
			yield return Utils.GetDelay(updateInterval);
			UpdateTransforms();
		}
	}

	private void UpdateTransforms()
	{
		Transform player = CustomSingleton<PlayerManager>.Instance.Player.transform;
		ObjectTransform me = new ObjectTransform
		{
			position = player.position,
			rotationY = player.rotation.y,
			rotationW = player.rotation.w
		};
		if (UserGameSettings.isMultiplayerHost)
		{
			Vector3 enemy = CustomSingleton<EnemyManager>.Instance.CurrentEnemy.transform.position;
			Net.SendHostMove(new HostTransforms(me, enemy));
		}
		else
		{
			Net.SendGuestMove(me);
		}
	}

	private void HostLeftGame()
	{
		UserGameSettings.hostLeftGame = true;
		if (enemyAvatar != null)
		{
			enemyAvatar.SetActive(false);
		}
		CustomSingleton<EnemyManager>.Instance.ActivateEnemyWhenHostLeftGame();
		MessageKit.post(106);
	}
}
