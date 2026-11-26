using UnityEngine;

[RequireComponent(typeof(vp_DamageHandler))]
[RequireComponent(typeof(Rigidbody))]
public class vp_Grenade : MonoBehaviour
{
	public float LifeTime = 3f;

	public float RigidbodyForce = 10f;

	public float RigidbodySpin;

	protected Rigidbody m_Rigidbody;

	protected Transform m_Source;

	protected Transform m_OriginalSource;

	protected virtual void Awake()
	{
		m_Rigidbody = GetComponent<Rigidbody>();
	}

	protected virtual void OnEnable()
	{
		if (!(m_Rigidbody == null))
		{
			vp_Timer.In(LifeTime, delegate
			{
				base.transform.SendMessage("DieBySources", new Transform[2] { m_Source, m_OriginalSource }, SendMessageOptions.DontRequireReceiver);
			});
			if (RigidbodyForce != 0f)
			{
				m_Rigidbody.AddForce(base.transform.forward * RigidbodyForce, ForceMode.Impulse);
			}
			if (RigidbodySpin != 0f)
			{
				m_Rigidbody.AddTorque(Random.rotation.eulerAngles * RigidbodySpin);
			}
		}
	}

	public void SetSource(Transform source)
	{
		m_Source = base.transform;
		m_OriginalSource = source;
	}
}
