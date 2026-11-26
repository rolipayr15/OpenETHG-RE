using UnityEngine;

public class vp_Spin : MonoBehaviour
{
	public Vector3 RotationSpeed = new Vector3(0f, 90f, 0f);

	public bool Accelerate;

	public float Acceleration = 1f;

	protected Transform m_Transform;

	protected float m_CurrentRotationSpeed;

	protected virtual void Start()
	{
		m_Transform = base.transform;
	}

	private void OnEnable()
	{
		m_CurrentRotationSpeed = 0f;
	}

	protected virtual void Update()
	{
		if (Accelerate)
		{
			m_CurrentRotationSpeed = Mathf.Lerp(m_CurrentRotationSpeed, 1f, Time.deltaTime * Acceleration);
		}
		else
		{
			m_CurrentRotationSpeed = 1f;
		}
		m_Transform.Rotate(RotationSpeed * m_CurrentRotationSpeed * Time.deltaTime);
	}
}
