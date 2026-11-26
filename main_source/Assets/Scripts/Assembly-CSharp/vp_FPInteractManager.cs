using System.Collections.Generic;
using UnityEngine;

public class vp_FPInteractManager : MonoBehaviour
{
	public float InteractDistance = 2f;

	public float InteractDistance3rdPerson = 3f;

	public float MaxInteractDistance = 25f;

	protected vp_FPPlayerEventHandler m_Player;

	protected vp_FPCamera m_Camera;

	protected vp_Interactable m_CurrentInteractable;

	protected Texture m_OriginalCrosshair;

	protected Dictionary<Collider, vp_Interactable> m_Interactables = new Dictionary<Collider, vp_Interactable>();

	protected vp_Interactable m_LastInteractable;

	protected vp_Timer.Handle m_ShowTextTimer = new vp_Timer.Handle();

	protected bool m_CanInteract;

	public float CrosshairTimeoutTimer { get; set; }

	protected virtual vp_Interactable OnValue_Interactable
	{
		get
		{
			return m_CurrentInteractable;
		}
		set
		{
			m_CurrentInteractable = value;
		}
	}

	protected virtual bool OnValue_CanInteract
	{
		get
		{
			return m_CanInteract;
		}
		set
		{
			m_CanInteract = value;
		}
	}

	protected virtual void Awake()
	{
		m_Player = GetComponent<vp_FPPlayerEventHandler>();
		m_Camera = GetComponentInChildren<vp_FPCamera>();
	}

	protected virtual void OnEnable()
	{
		if (m_Player != null)
		{
			m_Player.Register(this);
		}
	}

	protected virtual void OnDisable()
	{
		if (m_Player != null)
		{
			m_Player.Unregister(this);
		}
	}

	public virtual void OnStart_Dead()
	{
		ShouldFinishInteraction();
	}

	public virtual void LateUpdate()
	{
		_ = m_Player.Dead.Active;
	}

	protected virtual bool CanStart_Interact()
	{
		if (ShouldFinishInteraction())
		{
			return false;
		}
		if (m_Player.SetWeapon.Active)
		{
			return false;
		}
		if (m_LastInteractable != null)
		{
			if (m_LastInteractable.InteractType != vp_Interactable.vp_InteractType.Normal)
			{
				return false;
			}
			if (!m_LastInteractable.TryInteract(m_Player))
			{
				return false;
			}
			ResetCrosshair(reset: false);
			return true;
		}
		return false;
	}

	protected virtual bool ShouldFinishInteraction()
	{
		if (m_Player.Interactable.Get() != null)
		{
			m_LastInteractable = null;
			ResetCrosshair();
			m_Player.Interactable.Get().FinishInteraction();
			m_Player.Interactable.Set(null);
			return true;
		}
		return false;
	}

	protected virtual void InteractCrosshair()
	{
		if (m_Player.Crosshair.Get() == null || m_Player.Interactable.Get() != null)
		{
			return;
		}
		vp_Interactable interactable = null;
		if (FindInteractable(out interactable))
		{
			if (!(interactable != m_LastInteractable) || (CrosshairTimeoutTimer > Time.time && m_LastInteractable != null && interactable.GetType() == m_LastInteractable.GetType()))
			{
				return;
			}
			m_CanInteract = true;
			m_LastInteractable = interactable;
			if (interactable.InteractText != "" && !m_ShowTextTimer.Active)
			{
				vp_Timer.In(interactable.DelayShowingText, delegate
				{
					m_Player.HUDText.Send(interactable.InteractText);
				}, m_ShowTextTimer);
			}
			if (!(interactable.m_InteractCrosshair == null))
			{
				m_Player.Crosshair.Set(interactable.m_InteractCrosshair);
			}
		}
		else
		{
			m_CanInteract = false;
			ResetCrosshair();
		}
	}

	protected virtual bool FindInteractable(out vp_Interactable interactable)
	{
		interactable = null;
		LayerMask layerMask = -1761607701;
		if (Physics.Raycast(m_Camera.Transform.position, m_Camera.Transform.forward, out var hitInfo, MaxInteractDistance, layerMask))
		{
			if (!m_Interactables.TryGetValue(hitInfo.collider, out interactable))
			{
				m_Interactables.Add(hitInfo.collider, interactable = hitInfo.collider.GetComponent<vp_Interactable>());
			}
			if (interactable == null)
			{
				return false;
			}
			if (interactable.InteractDistance == 0f && hitInfo.distance >= (m_Player.IsFirstPerson.Get() ? InteractDistance : InteractDistance3rdPerson))
			{
				return false;
			}
			if (interactable.InteractDistance > 0f && hitInfo.distance >= interactable.InteractDistance)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	protected virtual void ResetCrosshair(bool reset = true)
	{
		if (!(m_OriginalCrosshair == null) && !(m_Player.Crosshair.Get() == m_OriginalCrosshair))
		{
			m_ShowTextTimer.Cancel();
			if (reset)
			{
				m_Player.Crosshair.Set(m_OriginalCrosshair);
			}
			m_LastInteractable = null;
		}
	}

	protected virtual void OnControllerColliderHit(ControllerColliderHit hit)
	{
		Rigidbody attachedRigidbody = hit.collider.attachedRigidbody;
		if (!(attachedRigidbody == null) && !attachedRigidbody.isKinematic)
		{
			vp_Interactable value = null;
			if (!m_Interactables.TryGetValue(hit.collider, out value))
			{
				m_Interactables.Add(hit.collider, value = hit.collider.GetComponent<vp_Interactable>());
			}
			if (!(value == null) && value.InteractType == vp_Interactable.vp_InteractType.CollisionTrigger)
			{
				hit.gameObject.SendMessage("TryInteract", m_Player, SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}
