using Characters.Strategies;
using Eyes.DailyChallenges;
using DarkTonic.MasterAudio;
using Prime31.MessageKit;
using System;
using Characters.Mover;
using Characters.States.Dog;
using Characters.Vision;
using UnityEngine;

namespace Characters.Agents
{
    // Token: 0x020003C1 RID: 961
    [RequireComponent(typeof(DogStateController), typeof(PlayerOnDogVision))]
    [RequireComponent(typeof(DogMover), typeof(EnemyVision))]
    public class DogAgent : EnemyAgent
    {
        // Token: 0x17000352 RID: 850
        // (get) Token: 0x06001F9D RID: 8093 RVA: 0x0000859F File Offset: 0x0000679F
        // (set) Token: 0x06001F9E RID: 8094 RVA: 0x000085A2 File Offset: 0x000067A2
        public bool IsMeatFound
        {
            get
            {
                return isMeatFound;
            }
            set
            {
isMeatFound = value;
}
        }

        // Token: 0x17000353 RID: 851
        // (get) Token: 0x06001F9F RID: 8095 RVA: 0x000085A4 File Offset: 0x000067A4
        // (set) Token: 0x06001FA0 RID: 8096 RVA: 0x000085A7 File Offset: 0x000067A7
        public bool IsDogEating
        {
            get
            {
                return isDogEating;
            }
            set
            {
isDogEating = value;
}
        }

        // Token: 0x06001FA1 RID: 8097 RVA: 0x000085A9 File Offset: 0x000067A9
        public void SetDogEating(bool isEating, string reason, UnityEngine.Object context = null)
        {
        	isDogEating = isEating;
        }

        // Token: 0x06001FA2 RID: 8098 RVA: 0x000085AB File Offset: 0x000067AB
        private void OnDisable()
        {
        	if (isDogEating)
        	{
        		isDogEating = false;
        	}
        	HideCommittedMeat("dog disabled");
        	ClearCommittedMeat();
        }

        // Token: 0x06001FA3 RID: 8099 RVA: 0x000085AD File Offset: 0x000067AD
        protected override void Start()
        {
        	dogStateController = GetComponent<DogStateController>();
        	int difficulty = (int)UserGameSettings.currentDifficulty;
        	if (difficulty >= 0 && difficulty < 5)
        	{
        		maxPatrolTime = new float[] { 999f, 999f, 45f, 30f, 20f }[difficulty];
        	}
        	patrolTime = Time.time;
        	MessageKit.addObserver(27, MeatFound);
        	base.Start();
        }

        // Token: 0x06001FA4 RID: 8100 RVA: 0x000085AF File Offset: 0x000067AF
        private void Update()
        {
        	bool isActive = dogStateController != null && dogStateController.IsActive;
        	bool isPatrolling = dogStateController != null && dogStateController.IsPatrolling;
        	if (DogPeriodicHuntScheduler.ShouldQueuePeriodicHunt(isActive, isPatrolling, patrolTime, maxPatrolTime, Time.time) && enemyVision.HostT != null)
        	{
        		ForceCheckEnemyTarget = (EnemyTarget)1;
        		ForceCheckDestination = DogPeriodicHuntScheduler.CapturePeriodicHuntDestination(enemyVision.HostT.position);
        		patrolTime = Time.time;
        	}
        }

        // Token: 0x06001FA5 RID: 8101 RVA: 0x000085B1 File Offset: 0x000067B1
        public override void PlayAttackSound()
        {
        	MasterAudio.PlaySound("DogFx", 1f, null, 0f, "attack");
        }

        // Token: 0x06001FA6 RID: 8102 RVA: 0x000085B3 File Offset: 0x000067B3
        private void MeatFound()
        {
        	isMeatFound = true;
        }

        // Token: 0x06001FA7 RID: 8103 RVA: 0x000085B5 File Offset: 0x000067B5
        public void GoForTheMeatPiece(Transform meatT, GameObject meatRoot = null)
        {
        	if (meatT != null)
        	{
        		committedMeatPosition = meatT.position;
        		committedMeatRoot = meatRoot != null ? meatRoot : meatT.gameObject;
        	}
        }

        // Token: 0x06001FA8 RID: 8104 RVA: 0x000085B7 File Offset: 0x000067B7
        public bool TryGetCommittedMeatPosition(out Vector3 position)
        {
        	position = committedMeatPosition.HasValue ? committedMeatPosition.Value : default(Vector3);
        	return committedMeatPosition.HasValue;
        }

        // Token: 0x06001FA9 RID: 8105 RVA: 0x000085C1 File Offset: 0x000067C1
        public void GrabCommittedMeat()
        {
        	if (committedMeatRoot != null && committedMeatRoot.activeSelf)
        	{
        		HideCommittedMeat("grabbed");
        		DailyChallengeRuntime.GoodBoyFed(this);
        	}
        }

        // Token: 0x06001FAA RID: 8106 RVA: 0x000085C3 File Offset: 0x000067C3
        public void HideCommittedMeat(string reason)
        {
        	if (committedMeatRoot != null && committedMeatRoot.activeSelf)
        	{
        		committedMeatRoot.SetActive(false);
        	}
        }

        // Token: 0x06001FAB RID: 8107 RVA: 0x000085C5 File Offset: 0x000067C5
        public void ClearCommittedMeat()
        {
        	committedMeatRoot = null;
        	committedMeatPosition = null;
        }

        // Token: 0x06001FAC RID: 8108 RVA: 0x000085C7 File Offset: 0x000067C7
        public void PlayEatMeatSound()
        {
        	MasterAudio.PlaySound3DAtTransform("DogFx", enemyVision.HostT.transform, 1f, null, 0f, "eat");
        }

        // Token: 0x06001FAD RID: 8109 RVA: 0x000085C9 File Offset: 0x000067C9
        public void ResetPatrolTime()
        {
        	patrolTime = Time.time;
        }

        // Token: 0x04001AE8 RID: 6888
        private float patrolTime;

        // Token: 0x04001AE9 RID: 6889
        private float maxPatrolTime;

        // Token: 0x04001AEA RID: 6890
        private bool isMeatFound;

        // Token: 0x04001AEB RID: 6891
        private bool isDogEating;

        // Token: 0x04001AEC RID: 6892
        private Vector3? committedMeatPosition;

        // Token: 0x04001AED RID: 6893
        private GameObject committedMeatRoot;

        // Token: 0x04001AEE RID: 6894
        private DogStateController dogStateController;
    }
}
