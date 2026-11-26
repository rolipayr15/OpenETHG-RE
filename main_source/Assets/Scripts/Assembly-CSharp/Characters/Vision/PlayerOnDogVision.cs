using UnityEngine;

namespace Characters.Vision
{
	public class PlayerOnDogVision : PlayerOnEnemyVision
	{
		public override void UpdateMonsterOverlayFieldOfView()
		{
			iTween.ValueTo(gameObject, iTween.Hash("from", monsterOverlayCamera.fieldOfView, "to", 35, "time", 0.5f, "onupdate", "UpdateFieldOfView"));
		}

		public override Vector3 GetGirlLookAtPoint()
		{
			return CustomSingleton<PlayerManager>.Instance.Heart.transform.position;
		}
	}
}
