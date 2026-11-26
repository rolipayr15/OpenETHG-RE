using Firebase.Crashlytics;
using EyesAnalytics;
using DarkTonic.MasterAudio;
using Prime31.MessageKit;
using System;
using System.Collections;
using Characters.Vision;
using UnityEngine;

namespace Characters.EnemyAvatar
{
    // Token: 0x020003BC RID: 956
    public abstract class EnemyAvatar : CustomSingleton<EnemyAvatar>
    {
        // Token: 0x06001F7A RID: 8058 RVA: 0x0000853B File Offset: 0x0000673B
        private void Awake()
        {
        	playerOnEnemyVision = GetComponent<PlayerOnEnemyVision>();
        }

        // Token: 0x06001F7B RID: 8059 RVA: 0x0000853D File Offset: 0x0000673D
        protected virtual void Start()
        {
        	if (!UserGameSettings.IsMultiplayer())
        	{
        		return;
        	}
        	MessageKit<byte, byte>.addObserver(116, HostEnemyStateChanged);
        	playerCamera = CustomSingleton<PlayerManager>.Instance.PlayerCamera;
        	playerCameraComponent = playerCamera.GetComponent<Camera>();
        	if (!UserGameSettings.isMultiplayerHost)
        	{
        		MessageKit.addObserver(92, GuestKilledByEnemy);
        	}
        	float[] speed = CharactersConfig.GetEnemySpeed(UserGameSettings.currentDifficulty, UserGameSettings.currentLevel, UserGameSettings.platform, UserGameSettings.currentEnemy);
        	patrolSpeed = speed[0];
        	chaseSpeed = speed[1];
        }

        // Token: 0x06001F7C RID: 8060 RVA: 0x0000853F File Offset: 0x0000673F
        protected override void OnDestroy()
        {
        	MessageKit<byte, byte>.removeObserver(116, HostEnemyStateChanged);
        	MessageKit.removeObserver(92, GuestKilledByEnemy);
        	base.OnDestroy();
        }

        // Token: 0x06001F7D RID: 8061
        protected abstract void EnemySpecificStateChangedBehavior(byte enemySymbol, byte enemyStateSymbol);

        // Token: 0x06001F7E RID: 8062 RVA: 0x00008541 File Offset: 0x00006741
        private void HostEnemyStateChanged(byte enemySymbol, byte enemyStateSymbol)
        {
        	EnemySpecificStateChangedBehavior(enemySymbol, enemyStateSymbol);
        }

        // Token: 0x06001F7F RID: 8063 RVA: 0x00008543 File Offset: 0x00006743
        public void GuestKilledByEnemy()
        {
        	StartCoroutine(DeathGuest());
        }

        // Token: 0x06001F80 RID: 8064
        protected abstract IEnumerator BeforeAttackSoundDelay();

        // Token: 0x06001F81 RID: 8065
        protected abstract void PlayAttackSound();

        // Token: 0x06001F82 RID: 8066
        protected abstract IEnumerator FadeEffect();

        // Token: 0x06001F83 RID: 8067 RVA: 0x00008545 File Offset: 0x00006745
        private void DoAttack()
        {
        	transform.LookAt(CustomSingleton<PlayerManager>.Instance.Player.transform);
        	MasterAudio.MuteGroup("breathe");
        	anim.SetTrigger("attack");
        }

        // Token: 0x06001F84 RID: 8068 RVA: 0x00008547 File Offset: 0x00006747
        public IEnumerator DeathGuest()
        {
        	Game.PlayerDeath(UserGameSettings.bagsCollected, UserGameSettings.bagsToCollect, (int)Time.timeSinceLevelLoad, gameObject.scene.handle, UserGameSettings.myBagsCollected, AttemptPlaceReader.Read(transform));
        	CustomSingleton<PlayerManager>.Instance.PlayerModel.SetActive(false);
        	MessageKit.post(17);
        	if (FirebaseState.IsInitialized)
        	{
        		Crashlytics.Log("Avatar killed player");
        	}
        	MessageKit.post(40);
        	playerCamera.GetComponent<vp_FPCamera>().enabled = false;
        	DoAttack();
        	playerOnEnemyVision.StartFollowMonsterFace();
        	CustomSingleton<PlayerManager>.Instance.playerHandler.SetState("Freeze", true, true, false);
        	yield return BeforeAttackSoundDelay();
        	PlayAttackSound();
        	yield return FadeEffect();
        	MasterAudio.PlaySound3DAtTransform("music", CustomSingleton<PlayerManager>.Instance.Player.transform);
        }

        // Token: 0x06001F85 RID: 8069 RVA: 0x0000854A File Offset: 0x0000674A
        public Camera GetCamera()
        {
        	return enemyCamera;
        }

        // Token: 0x04001AD6 RID: 6870
        private GameObject playerCamera;

        // Token: 0x04001AD7 RID: 6871
        private Camera playerCameraComponent;

        // Token: 0x04001AD8 RID: 6872
        [SerializeField]
        protected Animator anim;

        // Token: 0x04001AD9 RID: 6873
        [SerializeField]
        protected Camera monsterOverlayCamera;

        // Token: 0x04001ADA RID: 6874
        protected float patrolSpeed;

        // Token: 0x04001ADB RID: 6875
        protected float chaseSpeed;

        // Token: 0x04001ADC RID: 6876
        [SerializeField]
        private Camera enemyCamera;

        // Token: 0x04001ADD RID: 6877
        private PlayerOnEnemyVision playerOnEnemyVision;
    }
}
