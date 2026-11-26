using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vp_WeaponHandler : MonoBehaviour
{
	protected class WeaponComparer : IComparer
	{
		int IComparer.Compare(object x, object y)
		{
			return new CaseInsensitiveComparer().Compare(((vp_Weapon)x).gameObject.name, ((vp_Weapon)y).gameObject.name);
		}
	}

	public int StartWeapon;

	public float AttackStateDisableDelay = 0.5f;

	public float SetWeaponRefreshStatesDelay = 0.5f;

	public float SetWeaponDuration = 0.1f;

	public float SetWeaponReloadSleepDuration = 0.3f;

	public float SetWeaponZoomSleepDuration = 0.3f;

	public float SetWeaponAttackSleepDuration = 0.3f;

	public float ReloadAttackSleepDuration = 0.3f;

	public bool ReloadAutomatically = true;

	protected vp_PlayerEventHandler m_Player;

	protected List<vp_Weapon> m_Weapons;

	protected List<List<vp_Weapon>> m_WeaponLists = new List<List<vp_Weapon>>();

	protected int m_CurrentWeaponIndex = -1;

	protected vp_Weapon m_CurrentWeapon;

	protected vp_Shooter m_CurrentShooter;

	protected vp_Timer.Handle m_SetWeaponTimer = new vp_Timer.Handle();

	protected vp_Timer.Handle m_SetWeaponRefreshTimer = new vp_Timer.Handle();

	protected vp_Timer.Handle m_DisableAttackStateTimer = new vp_Timer.Handle();

	protected vp_Timer.Handle m_DisableReloadStateTimer = new vp_Timer.Handle();

	public List<vp_Weapon> Weapons
	{
		get
		{
			if (m_Weapons == null)
			{
				InitWeaponLists();
			}
			return m_Weapons;
		}
		set
		{
			m_Weapons = value;
		}
	}

	public vp_Weapon CurrentWeapon => m_CurrentWeapon;

	public vp_Shooter CurrentShooter
	{
		get
		{
			if (CurrentWeapon == null)
			{
				return null;
			}
			if (m_CurrentShooter != null && (!m_CurrentShooter.enabled || !vp_Utility.IsActive(m_CurrentShooter.gameObject)))
			{
				return null;
			}
			return m_CurrentShooter;
		}
	}

	[Obsolete("Please use the 'CurrentWeaponIndex' parameter instead.")]
	public int CurrentWeaponID => m_CurrentWeaponIndex;

	public int CurrentWeaponIndex => m_CurrentWeaponIndex;

	public vp_Weapon WeaponBeingSet
	{
		get
		{
			if (!m_Player.SetWeapon.Active)
			{
				return null;
			}
			if (m_Player.SetWeapon.Argument == null)
			{
				return null;
			}
			return Weapons[Mathf.Max(0, (int)m_Player.SetWeapon.Argument - 1)];
		}
	}

	protected virtual bool OnValue_CurrentWeaponWielded
	{
		get
		{
			if (m_CurrentWeapon == null)
			{
				return false;
			}
			return m_CurrentWeapon.Wielded;
		}
	}

	protected virtual string OnValue_CurrentWeaponName
	{
		get
		{
			if (m_CurrentWeapon == null || Weapons == null)
			{
				return "";
			}
			return m_CurrentWeapon.name;
		}
	}

	protected virtual int OnValue_CurrentWeaponID => m_CurrentWeaponIndex;

	protected virtual int OnValue_CurrentWeaponIndex => m_CurrentWeaponIndex;

	public virtual int OnValue_CurrentWeaponType
	{
		get
		{
			if (!(CurrentWeapon == null))
			{
				return CurrentWeapon.AnimationType;
			}
			return 0;
		}
	}

	public virtual int OnValue_CurrentWeaponGrip
	{
		get
		{
			if (!(CurrentWeapon == null))
			{
				return CurrentWeapon.AnimationGrip;
			}
			return 0;
		}
	}

	protected virtual void Awake()
	{
		m_Player = (vp_PlayerEventHandler)base.transform.root.GetComponentInChildren(typeof(vp_PlayerEventHandler));
		if (Weapons != null)
		{
			StartWeapon = Mathf.Clamp(StartWeapon, 0, Weapons.Count);
		}
	}

	protected void InitWeaponLists()
	{
		List<vp_Weapon> list = null;
		vp_FPCamera componentInChildren = base.transform.GetComponentInChildren<vp_FPCamera>();
		if (componentInChildren != null)
		{
			list = GetWeaponList(componentInChildren.transform);
			if (list != null && list.Count > 0)
			{
				m_WeaponLists.Add(list);
			}
		}
		List<vp_Weapon> list2 = new List<vp_Weapon>(base.transform.GetComponentsInChildren<vp_Weapon>());
		if (list != null && list.Count == list2.Count)
		{
			Weapons = m_WeaponLists[0];
			return;
		}
		List<Transform> list3 = new List<Transform>();
		foreach (vp_Weapon item in list2)
		{
			if ((!(componentInChildren != null) || !list.Contains(item)) && !list3.Contains(item.Parent))
			{
				list3.Add(item.Parent);
			}
		}
		foreach (Transform item2 in list3)
		{
			List<vp_Weapon> weaponList = GetWeaponList(item2);
			DeactivateAll(weaponList);
			m_WeaponLists.Add(weaponList);
		}
		if (m_WeaponLists.Count < 1)
		{
			Debug.LogError("Error (" + this?.ToString() + ") WeaponHandler found no weapons in its hierarchy. Disabling self.");
			base.enabled = false;
		}
		else
		{
			Weapons = m_WeaponLists[0];
		}
	}

	public void EnableWeaponList(int index)
	{
		if (m_WeaponLists != null && m_WeaponLists.Count >= 1 && index >= 0 && index <= m_WeaponLists.Count - 1)
		{
			Weapons = m_WeaponLists[index];
		}
	}

	protected List<vp_Weapon> GetWeaponList(Transform target)
	{
		List<vp_Weapon> list = new List<vp_Weapon>();
		if ((bool)target.GetComponent<vp_Weapon>())
		{
			Debug.LogError("Error: (" + this?.ToString() + ") Hierarchy error. This component should sit above any vp_Weapons in the gameobject hierarchy.");
			return list;
		}
		vp_Weapon[] componentsInChildren = target.GetComponentsInChildren<vp_Weapon>(includeInactive: true);
		foreach (vp_Weapon item in componentsInChildren)
		{
			list.Insert(list.Count, item);
		}
		if (list.Count == 0)
		{
			Debug.LogError("Error: (" + this?.ToString() + ") Hierarchy error. This component must be added to a gameobject with vp_Weapon components in child gameobjects.");
			return list;
		}
		IComparer comparer = new WeaponComparer();
		list.Sort(comparer.Compare);
		return list;
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

	protected virtual void Update()
	{
		InitWeapon();
		UpdateFiring();
	}

	protected virtual void UpdateFiring()
	{
		if ((m_Player.IsLocal.Get() || m_Player.IsAI.Get()) && m_Player.Attack.Active && !m_Player.SetWeapon.Active && (!(m_CurrentWeapon != null) || m_CurrentWeapon.Wielded))
		{
			m_Player.Fire.Try();
		}
	}

	public virtual void SetWeapon(int weaponIndex)
	{
		if (Weapons == null || Weapons.Count < 1)
		{
			Debug.LogError("Error: (" + this?.ToString() + ") Tried to set weapon with an empty weapon list.");
			return;
		}
		if (weaponIndex < 0 || weaponIndex > Weapons.Count)
		{
			Debug.LogError("Error: (" + this?.ToString() + ") Weapon list does not have a weapon with index: " + weaponIndex);
			return;
		}
		if (m_CurrentWeapon != null)
		{
			m_CurrentWeapon.ResetState();
		}
		DeactivateAll(Weapons);
		ActivateWeapon(weaponIndex);
	}

	public void DeactivateAll(List<vp_Weapon> weaponList)
	{
		foreach (vp_Weapon weapon in weaponList)
		{
			weapon.ActivateGameObject(setActive: false);
			vp_FPWeapon vp_FPWeapon2 = weapon as vp_FPWeapon;
			if (vp_FPWeapon2 != null && vp_FPWeapon2.Weapon3rdPersonModel != null)
			{
				vp_Utility.Activate(vp_FPWeapon2.Weapon3rdPersonModel, activate: false);
			}
		}
		m_CurrentShooter = null;
	}

	public void ActivateWeapon(int index)
	{
		m_CurrentWeaponIndex = index;
		m_CurrentWeapon = null;
		if (m_CurrentWeaponIndex > 0)
		{
			m_CurrentWeapon = Weapons[m_CurrentWeaponIndex - 1];
			if (m_CurrentWeapon != null)
			{
				m_CurrentWeapon.ActivateGameObject();
			}
		}
		if (m_CurrentWeapon != null)
		{
			m_CurrentShooter = CurrentWeapon.GetComponent<vp_Shooter>();
		}
	}

	public virtual void CancelTimers()
	{
		vp_Timer.CancelAll("EjectShell");
		m_DisableAttackStateTimer.Cancel();
		m_SetWeaponTimer.Cancel();
		m_SetWeaponRefreshTimer.Cancel();
	}

	public virtual void SetWeaponLayer(int layer)
	{
		if (m_CurrentWeaponIndex >= 1 && m_CurrentWeaponIndex <= Weapons.Count)
		{
			vp_Layer.Set(Weapons[m_CurrentWeaponIndex - 1].gameObject, layer, recursive: true);
		}
	}

	private void InitWeapon()
	{
		if (m_CurrentWeaponIndex != -1)
		{
			return;
		}
		SetWeapon(0);
		vp_Timer.In(SetWeaponDuration + 0.1f, delegate
		{
			if (StartWeapon > 0 && StartWeapon < Weapons.Count + 1 && !m_Player.SetWeapon.TryStart(StartWeapon) && !vp_Gameplay.IsMultiplayer)
			{
				Debug.LogWarning("Warning (" + this?.ToString() + ") Requested 'StartWeapon' (" + Weapons[StartWeapon - 1].name + ") was denied, likely by the inventory. Make sure it's present in the inventory from the beginning.");
			}
		});
	}

	public void RefreshAllWeapons()
	{
		foreach (vp_Weapon weapon in Weapons)
		{
			weapon.Refresh();
			weapon.RefreshWeaponModel();
		}
	}

	public int GetWeaponIndex(vp_Weapon weapon)
	{
		return Weapons.IndexOf(weapon) + 1;
	}

	protected virtual void OnStart_Reload()
	{
		m_Player.Attack.Stop(m_Player.CurrentWeaponReloadDuration.Get() + ReloadAttackSleepDuration);
	}

	protected virtual void OnStart_SetWeapon()
	{
		CancelTimers();
		if (WeaponBeingSet == null || WeaponBeingSet.AnimationType != 2)
		{
			m_Player.Reload.Stop(SetWeaponDuration + SetWeaponReloadSleepDuration);
			m_Player.Zoom.Stop(SetWeaponDuration + SetWeaponZoomSleepDuration);
			m_Player.Attack.Stop(SetWeaponDuration + SetWeaponAttackSleepDuration);
		}
		if (m_CurrentWeapon != null)
		{
			m_CurrentWeapon.Wield(isWielding: false);
		}
		m_Player.SetWeapon.AutoDuration = SetWeaponDuration;
	}

	protected virtual void OnStop_SetWeapon()
	{
		int weapon = 0;
		if (m_Player.SetWeapon.Argument != null)
		{
			weapon = (int)m_Player.SetWeapon.Argument;
		}
		SetWeapon(weapon);
		if (m_CurrentWeapon != null)
		{
			m_CurrentWeapon.Wield();
		}
		vp_Timer.In(SetWeaponRefreshStatesDelay, delegate
		{
			if (this != null && m_Player != null)
			{
				m_Player.RefreshActivityStates();
				if (m_CurrentWeapon != null && m_Player.CurrentWeaponAmmoCount.Get() == 0)
				{
					m_Player.AutoReload.Try();
				}
			}
		}, m_SetWeaponRefreshTimer);
	}

	protected virtual bool CanStart_SetWeapon()
	{
		int num = (int)m_Player.SetWeapon.Argument;
		if (num == m_CurrentWeaponIndex)
		{
			return false;
		}
		if (num < 0 || num > Weapons.Count)
		{
			return false;
		}
		if (m_Player.Reload.Active)
		{
			return false;
		}
		return true;
	}

	protected virtual bool CanStart_Attack()
	{
		if (m_CurrentWeapon == null)
		{
			return false;
		}
		if (m_Player.Attack.Active)
		{
			return false;
		}
		if (m_Player.SetWeapon.Active)
		{
			return false;
		}
		if (m_Player.Reload.Active)
		{
			return false;
		}
		return true;
	}

	protected virtual void OnStop_Attack()
	{
		vp_Timer.In(AttackStateDisableDelay, delegate
		{
			if (this != null && m_Player != null && !m_Player.Attack.Active && m_CurrentWeapon != null)
			{
				m_CurrentWeapon.SetState("Attack", enabled: false);
			}
		}, m_DisableAttackStateTimer);
	}

	protected virtual bool OnAttempt_SetPrevWeapon()
	{
		int num = m_CurrentWeaponIndex - 1;
		if (num < 1)
		{
			num = Weapons.Count;
		}
		int num2 = 0;
		while (!m_Player.SetWeapon.TryStart(num))
		{
			num--;
			if (num < 1)
			{
				num = Weapons.Count;
			}
			num2++;
			if (num2 > Weapons.Count)
			{
				return false;
			}
		}
		return true;
	}

	protected virtual bool OnAttempt_SetNextWeapon()
	{
		int num = m_CurrentWeaponIndex + 1;
		int num2 = 0;
		while (!m_Player.SetWeapon.TryStart(num))
		{
			if (num > Weapons.Count + 1)
			{
				num = 0;
			}
			num++;
			num2++;
			if (num2 > Weapons.Count)
			{
				return false;
			}
		}
		return true;
	}

	protected virtual bool OnAttempt_SetWeaponByName(string name)
	{
		for (int i = 0; i < Weapons.Count; i++)
		{
			if (Weapons[i].name == name)
			{
				return m_Player.SetWeapon.TryStart(i + 1);
			}
		}
		return false;
	}

	public virtual void OnMessage_Unwield()
	{
		m_Player.SetWeapon.TryStart(0);
	}
}
