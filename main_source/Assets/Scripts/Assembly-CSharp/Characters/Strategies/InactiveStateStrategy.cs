using Bibloteka;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Characters.States;

namespace Characters.Strategies
{
	public class InactiveStateStrategy : IStateStrategy
	{



		public IEnumerator Start(EnemyStateController stateController)
		{
			yield return null;
		}

		public IEnumerator Stop(EnemyStateController stateController)
		{
			yield return null;
		}

		public IEnumerator Update(EnemyStateController stateController)
		{
			yield return Utils.GetDelay(0.1f);
		}
	}
}
