using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[RequireComponent(typeof(SphereCollider))]
public class vp_ItemPickup : MonoBehaviour
{
	[Serializable]
	public class ItemSection
	{
		public vp_ItemType Type;

		public bool GiveOnContact = true;
	}

	[Serializable]
	public class RecipientTagsSection
	{
		public List<string> Tags = new List<string>();
	}

	[Serializable]
	public class SoundSection
	{
		public AudioClip PickupSound;

		public bool PickupSoundSlomo = true;

		public AudioClip PickupFailSound;

		public bool FailSoundSlomo = true;
	}

	[Serializable]
	public class MessageSection
	{
		public string SuccessSingle = "Picked up {2}.";

		public string SuccessMultiple = "Picked up {4} {1}s.";

		public string FailSingle = "Can't pick up {2} right now.";

		public string FailMultiple = "Can't pick up {4} {1}s right now.";
	}

	public int ID;

	public int Amount;

	protected Type m_ItemType;

	protected vp_ItemType m_ItemTypeObject;

	protected AudioSource m_Audio;

	protected Collider m_Collider;

	protected vp_Respawner m_Respawner;

	protected Collider[] m_Colliders;

	protected Renderer m_Renderer;

	protected Rigidbody m_Rigidbody;

	private bool m_HaveRigidbody;

	private bool m_RigidbodyWasCached;

	[SerializeField]
	protected ItemSection m_Item;

	[SerializeField]
	protected RecipientTagsSection m_Recipient;

	[SerializeField]
	protected SoundSection m_Sound;

	[SerializeField]
	protected MessageSection m_Messages;

	protected bool m_Depleted;

	protected int m_PickedUpAmount;

	protected string MissingItemTypeError = "Warning: {0} has no ItemType object!";

	protected bool m_AlreadyFailed;

	private static Dictionary<Collider, vp_Inventory> m_ColliderInventories = new Dictionary<Collider, vp_Inventory>();

	protected const float COLLIDER_DISABLE_DELAY = 0.5f;

	private bool m_WasSleepingLastFrame;

	protected Type ItemType
	{
		get
		{
			if (m_ItemType == null)
			{
				m_ItemType = m_Item.Type.GetType();
			}
			return m_ItemType;
		}
	}

	public vp_ItemType ItemTypeObject
	{
		get
		{
			if (m_ItemTypeObject == null)
			{
				m_ItemTypeObject = m_Item.Type;
			}
			return m_ItemTypeObject;
		}
	}

	protected AudioSource Audio
	{
		get
		{
			if (m_Audio == null)
			{
				m_Audio = GetComponent<AudioSource>();
				if (m_Audio == null)
				{
					m_Audio = base.gameObject.AddComponent<AudioSource>();
				}
			}
			return m_Audio;
		}
	}

	protected Collider Collider
	{
		get
		{
			if (m_Collider == null)
			{
				m_Collider = GetComponent<Collider>();
			}
			return m_Collider;
		}
	}

	protected vp_Respawner Respawner
	{
		get
		{
			if (m_Respawner == null)
			{
				m_Respawner = GetComponent<vp_Respawner>();
			}
			return m_Respawner;
		}
	}

	protected Collider[] Colliders
	{
		get
		{
			if (m_Colliders == null)
			{
				m_Colliders = GetComponents<Collider>();
			}
			return m_Colliders;
		}
	}

	protected Renderer Renderer
	{
		get
		{
			if (m_Renderer == null)
			{
				m_Renderer = GetComponent<Renderer>();
			}
			return m_Renderer;
		}
	}

	protected Rigidbody Rigidbody
	{
		get
		{
			if (this != null && !m_RigidbodyWasCached && m_Rigidbody == null)
			{
				m_Rigidbody = GetComponent<Rigidbody>();
				if (m_Rigidbody != null)
				{
					m_HaveRigidbody = true;
				}
				m_RigidbodyWasCached = true;
			}
			return m_Rigidbody;
		}
	}

