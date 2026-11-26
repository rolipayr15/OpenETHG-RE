using UnityEngine;

namespace Characters.Vision
{
	public class PlayerOnWitchVision : PlayerOnEnemyVision
	{
		public override Vector3 GetGirlLookAtPoint()
		{
			return CustomSingleton<PlayerManager>.Instance.Heart.transform.position;
		}

		public override void UpdateMonsterOverlayFieldOfView()
		{
			iTween.ValueTo(gameObject, iTween.Hash("from", monsterOverlayCamera.fieldOfView, "to", 50, "time", 0.5f, "onupdate", "UpdateFieldOfView"));
		}
	}
}
