using Characters.Strategies;
using System;
using Characters.Agents;

namespace Characters.States.Charlie
{
    // Token: 0x020003A9 RID: 937
    public class CharlieStateController : EnemyStateController
    {
        // Token: 0x1700033C RID: 828
        // (get) Token: 0x06001EDA RID: 7898 RVA: 0x00008309 File Offset: 0x00006509
        protected override byte EnemySymbol
        {
        	get => 67;
        }

        // Token: 0x06001EDB RID: 7899 RVA: 0x0000830C File Offset: 0x0000650C
        protected override void SetInitialActiveState()
        {
        	initialActiveState = patrolState;
        }

        // Token: 0x06001EDC RID: 7900 RVA: 0x0000830E File Offset: 0x0000650E
        protected override void SetStatesComponents()
        {
        	charlieAgent = GetComponent<CharlieAgent>();
        	inactiveState = new InactiveState(new InactiveStateStrategy());
        	patrolState = new PatrolState(new CharliePatrolStrategy(charlieAgent));
        	chaseState = new ChaseState(new SimpleChaseStrategy(charlieAgent));
        	checkLastLocationState = new CheckLastLocationState(new SimpleCheckLastLocationStrategy(charlieAgent));
        	attackState = new AttackState(new CharlieAttackStrategy(charlieAgent));
        }

        // Token: 0x04001A62 RID: 6754
        private CharlieAgent charlieAgent;
    }
}