	public bool GiveOnContact
	{
		get
		{
			return m_Item.GiveOnContact;
		}
		set
		{
			m_Item.GiveOnContact = value;
		}
	}

	protected virtual void Awake()
	{
		if (ItemType == typeof(vp_UnitType))
		{
			Amount = Mathf.Max(1, Amount);
		}
		Collider.isTrigger = true;
		if (m_Sound.PickupSound != null || m_Sound.PickupFailSound != null)
		{
			Audio.clip = m_Sound.PickupSound;
			Audio.playOnAwake = false;
		}
	}

	protected virtual void Update()
	{
		TryRemoveOnDeplete();
		TryDisableColliderOnSleep();
	}

	protected virtual void TryRemoveOnDeplete()
	{
		if (m_Depleted && !Audio.isPlaying)
		{
			if (Respawner != null)
			{
				SendMessage("Die", SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				vp_Utility.Destroy(base.gameObject);
			}
		}
	}

	protected virtual void TryDisableColliderOnSleep()
	{
		if (!m_HaveRigidbody || m_Depleted || Rigidbody.isKinematic || !Rigidbody.IsSleeping() || m_WasSleepingLastFrame)
		{
			return;
		}
		vp_Timer.In(0.5f, delegate
		{
			if (Rigidbody != null)
			{
				Rigidbody.isKinematic = true;
			}
			for (int i = 0; i < Colliders.Length; i++)
			{
				if (!(Colliders[i] == null) && !Colliders[i].isTrigger)
				{
					Colliders[i].enabled = false;
				}
			}
		});
		m_WasSleepingLastFrame = Rigidbody.IsSleeping();
	}

	protected virtual void OnEnable()
	{
		if (Rigidbody != null)
		{
			Rigidbody.isKinematic = false;
			Collider[] colliders = Colliders;
			foreach (Collider collider in colliders)
			{
				if (!collider.isTrigger)
				{
					collider.enabled = true;
				}
			}
		}
		Renderer.enabled = true;
		m_Depleted = false;
		m_AlreadyFailed = false;
		vp_GlobalEvent<vp_ItemPickup>.Send("TransmitPickupRespawn", this);
	}

	protected virtual void OnTriggerEnter(Collider col)
	{
		if (m_Item.GiveOnContact && !(ItemType == null) && vp_Gameplay.IsMaster && Collider.enabled)
		{
			TryGiveTo(col);
		}
	}

	public void TryGiveTo(Collider col)
	{
		if (m_Depleted)
		{
			return;
		}
		if (!m_ColliderInventories.TryGetValue(col, out var value))
		{
			value = vp_TargetEventReturn<vp_Inventory>.SendUpwards(col, "GetInventory");
			m_ColliderInventories.Add(col, value);
		}
		if (!(value == null) && (m_Recipient.Tags.Count <= 0 || m_Recipient.Tags.Contains(col.gameObject.tag)))
		{
			bool flag = false;
			int num = vp_TargetEventReturn<vp_ItemType, int>.SendUpwards(col, "GetItemCount", m_Item.Type);
			if (ItemType == typeof(vp_ItemType))
			{
				flag = vp_TargetEventReturn<vp_ItemType, int, bool>.SendUpwards(col, "TryGiveItem", m_Item.Type, ID);
			}
			else if (ItemType == typeof(vp_UnitBankType))
			{
				flag = vp_TargetEventReturn<vp_UnitBankType, int, int, bool>.SendUpwards(col, "TryGiveUnitBank", m_Item.Type as vp_UnitBankType, Amount, ID);
			}
			else if (ItemType == typeof(vp_UnitType))
			{
				flag = vp_TargetEventReturn<vp_UnitType, int, bool>.SendUpwards(col, "TryGiveUnits", m_Item.Type as vp_UnitType, Amount);
			}
			else if (ItemType.BaseType == typeof(vp_ItemType))
			{
				flag = vp_TargetEventReturn<vp_ItemType, int, bool>.SendUpwards(col, "TryGiveItem", m_Item.Type, ID);
			}
			else if (ItemType.BaseType == typeof(vp_UnitBankType))
			{
				flag = vp_TargetEventReturn<vp_UnitBankType, int, int, bool>.SendUpwards(col, "TryGiveUnitBank", m_Item.Type as vp_UnitBankType, Amount, ID);
			}
			else if (ItemType.BaseType == typeof(vp_UnitType))
			{
				flag = vp_TargetEventReturn<vp_UnitType, int, bool>.SendUpwards(col, "TryGiveUnits", m_Item.Type as vp_UnitType, Amount);
			}
			if (flag)
			{
				m_PickedUpAmount = vp_TargetEventReturn<vp_ItemType, int>.SendUpwards(col, "GetItemCount", m_Item.Type) - num;
				OnSuccess(col.transform);
			}
			else
			{
				OnFail(col.transform);
			}
		}
	}

	protected virtual void OnTriggerExit()
	{
		m_AlreadyFailed = false;
	}

	protected virtual void OnSuccess(Transform recipient)
	{
		m_Depleted = true;
		if (m_Sound.PickupSound != null && vp_Utility.IsActive(base.gameObject) && Audio.enabled)
		{
			Audio.pitch = (m_Sound.PickupSoundSlomo ? Time.timeScale : 1f);
			Audio.Play();
		}
		Renderer.enabled = false;
		string text = "";
		text = ((m_PickedUpAmount >= 2 && !(ItemType == typeof(vp_UnitBankType)) && !(ItemType.BaseType == typeof(vp_UnitBankType))) ? string.Format(m_Messages.SuccessMultiple, m_Item.Type.IndefiniteArticle, m_Item.Type.DisplayName, m_Item.Type.DisplayNameFull, m_Item.Type.Description, m_PickedUpAmount.ToString()) : string.Format(m_Messages.SuccessSingle, m_Item.Type.IndefiniteArticle, m_Item.Type.DisplayName, m_Item.Type.DisplayNameFull, m_Item.Type.Description, m_PickedUpAmount.ToString()));
		vp_FPPlayerEventHandler componentInChildren = recipient.transform.root.GetComponentInChildren<vp_FPPlayerEventHandler>();
		if (componentInChildren != null)
		{
			componentInChildren.HUDText.Send(text);
		}
		if (vp_Gameplay.IsMultiplayer && vp_Gameplay.IsMaster)
		{
			vp_GlobalEvent<vp_ItemPickup, Transform>.Send("TransmitPickup", this, recipient);
		}
	}

	protected virtual void Die()
	{
		vp_Utility.Activate(base.gameObject, activate: false);
	}

	protected virtual void OnFail(Transform recipient)
	{
		vp_FPPlayerEventHandler componentInChildren = recipient.transform.root.GetComponentInChildren<vp_FPPlayerEventHandler>();
		if (!(componentInChildren != null) || !componentInChildren.Dead.Active)
		{
			if (!m_AlreadyFailed && m_Sound.PickupFailSound != null && (!vp_Gameplay.IsMultiplayer || (vp_Gameplay.IsMultiplayer && recipient.GetComponent<vp_FPPlayerEventHandler>() != null)))
			{
				Audio.pitch = (m_Sound.FailSoundSlomo ? Time.timeScale : 1f);
				Audio.PlayOneShot(m_Sound.PickupFailSound);
			}
			m_AlreadyFailed = true;
			string text = "";
			text = ((m_PickedUpAmount >= 2 && !(ItemType == typeof(vp_UnitBankType)) && !(ItemType.BaseType == typeof(vp_UnitBankType))) ? string.Format(m_Messages.FailMultiple, m_Item.Type.IndefiniteArticle, m_Item.Type.DisplayName, m_Item.Type.DisplayNameFull, m_Item.Type.Description, Amount.ToString()) : string.Format(m_Messages.FailSingle, m_Item.Type.IndefiniteArticle, m_Item.Type.DisplayName, m_Item.Type.DisplayNameFull, m_Item.Type.Description, Amount.ToString()));
			vp_GlobalEvent<Transform, string>.Send("HUDText", recipient, text);
		}
	}
}
