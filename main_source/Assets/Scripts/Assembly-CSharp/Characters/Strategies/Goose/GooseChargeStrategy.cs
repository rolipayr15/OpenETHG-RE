using Characters.States.Goose;
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

namespace Characters.Strategies.Goose
{
	public class GooseChargeStrategy : IStateStrategy
	{



		private readonly GooseAgent agent;

		private readonly GooseMover mover;

		private readonly EnemyVision vision;

		private float startedAt;

		public GooseChargeStrategy(GooseAgent agent)
		{
			this.agent = agent;
			mover = (GooseMover)agent.EnemyMover;
			vision = agent.EnemyVision;
		}

		public IEnumerator Start(EnemyStateController stateController)
		{
			agent.ResetHit();
			mover.ClearDoorDetected();
			Vector3 destination = agent.ChargeDestination;
			yield return mover.BeginCharge(destination, mover.ChargeSpeedWithRamp(agent.Profile.ChargeSpeed));
			float straight = Vector3.Distance(agent.transform.position, destination);
			if (mover.HasCompletePath && !GooseChargeRules.IsDetour(mover.CurrentPathLength(), straight, GooseChargeRules.DetourRatio))
			{
				mover.TriggerCharge();
				if (agent.Tint != null)
				{
					agent.Tint.HoldCharge();
				}
				agent.SetBodyLifted(true);
				agent.SetHitTriggerEnabled(true);
				startedAt = Time.time;
			}
			else
			{
				agent.RequestStalk();
				if (agent.Tint != null)
				{
					agent.Tint.StartCooldown(0.5f);
				}
				stateController.StartNewState(stateController.CheckLastLocationState);
			}
		}

		public IEnumerator Update(EnemyStateController stateController)
		{
			bool hit = agent.HitRegistered || IsInAttackDistance(vision.HostHeartT.position) || (UserGameSettings.IsMultiplayer() && IsInAttackDistance(vision.GuestHeartT.position));
			GooseChargeRules.ChargeOutcome outcome = GooseChargeRules.EvaluateCharge(hit, mover.PathPending, mover.HasCompletePath, mover.RemainingDistance, mover.StopDistance, Time.time - startedAt, GooseChargeRules.ChargeTimeoutSeconds, mover.IsDoorDetected);
			switch (outcome)
			{
			case GooseChargeRules.ChargeOutcome.Hit:
				stateController.StartNewState(stateController.AttackState);
				break;
			case GooseChargeRules.ChargeOutcome.Finished:
				stateController.StartNewState(((GooseStateController)stateController).RecoveryState);
				break;
			default:
				yield return null;
				break;
			}
		}

		public IEnumerator Stop(EnemyStateController stateController)
		{
			agent.SetHitTriggerEnabled(false);
			agent.SetBodyLifted(false);
			mover.EndCharge();
			yield return null;
		}

		private bool IsInAttackDistance(Vector3 heart)
		{
			return (heart - vision.RaycastOrigin.position).sqrMagnitude < agent.AttackDistance;
		}
	}
}
