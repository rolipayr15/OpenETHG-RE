using System;
using System.Collections.Generic;
using UnityEngine;

public class vp_PlayerInventory : vp_Inventory
{
	[Serializable]
	public class AutoWieldSection
	{
		public bool Always;

		public bool IfUnarmed = true;

		public bool IfOutOfAmmo = true;

		public bool IfNotPresent = true;

		public bool FirstTimeOnly = true;
	}

	[Serializable]
	public class MiscSection
	{
		public bool ResetOnRespawn = true;
	}

	protected Dictionary<vp_ItemType, object> m_PreviouslyOwnedItems = new Dictionary<vp_ItemType, object>();

	protected vp_ItemIdentifier m_WeaponIdentifierResult;

	protected string m_MissingHandlerError = "Error (vp_PlayerInventory) this component must be on the same transform as a vp_PlayerEventHandler + vp_WeaponHandler.";

	protected Dictionary<vp_ItemInstance, vp_Weapon> m_ItemWeapons;

	protected Dictionary<vp_ItemType, vp_UnitBankInstance> m_ThrowingWeaponUnitBankInstances = new Dictionary<vp_ItemType, vp_UnitBankInstance>();

	protected Dictionary<vp_UnitType, vp_UnitBankType> m_ThrowingWeaponUnitBankTypes = new Dictionary<vp_UnitType, vp_UnitBankType>();

	protected List<vp_UnitType> m_ThrowingWeaponUnitTypes = new List<vp_UnitType>();

	protected bool m_HaveThrowingWeaponInfo;

	protected Dictionary<vp_Weapon, vp_ItemIdentifier> m_WeaponIdentifiers;

	protected Dictionary<vp_UnitType, List<vp_Weapon>> m_WeaponsByUnit;

	protected vp_ItemInstance m_CurrentWeaponInstance;

	private vp_PlayerEventHandler m_Player;

	private vp_WeaponHandler m_WeaponHandler;

	[SerializeField]
	protected AutoWieldSection m_AutoWield;

	[SerializeField]
	protected MiscSection m_Misc;

	public Dictionary<vp_Weapon, vp_ItemIdentifier> WeaponIdentifiers
	{
		get
		{
			if (m_WeaponIdentifiers == null)
			{
				m_WeaponIdentifiers = new Dictionary<vp_Weapon, vp_ItemIdentifier>();
				foreach (vp_Weapon weapon in WeaponHandler.Weapons)
				{
					vp_ItemIdentifier component = weapon.GetComponent<vp_ItemIdentifier>();
					if (component != null)
					{
						m_WeaponIdentifiers.Add(weapon, component);
					}
				}
			}
			return m_WeaponIdentifiers;
		}
	}

	public Dictionary<vp_UnitType, List<vp_Weapon>> WeaponsByUnit
	{
		get
		{
			if (m_WeaponsByUnit == null)
			{
				m_WeaponsByUnit = new Dictionary<vp_UnitType, List<vp_Weapon>>();
				foreach (vp_Weapon weapon in WeaponHandler.Weapons)
				{
					if (!WeaponIdentifiers.TryGetValue(weapon, out var value) || !(value != null))
					{
						continue;
					}
					vp_UnitBankType vp_UnitBankType2 = value.Type as vp_UnitBankType;
					if (!(vp_UnitBankType2 != null) || !(vp_UnitBankType2.Unit != null))
					{
						continue;
					}
					if (m_WeaponsByUnit.TryGetValue(vp_UnitBankType2.Unit, out var value2))
					{
						if (value2 == null)
						{
							value2 = new List<vp_Weapon>();
						}
						m_WeaponsByUnit.Remove(vp_UnitBankType2.Unit);
					}
					else
					{
						value2 = new List<vp_Weapon>();
					}
					value2.Add(weapon);
					m_WeaponsByUnit.Add(vp_UnitBankType2.Unit, value2);
				}
			}
			return m_WeaponsByUnit;
		}
	}

