using UnityEngine.SceneManagement;
using Firebase.Crashlytics;
using EyesAnalytics;
using DarkTonic.MasterAudio;
using Prime31.MessageKit;
using System;
using Characters.Mover;
using Characters.States;
using Characters.Vision;
using UnityEngine;

namespace Characters.Agents
{
    // Token: 0x020003C2 RID: 962
    [RequireComponent(typeof(EnemyVision))]
    public abstract class EnemyAgent : MonoBehaviour
    {
        // Token: 0x17000354 RID: 852
        // (get) Token: 0x06001FAF RID: 8111 RVA: 0x000085D3 File Offset: 0x000067D3
        // (set) Token: 0x06001FB0 RID: 8112 RVA: 0x000085D6 File Offset: 0x000067D6
        public EnemyTarget ForceCheckEnemyTarget
        {
            get
            {
                return forceCheckEnemyTarget;
            }
            set
            {
forceCheckEnemyTarget = value;
if ((int)value == 0)
{
ForceCheckDestination = null;
}
}
        }

        // Token: 0x17000355 RID: 853
        // (get) Token: 0x06001FB1 RID: 8113 RVA: 0x000085D8 File Offset: 0x000067D8
        // (set) Token: 0x06001FB2 RID: 8114 RVA: 0x000085E0 File Offset: 0x000067E0
        public Vector3? ForceCheckDestination { get; set; }

        // Token: 0x17000356 RID: 854
        // (get) Token: 0x06001FB3 RID: 8115 RVA: 0x000085E9 File Offset: 0x000067E9
        public EnemyVision EnemyVision
        {
            get
            {
                return enemyVision;
            }
        }

        // Token: 0x17000357 RID: 855
        // (get) Token: 0x06001FB4 RID: 8116 RVA: 0x000085EC File Offset: 0x000067EC
        public EnemyMover EnemyMover
        {
            get
            {
                return enemyMover;
            }
        }

        // Token: 0x17000358 RID: 856
        // (get) Token: 0x06001FB5 RID: 8117 RVA: 0x000085EF File Offset: 0x000067EF
        public float AttackDistance
        {
            get
            {
                return attackDistance;
            }
        }

        // Token: 0x06001FB6 RID: 8118 RVA: 0x000085F6 File Offset: 0x000067F6
        private void Awake()
        {
        	controller = GetComponent<EnemyStateController>();
        	enemyVision = GetComponent<EnemyVision>();
        	playerOnEnemyVision = GetComponent<PlayerOnEnemyVision>();
        	enemyMover = GetComponent<EnemyMover>();
        }

        // Token: 0x06001FB7 RID: 8119 RVA: 0x000085F8 File Offset: 0x000067F8
        protected virtual void Start()
        {
        	MessageKit.addObserver(26, PlayerWin);
        	MessageKit.addObserver(53, Revive);
        	MessageKit.addObserver(17, PlayerIsDead);
        }

        // Token: 0x06001FB8 RID: 8120
        public abstract void PlayAttackSound();

        // Token: 0x06001FB9 RID: 8121 RVA: 0x000085FA File Offset: 0x000067FA
        public void PlayerWin()
        {
        	controller.StartNewState(controller.InactiveState);
        	enemyMover.StopAgent();
        	StopAllCoroutines();
        }

        // Token: 0x06001FBA RID: 8122 RVA: 0x000085FC File Offset: 0x000067FC
        public void Revive()
        {
        	CustomSingleton<PlayerManager>.Instance.PlayerModel.SetActive(true);
        	CustomSingleton<PlayerManager>.Instance.playerHandler.SetState("Freeze", false, true, false);
        	enemyMover.MoveAgentAwayFromPlayer();
        	controller.StartNewState(controller.InitialActiveState);
        	enemyMover.SetReviveAnimation();
        	isPlayerDead = false;
        	haveIKilledThePlayer = false;
        	MessageKit.post(39);
        	enemyMover.RestartAgent();
        	playerOnEnemyVision.EnablePlayerFPCamera();
        	CustomSingleton<ScreenFader>.Instance.fadeIn = true;
        	playerOnEnemyVision.StopFollowMonsterFace();
        	MasterAudio.UnmuteGroup("randomSounds");
        	MasterAudio.StopAllOfSound("music");
        	Game.ReviveFlow((Game.ReviveStage)3, null, null, null, gameObject.scene.handle);
        	AfterRevive();
        }

