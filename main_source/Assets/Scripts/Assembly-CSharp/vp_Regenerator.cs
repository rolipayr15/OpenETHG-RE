using UnityEngine;

public class vp_Regenerator : MonoBehaviour
{
	public float DelayAfterDamage = 5f;

	public float TickRate = 1f;

	public float TickAmount = 0.1f;

	protected float m_NextAllowedTickTime;

	protected vp_DamageHandler m_DamageHandler;

	protected bool m_TriedGetDamageHandler;

	public vp_DamageHandler DamageHandler
	{
		get
		{
			if (!m_TriedGetDamageHandler && m_DamageHandler == null)
			{
				m_DamageHandler = base.transform.root.GetComponentInChildren<vp_DamageHandler>();
				m_TriedGetDamageHandler = true;
			}
			return m_DamageHandler;
		}
	}

	protected void Update()
	{
		if (DamageHandler == null)
		{
			Debug.LogError("Error (" + this?.ToString() + ") This component requires a DamageHandler. Disabling self.");
			base.enabled = false;
		}
		else
		{
			if (DamageHandler.CurrentHealth >= DamageHandler.MaxHealth || DamageHandler.CurrentHealth <= 0f)
			{
				return;
			}
			if (Mathf.Abs(Mathf.Abs(Time.time) - Mathf.Abs(DamageHandler.LastDamageTime)) < 0.1f)
			{
				m_NextAllowedTickTime = Time.time + DelayAfterDamage;
			}
			else if (!(Time.time < m_NextAllowedTickTime))
			{
				m_NextAllowedTickTime = Time.time + TickRate;
				if (vp_Gameplay.IsMaster)
				{
					DamageHandler.CurrentHealth += Mathf.Max(0f, TickAmount);
				}
				if (vp_Gameplay.IsMultiplayer && vp_Gameplay.IsMaster)
				{
					vp_GlobalEvent<Transform, Transform, float>.Send("TransmitDamage", base.transform.root, base.transform.root, 0f - Mathf.Max(0f, TickAmount));
				}
			}
		}
	}
}