	protected virtual vp_ItemInstance CurrentWeaponInstance
	{
		get
		{
			if (Application.isPlaying && WeaponHandler.CurrentWeaponIndex == 0)
			{
				m_CurrentWeaponInstance = null;
				return null;
			}
			if (m_CurrentWeaponInstance == null)
			{
				if (CurrentWeaponIdentifier == null)
				{
					MissingIdentifierError();
					m_CurrentWeaponInstance = null;
					return null;
				}
				m_CurrentWeaponInstance = GetItem(CurrentWeaponIdentifier.Type, CurrentWeaponIdentifier.ID);
			}
			return m_CurrentWeaponInstance;
		}
	}

	protected vp_PlayerEventHandler Player
	{
		get
		{
			if (this == null)
			{
				return null;
			}
			if (m_Player == null)
			{
				m_Player = base.transform.GetComponent<vp_PlayerEventHandler>();
			}
			return m_Player;
		}
	}

	protected vp_WeaponHandler WeaponHandler
	{
		get
		{
			if (m_WeaponHandler == null)
			{
				m_WeaponHandler = base.transform.GetComponent<vp_WeaponHandler>();
			}
			return m_WeaponHandler;
		}
	}

	public vp_ItemIdentifier CurrentWeaponIdentifier
	{
		get
		{
			if (!Application.isPlaying)
			{
				return null;
			}
			return GetWeaponIdentifier(WeaponHandler.CurrentWeapon);
		}
	}

	protected virtual int OnValue_CurrentWeaponAmmoCount
	{
		get
		{
			if (!(CurrentWeaponInstance is vp_UnitBankInstance vp_UnitBankInstance2))
			{
				return 0;
			}
			return vp_UnitBankInstance2.Count;
		}
		set
		{
			if (CurrentWeaponInstance is vp_UnitBankInstance vp_UnitBankInstance2)
			{
				vp_UnitBankInstance2.TryGiveUnits(value);
			}
		}
	}

	protected virtual int OnValue_CurrentWeaponMaxAmmoCount
	{
		get
		{
			if (!(CurrentWeaponInstance is vp_UnitBankInstance vp_UnitBankInstance2))
			{
				return 0;
			}
			return vp_UnitBankInstance2.Capacity;
		}
	}

	protected virtual int OnValue_CurrentWeaponClipCount
	{
		get
		{
			if (!(CurrentWeaponInstance is vp_UnitBankInstance vp_UnitBankInstance2))
			{
				return 0;
			}
			return GetUnitCount(vp_UnitBankInstance2.UnitType);
		}
	}

	protected virtual Texture2D OnValue_CurrentAmmoIcon
	{
		get
		{
			if (CurrentWeaponInstance == null)
			{
				return null;
			}
			if (CurrentWeaponInstance.Type == null)
			{
				return null;
			}
			vp_UnitBankType vp_UnitBankType2 = CurrentWeaponInstance.Type as vp_UnitBankType;
			if (vp_UnitBankType2 == null)
			{
				return null;
			}
			if (vp_UnitBankType2.Unit == null)
			{
				return null;
			}
			return vp_UnitBankType2.Unit.Icon;
		}
	}

	protected virtual vp_ItemIdentifier GetWeaponIdentifier(vp_Weapon weapon)
	{
		if (!Application.isPlaying)
		{
			return null;
		}
		if (weapon == null)
		{
			return null;
		}
		if (!WeaponIdentifiers.TryGetValue(weapon, out m_WeaponIdentifierResult))
		{
			if (weapon == null)
			{
				return null;
			}
			m_WeaponIdentifierResult = weapon.GetComponent<vp_ItemIdentifier>();
			if (m_WeaponIdentifierResult == null)
			{
				return null;
			}
			if (m_WeaponIdentifierResult.Type == null)
			{
				return null;
			}
			WeaponIdentifiers.Add(weapon, m_WeaponIdentifierResult);
		}
		return m_WeaponIdentifierResult;
	}

