using UnityEngine;
using Prime31.MessageKit;
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
	public class GooseWalkingStrategy : IStateStrategy
	{



		private readonly IStateStrategy inner;

		private readonly GooseAgent agent;

		private readonly GooseMover mover;

		private readonly EnemyVision vision;

		private readonly bool isPatrol;

		private bool stalking;

		private float stalkStartedAt;

		public GooseWalkingStrategy(IStateStrategy inner, GooseAgent agent, bool isPatrol)
		{
			this.inner = inner;
			this.agent = agent;
			this.isPatrol = isPatrol;
			mover = (GooseMover)agent.EnemyMover;
			vision = agent.EnemyVision;
		}

		public IEnumerator Start(EnemyStateController stateController)
		{
			GooseChaseSignal signal = GooseChaseSignal.OnWalkingStart(isPatrol, agent.ChaseRaisedLocal, agent.ChaseRaisedGuest);
			if (signal.PostLocal)
			{
				MessageKit<bool, string>.post(5, signal.LocalValue, agent.name);
			}
			if (signal.PostGuest)
			{
				MessageKit<bool, string>.post(93, signal.GuestValue, agent.name);
			}
			if (isPatrol)
			{
				agent.ClearChaseRaised();
				if (agent.Tint != null)
				{
					agent.Tint.StartCooldown(0.5f);
				}
			}
			mover.TriggerWalk();
			if (agent.StalkRequested)
			{
				agent.ClearStalk();
				stalking = true;
				stalkStartedAt = Time.time;
				vision.SetRaycastDisabled(true);
				vision.ResetSight();
			}
			yield return inner.Start(stateController);
		}

		public IEnumerator Update(EnemyStateController stateController)
		{
			if (stalking)
			{
				float moved = Vector3.Distance(agent.transform.position, agent.StalkOrigin);
				if (GooseChargeRules.IsStalkDone(moved, Time.time - stalkStartedAt, GooseChargeRules.StalkDistance, GooseChargeRules.StalkSeconds))
				{
					EndStalk();
				}
			}
			yield return inner.Update(stateController);
		}

		public IEnumerator Stop(EnemyStateController stateController)
		{
			EndStalk();
			yield return inner.Stop(stateController);
		}

		private void EndStalk()
		{
			if (stalking)
			{
				stalking = false;
				vision.SetRaycastDisabled(false);
			}
		}
	}
}
