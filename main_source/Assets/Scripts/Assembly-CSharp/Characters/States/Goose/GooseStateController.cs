using Characters.Strategies.Goose;
using Characters.Strategies;
using System;
using Characters.Agents;
using UnityEngine;

namespace Characters.States.Goose
{
    // Token: 0x020003A4 RID: 932
    [RequireComponent(typeof(GooseAgent))]
    public class GooseStateController : EnemyStateController
    {
        // Token: 0x17000331 RID: 817
        // (get) Token: 0x06001EC6 RID: 7878 RVA: 0x000082B5 File Offset: 0x000064B5
        public ChargeState ChargeState
        {
        	get => chargeState;
        }

        // Token: 0x17000332 RID: 818
        // (get) Token: 0x06001EC7 RID: 7879 RVA: 0x000082B8 File Offset: 0x000064B8
        public RecoveryState RecoveryState
        {
        	get => recoveryState;
        }

        // Token: 0x17000333 RID: 819
        // (get) Token: 0x06001EC8 RID: 7880 RVA: 0x000082BB File Offset: 0x000064BB
        protected override byte EnemySymbol
        {
        	get => 71;
        }

        // Token: 0x06001EC9 RID: 7881 RVA: 0x000082BE File Offset: 0x000064BE
        protected override void SetInitialActiveState()
        {
        	initialActiveState = patrolState;
        }

        // Token: 0x06001ECA RID: 7882 RVA: 0x000082C0 File Offset: 0x000064C0
        protected override void SetStatesComponents()
        {
        	gooseAgent = GetComponent<GooseAgent>();
        	inactiveState = new InactiveState(new InactiveStateStrategy());
        	patrolState = new PatrolState(new GooseWalkingStrategy(new SimplePatrolStrategy(gooseAgent), gooseAgent, true));
        	chaseState = new WindupState(new GooseWindupStrategy(gooseAgent));
        	chargeState = new ChargeState(new GooseChargeStrategy(gooseAgent));
        	recoveryState = new RecoveryState(new GooseRecoveryStrategy(gooseAgent));
        	checkLastLocationState = new CheckLastLocationState(new GooseWalkingStrategy(new SimpleCheckLastLocationStrategy(gooseAgent), gooseAgent, false));
        	attackState = new AttackState(new SimpleAttackStrategy(gooseAgent));
        }

        // Token: 0x04001A5D RID: 6749
        private GooseAgent gooseAgent;

        // Token: 0x04001A5E RID: 6750
        private ChargeState chargeState;

        // Token: 0x04001A5F RID: 6751
        private RecoveryState recoveryState;
    }
}
