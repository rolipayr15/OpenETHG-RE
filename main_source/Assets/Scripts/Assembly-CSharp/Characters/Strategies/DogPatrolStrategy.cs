using Bibloteka;
using System;
using System.Collections;
using Characters.Agents;
using Characters.Mover;
using Characters.States;
using Characters.States.Dog;
using Characters.Vision;

namespace Characters.Strategies
{
    // Token: 0x0200038B RID: 907
    public class DogPatrolStrategy : IStateStrategy
    {
        // Token: 0x06001E48 RID: 7752 RVA: 0x000080DB File Offset: 0x000062DB
        public DogPatrolStrategy(DogAgent dogAgent)
        {
        	this.dogAgent = dogAgent;
        	dogMover = (DogMover)dogAgent.EnemyMover;
        	enemyVision = dogAgent.EnemyVision;
        }

        // Token: 0x06001E49 RID: 7753 RVA: 0x000080E3 File Offset: 0x000062E3
        public IEnumerator Start(DogStateController stateController)
        {
        	dogAgent.ResetPatrolTime();
        	yield return dogMover.GoToNewRandomDestination();
        }

        // Token: 0x06001E4A RID: 7754 RVA: 0x000080E6 File Offset: 0x000062E6
        public IEnumerator Start(EnemyStateController stateController)
        {
        	dogMover.Walk();
        	yield return Start((DogStateController)stateController);
        }

        // Token: 0x06001E4B RID: 7755 RVA: 0x000080E9 File Offset: 0x000062E9
        public IEnumerator Stop(EnemyStateController stateController)
        {
        	yield return null;
        }

        // Token: 0x06001E4C RID: 7756 RVA: 0x000080EC File Offset: 0x000062EC
        public IEnumerator Update(DogStateController stateController)
        {
        	if (dogAgent.IsMeatFound)
        	{
        		dogAgent.IsMeatFound = false;
        		stateController.StartNewState(stateController.EatState);
        	}
        	else if (dogMover.IsDoorDetected)
        	{
        		stateController.StartNewState(stateController.PatrolState);
        	}
        	else if (enemyVision.CanSeePlayer())
        	{
        		dogAgent.ResetPatrolTime();
        		stateController.StartNewState(stateController.ChaseState);
        	}
        	else if ((int)dogAgent.ForceCheckEnemyTarget != 0)
        	{
        		stateController.StartNewState(stateController.CheckLastLocationState);
        	}
        	else if (dogMover.IsTargetReached())
        	{
        		yield return dogMover.GoToNewRandomDestination();
        	}
        	else
        	{
        		yield return Utils.GetDelay(0.1f);
        	}
        }

        // Token: 0x06001E4D RID: 7757 RVA: 0x000080EF File Offset: 0x000062EF
        public IEnumerator Update(EnemyStateController stateController)
        {
        	yield return Update((DogStateController)stateController);
        }

        // Token: 0x04001A1F RID: 6687
        private DogAgent dogAgent;

        // Token: 0x04001A20 RID: 6688
        private DogMover dogMover;

        // Token: 0x04001A21 RID: 6689
        private EnemyVision enemyVision;
    }
}