	protected override void Awake()
	{
		base.Awake();
		if (Player == null || WeaponHandler == null)
		{
			Debug.LogError(m_MissingHandlerError);
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (Player != null)
		{
			Player.Register(this);
		}
		UnwieldMissingWeapon();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (Player != null)
		{
			Player.Unregister(this);
		}
	}

	protected virtual bool MissingIdentifierError(int weaponIndex = 0)
	{
		if (!Application.isPlaying)
		{
			return false;
		}
		if (weaponIndex < 1)
		{
			return false;
		}
		if (WeaponHandler == null)
		{
			return false;
		}
		if (WeaponHandler.Weapons.Count <= weaponIndex - 1)
		{
			return false;
		}
		Debug.LogWarning(string.Format("Warning: Weapon gameobject '" + WeaponHandler.Weapons[weaponIndex - 1].name + "' lacks a properly set up vp_ItemIdentifier component!"));
		return false;
	}

	protected override void DoAddItem(vp_ItemType type, int id)
	{
		bool alreadyHaveIt = (vp_Gameplay.IsMultiplayer ? HaveItem(type) : HaveItem(type, id));
		base.DoAddItem(type, id);
		TryWieldNewItem(type, alreadyHaveIt);
	}

	protected override void DoRemoveItem(vp_ItemInstance item)
	{
		Unwield(item);
		base.DoRemoveItem(item);
	}

	protected override void DoAddUnitBank(vp_UnitBankType unitBankType, int id, int unitsLoaded)
	{
		bool alreadyHaveIt = (vp_Gameplay.IsMultiplayer ? HaveItem(unitBankType) : HaveItem(unitBankType, id));
		base.DoAddUnitBank(unitBankType, id, unitsLoaded);
		TryWieldNewItem(unitBankType, alreadyHaveIt);
	}

	protected virtual void TryWieldNewItem(vp_ItemType type, bool alreadyHaveIt)
	{
		bool flag = m_PreviouslyOwnedItems.ContainsKey(type);
		if (!flag)
		{
			m_PreviouslyOwnedItems.Add(type, null);
		}
		if ((m_AutoWield == null || !m_AutoWield.Always) && (m_AutoWield == null || !m_AutoWield.IfUnarmed || WeaponHandler.CurrentWeaponIndex >= 1) && (m_AutoWield == null || !m_AutoWield.IfOutOfAmmo || WeaponHandler.CurrentWeaponIndex <= 0 || WeaponHandler.CurrentWeapon.AnimationType == 2 || m_Player.CurrentWeaponAmmoCount.Get() >= 1) && (m_AutoWield == null || !m_AutoWield.IfNotPresent || m_AutoWield.FirstTimeOnly || alreadyHaveIt) && (m_AutoWield == null || !m_AutoWield.FirstTimeOnly || flag))
		{
			return;
		}
		if (type is vp_UnitBankType)
		{
			TryWield(GetItem(type));
			return;
		}
		if (type is vp_UnitType)
		{
			TryWieldByUnit(type as vp_UnitType);
			return;
		}
		if ((object)type != null)
		{
			TryWield(GetItem(type));
			return;
		}
		Type type2 = type.GetType();
		if (!(type2 == null))
		{
			type2 = type2.BaseType;
			if (type2 == typeof(vp_UnitBankType))
			{
				TryWield(GetItem(type));
			}
			else if (type2 == typeof(vp_UnitType))
			{
				TryWieldByUnit(type as vp_UnitType);
			}
			else if (type2 == typeof(vp_ItemType))
			{
				TryWield(GetItem(type));
			}
		}
	}

	protected override void DoRemoveUnitBank(vp_UnitBankInstance bank)
	{
		Unwield(bank);
		base.DoRemoveUnitBank(bank);
	}

	public virtual vp_Weapon GetWeaponOfItemInstance(vp_ItemInstance itemInstance)
	{
		if (m_ItemWeapons == null)
		{
			m_ItemWeapons = new Dictionary<vp_ItemInstance, vp_Weapon>();
		}
		m_ItemWeapons.TryGetValue(itemInstance, out var value);
		if (value != null)
		{
			return value;
		}
		try
		{
			for (int i = 0; i < WeaponHandler.Weapons.Count; i++)
			{
				vp_ItemInstance itemInstanceOfWeapon = GetItemInstanceOfWeapon(WeaponHandler.Weapons[i]);
				Debug.Log("weapon with index: " + i + ", item instance: " + ((itemInstanceOfWeapon == null) ? "(have none)" : itemInstanceOfWeapon.Type.ToString()));
				if (itemInstanceOfWeapon != null && itemInstanceOfWeapon.Type == itemInstance.Type)
				{
					value = WeaponHandler.Weapons[i];
					m_ItemWeapons.Add(itemInstanceOfWeapon, value);
					return value;
				}
			}
		}
		catch
		{
			Debug.LogError("Exception " + this?.ToString() + " Crashed while trying to get item instance for a weapon. Likely a nullreference.");
		}
		return null;
	}

	public override bool DoAddUnits(vp_UnitBankInstance bank, int amount)
	{
		if (bank == null)
		{
			return false;
		}
		int unitCount = GetUnitCount(bank.UnitType);
		bool flag = base.DoAddUnits(bank, amount);
		if (flag && bank.IsInternal)
		{
			try
			{
				TryWieldNewItem(bank.UnitType, unitCount != 0);
			}
			catch
			{
			}
			if ((!Application.isPlaying || WeaponHandler.CurrentWeaponIndex != 0) && CurrentWeaponInstance is vp_UnitBankInstance vp_UnitBankInstance2 && bank.UnitType == vp_UnitBankInstance2.UnitType && vp_UnitBankInstance2.Count == 0)
			{
				Player.AutoReload.Try();
			}
		}
		return flag;
	}

	public override bool DoRemoveUnits(vp_UnitBankInstance bank, int amount)
	{
		bool result = base.DoRemoveUnits(bank, amount);
		if (bank.Count == 0)
		{
			vp_Timer.In(0.3f, delegate
			{
				Player.AutoReload.Try();
			});
		}
		return result;
	}

	public vp_UnitBankInstance GetUnitBankInstanceOfWeapon(vp_Weapon weapon)
	{
		return GetItemInstanceOfWeapon(weapon) as vp_UnitBankInstance;
	}

	public vp_ItemInstance GetItemInstanceOfWeapon(vp_Weapon weapon)
	{
		vp_ItemIdentifier weaponIdentifier = GetWeaponIdentifier(weapon);
		if (weaponIdentifier == null)
		{
			return null;
		}
		return GetItem(weaponIdentifier.Type);
	}

	public int GetAmmoInWeapon(vp_Weapon weapon)
	{
		return GetUnitBankInstanceOfWeapon(weapon)?.Count ?? 0;
	}

	public int GetExtraAmmoForWeapon(vp_Weapon weapon)
	{
		vp_UnitBankInstance unitBankInstanceOfWeapon = GetUnitBankInstanceOfWeapon(weapon);
		if (unitBankInstanceOfWeapon == null)
		{
			return 0;
		}
		return GetUnitCount(unitBankInstanceOfWeapon.UnitType);
	}

	public int GetAmmoInCurrentWeapon()
	{
		return OnValue_CurrentWeaponAmmoCount;
	}

	public int GetExtraAmmoForCurrentWeapon()
	{
		return OnValue_CurrentWeaponClipCount;
	}

	protected virtual void StoreThrowingWeaponInfo()
	{
		foreach (vp_Weapon weapon in WeaponHandler.Weapons)
		{
			if (weapon.AnimationType != 3)
			{
				continue;
			}
			vp_ItemIdentifier component = weapon.GetComponent<vp_ItemIdentifier>();
			if (component == null)
			{
				continue;
			}
			vp_UnitBankType vp_UnitBankType2 = component.GetItemType() as vp_UnitBankType;
			if (!(vp_UnitBankType2 == null))
			{
				if (!m_ThrowingWeaponUnitBankTypes.ContainsKey(vp_UnitBankType2.Unit))
				{
					m_ThrowingWeaponUnitBankTypes.Add(vp_UnitBankType2.Unit, vp_UnitBankType2);
				}
				if (!m_ThrowingWeaponUnitTypes.Contains(vp_UnitBankType2.Unit))
				{
					m_ThrowingWeaponUnitTypes.Add(vp_UnitBankType2.Unit);
				}
				vp_UnitBankInstance unitBankInstanceOfWeapon = GetUnitBankInstanceOfWeapon(weapon);
				if (unitBankInstanceOfWeapon != null && !m_ThrowingWeaponUnitBankInstances.ContainsKey(vp_UnitBankType2))
				{
					m_ThrowingWeaponUnitBankInstances.Add(vp_UnitBankType2, unitBankInstanceOfWeapon);
				}
			}
		}
		m_HaveThrowingWeaponInfo = true;
	}

	public virtual vp_UnitBankInstance GetThrowingWeaponUnitBankInstance(vp_UnitBankType unitBankType)
	{
		if (WeaponHandler == null)
		{
			return null;
		}
		if (!m_HaveThrowingWeaponInfo)
		{
			StoreThrowingWeaponInfo();
		}
		vp_UnitBankInstance value = null;
		m_ThrowingWeaponUnitBankInstances.TryGetValue(unitBankType, out value);
		return value;
	}

	public virtual vp_UnitBankType GetThrowingWeaponUnitBankType(vp_UnitType unitType)
	{
		if (!m_HaveThrowingWeaponInfo)
		{
			StoreThrowingWeaponInfo();
		}
		vp_UnitBankType value = null;
		m_ThrowingWeaponUnitBankTypes.TryGetValue(unitType, out value);
		return value;
	}

	public virtual bool IsThrowingUnitBank(vp_UnitBankType unitBankType)
	{
		return GetThrowingWeaponUnitBankInstance(unitBankType) != null;
	}

	public virtual bool IsThrowingUnit(vp_UnitType unitType)
	{
		if (!m_HaveThrowingWeaponInfo)
		{
			StoreThrowingWeaponInfo();
		}
		return m_ThrowingWeaponUnitTypes.Contains(unitType);
	}

	protected virtual void UnwieldMissingWeapon()
	{
		if (Application.isPlaying && WeaponHandler.CurrentWeaponIndex >= 1 && (!(CurrentWeaponIdentifier != null) || !HaveItem(CurrentWeaponIdentifier.Type, CurrentWeaponIdentifier.ID)))
		{
			if (CurrentWeaponIdentifier == null)
			{
				MissingIdentifierError(WeaponHandler.CurrentWeaponIndex);
			}
			Player.SetWeapon.TryStart(0);
		}
	}

	protected bool TryWieldByUnit(vp_UnitType unitType)
	{
		if (WeaponsByUnit.TryGetValue(unitType, out var value) && value != null && value.Count > 0)
		{
			foreach (vp_Weapon item in value)
			{
				if (m_Player.SetWeapon.TryStart(WeaponHandler.Weapons.IndexOf(item) + 1))
				{
					return true;
				}
			}
		}
		return false;
	}

	protected virtual void TryWield(vp_ItemInstance item)
	{
		if (!Application.isPlaying || Player.Dead.Active || !WeaponHandler.enabled)
		{
			return;
		}
		int num = 1;
		while (true)
		{
			if (num < WeaponHandler.Weapons.Count + 1)
			{
				vp_ItemIdentifier weaponIdentifier = GetWeaponIdentifier(WeaponHandler.Weapons[num - 1]);
				if (!(weaponIdentifier == null) && !(item.Type != weaponIdentifier.Type) && (weaponIdentifier.ID == 0 || item.ID == weaponIdentifier.ID))
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		Player.SetWeapon.TryStart(num);
	}

	protected virtual void Unwield(vp_ItemInstance item)
	{
		if (!Application.isPlaying || WeaponHandler.CurrentWeaponIndex == 0)
		{
			return;
		}
		if (CurrentWeaponIdentifier == null)
		{
			MissingIdentifierError();
		}
		else
		{
			if (item.Type != CurrentWeaponIdentifier.Type || (CurrentWeaponIdentifier.ID != 0 && item.ID != CurrentWeaponIdentifier.ID))
			{
				return;
			}
			Player.SetWeapon.Start();
			if (!Player.Dead.Active)
			{
				vp_Timer.In(0.35f, delegate
				{
					Player.SetNextWeapon.Try();
				});
			}
			vp_Timer.In(1f, UnwieldMissingWeapon);
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		UnwieldMissingWeapon();
	}

	protected virtual bool CanStart_SetWeapon()
	{
		int num = (int)Player.SetWeapon.Argument;
		if (num == 0)
		{
			return true;
		}
		if (num < 1 || num > WeaponHandler.Weapons.Count)
		{
			return false;
		}
		vp_ItemIdentifier weaponIdentifier = GetWeaponIdentifier(WeaponHandler.Weapons[num - 1]);
		if (weaponIdentifier == null)
		{
			return MissingIdentifierError(num);
		}
		bool flag = HaveItem(weaponIdentifier.Type, weaponIdentifier.ID);
		if (flag && WeaponHandler.Weapons[num - 1].AnimationType == 3 && GetAmmoInWeapon(WeaponHandler.Weapons[num - 1]) < 1)
		{
			vp_UnitBankType vp_UnitBankType2 = weaponIdentifier.Type as vp_UnitBankType;
			if (vp_UnitBankType2 == null)
			{
				Debug.LogError("Error (" + this?.ToString() + ") Tried to wield thrown weapon " + WeaponHandler.Weapons[num - 1]?.ToString() + " but its item identifier does not point to a UnitBank.");
				return false;
			}
			if (!TryReload(vp_UnitBankType2, weaponIdentifier.ID))
			{
				return false;
			}
		}
		return flag;
	}

	protected virtual bool OnAttempt_DepleteAmmo()
	{
		if (CurrentWeaponIdentifier == null)
		{
			return MissingIdentifierError();
		}
		if (WeaponHandler.CurrentWeapon.AnimationType == 2)
		{
			return true;
		}
		if (WeaponHandler.CurrentWeapon.AnimationType == 3)
		{
			TryReload(CurrentWeaponInstance as vp_UnitBankInstance);
		}
		return TryDeduct(CurrentWeaponIdentifier.Type as vp_UnitBankType, CurrentWeaponIdentifier.ID, 1);
	}

	protected virtual bool OnAttempt_RefillCurrentWeapon()
	{
		if (CurrentWeaponIdentifier == null)
		{
			return MissingIdentifierError();
		}
		return TryReload(CurrentWeaponIdentifier.Type as vp_UnitBankType, CurrentWeaponIdentifier.ID);
	}

	public override void Reset()
	{
		m_PreviouslyOwnedItems.Clear();
		m_CurrentWeaponInstance = null;
		if (m_Misc == null || m_Misc.ResetOnRespawn)
		{
			base.Reset();
		}
	}

	protected virtual int OnMessage_GetItemCount(string itemTypeObjectName)
	{
		vp_ItemInstance item = GetItem(itemTypeObjectName);
		if (item == null)
		{
			return 0;
		}
		if (item is vp_UnitBankInstance { IsInternal: not false } vp_UnitBankInstance2)
		{
			return GetItemCount(vp_UnitBankInstance2.UnitType);
		}
		return GetItemCount(item.Type);
	}

	protected virtual bool OnAttempt_AddItem(object args)
	{
		object[] array = (object[])args;
		vp_ItemType vp_ItemType2 = array[0] as vp_ItemType;
		if (vp_ItemType2 == null)
		{
			return false;
		}
		int amount = ((array.Length != 2) ? 1 : ((int)array[1]));
		if (vp_ItemType2 is vp_UnitType)
		{
			return TryGiveUnits(vp_ItemType2 as vp_UnitType, amount);
		}
		return TryGiveItems(vp_ItemType2, amount);
	}

	protected virtual bool OnAttempt_RemoveItem(object args)
	{
		object[] array = (object[])args;
		vp_ItemType vp_ItemType2 = array[0] as vp_ItemType;
		if (vp_ItemType2 == null)
		{
			return false;
		}
		int amount = ((array.Length != 2) ? 1 : ((int)array[1]));
		if (vp_ItemType2 is vp_UnitType)
		{
			return TryRemoveUnits(vp_ItemType2 as vp_UnitType, amount);
		}
		return TryRemoveItems(vp_ItemType2, amount);
	}

	protected virtual void OnStop_SetWeapon()
	{
		m_CurrentWeaponInstance = null;
	}
}
