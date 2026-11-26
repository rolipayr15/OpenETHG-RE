using UnityEngine;

namespace Characters.Vision
{
	public class PlayerOnKrasueVision : PlayerOnEnemyVision
	{
		public override Vector3 GetGirlLookAtPoint()
		{
			return CustomSingleton<PlayerManager>.Instance.Heart.transform.position;
		}

		public override void UpdateMonsterOverlayFieldOfView()
		{
		}
	}
}
