public class vp_FPWeaponHandler : vp_WeaponHandler
{
	protected virtual bool OnAttempt_AutoReload()
	{
		if (!ReloadAutomatically)
		{
			return false;
		}
		if (base.CurrentWeapon == null)
		{
			return false;
		}
		if (base.CurrentWeapon.AnimationType == 2)
		{
			return false;
		}
		return m_Player.Reload.TryStart();
	}
}
