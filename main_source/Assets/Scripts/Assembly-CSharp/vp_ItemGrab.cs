using UnityEngine;

public class vp_ItemGrab : vp_Interactable
{
	private vp_ItemPickup m_ItemPickup;

	private vp_ItemPickup ItemPickup
	{
		get
		{
			if (m_ItemPickup == null)
			{
				m_ItemPickup = base.transform.GetComponent<vp_ItemPickup>();
			}
			if (m_ItemPickup == null)
			{
				Debug.LogError("Error (" + this?.ToString() + ") This component requires a vp_ItemPickup (disabling self).");
			}
			return m_ItemPickup;
		}
	}

	protected override void Start()
	{
		base.Start();
		if (InteractDistance == 0f)
		{
			InteractDistance = 2.5f;
		}
	}

	public override bool TryInteract(vp_PlayerEventHandler player)
	{
		if (ItemPickup == null)
		{
			return false;
		}
		if (m_Player == null)
		{
			m_Player = player;
		}
		ItemPickup.TryGiveTo(m_Player.GetComponent<Collider>());
		return true;
	}
}
