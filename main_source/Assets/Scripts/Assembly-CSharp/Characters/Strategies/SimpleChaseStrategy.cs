using Eyes.DailyChallenges;
using Prime31.MessageKit;
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
using UnityEngine;

namespace Characters.Strategies
{
	public class SimpleChaseStrategy : IStateStrategy
	{



		private EnemyVision enemyVision;

		private EnemyMover enemyMover;

		private EnemyAgent enemyAgent;

		private bool isChasingHost;

		private bool isChasingGuest;

		public SimpleChaseStrategy(EnemyAgent enemyAgent)
		{
			this.enemyAgent = enemyAgent;
			enemyMover = enemyAgent.EnemyMover;
			enemyVision = enemyAgent.EnemyVision;
		}

		private EnemyTarget GetChaseTarget()
		{
			if (!enemyVision.CanSeeHost)
			{
				return enemyVision.CanSeeGuest ? (EnemyTarget)2 : EnemyTarget.None;
			}
			if (!enemyVision.CanSeeGuest)
			{
				return (EnemyTarget)1;
			}
			return EnemyTargetUtils.FindNearestTarget(enemyAgent.gameObject.transform.position, enemyVision.HostT.position, enemyVision.GuestT.position);
		}

		private bool IsInAttackDistance(Vector3 heartT)
		{
			return (heartT - enemyVision.RaycastOrigin.position).sqrMagnitude < enemyAgent.AttackDistance;
		}

		protected virtual bool TryStartMeatEat(EnemyStateController stateController)
		{
			return false;
		}

		public virtual IEnumerator Start(EnemyStateController stateController)
		{
			enemyAgent.ForceCheckEnemyTarget = EnemyTarget.None;
			yield return null;
		}

		public IEnumerator Stop(EnemyStateController stateController)
		{
			MessageKit<bool, string>.post(5, false, enemyAgent.name);
			MessageKit<bool, string>.post(93, false, enemyAgent.name);
			isChasingHost = false;
			isChasingGuest = false;
			yield return null;
		}

		public virtual IEnumerator Update(EnemyStateController stateController)
		{
			if (TryStartMeatEat(stateController))
			{
				yield break;
			}
			EnemyTarget chaseTarget = GetChaseTarget();
			if ((int)chaseTarget == 1)
			{
				yield return enemyMover.GoToDestination(enemyVision.HostT.position);
				if (TryStartMeatEat(stateController))
				{
					yield break;
				}
				if (!isChasingHost)
				{
					stateController.Pursuit.ChaseTargetsLocalPlayer((int)Time.timeSinceLevelLoad);
					DailyChallengeRuntime.ChaseStarted(enemyAgent);
					MessageKit<bool, string>.post(5, true, enemyAgent.name);
					isChasingHost = true;
					yield return null;
					if (TryStartMeatEat(stateController))
					{
						yield break;
					}
				}
			}
			if (UserGameSettings.IsMultiplayer() && (int)chaseTarget == 2)
			{
				yield return enemyMover.GoToDestination(enemyVision.GuestT.position);
				if (TryStartMeatEat(stateController))
				{
					yield break;
				}
				if (!isChasingGuest)
				{
					MessageKit<bool, string>.post(93, true, enemyAgent.name);
					isChasingGuest = true;
				}
			}
			if (TryStartMeatEat(stateController))
			{
				yield break;
			}
			chaseTarget = GetChaseTarget();
			if ((int)chaseTarget == 0)
			{
				stateController.StartNewState(stateController.CheckLastLocationState);
				yield break;
			}
			if (enemyMover.IsDoorDetected)
			{
				stateController.StartNewState(stateController.PatrolState);
				yield break;
			}
			if (IsInAttackDistance(enemyVision.HostHeartT.position) || (UserGameSettings.IsMultiplayer() && IsInAttackDistance(enemyVision.GuestHeartT.position)))
			{
				stateController.StartNewState(stateController.AttackState);
				yield break;
			}
			yield return Utils.GetDelay(0.1f);
		}
	}
}
