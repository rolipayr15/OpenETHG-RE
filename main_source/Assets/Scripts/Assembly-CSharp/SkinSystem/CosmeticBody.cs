using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkinSystem
{
	public sealed class CosmeticBody : MonoBehaviour
	{
		[SerializeField]
		private string targetId;

		[SerializeField]
		private List<SlotAnchor> anchors;

		private readonly Dictionary<Type, ISkinApplicationStrategy> strategies;

		private ISkinVisualResolver resolver;

		public string TargetId => null;

		public void Initialize(ISkinVisualResolver visualResolver, IEnumerable<ISkinApplicationStrategy> applicationStrategies)
		{
		}

		public void Apply(SkinLoadout loadout)
		{
		}

		private void ClearAnchor(SlotAnchor anchor)
		{
		}
	}
}
