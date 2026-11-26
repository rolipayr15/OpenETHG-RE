using System;
using System.Collections.Generic;
using UnityEngine;

public class vp_PlayerDamageHandler : vp_DamageHandler
{
	private vp_PlayerEventHandler m_Player;

	private vp_PlayerInventory m_Inventory;

	public bool AllowFallDamage = true;

	public float FallDamageThreshold = 0.15f;

	public bool DeathOnFallImpactThreshold;

	protected float m_FallImpactMultiplier = 2f;

	protected bool m_InventoryWasEnabledAtStart = true;

	protected List<Collider> m_Colliders;

	protected vp_PlayerEventHandler Player
	{
		get
		{
			if (m_Player == null)
			{
				m_Player = base.transform.GetComponent<vp_PlayerEventHandler>();
			}
			return m_Player;
		}
	}

	protected vp_PlayerInventory Inventory
	{
		get
		{
			if (m_Inventory == null)
			{
				m_Inventory = base.transform.root.GetComponentInChildren<vp_PlayerInventory>();
			}
			return m_Inventory;
		}
	}

	[Obsolete("Please use the 'FallDamageThreshold' parameter instead.")]
	public float FallImpactThreshold
	{
		get
		{
			return FallDamageThreshold;
		}
		set
		{
			FallDamageThreshold = value;
		}
	}

	protected List<Collider> Colliders
	{
		get
		{
			if (m_Colliders == null)
			{
				m_Colliders = new List<Collider>();
				Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
				foreach (Collider collider in componentsInChildren)
				{
					if (collider.gameObject.layer == 23)
					{
						m_Colliders.Add(collider);
					}
				}
			}
			return m_Colliders;
		}
	}

	protected virtual float OnValue_Health
	{
		get
		{
			return CurrentHealth;
		}
		set
		{
			CurrentHealth = Mathf.Min(value, MaxHealth);
		}
	}

	protected virtual float OnValue_MaxHealth
	{
		get
		{
			return MaxHealth;
		}
		set
		{
			MaxHealth = value;
			CurrentHealth = Mathf.Max(CurrentHealth, MaxHealth);
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (Player != null)
		{
			Player.Register(this);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (Player != null)
		{
			Player.Unregister(this);
		}
	}

	private void Start()
	{
		if (Inventory != null)
		{
			m_InventoryWasEnabledAtStart = Inventory.enabled;
		}
	}

	public override void Die()
	{
		if (!base.enabled || !vp_Utility.IsActive(base.gameObject))
		{
			return;
		}
		if (m_Audio != null)
		{
			m_Audio.pitch = Time.timeScale;
			m_Audio.PlayOneShot(DeathSound);
		}
		GameObject[] deathSpawnObjects = DeathSpawnObjects;
		foreach (GameObject gameObject in deathSpawnObjects)
		{
			if (gameObject != null)
			{
				vp_Utility.Instantiate(gameObject, base.transform.position, base.transform.rotation);
			}
		}
		foreach (Collider collider in Colliders)
		{
			collider.enabled = false;
		}
		if (Inventory != null && Inventory.enabled)
		{
			Inventory.enabled = false;
		}
		Player.SetWeapon.Argument = 0;
		Player.SetWeapon.Start();
		Player.Dead.Start();
		Player.Run.Stop();
		Player.Jump.Stop();
		Player.Crouch.Stop();
		Player.Zoom.Stop();
		Player.Attack.Stop();
		Player.Reload.Stop();
		Player.Climb.Stop();
		Player.Interact.Stop();
		if (vp_Gameplay.IsMultiplayer && vp_Gameplay.IsMaster)
		{
			vp_GlobalEvent<Transform>.Send("TransmitKill", base.transform.root);
		}
	}

	protected override void Reset()
	{
		base.Reset();
		if (!Application.isPlaying)
		{
			return;
		}
		Player.Dead.Stop();
		Player.Stop.Send();
		foreach (Collider collider in Colliders)
		{
			collider.enabled = true;
		}
		if (Inventory != null && !Inventory.enabled)
		{
			Inventory.enabled = m_InventoryWasEnabledAtStart;
		}
		if (m_Audio != null)
		{
			m_Audio.pitch = Time.timeScale;
			m_Audio.PlayOneShot(RespawnSound);
		}
	}

	protected virtual void OnMessage_FallImpact(float impact)
	{
		if (AllowFallDamage && !Player.Dead.Active && !(impact <= FallDamageThreshold))
		{
			float damage = Mathf.Abs(DeathOnFallImpactThreshold ? MaxHealth : (MaxHealth * impact));
			Damage(new vp_DamageInfo(damage, base.transform, base.transform, vp_DamageInfo.DamageType.Fall));
		}
	}
}
