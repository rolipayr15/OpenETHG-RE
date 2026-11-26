using UnityEngine;

[RequireComponent(typeof(vp_Shooter))]
public class vp_SimpleAITurret : MonoBehaviour
{
	public float ViewRange = 10f;

	public float AimSpeed = 50f;

	public float WakeInterval = 2f;

	public float FireAngle = 10f;

	protected vp_Shooter m_Shooter;

	protected Transform m_Transform;

	protected Transform m_Target;

	protected Collider m_TargetCollider;

	protected vp_Timer.Handle m_Timer = new vp_Timer.Handle();

	protected virtual void Start()
	{
		m_Shooter = GetComponent<vp_Shooter>();
		m_Transform = base.transform;
	}

	protected virtual void Update()
	{
		if (!m_Timer.Active)
		{
			vp_Timer.In(WakeInterval, delegate
			{
				if (m_Target == null)
				{
					m_Target = ScanForLocalPlayer();
				}
				else
				{
					m_Target = null;
					m_TargetCollider = null;
				}
			}, m_Timer);
		}
		if (m_Target != null)
		{
			AttackTarget();
		}
	}

	protected virtual Transform ScanForLocalPlayer()
	{
		Collider[] array = Physics.OverlapSphere(m_Transform.position, ViewRange, 1073741824);
		foreach (Collider collider in array)
		{
			Physics.Linecast(m_Transform.position, collider.transform.position + Vector3.up, out var hitInfo, vp_Layer.Mask.BulletBlockers);
			if (!(hitInfo.collider != null) || !(hitInfo.collider != collider))
			{
				return collider.transform;
			}
		}
		return null;
	}

	protected virtual void AttackTarget()
	{
		if (m_TargetCollider == null)
		{
			m_TargetCollider = m_Target.GetComponent<Collider>();
		}
		Vector3 forward = ((!(m_TargetCollider != null)) ? (m_Target.transform.position - m_Transform.position) : (m_TargetCollider.bounds.center - m_Transform.position));
		m_Transform.rotation = Quaternion.RotateTowards(m_Transform.rotation, Quaternion.LookRotation(forward), Time.deltaTime * AimSpeed);
		if (Mathf.Abs(vp_3DUtility.LookAtAngleHorizontal(m_Transform.position, m_Transform.forward, m_Target.position)) < FireAngle)
		{
			m_Shooter.TryFire();
		}
	}
}
