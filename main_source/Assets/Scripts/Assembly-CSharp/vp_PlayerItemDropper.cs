using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class vp_PlayerItemDropper : MonoBehaviour
{
	public enum DeathDropMode
	{
		Nothing,
		CurrentWeapon,
		AllWeapons,
		AllAmmo,
		Everything
	}

	private struct AmmoClipType
	{
		public string UnitTypeName;

		public int Units;

		public AmmoClipType(string unitTypeName, int units)
		{
			UnitTypeName = unitTypeName;
			Units = units;
		}
	}

	public DeathDropMode DropOnDeath = DeathDropMode.CurrentWeapon;

	public float SpawnHeight = 1.25f;

	public float SpawnDistance = 1f;

	public float TossDistance = 1f;

	public int MaxDrops = 100;

	public float ItemLifeTime;

	public bool DisableBob;

	public bool DisableSpin;

	protected int m_LastDropFrame = -1;

	protected int m_ItemDropsThisFrame = -1;

	public List<AudioClip> TossSounds = new List<AudioClip>();

	protected AudioSource m_Audio;

	protected float m_NextAllowedDropSoundTime;

	public List<GameObject> ExtraPickups = new List<GameObject>();

	protected static Dictionary<string, GameObject> m_SceneDroppables = new Dictionary<string, GameObject>();

	protected static Dictionary<string, vp_ItemType> m_SceneItemTypesByName = new Dictionary<string, vp_ItemType>();

	protected static List<int> m_AvailableUnitAmounts = new List<int>();

	private List<AmmoClipType> m_AvailableAmmoClips = new List<AmmoClipType>();

	protected Transform m_DroppablesGroup;

	protected int m_UnitsInLastRemovedItem = -1;

	protected int m_IDOfLastRemovedItem = -1;

	protected vp_PlayerInventory m_Inventory;

	protected vp_PlayerEventHandler m_Player;

	private vp_WeaponHandler m_WeaponHandler;

	protected AudioSource Audio
	{
		get
		{
			if (m_Audio == null)
			{
				m_Audio = GetComponent<AudioSource>();
			}
			return m_Audio;
		}
	}

	protected vp_PlayerInventory Inventory
	{
		get
		{
			if (m_Inventory == null)
			{
				m_Inventory = GetComponent<vp_PlayerInventory>();
			}
			return m_Inventory;
		}
	}

	public vp_PlayerEventHandler Player
	{
		get
		{
			if (m_Player == null)
			{
				m_Player = (vp_PlayerEventHandler)base.transform.root.GetComponentInChildren(typeof(vp_PlayerEventHandler));
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

	protected virtual void OnEnable()
	{
		GenerateItemDrops();
		m_LastDropFrame = -1;
		m_ItemDropsThisFrame = -1;
		SceneManager.sceneLoaded += OnLevelLoad;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnLevelLoad;
	}

	protected virtual void GenerateItemDrops()
	{
		List<vp_ItemPickup> list = new List<vp_ItemPickup>(Object.FindObjectsOfType<vp_ItemPickup>());
		foreach (GameObject extraPickup in ExtraPickups)
		{
			if (!(extraPickup == null))
			{
				vp_ItemPickup component = extraPickup.GetComponent<vp_ItemPickup>();
				if (!(component == null))
				{
					list.Add(component);
				}
			}
		}
		GameObject gameObject = GameObject.Find("Droppables");
		if (gameObject != null)
		{
			m_DroppablesGroup = gameObject.transform;
		}
		if (m_DroppablesGroup == null)
		{
			m_DroppablesGroup = new GameObject("Droppables").transform;
		}
		foreach (vp_ItemPickup item in list)
		{
			string text = item.ItemTypeObject.name;
			if (item.ItemTypeObject is vp_UnitType)
			{
				text = text + "," + item.Amount;
			}
			if (m_SceneDroppables.ContainsKey(text))
			{
				continue;
			}
			GameObject gameObject2 = ClonePickup(text, item);
			if (gameObject2 != null)
			{
				m_SceneDroppables.Add(text, gameObject2);
				if (!m_SceneItemTypesByName.ContainsKey(item.ItemTypeObject.name))
				{
					m_SceneItemTypesByName.Add(item.ItemTypeObject.name, item.ItemTypeObject);
				}
				if (item.ItemTypeObject is vp_UnitType && !m_AvailableUnitAmounts.Contains(item.Amount))
				{
					m_AvailableUnitAmounts.Add(item.Amount);
					m_AvailableUnitAmounts.Sort();
				}
			}
		}
		StoreAmmoClipTypes();
	}

	protected void ClearItemDrops()
	{
		m_SceneDroppables.Clear();
		m_SceneItemTypesByName.Clear();
		m_AvailableUnitAmounts.Clear();
	}

	protected virtual GameObject ClonePickup(string name, vp_ItemPickup i)
	{
		GameObject gameObject = (GameObject)vp_Utility.Instantiate(i.gameObject, Vector3.zero, i.gameObject.transform.rotation);
		gameObject.name = name;
		vp_Remover vp_Remover2 = gameObject.GetComponent<vp_Remover>();
		if (vp_Remover2 == null)
		{
			vp_Remover2 = gameObject.AddComponent<vp_Remover>();
		}
		vp_Remover2.LifeTime = ItemLifeTime;
		vp_Respawner component = gameObject.GetComponent<vp_Respawner>();
		if (component != null)
		{
			if (ItemLifeTime == 0f)
			{
				vp_Remover2.LifeTime = component.MaxRespawnTime;
			}
			Object.Destroy(component);
		}
		if (vp_Remover2.LifeTime == 0f)
		{
			vp_Remover2.LifeTime = 10f;
		}
		vp_MathUtility.SetSeed(i.ID);
		vp_Remover2.LifeTime += Random.value * 2f;
		Rigidbody component2 = gameObject.GetComponent<Rigidbody>();
		if (component2 != null)
		{
			component2.isKinematic = true;
		}
		Collider[] components = gameObject.GetComponents<Collider>();
		for (int j = 0; j < components.Length; j++)
		{
			components[j].enabled = false;
		}
		vp_ItemPickup component3 = gameObject.GetComponent<vp_ItemPickup>();
		if (component3 != null)
		{
			component3.ID = 0;
		}
		component3.Amount = i.Amount;
		vp_Utility.Activate(gameObject, activate: false);
		gameObject.transform.parent = m_DroppablesGroup;
		return gameObject;
	}

	protected virtual GameObject GetPickupPrefab(vp_ItemType itemType, int units = 0)
	{
		if (itemType == null)
		{
			return null;
		}
		GameObject value = null;
		if (itemType is vp_UnitType)
		{
			m_SceneDroppables.TryGetValue(itemType.name + "," + units, out value);
		}
		else
		{
			m_SceneDroppables.TryGetValue(itemType.name, out value);
		}
		if (value == null)
		{
			return null;
		}
		return value;
	}

	protected virtual GameObject SpawnPickup(GameObject prefab, Vector3 position)
	{
		GameObject obj = (GameObject)vp_Utility.Instantiate(prefab, position, Quaternion.Euler(new Vector3(prefab.transform.eulerAngles.x, Random.rotation.eulerAngles.y, prefab.transform.eulerAngles.z)));
		vp_Utility.Activate(obj);
		Rigidbody component = obj.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.isKinematic = true;
		}
		return obj;
	}

	protected virtual vp_Toss InitPickup(GameObject pickup, int units, int id)
	{
		Collider[] componentsInChildren = pickup.GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren)
		{
			if (collider.isTrigger)
			{
				Collider cc = collider;
				vp_Timer.In(1f, delegate
				{
					if (cc != null)
					{
						cc.enabled = true;
					}
				});
			}
			else
			{
				collider.enabled = false;
			}
		}
		vp_ItemPickup[] componentsInChildren2 = pickup.GetComponentsInChildren<vp_ItemPickup>();
		if (componentsInChildren2.Length == 0)
		{
			return null;
		}
		for (int num = 0; num < componentsInChildren2.Length; num++)
		{
			if (componentsInChildren2[num] == null)
			{
				if (num == 0)
				{
					return null;
				}
				continue;
			}
			componentsInChildren2[num].ID = id + num;
			if (num == 0)
			{
				componentsInChildren2[num].Amount = units;
			}
			else
			{
				componentsInChildren2[num].Amount = 0;
			}
			if (componentsInChildren2[num].ItemTypeObject is vp_UnitType && units == 0)
			{
				return null;
			}
			vp_GlobalEvent<vp_ItemPickup>.Send("RegisterPickup", componentsInChildren2[num]);
		}
		vp_Toss vp_Toss2 = pickup.GetComponent<vp_Toss>();
		if (vp_Toss2 == null)
		{
			vp_Toss2 = pickup.AddComponent<vp_Toss>();
		}
		return vp_Toss2;
	}

	public virtual void TryDropCurrentWeapon(float pauseForUnwield = 0.3f)
	{
		if (WeaponHandler == null || WeaponHandler.CurrentWeapon == null)
		{
			return;
		}
		vp_ItemIdentifier component = WeaponHandler.CurrentWeapon.GetComponent<vp_ItemIdentifier>();
		if (component == null)
		{
			return;
		}
		vp_ItemType itemType = component.GetItemType();
		if (!(itemType == null))
		{
			int units = Inventory.GetAmmoInCurrentWeapon();
			Player.Unwield.Send();
			vp_Timer.In(Mathf.Max(pauseForUnwield, 0f), delegate
			{
				TryDropWeapon(itemType, units);
			});
		}
	}

	public virtual void TryDropWeapon(vp_ItemType itemType, int units = 0, int id = 0)
	{
		if (WeaponHandler == null || itemType == null || Player == null || Player.Reload.Active)
		{
			return;
		}
		if (Player.Attack.Active)
		{
			Player.Attack.Stop();
		}
		for (int num = WeaponHandler.Weapons.Count - 1; num > -1; num--)
		{
			if (!(WeaponHandler.Weapons[num] == null))
			{
				vp_ItemIdentifier component = WeaponHandler.Weapons[num].GetComponent<vp_ItemIdentifier>();
				if (!(component == null))
				{
					vp_ItemType itemType2 = component.GetItemType();
					if (!(itemType2 == null) && !(itemType2 != itemType))
					{
						if (id <= 0)
						{
							TryDropItem(itemType, units, 0);
						}
						else
						{
							TryDropItem(itemType, units, component.ID);
						}
						break;
					}
				}
			}
		}
	}

	public virtual void TryDropWeapon(string itemTypeName, int units = 0, int id = 0)
	{
		if (m_SceneItemTypesByName.TryGetValue(itemTypeName, out var value))
		{
			TryDropItemInternal(value, base.transform.forward, units, id);
		}
	}

	public virtual bool TryDropAmmoClip(string unitTypeName, int clipSize)
	{
		return TryDropAmmoClip(unitTypeName, base.transform.forward, clipSize);
	}

	public virtual bool TryDropAmmoClip(string unitTypeName, Vector3 direction, int clipSize)
	{
		if (!m_SceneItemTypesByName.TryGetValue(unitTypeName, out var value))
		{
			return false;
		}
		return TryDropItemInternal(value, direction, clipSize);
	}

	public virtual void TryDropAmmoClip(vp_UnitType unitType, int clipSize)
	{
		TryDropItemInternal(unitType, base.transform.forward, clipSize);
	}

	public virtual void TryDropItem(string itemTypeName, int id = 0)
	{
		TryDropItem(itemTypeName, base.transform.forward, id);
	}

	public virtual void TryDropItem(string itemTypeName, Vector3 direction, int id)
	{
		if (m_SceneItemTypesByName.TryGetValue(itemTypeName, out var value))
		{
			TryDropItemInternal(value, direction, 0, id);
		}
	}

	public virtual void TryDropItem(vp_ItemType itemType, int id)
	{
		TryDropItemInternal(itemType, base.transform.forward, 0, id);
	}

	public virtual void TryDropItem(vp_ItemType itemType, int units, int id)
	{
		TryDropItemInternal(itemType, base.transform.forward, units, id);
	}

	public virtual void TryDropEverything()
	{
		TryDropAllWeapons();
		for (int num = Inventory.UnitBankInstances.Count - 1; num > -1; num--)
		{
			if (Inventory.UnitBankInstances[num] != null)
			{
				Inventory.UnitBankInstances[num].Type.ToString();
				TryDropItemInternal(Inventory.UnitBankInstances[num].Type, base.transform.forward, Inventory.UnitBankInstances[num].Count, Inventory.UnitBankInstances[num].ID);
			}
		}
		for (int num2 = Inventory.ItemInstances.Count - 1; num2 > -1; num2--)
		{
			if (Inventory.ItemInstances[num2] != null)
			{
				Inventory.ItemInstances[num2].Type.ToString();
				TryDropItemInternal(Inventory.ItemInstances[num2].Type, base.transform.forward, 0, Inventory.ItemInstances[num2].ID);
			}
		}
		TryDropAllAmmo();
	}

	protected virtual void TryDropAllWeapons()
	{
		if (WeaponHandler == null)
		{
			return;
		}
		foreach (KeyValuePair<vp_Weapon, vp_ItemIdentifier> weaponIdentifier in Inventory.WeaponIdentifiers)
		{
			vp_Weapon key = weaponIdentifier.Key;
			vp_ItemIdentifier value = weaponIdentifier.Value;
			vp_ItemType itemType = value.GetItemType();
			if (!(itemType == null))
			{
				int units = 0;
				vp_UnitBankInstance unitBankInstanceOfWeapon = Inventory.GetUnitBankInstanceOfWeapon(key);
				if (unitBankInstanceOfWeapon != null)
				{
					units = unitBankInstanceOfWeapon.Count;
				}
				TryDropWeapon(itemType, units, value.ID);
			}
		}
	}

	public virtual void TryDropAllAmmo()
	{
		bool flag = true;
		int num = 100;
		while (flag && num > 0)
		{
			flag = false;
			foreach (vp_ItemType value in m_SceneItemTypesByName.Values)
			{
				if (!(value is vp_UnitType))
				{
					continue;
				}
				vp_UnitType vp_UnitType2 = value as vp_UnitType;
				for (int num2 = m_AvailableUnitAmounts.Count - 1; num2 > -1; num2--)
				{
					if (m_AvailableAmmoClips.Contains(new AmmoClipType(vp_UnitType2.name, m_AvailableUnitAmounts[num2])))
					{
						TryDropAmmoClip(vp_UnitType2.name, m_AvailableUnitAmounts[num2]);
						if (Inventory.GetUnitCount(vp_UnitType2) >= m_AvailableUnitAmounts[num2])
						{
							flag = true;
						}
					}
				}
				if (Inventory.IsThrowingUnit(vp_UnitType2))
				{
					TryDropLoadedThrowingUnit(vp_UnitType2);
				}
			}
			num--;
		}
	}

	protected virtual void StoreAmmoClipTypes()
	{
		foreach (vp_ItemType value in m_SceneItemTypesByName.Values)
		{
			if (!(value is vp_UnitType))
			{
				continue;
			}
			foreach (GameObject value2 in m_SceneDroppables.Values)
			{
				if (value2 == null)
				{
					continue;
				}
				vp_ItemPickup component = value2.GetComponent<vp_ItemPickup>();
				if (component == null || !(component.ItemTypeObject is vp_UnitType))
				{
					continue;
				}
				for (int num = m_AvailableUnitAmounts.Count - 1; num > -1; num--)
				{
					if (m_AvailableUnitAmounts[num] == component.Amount && !m_AvailableAmmoClips.Contains(new AmmoClipType(component.ItemTypeObject.name, component.Amount)))
					{
						m_AvailableAmmoClips.Add(new AmmoClipType(component.ItemTypeObject.name, component.Amount));
					}
				}
			}
		}
	}

	protected virtual void TryDropLoadedThrowingUnit(vp_UnitType unitType)
	{
		vp_UnitBankType throwingWeaponUnitBankType = Inventory.GetThrowingWeaponUnitBankType(unitType);
		if (throwingWeaponUnitBankType == null)
		{
			return;
		}
		for (int num = WeaponHandler.Weapons.Count - 1; num > -1; num--)
		{
			if (!(WeaponHandler.Weapons[num] == null))
			{
				vp_ItemIdentifier component = WeaponHandler.Weapons[num].GetComponent<vp_ItemIdentifier>();
				if (!(component == null))
				{
					vp_ItemType itemType = component.GetItemType();
					if (!(itemType == null) && !(itemType != throwingWeaponUnitBankType))
					{
						for (int num2 = Inventory.UnitBankInstances.Count - 1; num2 > -1; num2--)
						{
							if (Inventory.UnitBankInstances[num2].UnitType == throwingWeaponUnitBankType.Unit && Inventory.UnitBankInstances[num2].Count == 1)
							{
								Inventory.UnitBankInstances[num2].DoRemoveUnits(1);
								Inventory.GetInternalUnitBank(Inventory.UnitBankInstances[num2].UnitType).DoAddUnits(1);
								TryDropAmmoClip(Inventory.UnitBankInstances[num2].UnitType.name, 1);
							}
						}
					}
				}
			}
		}
	}

	protected virtual bool TryDropItemInternal(vp_ItemType itemType, Vector3 direction, int units = 0, int id = 0)
	{
		if (!vp_Gameplay.IsMaster)
		{
			return false;
		}
		GameObject pickupPrefab = GetPickupPrefab(itemType, units);
		if (pickupPrefab == null)
		{
			return false;
		}
		if (!TryRemoveItem(itemType, units, id))
		{
			return false;
		}
		UpdateDropCount();
		if (m_ItemDropsThisFrame >= MaxDrops)
		{
			return false;
		}
		GameObject gameObject = SpawnPickup(pickupPrefab, base.transform.position + Vector3.up * SpawnHeight + direction * SpawnDistance);
		if (gameObject == null)
		{
			return false;
		}
		if (vp_Gameplay.IsMultiplayer)
		{
			id = vp_Utility.UniqueID;
		}
		vp_Toss vp_Toss2 = InitPickup(gameObject, units, id);
		if (vp_Toss2 == null)
		{
			Object.Destroy(gameObject);
			return false;
		}
		TryPlayDropSound();
		direction = GetCircleDirection(direction);
		float num = (float)((m_ItemDropsThisFrame - 1) / 8) * (TossDistance * 0.75f);
		float num2 = Random.Range(-180, 180);
		Vector3 vector = vp_Toss2.Toss(gameObject.transform.position, num2, direction, TossDistance + num, !DisableBob, !DisableSpin);
		vp_ItemPickup componentInChildren = gameObject.GetComponentInChildren<vp_ItemPickup>();
		if (componentInChildren != null)
		{
			vp_GlobalEvent<object[]>.Send("TransmitDropItem", new object[6]
			{
				itemType.name,
				vector,
				num2,
				base.transform.root,
				componentInChildren.ID,
				componentInChildren.Amount
			});
		}
		return true;
	}

	protected virtual void UpdateDropCount()
	{
		if (m_ItemDropsThisFrame == 0)
		{
			m_LastDropFrame = Time.frameCount;
		}
		if (Time.frameCount == m_LastDropFrame)
		{
			m_ItemDropsThisFrame++;
		}
		else
		{
			m_ItemDropsThisFrame = 0;
		}
	}

	protected virtual Vector3 GetCircleDirection(Vector3 direction)
	{
		float num = (float)m_ItemDropsThisFrame * 22.5f;
		num *= (float)((Random.value < 0.5f) ? 1 : (-1));
		return Quaternion.Euler(0f, num, 0f) * direction;
	}

	public virtual void MPClientDropItem(string itemTypeName, Vector3 targetPosition, float targetYaw, int id, int units)
	{
		if (!m_SceneItemTypesByName.TryGetValue(itemTypeName, out var value) || value == null)
		{
			return;
		}
		if (value is vp_UnitType)
		{
			TryRemoveItem(value, units);
		}
		else
		{
			TryRemoveItem(value);
		}
		GameObject pickupPrefab = GetPickupPrefab(value, units);
		if (pickupPrefab == null)
		{
			return;
		}
		GameObject gameObject = SpawnPickup(pickupPrefab, base.transform.position + Vector3.up * SpawnHeight + vp_3DUtility.HorizontalVector((targetPosition - base.transform.position).normalized * SpawnDistance));
		if (!(gameObject == null))
		{
			vp_Toss vp_Toss2 = InitPickup(gameObject, units, id);
			if (vp_Toss2 == null)
			{
				Object.Destroy(gameObject);
				return;
			}
			vp_Toss2.Toss(gameObject.transform.position, targetPosition, targetYaw, !DisableBob, !DisableSpin);
			TryPlayDropSound();
		}
	}

	protected virtual void TryPlayDropSound()
	{
		if (!(Time.time < m_NextAllowedDropSoundTime))
		{
			vp_AudioUtility.PlayRandomSound(Audio, TossSounds);
			m_NextAllowedDropSoundTime = Time.time + 0.2f;
		}
	}

	protected virtual bool TryRemoveItem(vp_ItemType itemType, int units = 1, int id = 0)
	{
		if (Inventory == null)
		{
			Debug.LogError("Error (" + this?.ToString() + ") Tried to remove an item but there is no vp_Inventory!");
			return false;
		}
		vp_ItemInstance vp_ItemInstance2 = null;
		vp_ItemInstance2 = ((id == 0) ? Inventory.GetItem(itemType.name) : Inventory.GetItem(itemType, id));
		if (vp_ItemInstance2 == null)
		{
			return false;
		}
		id = vp_ItemInstance2.ID;
		if (itemType is vp_UnitType)
		{
			if (!Inventory.TryRemoveUnits(itemType as vp_UnitType, units))
			{
				return false;
			}
		}
		else if (!Inventory.TryRemoveItems(itemType, 1))
		{
			return false;
		}
		return true;
	}

	protected virtual void Die()
	{
		switch (DropOnDeath)
		{
		case DeathDropMode.Nothing:
			break;
		case DeathDropMode.CurrentWeapon:
			TryDropCurrentWeapon(0.2f);
			break;
		case DeathDropMode.AllWeapons:
			TryDropAllWeapons();
			break;
		case DeathDropMode.AllAmmo:
			TryDropAllAmmo();
			break;
		case DeathDropMode.Everything:
			TryDropEverything();
			break;
		}
	}

	protected void OnLevelLoad(Scene scene, LoadSceneMode mode)
	{
		ClearItemDrops();
		GenerateItemDrops();
	}
}
