using UnityEngine;

public class vp_HealthPowerup : vp_Powerup
{
	public float Health = 1f;

	protected override bool TryGive(vp_PlayerEventHandler player)
	{
		if (player.Health.Get() < 0f)
		{
			return false;
		}
		if (player.Health.Get() >= player.MaxHealth.Get())
		{
			return false;
		}
		if (vp_Gameplay.IsMaster)
		{
			player.Health.Set(Mathf.Min(player.MaxHealth.Get(), player.Health.Get() + Health));
		}
		if (vp_Gameplay.IsMultiplayer && vp_Gameplay.IsMaster)
		{
			vp_GlobalEvent<Transform, Transform, float>.Send("TransmitDamage", player.transform.root, base.transform.root, 0f - Health);
		}
		return true;
	}
}
