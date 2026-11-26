using UnityEngine;

public class vp_ParticleFXPooler : MonoBehaviour
{
	private ParticleSystem m_ShurikenParticleSystem;

	private void Awake()
	{
		m_ShurikenParticleSystem = GetComponent<ParticleSystem>();
	}

	private void Update()
	{
		if (m_ShurikenParticleSystem != null && !m_ShurikenParticleSystem.IsAlive() && base.gameObject != null)
		{
			vp_Utility.Destroy(base.gameObject);
		}
	}
}
