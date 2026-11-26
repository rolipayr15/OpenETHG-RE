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

namespace Characters.Strategies.Goose
{
	public class GooseRecoveryStrategy : IStateStrategy
	{



		private readonly GooseAgent agent;

		private readonly GooseMover mover;

		private readonly EnemyVision vision;

		private float startedAt;

		public GooseRecoveryStrategy(GooseAgent agent)
		{
			this.agent = agent;
			mover = (GooseMover)agent.EnemyMover;
			vision = agent.EnemyVision;
		}

		public IEnumerator Start(EnemyStateController stateController)
		{
			agent.SetHitTriggerEnabled(false);
			agent.SetBodyLifted(false);
			mover.EndCharge();
			mover.Halt();
			mover.ClearDoorDetected();
			mover.TriggerLand();
			agent.PlayDazeSound();
			if (agent.Tint != null)
			{
				agent.Tint.StartCooldown(agent.Profile.DazeSeconds);
			}
			startedAt = Time.time;
			yield return null;
		}

		public IEnumerator Update(EnemyStateController stateController)
		{
			if (mover.HasMovementRequest)
			{
				mover.Halt();
			}
			switch (GooseChargeRules.EvaluateRecovery(Time.time - startedAt, agent.Profile.DazeSeconds, vision.CanSeePlayer()))
			{
			case GooseChargeRules.RecoveryOutcome.CheckLastLocation:
				stateController.StartNewState(stateController.CheckLastLocationState);
				break;
			case GooseChargeRules.RecoveryOutcome.Windup:
				stateController.StartNewState(stateController.ChaseState);
				break;
			default:
				yield return Utils.GetDelay(0.1f);
				break;
			}
		}

		public IEnumerator Stop(EnemyStateController stateController)
		{
			yield return null;
		}
	}
}
