using Characters.States.Goose;
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

namespace Characters.Strategies.Goose
{
	public class GooseWindupStrategy : IStateStrategy
	{



		private readonly GooseAgent agent;

		private readonly GooseMover mover;

		private readonly EnemyVision vision;

		private float startedAt;

		private EnemyTarget target;

		private Vector3 lastSeenHeart;

		public GooseWindupStrategy(GooseAgent agent)
		{
			this.agent = agent;
			mover = (GooseMover)agent.EnemyMover;
			vision = agent.EnemyVision;
		}

		public IEnumerator Start(EnemyStateController stateController)
		{
			agent.ForceCheckEnemyTarget = EnemyTarget.None;
			mover.ClearDoorDetected();
			mover.Halt();
			target = PickTarget();
			lastSeenHeart = HeartOf(target);
			mover.FaceTowards(lastSeenHeart);
			mover.TriggerWindup();
			agent.PlayWindupSound();
			if (agent.Tint != null)
			{
				agent.Tint.StartWindup(agent.Profile.WindupSeconds);
			}
			GooseChaseSignal signal = GooseChaseSignal.OnWindup(target);
			if (signal.CountsLocalPursuit)
			{
				stateController.Pursuit.ChaseTargetsLocalPlayer((int)Time.timeSinceLevelLoad);
				DailyChallengeRuntime.ChaseStarted(agent);
			}
			if (signal.PostLocal)
			{
				MessageKit<bool, string>.post(5, signal.LocalValue, agent.name);
			}
			if (signal.PostGuest)
			{
				MessageKit<bool, string>.post(93, signal.GuestValue, agent.name);
			}
			agent.MarkChaseRaised((int)target != 2, (int)target == 2);
			startedAt = Time.time;
			yield return null;
		}

		public IEnumerator Update(EnemyStateController stateController)
		{
			if (mover.HasMovementRequest)
			{
				mover.Halt();
			}
			bool visible = IsVisible(target);
			Vector3 heart = HeartOf(target);
			if (visible)
			{
				lastSeenHeart = heart;
			}
			float elapsed = Time.time - startedAt;
			float distance = Vector3.Distance(heart, agent.transform.position);
			switch (GooseChargeRules.EvaluateWindup(elapsed, agent.Profile.WindupSeconds, distance, GooseChargeRules.MinimumChargeDistance))
			{
			case GooseChargeRules.WindupOutcome.Attack:
				stateController.StartNewState(stateController.AttackState);
				break;
			case GooseChargeRules.WindupOutcome.Charge:
				agent.ChargeDestination = GooseChargeRules.ChargeDestination(visible, heart, lastSeenHeart);
				stateController.StartNewState(((GooseStateController)stateController).ChargeState);
				break;
			default:
				yield return Utils.GetDelay(0.05f);
				break;
			}
		}

		public IEnumerator Stop(EnemyStateController stateController)
		{
			yield return null;
		}

		private EnemyTarget PickTarget()
		{
			if (!vision.CanSeeHost)
			{
				return vision.CanSeeGuest ? (EnemyTarget)2 : (EnemyTarget)1;
			}
			if (!vision.CanSeeGuest)
			{
				return (EnemyTarget)1;
			}
			return EnemyTargetUtils.FindNearestTarget(agent.transform.position, vision.HostT.position, vision.GuestT.position);
		}

		private bool IsVisible(EnemyTarget t)
		{
			return (int)t == 2 ? vision.CanSeeGuest : vision.CanSeeHost;
		}

		private Vector3 HeartOf(EnemyTarget t)
		{
			return (int)t == 2 ? vision.GuestHeartT.position : vision.HostHeartT.position;
		}
	}
}
