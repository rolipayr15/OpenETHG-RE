using UnityEngine;

public class vp_DamageTransfer : MonoBehaviour
{
	public GameObject TargetObject;

	protected vp_DamageHandler m_TargetDamageHandler;

	protected Collider m_Collider;

	private void Start()
	{
		m_Collider = base.transform.GetComponent<Collider>();
		if (m_Collider == null)
		{
			Debug.LogError("Error (" + this?.ToString() + ") This component requires a collider. Disabling self!");
			base.enabled = false;
			return;
		}
		if (TargetObject != null)
		{
			m_TargetDamageHandler = TargetObject.GetComponentInChildren<vp_DamageHandler>();
			return;
		}
		m_TargetDamageHandler = vp_DamageHandler.GetDamageHandlerOfCollider(m_Collider);
		if (m_TargetDamageHandler != null)
		{
			TargetObject = m_TargetDamageHandler.gameObject;
		}
	}

	public virtual void Damage(vp_DamageInfo damageInfo)
	{
		if (base.enabled)
		{
			if (m_TargetDamageHandler != null)
			{
				m_TargetDamageHandler.Damage(damageInfo);
			}
			else
			{
				Damage(damageInfo.Damage);
			}
		}
	}

	public virtual void Damage(float damage)
	{
		if (base.enabled)
		{
			if (m_TargetDamageHandler != null)
			{
				m_TargetDamageHandler.Damage(damage);
			}
			else if (TargetObject != null)
			{
				TargetObject.SendMessage("Damage", damage, SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				base.gameObject.SendMessageUpwards("Damage", damage, SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}
