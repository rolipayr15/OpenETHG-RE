using UnityEngine;

namespace Characters.Vision
{
	public class PlayerOnGooseVision : PlayerOnEnemyVision
	{
		[Tooltip("Vertical offset below the player's heart the goose looks at when it kills; the goose is short.")]
		[SerializeField]
		private float lookAtDrop = 0.5f;

		public override Vector3 GetGirlLookAtPoint()
		{
			return CustomSingleton<PlayerManager>.Instance.Heart.transform.position + Vector3.down * lookAtDrop;
		}

		public override void UpdateMonsterOverlayFieldOfView()
		{
		}
	}
}
