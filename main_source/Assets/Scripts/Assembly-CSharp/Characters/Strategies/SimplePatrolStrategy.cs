using Bibloteka;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Characters.Agents;
using Characters.Mover;
using Characters.States;
using Characters.Vision;

namespace Characters.Strategies
{
	public class SimplePatrolStrategy : IStateStrategy
	{



		private EnemyVision enemyVision;

		private EnemyMover enemyMover;

		private EnemyAgent enemyAgent;

		public SimplePatrolStrategy(EnemyAgent enemyAgent)
		{
			this.enemyAgent = enemyAgent;
			enemyMover = enemyAgent.EnemyMover;
			enemyVision = enemyAgent.EnemyVision;
		}

		public IEnumerator Start(EnemyStateController stateController)
		{
			yield return enemyMover.GoToNewRandomDestination();
		}

		public IEnumerator Stop(EnemyStateController stateController)
		{
			yield return null;
		}

		public virtual IEnumerator Update(EnemyStateController stateController)
		{
			if (enemyMover.IsDoorDetected)
			{
				yield return enemyMover.GoToNewRandomDestination();
			}
			if (enemyVision.CanSeePlayer())
			{
				stateController.StartNewState(stateController.ChaseState);
			}
			if (enemyMover.IsTargetReached())
			{
				yield return enemyMover.GoToNewRandomDestination();
			}
			if ((int)enemyAgent.ForceCheckEnemyTarget != 0)
			{
				stateController.StartNewState(stateController.CheckLastLocationState);
			}
			yield return Utils.GetDelay(0.1f);
		}
	}
}
