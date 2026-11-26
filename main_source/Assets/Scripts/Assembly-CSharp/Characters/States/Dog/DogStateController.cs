using Characters.Strategies;
using System;
using Characters.Agents;
using UnityEngine;

namespace Characters.States.Dog
{
    // Token: 0x020003A7 RID: 935
    [RequireComponent(typeof(DogAgent))]
    public class DogStateController : EnemyStateController
    {
        // Token: 0x17000338 RID: 824
        // (get) Token: 0x06001ED2 RID: 7890 RVA: 0x000082E8 File Offset: 0x000064E8
        public EatState EatState
        {
        	get => eatState;
        }

        // Token: 0x17000339 RID: 825
        // (get) Token: 0x06001ED3 RID: 7891 RVA: 0x000082EB File Offset: 0x000064EB
        protected override byte EnemySymbol
        {
        	get => 68;
        }

        // Token: 0x06001ED4 RID: 7892 RVA: 0x000082EE File Offset: 0x000064EE
        protected override void SetInitialActiveState()
        {
        	initialActiveState = patrolState;
        }

        // Token: 0x06001ED5 RID: 7893 RVA: 0x000082F0 File Offset: 0x000064F0
        protected override void SetStatesComponents()
        {
        	dogAgent = GetComponent<DogAgent>();
        	inactiveState = new InactiveState(new InactiveStateStrategy());
        	patrolState = new PatrolState(new DogPatrolStrategy(dogAgent));
        	eatState = new EatState(new DogEatStrategy(dogAgent));
        	chaseState = new ChaseState(new DogChaseStrategy(dogAgent));
        	checkLastLocationState = new CheckLastLocationState(new DogCheckLastLocationStrategy(dogAgent));
        	attackState = new AttackState(new DogAttackStrategy(dogAgent));
        }

        // Token: 0x04001A60 RID: 6752
        private DogAgent dogAgent;

        // Token: 0x04001A61 RID: 6753
        private EatState eatState;
    }
}
