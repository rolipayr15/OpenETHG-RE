using UnityEngine;
using Prime31.MessageKit;
using Bibloteka;
using System;
using System.Collections;
using Characters.Agents;
using Characters.Mover;
using Characters.States;

namespace Characters.Strategies
{
    // Token: 0x0200038A RID: 906
    public class DogEatStrategy : IStateStrategy
    {
        // Token: 0x06001E43 RID: 7747 RVA: 0x000080C7 File Offset: 0x000062C7
        public DogEatStrategy(DogAgent dogAgent)
        {
        	this.dogAgent = dogAgent;
        	dogMover = (DogMover)dogAgent.EnemyMover;
        }

        // Token: 0x06001E44 RID: 7748 RVA: 0x000080CF File Offset: 0x000062CF
        public IEnumerator Start(EnemyStateController stateController)
        {
        	dogAgent.IsDogEating = true;
        	yield return WalkToCommittedMeat(stateController);
        	yield return dogMover.EatMeat(dogAgent.PlayEatMeatSound, dogAgent.GrabCommittedMeat);
        	dogAgent.ClearCommittedMeat();
        	dogAgent.IsDogEating = false;
        	stateController.StartNewState(stateController.PatrolState);
        }

        // Token: 0x06001E45 RID: 7749 RVA: 0x000080D2 File Offset: 0x000062D2
        private IEnumerator WalkToCommittedMeat(EnemyStateController stateController)
        {
        	if (!dogAgent.TryGetCommittedMeatPosition(out Vector3 meat))
        	{
        		yield break;
        	}
        	Vector3 position = dogAgent.transform.position;
        	float dx = position.x - meat.x;
        	float dz = position.z - meat.z;
        	if (dx * dx + dz * dz <= MeatEatRange * MeatEatRange)
        	{
        		yield break;
        	}
        	dogMover.Walk();
        	yield return dogMover.GoToDestination(meat);
        	float startedAt = Time.time;
        	while (!dogMover.IsTargetReached() && !dogMover.IsDoorDetected && Time.time - startedAt <= MeatWalkTimeoutSeconds)
        	{
        		yield return Utils.GetDelay(0.1f);
        	}
        }

        // Token: 0x06001E46 RID: 7750 RVA: 0x000080D5 File Offset: 0x000062D5
        public IEnumerator Stop(EnemyStateController stateController)
        {
        	dogAgent.HideCommittedMeat("eat stopped");
        	dogAgent.ClearCommittedMeat();
        	if (dogAgent.IsDogEating)
        	{
        		dogAgent.IsDogEating = false;
        	}
        	MessageKit<bool, string>.post(5, false, dogAgent.name);
        	MessageKit<bool, string>.post(93, false, dogAgent.name);
        	yield return null;
        }

        // Token: 0x06001E47 RID: 7751 RVA: 0x000080D8 File Offset: 0x000062D8
        public IEnumerator Update(EnemyStateController stateController)
        {
        	yield return null;
        }

        // Token: 0x04001A1B RID: 6683
        private const float MeatEatRange = 1.75f;

        // Token: 0x04001A1C RID: 6684
        private const float MeatWalkTimeoutSeconds = 5f;

        // Token: 0x04001A1D RID: 6685
        private DogAgent dogAgent;

        // Token: 0x04001A1E RID: 6686
        private DogMover dogMover;
    }
}
