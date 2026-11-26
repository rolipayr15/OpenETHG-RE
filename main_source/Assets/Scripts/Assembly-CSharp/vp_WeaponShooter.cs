using UnityEngine;

public class vp_WeaponShooter : vp_Shooter
{
	protected vp_Weapon m_Weapon;

	public float ProjectileTapFiringRate = 0.1f;

	protected float m_LastFireTime;

	protected float m_OriginalProjectileSpawnDelay;

	public Vector3 MotionPositionRecoil = new Vector3(0f, 0f, -0.035f);

	public Vector3 MotionRotationRecoil = new Vector3(-10f, 0f, 0f);

	public float MotionRotationRecoilDeadZone = 0.5f;

	public float MotionDryFireRecoil = -0.1f;

	public float MotionRecoilDelay;

	public float MuzzleFlashFirstShotMaxDeviation = 180f;

	protected bool m_WeaponWasInAttackStateLastFrame;

	protected float m_MuzzleFlashWeaponAngle;

	protected float m_MuzzleFlashFireAngle;

	public AudioClip SoundDryFire;

	protected Quaternion m_MuzzlePointRotation = Quaternion.identity;

	protected bool m_3rdPersonFiredThisFrame;

	protected vp_PlayerEventHandler m_Player;

	private vp_PlayerEventHandler Player
	{
		get
		{
			if (m_Player == null && base.EventHandler != null)
			{
				m_Player = (vp_PlayerEventHandler)base.EventHandler;
			}
			return m_Player;
		}
	}

	public vp_Weapon Weapon
	{
		get
		{
			if (m_Weapon == null)
			{
				m_Weapon = base.transform.GetComponent<vp_Weapon>();
			}
			return m_Weapon;
		}
	}

	protected override void Awake()
	{
		if (m_ProjectileSpawnPoint == null && Weapon.Weapon3rdPersonModel != null)
		{
			m_ProjectileSpawnPoint = Weapon.Weapon3rdPersonModel;
		}
		if (GetFireSeed == null)
		{
			GetFireSeed = () => Random.Range(0, 100);
		}
		if (GetFirePosition == null)
		{
			GetFirePosition = () => FirePosition;
		}
		if (GetFireRotation == null)
		{
			GetFireRotation = delegate
			{
				Quaternion result = Quaternion.identity;
				if (Player.LookPoint.Get() - FirePosition != Vector3.zero)
				{
					result = vp_MathUtility.NaNSafeQuaternion(Quaternion.LookRotation(Player.LookPoint.Get() - FirePosition));
				}
				return result;
			};
		}
		base.Awake();
		m_NextAllowedFireTime = Time.time;
		ProjectileSpawnDelay = Mathf.Min(ProjectileSpawnDelay, ProjectileFiringRate - 0.1f);
		m_OriginalProjectileSpawnDelay = ProjectileSpawnDelay;
	}

	protected override void Start()
	{
		base.Start();
	}

	protected override void LateUpdate()
	{
		if (!(Player == null) && Player.IsFirstPerson != null)
		{
			if (!Player.IsFirstPerson.Get() && m_3rdPersonFiredThisFrame)
			{
				m_3rdPersonFiredThisFrame = false;
			}
			m_WeaponWasInAttackStateLastFrame = Weapon.StateManager.IsEnabled("Attack");
			base.LateUpdate();
		}
	}

	protected override void Fire()
	{
		if (vp_Gameplay.IsMultiplayer && !Player.IsLocal.Get())
		{
			ProjectileSpawnDelay = 0f;
		}
		m_LastFireTime = Time.time;
		if (!Player.IsFirstPerson.Get())
		{
			m_3rdPersonFiredThisFrame = true;
		}
		if (MotionRecoilDelay == 0f)
		{
			ApplyRecoil();
		}
		else
		{
			vp_Timer.In(MotionRecoilDelay, ApplyRecoil);
		}
		base.Fire();
		ProjectileSpawnDelay = m_OriginalProjectileSpawnDelay;
	}

	protected override void ShowMuzzleFlash()
	{
		if (m_MuzzleFlash == null)
		{
			return;
		}
		if (MuzzleFlashFirstShotMaxDeviation == 180f || Player.IsFirstPerson.Get() || m_WeaponWasInAttackStateLastFrame)
		{
			base.ShowMuzzleFlash();
			return;
		}
		m_MuzzleFlashWeaponAngle = base.Transform.eulerAngles.x + 90f;
		m_MuzzleFlashFireAngle = m_CurrentFireRotation.eulerAngles.x + 90f;
		m_MuzzleFlashWeaponAngle = ((m_MuzzleFlashWeaponAngle >= 360f) ? (m_MuzzleFlashWeaponAngle - 360f) : m_MuzzleFlashWeaponAngle);
		m_MuzzleFlashFireAngle = ((m_MuzzleFlashFireAngle >= 360f) ? (m_MuzzleFlashFireAngle - 360f) : m_MuzzleFlashFireAngle);
		if (Mathf.Abs(m_MuzzleFlashWeaponAngle - m_MuzzleFlashFireAngle) > MuzzleFlashFirstShotMaxDeviation)
		{
			m_MuzzleFlash.SendMessage("ShootLightOnly", SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			base.ShowMuzzleFlash();
		}
	}

	protected virtual void ApplyRecoil()
	{
		if (MotionRotationRecoil.z == 0f)
		{
			Weapon.AddForce2(MotionPositionRecoil, MotionRotationRecoil);
		}
		else
		{
			Weapon.AddForce2(MotionPositionRecoil, Vector3.Scale(MotionRotationRecoil, Vector3.one + Vector3.back) + ((Random.value < 0.5f) ? Vector3.forward : Vector3.back) * Random.Range(MotionRotationRecoil.z * MotionRotationRecoilDeadZone, MotionRotationRecoil.z));
		}
	}

	public virtual void DryFire()
	{
		if (base.Audio != null)
		{
			base.Audio.pitch = Time.timeScale;
			base.Audio.PlayOneShot(SoundDryFire);
		}
		DisableFiring();
		m_LastFireTime = Time.time;
		Weapon.AddForce2(MotionPositionRecoil * MotionDryFireRecoil, MotionRotationRecoil * MotionDryFireRecoil);
	}

	public void OnMessage_DryFire()
	{
		DryFire();
	}

	protected virtual void OnStop_Attack()
	{
		if (ProjectileFiringRate == 0f)
		{
			EnableFiring();
		}
		else
		{
			DisableFiring(ProjectileTapFiringRate - (Time.time - m_LastFireTime));
		}
	}

	protected virtual bool OnAttempt_Fire()
	{
		if (Time.time < m_NextAllowedFireTime)
		{
			return false;
		}
		if (!Player.DepleteAmmo.Try())
		{
			DryFire();
			return false;
		}
		Fire();
		return true;
	}
}
