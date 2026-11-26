using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class vp_UITouchWeaponSwitcher : vp_UIControl
{
	protected class WeaponComparer : IComparer
	{
		int IComparer.Compare(object x, object y)
		{
			return new CaseInsensitiveComparer().Compare(((vp_ItemIdentifier)x).gameObject.name, ((vp_ItemIdentifier)y).gameObject.name);
		}
	}

	public class WeaponScrollerItem
	{
		public string Name = "";

		public int Index;

		public int WeaponId = -1;

		public Transform Transform;
	}

	public Transform WeaponScroller;

	public bool ChangeOnRelease;

	public float ChangeWeaponThreshold = 100f;

	public float ItemWidth = 135f;

	public float Angle = 13f;

	public float ItemYOffset = 11f;

	public float WieldTouchDelay = 0.15f;

	protected Vector3 m_CachedScrollerPosition = Vector3.zero;

	protected Vector3 m_NewPosition = Vector3.zero;

	protected vp_Inventory m_Inventory;

	protected Vector3 m_TouchDownPosition = Vector3.zero;

	protected float m_LastMoveTime;

	protected Dictionary<string, WeaponScrollerItem> m_Weapons = new Dictionary<string, WeaponScrollerItem>();

	protected Dictionary<string, WeaponScrollerItem> m_EquippedWeapons = new Dictionary<string, WeaponScrollerItem>();

	protected WeaponScrollerItem m_CurrentItem;

	protected float m_TouchTime;

	protected int m_LastWeaponID;

	private List<vp_ItemIdentifier> m_WeaponItems;

	private vp_FPCamera m_FPCamera;

	protected Vector3 m_CurrentItemDefaultPosition => new Vector3(m_CachedScrollerPosition.x - m_CurrentItem.Transform.localPosition.x, m_CachedScrollerPosition.y - m_CurrentItem.Transform.localPosition.y, m_CachedScrollerPosition.z);

	protected override void Awake()
	{
		base.Awake();
		m_Inventory = Manager.Player.GetComponent<vp_Inventory>();
		m_FPCamera = Manager.Player.GetComponentInChildren<vp_FPCamera>();
		m_WeaponItems = m_FPCamera.GetComponentsInChildren<vp_ItemIdentifier>().ToList();
		IComparer comparer = new WeaponComparer();
		m_WeaponItems.Sort(comparer.Compare);
	}

	protected override void Start()
	{
		base.Start();
		m_CachedScrollerPosition = WeaponScroller.localPosition;
		m_NewPosition = m_CachedScrollerPosition;
		foreach (vp_ItemIdentifier weaponItem in m_WeaponItems)
		{
			vp_ItemType itemType = weaponItem.GetItemType();
			if (itemType == null || string.IsNullOrEmpty(weaponItem.name))
			{
				continue;
			}
			Transform transform = WeaponScroller.Find(weaponItem.name);
			if (!(transform == null))
			{
				m_Weapons.Add(weaponItem.name, new WeaponScrollerItem
				{
					Name = weaponItem.name,
					Transform = transform,
					WeaponId = 0
				});
				if (m_Inventory.HaveItem(itemType))
				{
					vp_Utility.Activate(transform.gameObject, activate: false);
				}
			}
		}
		OnEnable();
	}

	protected virtual void UpdateScrollerItems()
	{
		float num = 0f;
		float num2 = 0f;
		int num3 = 0;
		foreach (vp_ItemIdentifier weaponItem in m_WeaponItems)
		{
			vp_ItemType itemType = weaponItem.GetItemType();
			if (itemType == null || !m_Inventory.HaveItem(itemType))
			{
				continue;
			}
			if (!m_EquippedWeapons.ContainsKey(weaponItem.name))
			{
				m_Weapons.TryGetValue(weaponItem.name, out var value);
				if (value == null)
				{
					continue;
				}
				m_EquippedWeapons.Add(value.Name, value);
			}
			m_EquippedWeapons[weaponItem.name].Index = num3;
			m_EquippedWeapons[weaponItem.name].Transform.localPosition = new Vector3(num, num2, m_EquippedWeapons[weaponItem.name].Transform.localPosition.z);
			num += m_Weapons[weaponItem.name].Transform.localScale.x * 2f + ItemWidth * 0.5f;
			num2 += ItemYOffset;
			num3++;
		}
	}

	protected override void OnEnable()
	{
		if (m_Initialized)
		{
			base.OnEnable();
		}
	}

	protected virtual void OnStart_Dead()
	{
		m_CurrentItem = null;
		m_EquippedWeapons.Clear();
		foreach (WeaponScrollerItem value in m_Weapons.Values)
		{
			vp_Utility.Activate(value.Transform.gameObject, activate: false);
		}
	}

	protected virtual void OnStop_SetWeapon()
	{
		if (Manager.Player.Dead.Active)
		{
			return;
		}
		int num = (int)Manager.Player.SetWeapon.Argument;
		if (num != 0)
		{
			m_LastWeaponID = num;
		}
		m_Weapons.TryGetValue(Manager.Player.CurrentWeaponName.Get(), out var value);
		if (value != null)
		{
			if (!m_EquippedWeapons.ContainsKey(value.Name))
			{
				m_EquippedWeapons.Add(value.Name, value);
				UpdateScrollerItems();
			}
			m_CurrentItem = m_EquippedWeapons[value.Name];
			vp_Utility.Activate(value.Transform.gameObject);
			CurrentItemPosition();
		}
		else if (m_CurrentItem != null)
		{
			vp_Utility.Activate(m_CurrentItem.Transform.gameObject, activate: false);
		}
	}

	protected override void TouchesBegan(vp_Touch touch)
	{
		if (base.LastFingerID == -1 && !(WeaponScroller == null) && (from hit in Physics.RaycastAll(m_Camera.ScreenPointToRay(touch.Position))
			where hit.collider == m_Collider
			select hit).ToList().Count != 0)
		{
			m_TouchDownPosition = m_Camera.ScreenToWorldPoint(touch.Position);
			base.LastFingerID = touch.FingerID;
			m_TouchTime = Time.time + WieldTouchDelay;
		}
	}

	protected virtual void TouchesMoved(vp_Touch touch)
	{
		if (base.LastFingerID == touch.FingerID && Manager.Player.CurrentWeaponIndex.Get() != 0)
		{
			Vector3 vector = m_Camera.ScreenToWorldPoint(touch.Position) - m_TouchDownPosition;
			if (m_CurrentItem == null)
			{
				Debug.LogError("Error(" + this?.ToString() + ") m_CurrentItem is null. Please make sure you have added weapon icon gameobjects under the 'WeaponScroller' gameobject. There should be one icon object corresponding to each weapon under the weapon camera (same names).");
			}
			if (m_EquippedWeapons == null)
			{
				Debug.LogError("Error(" + this?.ToString() + ") m_EquippedWeapons is null.");
			}
			m_NewPosition = new Vector3(m_CachedScrollerPosition.x - m_CurrentItem.Transform.localPosition.x + (m_CurrentItem.Transform.localScale.x * 2f + ItemWidth * 0.5f * (float)m_EquippedWeapons.Count) * vector.x, m_CachedScrollerPosition.y - m_CurrentItem.Transform.localPosition.y + vector.x * Angle, m_CachedScrollerPosition.z);
			if ((m_NewPosition - m_CurrentItemDefaultPosition).magnitude > ChangeWeaponThreshold)
			{
				SetWeapon(touch);
			}
			m_LastMoveTime = Time.time + 0.5f;
		}
	}

	public override void TouchesFinished(vp_Touch touch)
	{
		if (base.LastFingerID != touch.FingerID)
		{
			return;
		}
		base.TouchesFinished(touch);
		if (Manager.Player.CurrentWeaponIndex.Get() > 0)
		{
			if (ChangeOnRelease)
			{
				SetWeapon(touch);
			}
			else
			{
				CurrentItemPosition();
			}
		}
		if (Time.time < m_TouchTime)
		{
			Manager.Player.SetWeapon.TryStart((Manager.Player.CurrentWeaponIndex.Get() == 0) ? m_LastWeaponID : 0);
		}
	}

	protected virtual void CurrentItemPosition()
	{
		m_NewPosition = m_CurrentItemDefaultPosition;
		m_LastMoveTime = Time.time + 0.5f;
	}

	protected virtual void SetWeapon(vp_Touch touch)
	{
		base.LastFingerID = -1;
		if (m_EquippedWeapons.Count > 1)
		{
			if (m_TouchDownPosition.x > m_Camera.ScreenToWorldPoint(touch.Position).x)
			{
				if (m_CurrentItem.Index <= m_EquippedWeapons.Count - 2)
				{
					Manager.Player.SetNextWeapon.Try();
				}
				else
				{
					CurrentItemPosition();
				}
			}
			else if (m_CurrentItem.Index > 0)
			{
				Manager.Player.SetPrevWeapon.Try();
			}
			else
			{
				CurrentItemPosition();
			}
		}
		else
		{
			CurrentItemPosition();
		}
	}

	protected override void Update()
	{
		if (!(WeaponScroller == null) && !(Time.time > m_LastMoveTime))
		{
			WeaponScroller.localPosition = Vector3.Lerp(WeaponScroller.localPosition, m_NewPosition, Time.deltaTime * 20f);
		}
	}
}
