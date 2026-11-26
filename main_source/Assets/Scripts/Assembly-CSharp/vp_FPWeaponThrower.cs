using UnityEngine;

public class vp_FPWeaponThrower : vp_WeaponThrower
{
	public Vector3 FirePositionOffset = new Vector3(0.35f, 0f, 0f);

	protected bool m_OriginalLookDownActive;

	protected vp_Timer.Handle m_Timer1 = new vp_Timer.Handle();

	protected vp_Timer.Handle m_Timer2 = new vp_Timer.Handle();

	protected vp_Timer.Handle m_Timer3 = new vp_Timer.Handle();

	protected vp_Timer.Handle m_Timer4 = new vp_Timer.Handle();

	protected vp_FPWeapon m_FPWeapon;

	protected vp_FPWeaponShooter m_FPWeaponShooter;

	protected Transform m_FirePosition;

	private Animation m_WeaponAnimation;

	public vp_FPWeapon FPWeapon
	{
		get
		{
			if (m_FPWeapon == null)
			{
				m_FPWeapon = (vp_FPWeapon)base.Transform.GetComponent(typeof(vp_FPWeapon));
			}
			return m_FPWeapon;
		}
	}

	public vp_FPWeaponShooter FPWeaponShooter
	{
		get
		{
			if (m_FPWeaponShooter == null)
			{
				m_FPWeaponShooter = (vp_FPWeaponShooter)base.Transform.GetComponent(typeof(vp_FPWeaponShooter));
			}
			return m_FPWeaponShooter;
		}
	}

	protected Transform FirePosition
	{
		get
		{
			if (m_FirePosition == null)
			{
				GameObject gameObject = new GameObject("ThrownWeaponFirePosition");
				m_FirePosition = gameObject.transform;
				m_FirePosition.parent = Camera.main.transform;
			}
			return m_FirePosition;
		}
	}

	public Animation WeaponAnimation
	{
		get
		{
			if (m_WeaponAnimation == null)
			{
				if (FPWeapon == null)
				{
					return null;
				}
				if (FPWeapon.WeaponModel == null)
				{
					return null;
				}
				m_WeaponAnimation = FPWeapon.WeaponModel.GetComponent<Animation>();
			}
			return m_WeaponAnimation;
		}
	}

	protected override void Start()
	{
		base.Start();
		m_OriginalLookDownActive = FPWeapon.LookDownActive;
	}

	protected virtual void RewindAnimation()
	{
		if (base.Player.IsFirstPerson.Get() && !(FPWeapon == null) && !(FPWeapon.WeaponModel == null) && !(WeaponAnimation == null) && !(FPWeaponShooter == null) && !(FPWeaponShooter.AnimationFire == null))
		{
			WeaponAnimation[FPWeaponShooter.AnimationFire.name].time = 0f;
			WeaponAnimation.Play();
			WeaponAnimation.Sample();
			WeaponAnimation.Stop();
		}
	}

	protected override void OnStart_Attack()
	{
		base.OnStart_Attack();
		if (base.Player.IsFirstPerson.Get())
		{
			base.Shooter.m_ProjectileSpawnPoint = FirePosition.gameObject;
			FirePosition.localPosition = FirePositionOffset;
			FirePosition.localEulerAngles = Vector3.zero;
		}
		else
		{
			base.Shooter.m_ProjectileSpawnPoint = base.Weapon.Weapon3rdPersonModel;
		}
		FPWeapon.LookDownActive = false;
		vp_Timer.In(base.Shooter.ProjectileSpawnDelay, delegate
		{
			if (!base.HaveAmmoForCurrentWeapon)
			{
				FPWeapon.SetState("ReWield");
				FPWeapon.Refresh();
				vp_Timer.In(1f, delegate
				{
					if (!base.Player.SetNextWeapon.Try())
					{
						vp_Timer.In(0.5f, delegate
						{
							RewindAnimation();
							base.Player.SetWeapon.Start();
						}, m_Timer2);
					}
				});
			}
			else if (base.Player.IsFirstPerson.Get())
			{
				FPWeapon.SetState("ReWield");
				FPWeapon.Refresh();
				vp_Timer.In(1f, delegate
				{
					RewindAnimation();
					FPWeapon.Rendering = true;
					FPWeapon.SetState("ReWield", enabled: false);
					FPWeapon.Refresh();
				}, m_Timer3);
			}
			else
			{
				vp_Timer.In(0.5f, delegate
				{
					base.Player.Attack.Stop();
				}, m_Timer4);
			}
		}, m_Timer1);
	}

	protected virtual void OnStart_SetWeapon()
	{
		RewindAnimation();
	}

	protected override void OnStop_Attack()
	{
		base.OnStop_Attack();
		FPWeapon.LookDownActive = m_OriginalLookDownActive;
	}
}
