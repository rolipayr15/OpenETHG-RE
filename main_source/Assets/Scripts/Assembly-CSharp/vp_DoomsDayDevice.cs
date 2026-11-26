using UnityEngine;

public class vp_DoomsDayDevice : MonoBehaviour
{
	public AudioClip EarthQuakeSound;

	protected vp_FPPlayerEventHandler m_Player;

	protected bool Initiated;

	protected vp_PulsingLight m_PulsingLight;

	protected AudioSource m_PlayerAudioSource;

	protected AudioSource m_DeviceAudioSource;

	protected Vector3 m_OriginalButtonPos;

	protected Color m_OriginalButtonColor;

	protected float m_OriginalPulsingLightMaxIntensity;

	protected Renderer m_ButtonRenderer;

	protected GameObject m_Button;

	private bool TriedFindButton;

	private bool TriedFindButtonRenderer;

	protected Renderer ButtonRenderer
	{
		get
		{
			if (m_ButtonRenderer == null && Button != null && !TriedFindButtonRenderer)
			{
				m_ButtonRenderer = Button.GetComponent<Renderer>();
				TriedFindButtonRenderer = true;
			}
			return m_ButtonRenderer;
		}
	}

	protected GameObject Button
	{
		get
		{
			if (m_Button == null && !TriedFindButton)
			{
				m_Button = GameObject.Find("ForbiddenButton");
				TriedFindButton = true;
			}
			return m_Button;
		}
	}

	protected virtual void Awake()
	{
		m_Player = Object.FindObjectOfType(typeof(vp_FPPlayerEventHandler)) as vp_FPPlayerEventHandler;
		if (m_Player != null)
		{
			m_PlayerAudioSource = m_Player.GetComponent<AudioSource>();
		}
		m_DeviceAudioSource = GetComponent<AudioSource>();
		if (Button != null)
		{
			m_OriginalButtonPos = Button.transform.localPosition;
			m_OriginalButtonColor = ButtonRenderer.material.color;
			m_PulsingLight = Button.GetComponentInChildren<vp_PulsingLight>();
		}
		if (m_PulsingLight != null)
		{
			m_OriginalPulsingLightMaxIntensity = m_PulsingLight.m_MaxIntensity;
		}
	}

	protected virtual void OnEnable()
	{
		if (m_Player != null)
		{
			m_Player.Register(this);
		}
		if (Button != null)
		{
			Button.transform.localPosition = m_OriginalButtonPos;
			ButtonRenderer.material.color = m_OriginalButtonColor;
		}
		if (m_DeviceAudioSource != null)
		{
			m_DeviceAudioSource.pitch = 1f;
			m_DeviceAudioSource.volume = 1f;
		}
		if (m_PulsingLight != null)
		{
			m_PulsingLight.m_MaxIntensity = m_OriginalPulsingLightMaxIntensity;
		}
	}

	protected virtual void OnDisable()
	{
		if (m_Player != null)
		{
			m_Player.Unregister(this);
		}
	}

	protected virtual void Update()
	{
		if (Initiated)
		{
			if (Button != null)
			{
				ButtonRenderer.material.color = Color.Lerp(ButtonRenderer.material.color, m_OriginalButtonColor * 0.2f, Time.deltaTime * 1.5f);
			}
			if (m_DeviceAudioSource != null)
			{
				m_DeviceAudioSource.pitch = Mathf.Max(0.1f, m_DeviceAudioSource.pitch - Time.deltaTime * 0.35f);
			}
			if (m_PulsingLight != null)
			{
				m_PulsingLight.m_MaxIntensity = 2.5f;
			}
		}
	}

	protected virtual void InitiateDoomsDay()
	{
		if (!Initiated)
		{
			Initiated = true;
			if (Button != null)
			{
				Button.transform.localPosition += Vector3.down * 0.02f;
			}
			if (m_PlayerAudioSource != null)
			{
				m_PlayerAudioSource.PlayOneShot(EarthQuakeSound);
			}
			m_Player.CameraEarthQuake.TryStart(new Vector3(0.05f, 0.05f, 10f));
			vp_Timer.In(3f, delegate
			{
				SendMessage("Die");
			});
			vp_Timer.In(3f, delegate
			{
				Initiated = false;
			});
		}
	}
}
