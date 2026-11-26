using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkinSystem
{
	[Serializable]
	public sealed class SlotAnchor
	{
		public string slotId;

		public Renderer renderer;

		public List<MeshVariant> variants;

		[NonSerialized]
		private Material capturedMaterial;

		[NonSerialized]
		private bool captured;

		public Material CapturedMaterial => null;

		public void CaptureDefaults()
		{
		}
	}
}
