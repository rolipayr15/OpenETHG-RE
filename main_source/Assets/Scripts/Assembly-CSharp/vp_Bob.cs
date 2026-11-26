using UnityEngine;

public class vp_Bob : MonoBehaviour
{
	public Vector3 BobAmp = new Vector3(0f, 0.1f, 0f);

	public Vector3 BobRate = new Vector3(0f, 4f, 0f);

	public float BobOffset;

	public float GroundOffset;

	public bool RandomizeBobOffset;

	public bool LocalMotion;

	public bool SmoothGroundOffset;

	protected Transform m_Transform;

	protected Vector3 m_Offset;

	protected float m_CurrentGroundOffset;

	private const float GROUND_OFFSET_LERP_SPEED = 2f;

	[HideInInspector]
	public Vector3 InitialPosition = Vector3.zero;

	protected virtual void Awake()
	{
		m_Transform = base.transform;
		InitialPosition = m_Transform.position;
	}

	protected virtual void OnEnable()
	{
		m_CurrentGroundOffset = 0f;
		if (RandomizeBobOffset)
		{
			BobOffset = Random.value;
		}
	}

	protected virtual void Update()
	{
		if (SmoothGroundOffset)
		{
			m_CurrentGroundOffset = Mathf.Lerp(m_CurrentGroundOffset, GroundOffset, Time.deltaTime * 2f);
		}
		else
		{
			m_CurrentGroundOffset = GroundOffset;
		}
		if (BobRate.x != 0f && BobAmp.x != 0f)
		{
			m_Offset.x = vp_MathUtility.Sinus(BobRate.x, BobAmp.x, BobOffset);
		}
		if (BobRate.y != 0f && BobAmp.y != 0f)
		{
			m_Offset.y = vp_MathUtility.Sinus(BobRate.y, BobAmp.y, BobOffset);
		}
		if (BobRate.z != 0f && BobAmp.z != 0f)
		{
			m_Offset.z = vp_MathUtility.Sinus(BobRate.z, BobAmp.z, BobOffset);
		}
		if (!LocalMotion)
		{
			m_Transform.position = InitialPosition + m_Offset + Vector3.up * m_CurrentGroundOffset;
			return;
		}
		m_Transform.position = InitialPosition + Vector3.up * m_CurrentGroundOffset;
		m_Transform.localPosition += m_Transform.TransformDirection(m_Offset);
	}
}
