using Bibloteka;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Characters.Agents;

namespace Characters.Strategies
{
	public class WitchAttackStrategy : SimpleAttackStrategy
	{

		public WitchAttackStrategy(EnemyAgent enemyAgent)
			: base(enemyAgent)
		{
		}

		protected override IEnumerator FadeEffect()
		{
			yield return Utils.GetDelay(2f);
			yield return base.FadeEffect();
		}
	}
}
