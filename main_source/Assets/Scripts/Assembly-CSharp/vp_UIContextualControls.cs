using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class vp_UIContextualControls : MonoBehaviour
{
	public class vp_UIContextualControl
	{
		public object Button;

		public vp_UITween.Handle Handle = new vp_UITween.Handle();

		public vp_UIContextualControl(GameObject obj, bool active = false)
		{
			Button = obj;
			if (obj.GetComponent<Renderer>() != null)
			{
				Color color = obj.GetComponent<Renderer>().material.color;
				color.a = (active ? 1 : 0);
				obj.GetComponent<Renderer>().material.color = color;
			}
		}
	}

	public float FadeDuration = 0.25f;

	public GameObject AttackButton;

	public GameObject JumpButton;

	public GameObject ReloadButton;

	public GameObject ZoomButton;

	public GameObject RunButton;

	protected vp_UIManager m_Manager;

	protected List<vp_UIContextualControl> m_Buttons = new List<vp_UIContextualControl>();

	protected bool m_Initialized;

	protected virtual void Awake()
	{
		m_Manager = GetComponent<vp_UIManager>();
		SetupButtons();
	}

	protected virtual void Start()
	{
		m_Initialized = true;
	}

	protected virtual void Update()
	{
		UpdateVisibility();
	}

	protected virtual void SetupButtons()
	{
		if (m_Buttons.Count == 0)
		{
			m_Buttons.Add(new vp_UIContextualControl(AttackButton));
			m_Buttons.Add(new vp_UIContextualControl(JumpButton, active: true));
			m_Buttons.Add(new vp_UIContextualControl(ReloadButton));
			m_Buttons.Add(new vp_UIContextualControl(ZoomButton));
			m_Buttons.Add(new vp_UIContextualControl(RunButton, active: true));
		}
	}

	protected virtual void UpdateVisibility()
	{
		if (m_Manager.Player == null || !m_Initialized)
		{
			return;
		}
		bool active = true;
		bool active2 = true;
		bool active3 = true;
		bool active4 = true;
		bool active5 = true;
		bool flag = m_Manager.Player.Interactable.Get() != null && m_Manager.Player.Interactable.Get().GetType() == typeof(vp_Grab);
		if (m_Manager.Player.Climb.Active || (!m_Manager.Player.CurrentWeaponWielded.Get() && !flag))
		{
			active = (active4 = (active3 = false));
			if (m_Manager.Player.Climb.Active)
			{
				active5 = false;
			}
		}
		if (flag || (m_Manager.Player.CurrentWeaponIndex.Get() > 0 && m_Manager.Player.CurrentWeaponMaxAmmoCount.Get() == 0))
		{
			active4 = (active3 = false);
		}
		if (!m_Manager.Player.SetWeapon.Active)
		{
			TweenColor(m_Buttons.FirstOrDefault((vp_UIContextualControl b) => b.Button == AttackButton), active);
			TweenColor(m_Buttons.FirstOrDefault((vp_UIContextualControl b) => b.Button == JumpButton), active2);
			TweenColor(m_Buttons.FirstOrDefault((vp_UIContextualControl b) => b.Button == ReloadButton), active3);
			TweenColor(m_Buttons.FirstOrDefault((vp_UIContextualControl b) => b.Button == ZoomButton), active4);
			TweenColor(m_Buttons.FirstOrDefault((vp_UIContextualControl b) => b.Button == RunButton), active5);
		}
	}

	protected virtual void TweenColor(vp_UIContextualControl button, bool active)
	{
		if (button != null)
		{
			GameObject gameObject = (GameObject)button.Button;
			if (!(gameObject == null))
			{
				vp_UITween.FadeTo(gameObject, active ? 1 : 0, FadeDuration, button.Handle);
			}
		}
	}
}
