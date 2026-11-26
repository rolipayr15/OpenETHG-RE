using Debug = UnityEngine.Debug;
using UnityEngine;
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
	public class SimpleCheckLastLocationStrategy : IStateStrategy
	{



		private EnemyVision enemyVision;

		private EnemyMover enemyMover;

		private EnemyAgent enemyAgent;

		public SimpleCheckLastLocationStrategy(EnemyAgent enemyAgent)
		{
			this.enemyAgent = enemyAgent;
			enemyMover = enemyAgent.EnemyMover;
			enemyVision = enemyAgent.EnemyVision;
		}

		public IEnumerator Start(EnemyStateController stateController)
		{
			EnemyTarget target = enemyVision.GetLastSeenEnemyTarget();
			EnemyTarget forced = enemyAgent.ForceCheckEnemyTarget;
			Vector3? snapshot = null;
			if ((int)forced == 0)
			{
				if ((int)target == 0)
				{
					Debug.LogWarning("Checking last known location of never seen target");
					stateController.StartNewState(stateController.PatrolState);
					yield return null;
					yield break;
				}
			}
			else
			{
				snapshot = enemyAgent.ForceCheckDestination;
				enemyAgent.ForceCheckEnemyTarget = EnemyTarget.None;
				target = forced;
			}
			bool isForced = (int)forced != 0;
			CheckLastLocationDestinationResolver.DescribeBasis(isForced, snapshot.HasValue);
			if ((int)target == 1)
			{
				yield return enemyMover.GoToDestination(CheckLastLocationDestinationResolver.Resolve(isForced, enemyVision.HostT.position, enemyVision.HostLastKnownLocation, snapshot));
			}
			else if ((int)target == 2 && UserGameSettings.IsMultiplayer())
			{
				yield return enemyMover.GoToDestination(CheckLastLocationDestinationResolver.Resolve(isForced, enemyVision.GuestT.position, enemyVision.GuestLastKnownLocation, snapshot));
			}
			yield return Utils.GetDelay(0.3f);
		}

		public IEnumerator Stop(EnemyStateController stateController)
		{
			yield return null;
		}

		public virtual IEnumerator Update(EnemyStateController stateController)
		{
			if (!enemyMover.IsDoorDetected)
			{
				if (enemyVision.CanSeePlayer())
				{
					stateController.StartNewState(stateController.ChaseState);
					yield break;
				}
				if ((int)enemyAgent.ForceCheckEnemyTarget != 0)
				{
					stateController.StartNewState(stateController.CheckLastLocationState);
					yield break;
				}
				if (!enemyMover.IsTargetReached())
				{
					yield return Utils.GetDelay(0.1f);
					yield break;
				}
			}
			stateController.StartNewState(stateController.PatrolState);
		}
	}
}
