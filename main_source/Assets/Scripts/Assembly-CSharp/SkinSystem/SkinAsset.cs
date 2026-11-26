using UnityEngine;

namespace SkinSystem
{
	[CreateAssetMenu(fileName = "Skin", menuName = "Skins/Skin")]
	public sealed class SkinAsset : ScriptableObject
	{
		[Tooltip("Stable id. This is SAVE DATA — never change it after release, and never derive it from the file name.")]
		[SerializeField]
		private string id;

		[SerializeField]
		private string targetId;

		[SerializeField]
		private string slotId;

		[Tooltip("I2.Localization key, not a display string.")]
		[SerializeField]
		private string displayNameKey;

		[SerializeReference]
		[Tooltip("MaterialSwapVisual or MeshActivationVisual. See design doc 5.6 before renaming those types — a rename nulls this field silently.")]
		private ISkinVisual visual;

		public string Id => null;

		public ISkinVisual Visual => null;

		public SkinDefinition ToDefinition()
		{
			return null;
		}
	}
}
