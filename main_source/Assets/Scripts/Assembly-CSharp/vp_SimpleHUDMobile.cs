using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class vp_SimpleHUDMobile : vp_SimpleHUD
{
	public float FadeDuration = 3f;

	public bool ShowTips = true;

	public GameObject AmmoLabel;

	public GameObject HealthLabel;

	public GameObject HintsLabel;

	private TextMesh m_AmmoLabel;

	private TextMesh m_HealthLabel;

	private TextMesh m_HintsLabel;

	protected vp_UITween.Handle m_HUDTextTweenHandle = new vp_UITween.Handle();

	protected Color m_MessageColorMobile = new Color(2f, 2f, 0f, 2f);

	protected Color m_InvisibleColorMobile = new Color(1f, 1f, 0f, 0f);

	protected string m_PickupMessageMobile = "";

	protected static GUIStyle m_MessageStyleMobile;

	protected vp_FPPlayerEventHandler m_PlayerEventHandler;

	public static GUIStyle MessageStyleMobile
	{
		get
		{
			if (m_MessageStyleMobile == null)
			{
				m_MessageStyleMobile = new GUIStyle("Label");
				m_MessageStyleMobile.alignment = TextAnchor.MiddleCenter;
			}
			return m_MessageStyleMobile;
		}
	}

	protected int m_Health
	{
		get
		{
			int num = (int)(m_PlayerEventHandler.Health.Get() * 100f);
			if (num >= 0)
			{
				return num;
			}
			return 0;
		}
	}

	protected override void Awake()
	{
		m_PlayerEventHandler = base.transform.root.GetComponentInChildren<vp_FPPlayerEventHandler>();
		if (AmmoLabel != null)
		{
			m_AmmoLabel = AmmoLabel.GetComponentInChildren<TextMesh>();
		}
		if (HealthLabel != null)
		{
			m_HealthLabel = HealthLabel.GetComponentInChildren<TextMesh>();
		}
		if (HintsLabel != null)
		{
			m_HintsLabel = HintsLabel.GetComponentInChildren<TextMesh>();
		}
		if (m_HintsLabel != null)
		{
			m_HintsLabel.text = "";
			m_HintsLabel.GetComponent<Renderer>().material.color = Color.clear;
		}
	}

	protected override void OnEnable()
	{
		if (m_PlayerEventHandler != null)
		{
			m_PlayerEventHandler.Register(this);
		}
	}

	protected override void OnDisable()
	{
		if (m_PlayerEventHandler != null)
		{
			m_PlayerEventHandler.Unregister(this);
		}
	}

	protected override void OnGUI()
	{
	}

	protected virtual void Update()
	{
		if (m_AmmoLabel != null)
		{
			if (m_PlayerEventHandler.CurrentWeaponIndex.Get() > 0)
			{
				m_AmmoLabel.text = (m_PlayerEventHandler.CurrentWeaponAmmoCount.Get() + "/" + m_PlayerEventHandler.CurrentWeaponClipCount.Get()).ToString();
			}
			else
			{
				m_AmmoLabel.text = "0/0";
			}
		}
		if (m_HealthLabel != null)
		{
			m_HealthLabel.text = m_Health + "%";
		}
	}

	protected override void OnMessage_HUDText(string message)
	{
		if (ShowTips && !(m_HintsLabel == null))
		{
			m_PickupMessageMobile = message;
			m_HintsLabel.text = m_PickupMessageMobile;
			vp_UITween.ColorTo(HintsLabel, Color.white, 0.25f, m_HUDTextTweenHandle, delegate
			{
				vp_UITween.ColorTo(HintsLabel, m_InvisibleColorMobile, FadeDuration, m_HUDTextTweenHandle);
			});
		}
	}
}
