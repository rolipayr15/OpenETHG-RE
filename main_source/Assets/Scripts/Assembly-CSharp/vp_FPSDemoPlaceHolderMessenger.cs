using UnityEngine;

public class vp_FPSDemoPlaceHolderMessenger : MonoBehaviour
{
	private vp_FPPlayerEventHandler Player;

	private bool m_WasSwingingMaceIn3rdPersonLastFrame;

	private bool m_WasClimbingIn3rdPersonLastFrame;

	private void Start()
	{
		Player = base.transform.root.GetComponent<vp_FPPlayerEventHandler>();
		if (Player == null)
		{
			base.enabled = false;
		}
	}

	private void Update()
	{
		if (Player == null)
		{
			return;
		}
		if (!Player.IsFirstPerson.Get() && Player.Climb.Active)
		{
			if (!m_WasClimbingIn3rdPersonLastFrame)
			{
				m_WasClimbingIn3rdPersonLastFrame = true;
				vp_Timer.In(0f, delegate
				{
					Player.HUDText.Send("PLACEHOLDER CLIMB ANIMATION");
				}, 3, 1f);
			}
		}
		else
		{
			m_WasClimbingIn3rdPersonLastFrame = false;
		}
		if (!Player.IsFirstPerson.Get() && Player.CurrentWeaponIndex.Get() == 4 && Player.Attack.Active)
		{
			if (!m_WasSwingingMaceIn3rdPersonLastFrame)
			{
				m_WasSwingingMaceIn3rdPersonLastFrame = true;
				vp_Timer.In(0f, delegate
				{
					Player.HUDText.Send("PLACEHOLDER MELEE ANIMATION");
				}, 3, 1f);
			}
		}
		else
		{
			m_WasSwingingMaceIn3rdPersonLastFrame = false;
		}
	}
}
