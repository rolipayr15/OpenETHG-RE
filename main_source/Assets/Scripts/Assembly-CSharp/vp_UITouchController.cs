using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class vp_UITouchController : vp_UIControl
{
	public enum vp_TouchControllerType
	{
		DynamicJoystick,
		StaticJoystick,
		TouchPad
	}

	public delegate void ControllerTypeChangedEventHandler(vp_TouchControllerType type);

	public Transform Knob;

	public Transform Background;

	[SerializeField]
	protected vp_TouchControllerType m_ControllerType;

	public float AnimationSpeed = 20f;

	public Vector2 Deadzone = new Vector2(0.25f, 0.25f);

	public Color KnobOnColor = new Color(1f, 1f, 1f, 1f);

	public Color BackgroundOnColor = new Color(1f, 1f, 1f, 0.4f);

	public Color KnobOffColor = new Color(1f, 1f, 1f, 0f);

	public Color BackgroundOffColor = new Color(1f, 1f, 1f, 0f);

	public ControllerTypeChangedEventHandler ControllerTypeChanged;

	protected Vector3 m_DefaultKnobPosition = Vector3.zero;

	protected Vector3 m_DefaultPanelPosition = Vector3.zero;

	protected Vector2 m_RawMove = Vector2.zero;

	protected Transform m_PanelTransform;

	protected Bounds m_KnobArea;

	protected ParticleSystem m_PadParticles;

	protected List<BoxCollider> m_BoxColliders = new List<BoxCollider>();

	public vp_TouchControllerType ControllerType
	{
		get
		{
			return m_ControllerType;
		}
		set
		{
			m_ControllerType = value;
			OnControllerTypeChanged(m_ControllerType);
		}
	}

	public virtual Color32 KnobColor
	{
		get
		{
			return Knob.GetComponent<Renderer>().material.color;
		}
		set
		{
			Knob.GetComponent<Renderer>().material.color = value;
		}
	}

	public virtual Color32 BackgroundColor
	{
		get
		{
			return Background.GetComponent<Renderer>().material.color;
		}
		set
		{
			Background.GetComponent<Renderer>().material.color = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_PanelTransform = Knob.parent;
		m_DefaultKnobPosition = Knob.localPosition;
		m_DefaultPanelPosition = m_PanelTransform.localPosition;
		m_PadParticles = Knob.GetComponent<ParticleSystem>();
		if (m_PadParticles != null)
		{
			ParticleSystem.EmissionModule emission = m_PadParticles.emission;
			emission.enabled = false;
		}
		if (Knob.GetComponent<Collider>() != null)
		{
			m_KnobArea = Knob.GetComponent<Collider>().bounds;
		}
		OnControllerTypeChanged(m_ControllerType);
	}

	protected override void OnEnable()
	{
		if (m_Initialized)
		{
			base.OnEnable();
			vp_InputMobile.MoveAxisCallback = (vp_RawAxisEventHandler)Delegate.Combine(vp_InputMobile.MoveAxisCallback, new vp_RawAxisEventHandler(GetMove));
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		vp_InputMobile.MoveAxisCallback = (vp_RawAxisEventHandler)Delegate.Remove(vp_InputMobile.MoveAxisCallback, new vp_RawAxisEventHandler(GetMove));
	}

	protected override void Update()
	{
		JoystickUpdate();
	}

	protected virtual void OnControllerTypeChanged(vp_TouchControllerType type)
	{
		if (!Application.isPlaying)
		{
			return;
		}
		Color color = BackgroundOnColor;
		Color color2 = BackgroundOffColor;
		Color color3 = KnobOnColor;
		Color color4 = KnobOffColor;
		m_PanelTransform.localPosition = m_DefaultPanelPosition;
		MeshRenderer component = Knob.GetComponent<MeshRenderer>();
		MeshRenderer component2 = Background.GetComponent<MeshRenderer>();
		if (m_ControllerType == vp_TouchControllerType.TouchPad)
		{
			color = Color.clear;
			color2 = Color.clear;
			color3 = Color.clear;
			color4 = Color.clear;
			if (component != null)
			{
				component.enabled = false;
			}
			if (component2 != null)
			{
				component2.enabled = false;
			}
		}
		else
		{
			if (component != null)
			{
				component.enabled = true;
			}
			if (component2 != null)
			{
				component2.enabled = true;
			}
		}
		KnobColor = ((m_ControllerType == vp_TouchControllerType.StaticJoystick) ? color3 : color4);
		BackgroundColor = ((m_ControllerType == vp_TouchControllerType.StaticJoystick) ? color : color2);
		if (m_PadParticles != null)
		{
			ParticleSystem.EmissionModule emission = m_PadParticles.emission;
			emission.enabled = false;
		}
		if (ControllerTypeChanged != null)
		{
			ControllerTypeChanged(m_ControllerType);
		}
	}

	protected override void TouchesBegan(vp_Touch touch)
	{
		if (base.LastFingerID == -1 && (m_ControllerType != vp_TouchControllerType.StaticJoystick || (from hit in Physics.RaycastAll(m_Camera.ScreenPointToRay(touch.Position))
			where hit.collider == Knob.GetComponent<Collider>()
			select hit).ToList().Count != 0) && RaycastControl(touch))
		{
			base.TouchesBegan(touch);
			if (m_ControllerType != vp_TouchControllerType.StaticJoystick)
			{
				m_PanelTransform.position = m_Camera.ScreenToWorldPoint(touch.Position);
			}
			m_KnobArea.center = m_PanelTransform.position;
		}
	}

	protected virtual void TouchesMoved(vp_Touch touch)
	{
		if (base.LastFingerID == touch.FingerID)
		{
			Vector3 position = Knob.position;
			Vector3 position2 = ConstrainToBounds(m_Camera.ScreenToWorldPoint(touch.Position));
			if (m_ControllerType == vp_TouchControllerType.TouchPad)
			{
				HandlePadParticles(enable: true);
				position2 = m_Camera.ScreenToWorldPoint(touch.Position);
			}
			position2.z = position.z;
			Knob.position = position2;
			float num = (Knob.localScale.x - m_KnobArea.max.x) / 2f;
			float num2 = (Knob.localScale.y - m_KnobArea.max.y) / 2f;
			Vector3 vector = Knob.localPosition - m_DefaultKnobPosition;
			float x = Mathf.Clamp(vector.x / num, -1f - Deadzone.x, 1f + Deadzone.x);
			float y = Mathf.Clamp(vector.y / num2, -1f - Deadzone.y, 1f + Deadzone.y);
			Vector2 rawMove = new Vector2(x, y);
			if (rawMove.x <= Deadzone.x && rawMove.x >= 0f - Deadzone.x)
			{
				rawMove.x = 0f;
			}
			else if (rawMove.x > 0f)
			{
				rawMove.x -= Deadzone.x;
			}
			else if (rawMove.x < 0f)
			{
				rawMove.x += Deadzone.x;
			}
			if (rawMove.y <= Deadzone.y && rawMove.y >= 0f - Deadzone.y)
			{
				rawMove.y = 0f;
			}
			else if (rawMove.y > 0f)
			{
				rawMove.y -= Deadzone.y;
			}
			else if (rawMove.y < 0f)
			{
				rawMove.y += Deadzone.y;
			}
			m_RawMove = rawMove;
		}
	}

	protected virtual Vector3 ConstrainToBounds(Vector3 pos)
	{
		if (pos.x > m_KnobArea.max.x)
		{
			pos.x = m_KnobArea.max.x;
		}
		if (pos.x < m_KnobArea.min.x)
		{
			pos.x = m_KnobArea.min.x;
		}
		if (pos.y > m_KnobArea.max.y)
		{
			pos.y = m_KnobArea.max.y;
		}
		if (pos.y < m_KnobArea.min.y)
		{
			pos.y = m_KnobArea.min.y;
		}
		return pos;
	}

	protected virtual void HandlePadParticles(bool enable)
	{
		if (m_ControllerType == vp_TouchControllerType.TouchPad && !(m_PadParticles == null))
		{
			ParticleSystem.EmissionModule emission = m_PadParticles.emission;
			emission.enabled = enable;
		}
	}

	protected virtual float GetMove(string id)
	{
		if (!(id == "Horizontal"))
		{
			if (!(id == "Vertical"))
			{
				return 0f;
			}
			return m_RawMove.y;
		}
		return m_RawMove.x;
	}

	public override void TouchesFinished(vp_Touch touch)
	{
		if (base.LastFingerID == touch.FingerID)
		{
			HandlePadParticles(enable: false);
			Manager.Player.Run.TryStop();
			base.LastFingerID = -1;
		}
	}

	protected virtual void JoystickUpdate()
	{
		if (base.LastFingerID != -1 && !Manager.Player.Run.Active)
		{
			if (m_ControllerType == vp_TouchControllerType.DynamicJoystick)
			{
				if (Knob != null)
				{
					KnobColor = Color.Lerp(KnobColor, KnobOnColor, Time.deltaTime * AnimationSpeed);
				}
				if (Background != null)
				{
					BackgroundColor = Color.Lerp(BackgroundColor, BackgroundOnColor, Time.deltaTime * AnimationSpeed);
				}
			}
			return;
		}
		if (m_ControllerType == vp_TouchControllerType.DynamicJoystick)
		{
			if (Knob != null)
			{
				KnobColor = Color.Lerp(KnobColor, KnobOffColor, Time.deltaTime * AnimationSpeed);
			}
			if (Background != null)
			{
				BackgroundColor = Color.Lerp(BackgroundColor, BackgroundOffColor, Time.deltaTime * AnimationSpeed);
			}
		}
		if (Knob != null && m_ControllerType != vp_TouchControllerType.TouchPad)
		{
			Knob.localPosition = Vector3.Slerp(Knob.localPosition, m_DefaultKnobPosition, Time.deltaTime * AnimationSpeed);
		}
		if (Manager.Player.Run.Active)
		{
			if (m_ControllerType != vp_TouchControllerType.StaticJoystick)
			{
				KnobColor = KnobOffColor;
				BackgroundColor = BackgroundOffColor;
			}
		}
		else
		{
			m_RawMove = Vector2.zero;
		}
	}
}
