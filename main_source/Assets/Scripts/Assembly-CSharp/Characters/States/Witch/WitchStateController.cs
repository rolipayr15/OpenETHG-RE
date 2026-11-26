using Characters.Strategies;
using System;
using Characters.Agents;
using UnityEngine;

namespace Characters.States.Witch
{
    // Token: 0x020003A1 RID: 929
    [RequireComponent(typeof(WitchAgent))]
    public class WitchStateController : EnemyStateController
    {
        // Token: 0x1700032D RID: 813
        // (get) Token: 0x06001EBB RID: 7867 RVA: 0x00008288 File Offset: 0x00006488
        protected override byte EnemySymbol
        {
        	get => 87;
        }

        // Token: 0x06001EBC RID: 7868 RVA: 0x0000828B File Offset: 0x0000648B
        protected override void SetInitialActiveState()
        {
        	initialActiveState = patrolState;
        }

        // Token: 0x06001EBD RID: 7869 RVA: 0x0000828D File Offset: 0x0000648D
        protected override void SetStatesComponents()
        {
        	witchAgent = GetComponent<WitchAgent>();
        	inactiveState = new InactiveState(new InactiveStateStrategy());
        	patrolState = new PatrolState(new SimplePatrolStrategy(witchAgent));
        	chaseState = new ChaseState(new SimpleChaseStrategy(witchAgent));
        	checkLastLocationState = new CheckLastLocationState(new SimpleCheckLastLocationStrategy(witchAgent));
        	attackState = new AttackState(new SimpleAttackStrategy(witchAgent));
        }

        // Token: 0x04001A5B RID: 6747
        private WitchAgent witchAgent;
    }
}
