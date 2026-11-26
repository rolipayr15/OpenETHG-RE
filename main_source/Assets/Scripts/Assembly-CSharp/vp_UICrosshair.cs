using System.Collections.Generic;
using UnityEngine;

public class vp_UICrosshair : vp_UIControl
{
	public Color EnemyColor = Color.red;

	public float ColorChangeDuration = 0.25f;

	public LayerMask EnemyMask = 33554432;

	public float InteractIconSize = 2f;

	public Texture EnemyCrosshair;

	public bool ScaleOnRun = true;

	public float ScaleMultiplier = 1.25f;

	public bool AssistedTargeting = true;

	public float AssistedTargetingSpeed = 20f;

	public float AssistedTargetingRadius = 1f;

	public float AssistedTargetingTrackingRadius = 2f;

	public bool AssistedTargetingFoldout;

	protected Renderer m_Renderer;

	protected GameObject m_GameObject;

	protected Color m_DefaultColor;

	protected Camera m_PlayerCamera;

	protected Vector3 m_ScreenCenter = Vector3.zero;

	protected Vector3 m_CachedScale = Vector3.one;

	protected string m_CachedTextureName = "";

	protected Dictionary<Collider, Transform> m_Enemies = new Dictionary<Collider, Transform>();

	protected Vector3 m_CrosshairNextPosition = Vector3.zero;

	protected Transform m_CurrentTrackedEnemy;

	protected bool m_Tracking;

	protected bool m_ShowCrosshair = true;

	protected bool m_Interacting;

	protected vp_UITween.Handle m_ColorHandle = new vp_UITween.Handle();

	protected vp_UITween.Handle m_ScaleCrosshairHandle = new vp_UITween.Handle();

	protected virtual Color m_CrosshairColor
	{
		get
		{
			Color result = m_DefaultColor;
			if (m_Tracking)
			{
				result = EnemyColor;
			}
			result.a = (m_ShowCrosshair ? 1 : 0);
			return result;
		}
	}

	protected virtual Texture OnValue_Crosshair
	{
		get
		{
			return m_Renderer.material.mainTexture;
		}
		set
		{
			if (m_Tracking && EnemyCrosshair != null)
			{
				m_Renderer.material.mainTexture = EnemyCrosshair;
			}
			else if (value.name == "")
			{
				m_ShowCrosshair = false;
			}
			else
			{
				if (!Manager.Player.Zoom.Active)
				{
					m_ShowCrosshair = true;
				}
				m_Renderer.material.mainTexture = value;
			}
			if (!m_Tracking)
			{
				Vector3 vector = ((value.name == m_CachedTextureName) ? m_CachedScale : (m_CachedScale * InteractIconSize));
				vector = ((Manager.Player.Interactable.Get() != null && Manager.Player.Interactable.Get().GetType() == typeof(vp_Grab) && value.name == "") ? (m_CachedScale * (InteractIconSize * 3f)) : vector);
				vp_UITween.ScaleTo(m_GameObject, vp_UITween.Hash("scale", vector, "duration", 0, "handle", m_ScaleCrosshairHandle));
			}
		}
	}

	protected virtual bool TrackingEnemy()
	{
		return m_Tracking;
	}

	protected override void Awake()
	{
		base.Awake();
		m_Renderer = GetComponent<Renderer>();
		m_GameObject = base.gameObject;
		if (m_Renderer != null && m_Renderer.material != null && m_Renderer.material.mainTexture != null)
		{
			m_DefaultColor = m_Renderer.material.color;
			m_CachedTextureName = m_Renderer.material.mainTexture.name;
		}
		m_PlayerCamera = Manager.Player.GetComponentInChildren<vp_FPCamera>().GetComponent<Camera>();
		m_CachedScale = m_Transform.localScale;
	}

	protected override void Start()
	{
		m_ScreenCenter = Manager.UICamera.GetComponent<Camera>().ScreenToWorldPoint(new Vector3((float)Screen.width * 0.5f, (float)Screen.height * 0.5f, m_Transform.position.z));
		base.Start();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		vp_GlobalEvent<bool>.Register("Display Crosshair", DisplayCrosshair);
		vp_GlobalEventReturn<bool>.Register("Tracking Enemy", TrackingEnemy);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		vp_GlobalEvent<bool>.Unregister("Display Crosshair", DisplayCrosshair);
		vp_GlobalEventReturn<bool>.Unregister("Tracking Enemy", TrackingEnemy);
	}

	protected override void Update()
	{
		m_Interacting = (Manager.Player.Interactable.Get() != null && Manager.Player.Interactable.Get().GetType() == typeof(vp_Grab)) || Manager.Player.Climb.Active;
		EnemyCheck();
		HandleCrosshairColor();
		if (m_CurrentTrackedEnemy == null)
		{
			m_CrosshairNextPosition = m_ScreenCenter;
		}
		m_Transform.position = Vector3.Lerp(m_Transform.position, m_CrosshairNextPosition, Time.deltaTime * AssistedTargetingSpeed);
		TrackEnemy();
		if (m_Tracking && EnemyCrosshair != null)
		{
			Manager.Player.Crosshair.Set(EnemyCrosshair);
		}
	}

