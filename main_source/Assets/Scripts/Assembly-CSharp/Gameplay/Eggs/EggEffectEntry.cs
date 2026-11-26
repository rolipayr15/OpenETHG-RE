using System;
using UnityEngine;

namespace Gameplay.Eggs
{
	[Serializable]
	public struct EggEffectEntry
	{
		public EggEffectId id;

		[Min(0f)]
		public int weight;

		[Min(0f)]
		public int amount;

		[Min(0f)]
		public float duration;

		public EggEffectEntry(EggEffectId id, int weight, int amount, float duration)
		{
			this.id = EggEffectId.None;
			this.weight = 0;
			this.amount = 0;
			this.duration = 0f;
		}
	}
}
