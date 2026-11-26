using Bibloteka;
using System;
using System.Collections;
using Characters.Agents;
using Characters.Mover;
using Characters.States;
using Characters.Vision;
using UnityEngine;

namespace Characters.Strategies
{
    // Token: 0x02000385 RID: 901
    public class CharliePatrolStrategy : IStateStrategy
    {
        // Token: 0x06001E33 RID: 7731 RVA: 0x0000807E File Offset: 0x0000627E
        public CharliePatrolStrategy(CharlieAgent charlieAgent)
        {
        	this.charlieAgent = charlieAgent;
        	charlieMover = (CharlieMover)charlieAgent.EnemyMover;
        	enemyVision = charlieAgent.EnemyVision;
        }

        // Token: 0x06001E34 RID: 7732 RVA: 0x00008086 File Offset: 0x00006286
        private void GetPortalsPair(out GameObject _walkToThePortal, out GameObject _teleportToThePortal, GameObject _previousTeleportToPortal)
        {
        	GameObject[] portals = charlieMover.AgentDestinations;
        	do
        	{
        		_walkToThePortal = portals[UnityEngine.Random.Range(0, portals.Length)];
        	}
        	while (_walkToThePortal == _previousTeleportToPortal);
        	do
        	{
        		_teleportToThePortal = portals[UnityEngine.Random.Range(0, portals.Length)];
        	}
        	while (_teleportToThePortal == _walkToThePortal);
        }

        // Token: 0x06001E35 RID: 7733 RVA: 0x0000808E File Offset: 0x0000628E
        public IEnumerator Start(EnemyStateController stateController)
        {
        	GetPortalsPair(out targetWalkingPortal, out targetTeleportingPortal, previousTeleportingPortal);
        	yield return charlieMover.GoToDestination(targetWalkingPortal.GetComponent<CharliePortal>().GetEntryPoint().position);
        	yield return null;
        }

        // Token: 0x06001E36 RID: 7734 RVA: 0x00008091 File Offset: 0x00006291
        public IEnumerator Update(EnemyStateController stateController)
        {
        	if (charlieMover.IsDoorDetected)
        	{
        		stateController.StartNewState(stateController.PatrolState);
        	}
        	if (enemyVision.CanSeeHost || enemyVision.CanSeeGuest)
        	{
        		stateController.StartNewState(stateController.ChaseState);
        	}
        	if (charlieMover.IsTargetReached())
        	{
        		yield return charlieMover.TravelBetweenPortals(targetWalkingPortal, targetTeleportingPortal);
        		previousTeleportingPortal = targetTeleportingPortal;
        		charlieAgent.SetCharlieTransparency(1f);
        		stateController.StartNewState(stateController.PatrolState);
        	}
        	yield return Utils.GetDelay(0.1f);
        }

        // Token: 0x06001E37 RID: 7735 RVA: 0x00008094 File Offset: 0x00006294
        public IEnumerator Stop(EnemyStateController stateController)
        {
        	yield return null;
        }

        // Token: 0x04001A12 RID: 6674
        private GameObject targetWalkingPortal;

        // Token: 0x04001A13 RID: 6675
        private GameObject targetTeleportingPortal;

        // Token: 0x04001A14 RID: 6676
        private GameObject previousTeleportingPortal;

        // Token: 0x04001A15 RID: 6677
        private CharlieAgent charlieAgent;

        // Token: 0x04001A16 RID: 6678
        private CharlieMover charlieMover;

        // Token: 0x04001A17 RID: 6679
        private EnemyVision enemyVision;
    }
}
