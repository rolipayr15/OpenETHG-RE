using UnityEngine;

public class vp_Toss : MonoBehaviour
{
	public float VerticalVelocity = 0.05f;

	public float Gravity = 0.0055f;

	protected Vector3 m_TargetPosition;

	protected Quaternion m_TargetRotation;

	protected float m_Duration;

	protected float m_StartTime;

	protected Vector3 m_Direction;

	protected float m_Distance;

	protected float m_YPos;

	protected bool m_ReenableBobWhenDone = true;

	protected bool m_ReenableSpinWhenDone = true;

	protected bool m_BeingTossed;

	private const float BOB_GROUND_OFFSET = 0.5f;

	protected Transform m_Transform;

	protected vp_RigidbodyFX m_RigidbodyFX;

	protected Transform Transform
	{
		get
		{
			if (m_Transform == null)
			{
				m_Transform = base.transform;
			}
			return m_Transform;
		}
	}

	protected vp_RigidbodyFX RigidbodyFX
	{
		get
		{
			if (m_RigidbodyFX == null)
			{
				m_RigidbodyFX = Transform.GetComponentInChildren<vp_RigidbodyFX>();
			}
			return m_RigidbodyFX;
		}
	}

	protected virtual void OnEnable()
	{
		vp_Bob componentInChildren = GetComponentInChildren<vp_Bob>();
		if (componentInChildren != null)
		{
			componentInChildren.enabled = false;
		}
		vp_Spin componentInChildren2 = GetComponentInChildren<vp_Spin>();
		if (componentInChildren2 != null)
		{
			componentInChildren2.enabled = false;
		}
		Gravity = 0.0055f * Random.Range(0.5f, 1f);
	}

	protected virtual void OnDisable()
	{
		if (m_ReenableBobWhenDone)
		{
			vp_Bob componentInChildren = GetComponentInChildren<vp_Bob>();
			if (componentInChildren != null)
			{
				componentInChildren.SmoothGroundOffset = true;
				componentInChildren.GroundOffset = 0.5f;
				componentInChildren.InitialPosition = Transform.position;
				componentInChildren.enabled = true;
			}
		}
		if (m_ReenableSpinWhenDone)
		{
			vp_Spin componentInChildren2 = GetComponentInChildren<vp_Spin>();
			if (componentInChildren2 != null)
			{
				componentInChildren2.Accelerate = true;
				componentInChildren2.enabled = true;
			}
		}
		m_BeingTossed = false;
	}

	public virtual Vector3 Toss(Vector3 startPos, float targetYaw, Vector3 direction, float distance, bool reenableBobWhenDone = false, bool reenableSpinWhenDone = false)
	{
		m_Direction = direction;
		m_Distance = distance;
		m_TargetPosition = GetTargetPosition();
		TossInternal(startPos, targetYaw, direction, reenableBobWhenDone, reenableSpinWhenDone);
		return m_TargetPosition;
	}

	public virtual void Toss(Vector3 startPos, Vector3 targetPos, float targetYaw, bool reenableBobWhenDone = false, bool reenableSpinWhenDone = false)
	{
		Vector3 normalized = (targetPos - startPos).normalized;
		m_TargetPosition = targetPos;
		TossInternal(startPos, targetYaw, normalized, reenableBobWhenDone, reenableSpinWhenDone);
	}

	protected virtual void TossInternal(Vector3 startPos, float targetYaw, Vector3 direction, bool reenableBobWhenDone, bool reenableSpinWhenDone)
	{
		Transform.position = startPos;
		Transform.LookAt(startPos + direction);
		Transform.eulerAngles = new Vector3(0f, Transform.eulerAngles.y, 0f);
		m_YPos = Transform.position.y;
		m_TargetRotation = Quaternion.Euler(Vector3.up * targetYaw);
		m_Duration = Vector3.Distance(Transform.position, m_TargetPosition);
		m_StartTime = Time.time;
		m_ReenableBobWhenDone = reenableBobWhenDone;
		m_ReenableSpinWhenDone = reenableSpinWhenDone;
		base.enabled = true;
		m_BeingTossed = true;
	}

	protected virtual void FixedUpdate()
	{
		if (m_BeingTossed)
		{
			Transform.position = vp_MathUtility.NaNSafeVector3(Vector3.Lerp(Transform.position, m_TargetPosition, Mathf.Lerp(0f, 1f, (Time.time - m_StartTime) / m_Duration)));
			Transform.rotation = vp_MathUtility.NaNSafeQuaternion(Quaternion.Lerp(Transform.rotation, m_TargetRotation, (Time.time - m_StartTime) / (m_Duration * 3f)), Transform.rotation);
			m_YPos += VerticalVelocity;
			VerticalVelocity -= Gravity;
			if (m_YPos <= m_TargetPosition.y)
			{
				m_YPos = m_TargetPosition.y;
				VerticalVelocity = (0f - VerticalVelocity) * 0.5f;
				TryPlayBounceFX();
			}
			Transform.position = new Vector3(Transform.position.x, m_YPos, Transform.position.z);
			if (Transform.position == m_TargetPosition)
			{
				base.enabled = false;
			}
		}
	}

	protected virtual bool TryPlayBounceFX()
	{
		if (RigidbodyFX == null)
		{
			return false;
		}
		if (!Physics.Raycast(new Ray(Transform.position + Vector3.up, Vector3.down), out var hitInfo, 2f, vp_Layer.Mask.BulletBlockers))
		{
			return false;
		}
		return RigidbodyFX.TryPlayFX(hitInfo);
	}

	protected virtual Vector3 GetTargetPosition()
	{
		Vector3 positionBelow = GetPositionBelow(Transform.position);
		if (Physics.SphereCast(positionBelow + Vector3.up * 0.15f, 0.1f, m_Direction, out var hitInfo, m_Distance, vp_Layer.Mask.BulletBlockers))
		{
			return GetPositionBelow(Vector3.Lerp(positionBelow, hitInfo.point, 0.5f));
		}
		return GetPositionBelow(Transform.position + m_Direction.normalized * m_Distance);
	}

	protected virtual Vector3 GetPositionBelow(Vector3 position)
	{
		if (Physics.Raycast(position, Vector3.down, out var hitInfo, 300f, vp_Layer.Mask.ExternalBlockers))
		{
			if (hitInfo.collider.gameObject.layer == 28 || hitInfo.collider.gameObject.layer == 26 || hitInfo.collider.gameObject.layer == 23)
			{
				position += base.transform.forward.normalized;
				return GetPositionBelow(position);
			}
			return hitInfo.point + Vector3.up * 0.1f;
		}
		return position + Vector3.down * 300f;
	}
}
