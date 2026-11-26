using UnityEngine;

public class DrawCollider : MonoBehaviour
{
	public static Color debugColliderColor = new Color(0.5f, 1f, 0.5f, 0.5f);

	public static Color debugTriggerColor = new Color(1f, 0.5f, 0.5f, 0.5f);

	private void OnDrawGizmos()
	{
		Gizmos.color = debugColliderColor;
		Gizmos.matrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, base.transform.lossyScale);
		BoxCollider[] boxes = GetComponents<BoxCollider>();
		SphereCollider[] spheres = GetComponents<SphereCollider>();
		CapsuleCollider[] capsules = GetComponents<CapsuleCollider>();
		foreach (BoxCollider box in boxes)
		{
			Gizmos.color = (box.isTrigger ? debugTriggerColor : debugColliderColor);
			Gizmos.DrawCube(box.center, box.size);
		}
		foreach (SphereCollider sphere in spheres)
		{
			Gizmos.color = (sphere.isTrigger ? debugTriggerColor : debugColliderColor);
			Gizmos.DrawSphere(sphere.center, sphere.radius);
		}
		foreach (CapsuleCollider capsule in capsules)
		{
			Gizmos.color = (capsule.isTrigger ? debugTriggerColor : debugColliderColor);
			Gizmos.DrawSphere(capsule.center, capsule.radius);
		}
	}
}
