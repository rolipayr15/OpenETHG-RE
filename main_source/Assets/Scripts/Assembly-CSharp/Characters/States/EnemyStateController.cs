using Bibloteka;
using Prime31.MessageKit;
using System;
using System.Collections;
using EyesAnalytics;
using UnityEngine;

namespace Characters.States
{
    // Token: 0x0200039E RID: 926
    public abstract class EnemyStateController : MonoBehaviour
    {
        // Token: 0x1700031E RID: 798
        // (get) Token: 0x06001EA0 RID: 7840 RVA: 0x00008235 File Offset: 0x00006435
        public bool IsActive
        {
            get
            {
                return isActive;
            }
        }

        // Token: 0x1700031F RID: 799
        // (get) Token: 0x06001EA1 RID: 7841 RVA: 0x00008238 File Offset: 0x00006438
        public InactiveState InactiveState
        {
            get
            {
                return inactiveState;
            }
        }

        // Token: 0x17000320 RID: 800
        // (get) Token: 0x06001EA2 RID: 7842 RVA: 0x0000823B File Offset: 0x0000643B
        public PatrolState PatrolState
        {
            get
            {
                return patrolState;
            }
        }

        // Token: 0x17000321 RID: 801
        // (get) Token: 0x06001EA3 RID: 7843 RVA: 0x0000823E File Offset: 0x0000643E
        public ChaseState ChaseState
        {
            get
            {
                return chaseState;
            }
        }

        // Token: 0x17000322 RID: 802
        // (get) Token: 0x06001EA4 RID: 7844 RVA: 0x00008241 File Offset: 0x00006441
        public CheckLastLocationState CheckLastLocationState
        {
            get
            {
                return checkLastLocationState;
            }
        }

        // Token: 0x17000323 RID: 803
        // (get) Token: 0x06001EA5 RID: 7845 RVA: 0x00008244 File Offset: 0x00006444
        public AttackState AttackState
        {
            get
            {
                return attackState;
            }
        }

        // Token: 0x17000324 RID: 804
        // (get) Token: 0x06001EA6 RID: 7846 RVA: 0x00008247 File Offset: 0x00006447
        public AbstractState InitialActiveState
        {
            get
            {
                return initialActiveState;
            }
        }

        // Token: 0x17000325 RID: 805
        // (get) Token: 0x06001EA7 RID: 7847 RVA: 0x0000824A File Offset: 0x0000644A
        internal string DiagnosticCurrentStateName
        {
            get
            {
                return currentState == null ? "<none>" : currentState.StateName;
            }
        }

        // Token: 0x17000326 RID: 806
        // (get) Token: 0x06001EA8 RID: 7848 RVA: 0x0000824D File Offset: 0x0000644D
        internal bool IsPatrolling
        {
            get
            {
                return isActive && currentState == patrolState;
            }
        }

        // Token: 0x17000327 RID: 807
        // (get) Token: 0x06001EA9 RID: 7849 RVA: 0x00008250 File Offset: 0x00006450
        internal PursuitTracker Pursuit
        {
            get
            {
                return pursuit;
            }
        }

        // Token: 0x17000328 RID: 808
        // (get) Token: 0x06001EAA RID: 7850
        protected abstract byte EnemySymbol { get; }

        // Token: 0x06001EAB RID: 7851
        protected abstract void SetInitialActiveState();

        // Token: 0x06001EAC RID: 7852
        protected abstract void SetStatesComponents();

        // Token: 0x06001EAD RID: 7853 RVA: 0x00008253 File Offset: 0x00006453
        private void DebugState(AbstractState state)
        {
        }

        // Token: 0x06001EAE RID: 7854 RVA: 0x00008255 File Offset: 0x00006455
        private void Start()
        {
        	SetStatesComponents();
        	SetInitialActiveState();
        	StartNewState(inactiveState);
        	if ((int)UserGameSettings.currentDifficulty == 4)
        	{
        		Activate(false);
        		MessageKit.post(25);
        	}
        	else
        	{
        		MessageKit.addObserver(25, Activate);
        	}
        	if ((int)UserGameSettings.currentMode == 1)
        	{
        		StartCoroutine(DelayedActivation());
        	}
        }

        // Token: 0x06001EAF RID: 7855 RVA: 0x00008257 File Offset: 0x00006457
        protected virtual void OnDestroy()
        {
        	MessageKit.removeObserver(25, Activate);
        }

        // Token: 0x06001EB0 RID: 7856 RVA: 0x00008259 File Offset: 0x00006459
        private IEnumerator DelayedActivation()
        {
        	yield return Utils.GetDelay(1f);
        	Game.NoteActivationCause((ActivationCause)1);
        	MessageKit.post(25);
        }

        // Token: 0x06001EB1 RID: 7857 RVA: 0x0000825C File Offset: 0x0000645C
        public void Activate()
        {
        	Activate(true);
        }

        // Token: 0x06001EB2 RID: 7858 RVA: 0x0000825E File Offset: 0x0000645E
        private void Activate(bool countActivation)
        {
        	Debug.Log("Monster activated " + GetInstanceID());
        	MessageKit.removeObserver(25, Activate);
        	if (!isActive)
        	{
        		isActive = true;
        		if (countActivation)
        		{
        			Game.CountMonsterActivation((int)Time.timeSinceLevelLoad);
        		}
        		StartNewState(initialActiveState);
        	}
        }

        // Token: 0x06001EB3 RID: 7859 RVA: 0x00008260 File Offset: 0x00006460
        internal void StartNewState(AbstractState newState)
        {
        	if (currentStateCoroutine != null)
        	{
        		StopCoroutine(currentStateCoroutine);
        		StartCoroutine(currentState.OnStateActionStop(this));
        	}
        	currentState = newState;
        	if (newState == null)
        	{
        		return;
        	}
        	pursuit.StateChanged(newState.StateSymbol);
        	if (UserGameSettings.IsMultiplayer() && UserGameSettings.isMultiplayerHost)
        	{
        		CustomSingleton<MultiplayerSettings>.Instance.SendEnemyStateChanged(EnemySymbol, newState.StateSymbol);
        	}
        	currentStateCoroutine = StartCoroutine(newState.DoStateAction(this));
        }

        // Token: 0x04001A51 RID: 6737
        private bool isActive;

        // Token: 0x04001A52 RID: 6738
        private Coroutine currentStateCoroutine;

        // Token: 0x04001A53 RID: 6739
        private readonly PursuitTracker pursuit = new PursuitTracker();

        // Token: 0x04001A54 RID: 6740
        private AbstractState currentState;

        // Token: 0x04001A55 RID: 6741
        protected AbstractState initialActiveState;

        // Token: 0x04001A56 RID: 6742
        protected InactiveState inactiveState;

        // Token: 0x04001A57 RID: 6743
        protected PatrolState patrolState;

        // Token: 0x04001A58 RID: 6744
        protected ChaseState chaseState;

        // Token: 0x04001A59 RID: 6745
        protected CheckLastLocationState checkLastLocationState;

        // Token: 0x04001A5A RID: 6746
        protected AttackState attackState;
    }
}
