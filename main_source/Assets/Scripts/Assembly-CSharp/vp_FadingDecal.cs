using UnityEngine;

public class vp_FadingDecal : MonoBehaviour
{
	protected Color m_OriginalColor = Color.white;

	protected Quaternion m_OriginalRotation = Quaternion.identity;

	[Range(0f, 5f)]
	public float FadeoutDelay = 1f;

	[Range(0.1f, 5f)]
	public float FadeoutSpeed = 0.25f;

	protected float m_ActualFadeoutSpeed;

	protected float m_StartFadeoutTime;

	[Range(0f, 5f)]
	public float ShrinkDelay = 2f;

	[Range(0f, 5f)]
	public float ShrinkSpeed = 0.1f;

	protected float m_ActualShrinkSpeed;

	protected float m_StartShrinkTime;

	protected Vector3 m_OriginalScale = Vector3.one;

	[Range(0f, 5f)]
	public float RotateDelay = 1f;

	[Range(0f, 300f)]
	public float RotateSpeed = 15f;

	protected float m_StartRotateTime;

	protected float m_CurrentRotateSpeed;

	protected float m_ActualRotateSpeed;

	protected bool m_FlipRotate;

	public bool RotateAccelerate = true;

	private Renderer m_Renderer;

	private Renderer Renderer
	{
		get
		{
			if (m_Renderer == null)
			{
				m_Renderer = GetComponent<Renderer>();
			}
			if (m_Renderer == null || m_Renderer.material == null)
			{
				base.enabled = false;
			}
			return m_Renderer;
		}
	}

	protected virtual void Awake()
	{
		if (!(Renderer == null))
		{
			m_OriginalColor = Renderer.material.color;
			m_OriginalRotation = base.transform.rotation;
			m_OriginalScale = base.transform.localScale;
		}
	}

	protected virtual void OnEnable()
	{
		if (!(Renderer == null))
		{
			Renderer.material.color = m_OriginalColor;
			base.transform.rotation = m_OriginalRotation;
			base.transform.localScale = m_OriginalScale;
			m_StartFadeoutTime = Time.time + FadeoutDelay;
			m_StartShrinkTime = Time.time + ShrinkDelay;
			m_StartRotateTime = Time.time + RotateDelay;
			m_FlipRotate = Random.value < 0.5f;
			m_ActualFadeoutSpeed = FadeoutSpeed + FadeoutSpeed * ((Random.value < 0.5f) ? (-0.5f) : 0.5f);
			m_ActualShrinkSpeed = ShrinkSpeed + ShrinkSpeed * ((Random.value < 0.5f) ? (-0.5f) : 0.5f);
			m_ActualRotateSpeed = RotateSpeed + RotateSpeed * ((Random.value < 0.5f) ? (-0.5f) : 0.5f);
			m_CurrentRotateSpeed = 0f;
		}
	}

	protected virtual void Update()
	{
		if (Renderer == null)
		{
			return;
		}
		if (Time.time > m_StartFadeoutTime)
		{
			Color color = Renderer.material.color;
			color.a = Mathf.Lerp(color.a, 0f, Time.deltaTime * m_ActualFadeoutSpeed);
			Renderer.material.color = color;
		}
		if (ShrinkSpeed > 0f && Time.time > m_StartShrinkTime)
		{
			base.transform.localScale = Vector3.Lerp(base.transform.localScale, Vector3.zero, Time.deltaTime * m_ActualShrinkSpeed);
		}
		if (RotateSpeed > 0f && Time.time > m_StartRotateTime)
		{
			if (RotateAccelerate)
			{
				m_CurrentRotateSpeed = Mathf.SmoothStep(m_CurrentRotateSpeed, RotateSpeed, Time.deltaTime * 2f);
			}
			else
			{
				m_CurrentRotateSpeed = RotateSpeed;
			}
			base.transform.eulerAngles += Vector3.forward * Time.deltaTime * (m_FlipRotate ? (0f - m_CurrentRotateSpeed) : m_CurrentRotateSpeed);
		}
		if (Renderer.material.color.a < 0.1f)
		{
			vp_Utility.Destroy(base.gameObject);
		}
	}
}