        // Token: 0x06001FBB RID: 8123 RVA: 0x000085FE File Offset: 0x000067FE
        protected virtual void AfterRevive()
        {
        }

        // Token: 0x06001FBC RID: 8124 RVA: 0x00008600 File Offset: 0x00006800
        public void PlayerIsDead()
        {
        	isPlayerDead = true;
        	MasterAudio.MuteGroup("randomSounds");
        	if ((int)UserGameSettings.currentEnemy == 5 && !haveIKilledThePlayer)
        	{
        		StopAllCoroutines();
        	}
        }

        // Token: 0x06001FBD RID: 8125 RVA: 0x00008602 File Offset: 0x00006802
        public bool IsPlayerDead()
        {
        	return isPlayerDead;
        }

        // Token: 0x06001FBE RID: 8126 RVA: 0x00008605 File Offset: 0x00006805
        public void Attack(EnemyTarget target)
        {
        	if (FirebaseState.IsInitialized)
        	{
        		Crashlytics.Log("Enemy attacks player");
        	}
        	MessageKit.post(40);
        	haveIKilledThePlayer = true;
        	playerOnEnemyVision.DisablePlayerFPCamera();
        	if ((int)target == 2)
        	{
        		MessageKit.post(92);
        		if (FirebaseState.IsInitialized)
        		{
        			Crashlytics.Log("Death guest");
        		}
        		playerOnEnemyVision.StartFollowMonsterFace();
        		enemyMover.AttackMove(enemyVision.GuestHeartT.position);
        	}
        	else if ((int)target == 1)
        	{
        		Game.PlayerDeath(UserGameSettings.bagsCollected, UserGameSettings.bagsToCollect, (int)Time.timeSinceLevelLoad, gameObject.scene.handle, UserGameSettings.IsMultiplayer() ? (int?)UserGameSettings.myBagsCollected : null, AttemptPlaceReader.Read(transform));
        		CustomSingleton<PlayerManager>.Instance.PlayerModel.SetActive(false);
        		CustomSingleton<MultiplayerSettings>.Instance.SendFinishGame((GameFinishedReason)2);
        		MessageKit.post(17);
        		if (FirebaseState.IsInitialized)
        		{
        			Crashlytics.Log("Death");
        		}
        		playerOnEnemyVision.StartFollowMonsterFace();
        		MessageKit.post(95);
        		enemyMover.AttackMove(playerOnEnemyVision.lookAtPoint);
        	}
        	CustomSingleton<PlayerManager>.Instance.playerHandler.SetState("Freeze", true, true, false);
        	MessageKit<bool, string>.post(5, false, name);
        }

        // Token: 0x04001AEF RID: 6895
        [SerializeField]
        private float attackDistance = 2f;

        // Token: 0x04001AF0 RID: 6896
        private EnemyTarget forceCheckEnemyTarget;

        // Token: 0x04001AF2 RID: 6898
        protected EnemyVision enemyVision;

        // Token: 0x04001AF3 RID: 6899
        private EnemyStateController controller;

        // Token: 0x04001AF4 RID: 6900
        private PlayerOnEnemyVision playerOnEnemyVision;

        // Token: 0x04001AF5 RID: 6901
        private EnemyMover enemyMover;

        // Token: 0x04001AF6 RID: 6902
        private bool isPlayerDead;

        // Token: 0x04001AF7 RID: 6903
        protected bool haveIKilledThePlayer;
    }
}