	protected virtual void HandleCrosshairColor()
	{
		if (m_ShowCrosshair)
		{
			vp_UITween.ColorTo(m_GameObject, m_CrosshairColor, ColorChangeDuration, m_ColorHandle);
		}
		else
		{
			m_GameObject.GetComponent<Renderer>().material.color = m_CrosshairColor;
		}
	}

	protected virtual void TrackEnemy()
	{
		m_Tracking = false;
		if (m_CurrentTrackedEnemy == null || m_Interacting || Manager.Player.Dead.Active)
		{
			return;
		}
		Transform value = null;
		RaycastHit[] array = Physics.SphereCastAll(m_PlayerCamera.transform.position, AssistedTargetingTrackingRadius, m_PlayerCamera.transform.forward, float.PositiveInfinity);
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit raycastHit = array[i];
			if (!m_Enemies.TryGetValue(raycastHit.collider, out value))
			{
				m_Enemies.Add(raycastHit.collider, value = raycastHit.collider.transform);
			}
			if (!(value != null) || !(value == m_CurrentTrackedEnemy))
			{
				continue;
			}
			m_Tracking = true;
			if (AssistedTargeting)
			{
				Vector3 position = new Vector3(m_CurrentTrackedEnemy.GetComponent<Collider>().bounds.center.x, m_CurrentTrackedEnemy.GetComponent<Collider>().bounds.center.y + (m_CurrentTrackedEnemy.GetComponent<Collider>().bounds.max.y - m_CurrentTrackedEnemy.GetComponent<Collider>().bounds.center.y) / 2f, m_CurrentTrackedEnemy.GetComponent<Collider>().bounds.center.z);
				Vector3 position2 = m_PlayerCamera.WorldToViewportPoint(position);
				position2.z = m_ScreenCenter.z;
				m_CrosshairNextPosition = Manager.UICamera.ViewportToWorldPoint(position2);
				if (vp_Input.GetAxisRaw("Mouse X") == 0f && vp_Input.GetAxisRaw("Mouse Y") == 0f)
				{
					Quaternion quaternion = Quaternion.Slerp(m_PlayerCamera.transform.rotation, Quaternion.LookRotation(m_CurrentTrackedEnemy.GetComponent<Collider>().bounds.center - m_PlayerCamera.transform.position), Time.deltaTime * 3f);
					Manager.Player.Rotation.Set(new Vector2(quaternion.eulerAngles.x, quaternion.eulerAngles.y));
				}
			}
		}
		if (!m_Tracking)
		{
			m_CurrentTrackedEnemy = null;
		}
	}

	protected virtual void OnStart_Climb()
	{
		DisplayCrosshair(val: false);
	}

	protected virtual void OnStop_Climb()
	{
		DisplayCrosshair();
	}

	protected virtual void OnStart_Zoom()
	{
		DisplayCrosshair(val: false);
	}

	protected virtual void OnStop_Zoom()
	{
		DisplayCrosshair();
	}

	protected virtual void OnStart_Run()
	{
		if (!Manager.Player.CanInteract.Get() && ScaleOnRun)
		{
			vp_UITween.ScaleTo(m_GameObject, vp_UITween.Hash("scale", m_CachedScale * ScaleMultiplier, "duration", 0.15f, "handle", m_ScaleCrosshairHandle));
		}
	}

	protected virtual void OnStop_Run()
	{
		if (!Manager.Player.CanInteract.Get() && ScaleOnRun)
		{
			vp_UITween.ScaleTo(m_GameObject, vp_UITween.Hash("scale", m_CachedScale, "duration", 0.15f, "handle", m_ScaleCrosshairHandle));
		}
	}

	protected virtual void DisplayCrosshair(bool val = true)
	{
		m_ShowCrosshair = val;
	}

	protected void EnemyCheck()
	{
		if (!m_Interacting && !Manager.Player.Dead.Active)
		{
			Transform value = null;
			if (Physics.SphereCast(m_PlayerCamera.transform.position, AssistedTargetingRadius, m_PlayerCamera.transform.forward, out var hitInfo, float.PositiveInfinity, EnemyMask) && !m_Enemies.TryGetValue(hitInfo.collider, out value))
			{
				m_Enemies.Add(hitInfo.collider, value = hitInfo.collider.transform);
			}
			if (value != null)
			{
				m_CurrentTrackedEnemy = (Physics.Linecast(m_PlayerCamera.transform.position, value.position, ~((int)EnemyMask | 0x8000000)) ? null : value);
			}
		}
	}
}
