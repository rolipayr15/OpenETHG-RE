using Characters.States.Dog;
using System;
using System.Collections;
using Characters.Agents;
using Characters.States;

namespace Characters.Strategies
{
    // Token: 0x02000389 RID: 905
    public class DogCheckLastLocationStrategy : SimpleCheckLastLocationStrategy
    {
        // Token: 0x06001E40 RID: 7744 RVA: 0x000080B8 File Offset: 0x000062B8
        public DogCheckLastLocationStrategy(EnemyAgent enemyAgent) : base(enemyAgent)
        {
        	dogAgent = (DogAgent)enemyAgent;
        }

        // Token: 0x06001E41 RID: 7745 RVA: 0x000080C1 File Offset: 0x000062C1
        public override IEnumerator Update(EnemyStateController stateController)
        {
        	if (dogAgent.IsMeatFound)
        	{
        		dogAgent.IsMeatFound = false;
        		stateController.StartNewState(((DogStateController)stateController).EatState);
        		yield break;
        	}
        	yield return base.Update(stateController);
        }

        // Token: 0x04001A1A RID: 6682
        private DogAgent dogAgent;
    }
}
