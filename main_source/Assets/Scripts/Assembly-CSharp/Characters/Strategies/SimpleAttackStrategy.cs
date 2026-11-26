using DarkTonic.MasterAudio;
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
	public class SimpleAttackStrategy : IStateStrategy
	{




		protected EnemyVision enemyVision;

		protected EnemyMover enemyMover;

		protected EnemyAgent enemyAgent;

		public SimpleAttackStrategy(EnemyAgent enemyAgent)
		{
			this.enemyAgent = enemyAgent;
			enemyVision = enemyAgent.EnemyVision;
			enemyMover = enemyAgent.EnemyMover;
		}

		protected virtual IEnumerator FadeEffect()
		{
			yield return Utils.GetDelay(2f);
			CustomSingleton<ScreenFader>.Instance.fadeIn = false;
			yield return Utils.GetDelay(1f);
			MasterAudio.FadeOutAllOfSound("Enemy", 1f);
			MasterAudio.PlaySound3DAtTransform("music", CustomSingleton<PlayerManager>.Instance.Player.transform);
		}

		protected EnemyTarget GetAttackedTarget()
		{
			if (!UserGameSettings.IsMultiplayer())
			{
				return (EnemyTarget)1;
			}
			return EnemyTargetUtils.FindNearestTarget(enemyAgent.gameObject.transform.position, enemyVision.HostHeartT.position, enemyVision.GuestHeartT.position);
		}

		public virtual IEnumerator Start(EnemyStateController stateController)
		{
			enemyAgent.ForceCheckEnemyTarget = EnemyTarget.None;
			enemyAgent.Attack(GetAttackedTarget());
			enemyAgent.PlayAttackSound();
			yield return FadeEffect();
			stateController.StartNewState(stateController.InactiveState);
		}

		public IEnumerator Update(EnemyStateController stateController)
		{
			yield return Utils.GetDelay(2f);
		}

		public IEnumerator Stop(EnemyStateController stateController)
		{
			yield return null;
		}
	}
}
