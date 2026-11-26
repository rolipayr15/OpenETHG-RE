using Characters.States.Dog;
using System;
using System.Collections;
using Characters.Agents;
using Characters.Mover;
using Characters.States;

namespace Characters.Strategies
{
    // Token: 0x02000388 RID: 904
    public class DogChaseStrategy : SimpleChaseStrategy
    {
        // Token: 0x06001E3C RID: 7740 RVA: 0x000080A6 File Offset: 0x000062A6
        public DogChaseStrategy(EnemyAgent enemyAgent) : base(enemyAgent)
        {
        	dogAgent = (DogAgent)enemyAgent;
        	dogMover = (DogMover)dogAgent.EnemyMover;
        }

        // Token: 0x06001E3D RID: 7741 RVA: 0x000080AF File Offset: 0x000062AF
        public override IEnumerator Start(EnemyStateController stateController)
        {
        	dogMover.Run();
        	yield return base.Start(stateController);
        }

        // Token: 0x06001E3E RID: 7742 RVA: 0x000080B2 File Offset: 0x000062B2
        protected override bool TryStartMeatEat(EnemyStateController stateController)
        {
        	if (!dogAgent.IsMeatFound)
        	{
        		return false;
        	}
        	dogAgent.IsMeatFound = false;
        	stateController.StartNewState(((DogStateController)stateController).EatState);
        	return true;
        }

        // Token: 0x04001A18 RID: 6680
        private DogAgent dogAgent;

        // Token: 0x04001A19 RID: 6681
        private DogMover dogMover;
    }
}
