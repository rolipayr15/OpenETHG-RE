using Characters.Strategies;
using System;
using Characters.Agents;

namespace Characters.States.Krasue
{
    // Token: 0x020003A2 RID: 930
    public class KrasueStateController : EnemyStateController
    {
        // Token: 0x1700032E RID: 814
        // (get) Token: 0x06001EBF RID: 7871 RVA: 0x00008297 File Offset: 0x00006497
        protected override byte EnemySymbol
        {
        	get => 75;
        }

        // Token: 0x06001EC0 RID: 7872 RVA: 0x0000829A File Offset: 0x0000649A
        protected override void SetInitialActiveState()
        {
        	initialActiveState = patrolState;
        }

        // Token: 0x06001EC1 RID: 7873 RVA: 0x0000829C File Offset: 0x0000649C
        protected override void SetStatesComponents()
        {
        	krasueAgent = GetComponent<KrasueAgent>();
        	inactiveState = new InactiveState(new InactiveStateStrategy());
        	patrolState = new PatrolState(new SimplePatrolStrategy(krasueAgent));
        	chaseState = new ChaseState(new SimpleChaseStrategy(krasueAgent));
        	checkLastLocationState = new CheckLastLocationState(new SimpleCheckLastLocationStrategy(krasueAgent));
        	attackState = new AttackState(new SimpleAttackStrategy(krasueAgent));
        }

        // Token: 0x04001A5C RID: 6748
        private KrasueAgent krasueAgent;
    }
}
