using System;
using System.Collections.Generic;
using UnityEngine;

public class vp_FPWeaponMeleeAttack : vp_Component
{
	public string WeaponStatePull = "Pull";

	public string WeaponStateSwing = "Swing";

	public float SwingDelay = 0.5f;

	public float SwingDuration = 0.5f;

	public float SwingRate = 1f;

	protected float m_NextAllowedSwingTime;

	public int SwingSoftForceFrames = 50;

	public Vector3 SwingPositionSoftForce = new Vector3(-0.5f, -0.1f, 0.3f);

	public Vector3 SwingRotationSoftForce = new Vector3(50f, -25f, 0f);

	public float ImpactTime = 0.11f;

	public Vector3 ImpactPositionSpringRecoil = new Vector3(0.01f, 0.03f, -0.05f);

	public Vector3 ImpactPositionSpring2Recoil = Vector3.zero;

	public Vector3 ImpactRotationSpringRecoil = Vector3.zero;

	public Vector3 ImpactRotationSpring2Recoil = new Vector3(0f, 0f, 10f);

	public bool AttackPickRandomState = true;

	protected int m_AttackCurrent;

	public List<UnityEngine.Object> SoundSwing = new List<UnityEngine.Object>();

	public Vector2 SoundSwingPitch = new Vector2(0.5f, 1.5f);

	private vp_Timer.Handle SwingDelayTimer = new vp_Timer.Handle();

	private vp_Timer.Handle ImpactTimer = new vp_Timer.Handle();

	private vp_Timer.Handle SwingDurationTimer = new vp_Timer.Handle();

	private vp_Timer.Handle ResetTimer = new vp_Timer.Handle();

	private vp_FPPlayerEventHandler m_Player;

	private vp_FPWeapon m_FPWeapon;

	private vp_WeaponShooter m_WeaponShooter;

	protected vp_FPCamera m_FPCamera;

	protected vp_Controller m_FPController;

	protected vp_Bullet m_Bullet;

	private vp_FPPlayerEventHandler Player
	{
		get
		{
			if (m_Player == null && base.EventHandler != null)
			{
				m_Player = (vp_FPPlayerEventHandler)base.EventHandler;
			}
			return m_Player;
		}
	}

	private vp_FPWeapon FPWeapon
	{
		get
		{
			if (m_FPWeapon == null)
			{
				m_FPWeapon = base.Transform.GetComponent<vp_FPWeapon>();
			}
			return m_FPWeapon;
		}
	}

	private vp_WeaponShooter WeaponShooter
	{
		get
		{
			if (m_WeaponShooter == null)
			{
				m_WeaponShooter = base.Transform.GetComponent<vp_WeaponShooter>();
				if (m_WeaponShooter == null)
				{
					Debug.LogWarning("Warning (" + this?.ToString() + ") This component requires a vp_WeaponShooter (adding vp_WeaponShooter automatically).");
					m_WeaponShooter = base.gameObject.AddComponent<vp_WeaponShooter>();
				}
				else if (m_WeaponShooter is vp_FPWeaponShooter)
				{
					m_WeaponShooter.enabled = false;
					Debug.LogWarning("Warning (" + this?.ToString() + ") This component requires a vp_WeaponShooter. It does _not_ work with a vp_FPWeaponShooter!  (removing vp_FPWeaponShooter and adding vp_WeaponShooter automatically).");
					m_WeaponShooter = base.gameObject.AddComponent<vp_WeaponShooter>();
				}
			}
			return m_WeaponShooter;
		}
	}

	public vp_FPCamera FPCamera
	{
		get
		{
			if (m_FPCamera == null)
			{
				m_FPCamera = base.Root.GetComponentInChildren<vp_FPCamera>();
			}
			return m_FPCamera;
		}
	}

	public vp_Controller FPController
	{
		get
		{
			if (m_FPController == null)
			{
				m_FPController = base.Root.GetComponentInChildren<vp_Controller>();
			}
			return m_FPController;
		}
	}

	[Obsolete("Please use the 'Bullet' parameter instead.")]
	public vp_Bullet HitscanBullet => Bullet;

	public vp_Bullet Bullet
	{
		get
		{
			if (m_Bullet == null && WeaponShooter != null && WeaponShooter.ProjectilePrefab != null)
			{
				m_Bullet = WeaponShooter.ProjectilePrefab.GetComponent<vp_Bullet>();
				if (m_Bullet == null)
				{
					Debug.LogWarning("Warning (" + this?.ToString() + ") ProjectilePrefab of the WeaponShooter has no vp_Bullet-derived component (this melee weapon won't be able to do damage).");
				}
			}
			return m_Bullet;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (WeaponShooter != null)
		{
			WeaponShooter.ProjectileFiringRate = SwingRate;
			WeaponShooter.ProjectileTapFiringRate = SwingRate;
			WeaponShooter.ProjectileSpawnDelay = SwingDelay;
			WeaponShooter.ProjectileScale = 1f;
			WeaponShooter.ProjectileCount = 1;
			WeaponShooter.ProjectileSpread = 0f;
			if (WeaponShooter.ProjectilePrefab == null)
			{
				Debug.LogWarning("Warning (" + this?.ToString() + ") WeaponShooter for this melee weapon has no 'ProjectilePrefab' (it won't be able to do damage).");
			}
			if (WeaponShooter.Weapon != null)
			{
				WeaponShooter.Weapon.AnimationType = 2;
			}
		}
	}

	protected override void OnEnable()
	{
		RefreshFirePoint();
		base.OnEnable();
	}

	protected override void Update()
	{
		base.Update();
		UpdateAttack();
	}

	protected void UpdateAttack()
	{
		if (!Player.Attack.Active || Player.SetWeapon.Active || FPWeapon == null || !FPWeapon.Wielded || Time.time < m_NextAllowedSwingTime)
		{
			return;
		}
		m_NextAllowedSwingTime = Time.time + SwingRate;
		if (AttackPickRandomState)
		{
			PickAttack();
		}
		FPWeapon.SetState(WeaponStatePull);
		FPWeapon.Refresh();
		vp_Timer.In(SwingDelay, delegate
		{
			if (SoundSwing.Count > 0)
			{
				base.Audio.pitch = UnityEngine.Random.Range(SoundSwingPitch.x, SoundSwingPitch.y) * Time.timeScale;
				base.Audio.clip = (AudioClip)SoundSwing[UnityEngine.Random.Range(0, SoundSwing.Count)];
				if (vp_Utility.IsActive(base.gameObject))
				{
					base.Audio.Play();
				}
			}
			FPWeapon.SetState(WeaponStatePull, enabled: false);
			FPWeapon.SetState(WeaponStateSwing);
			FPWeapon.Refresh();
			FPWeapon.AddSoftForce(SwingPositionSoftForce, SwingRotationSoftForce, SwingSoftForceFrames);
			vp_Timer.In(ImpactTime, delegate
			{
				Physics.Raycast(new Ray(new Vector3(FPController.Transform.position.x, FPCamera.Transform.position.y, FPController.Transform.position.z), FPCamera.Transform.forward), out var hitInfo, (Bullet != null) ? Bullet.Range : 2f, vp_Layer.Mask.BulletBlockers);
				if (hitInfo.collider != null)
				{
					if (WeaponShooter != null)
					{
						WeaponShooter.FirePosition = Camera.main.transform.position;
						WeaponShooter.TryFire();
					}
					ApplyRecoil();
				}
				else
				{
					vp_Timer.In(SwingDuration - ImpactTime, delegate
					{
						FPWeapon.StopSprings();
						Reset();
					}, SwingDurationTimer);
				}
			}, ImpactTimer);
		}, SwingDelayTimer);
	}

	private void PickAttack()
	{
		int num = States.Count - 1;
		do
		{
			num = UnityEngine.Random.Range(0, States.Count - 1);
		}
		while (States.Count > 1 && num == m_AttackCurrent && UnityEngine.Random.value < 0.5f);
		m_AttackCurrent = num;
		SetState(States[m_AttackCurrent].Name);
	}

	private void ApplyRecoil()
	{
		FPWeapon.StopSprings();
		FPWeapon.AddForce(ImpactPositionSpringRecoil, ImpactRotationSpringRecoil);
		FPWeapon.AddForce2(ImpactPositionSpring2Recoil, ImpactRotationSpring2Recoil);
		Reset();
	}

	private void Reset()
	{
		vp_Timer.In(0.05f, delegate
		{
			if (FPWeapon != null)
			{
				FPWeapon.SetState(WeaponStatePull, enabled: false);
				FPWeapon.SetState(WeaponStateSwing, enabled: false);
				FPWeapon.Refresh();
				if (AttackPickRandomState)
				{
					ResetState();
				}
			}
		}, ResetTimer);
	}

	protected virtual void OnMessage_CameraToggle3rdPerson()
	{
		RefreshFirePoint();
	}

	private void RefreshFirePoint()
	{
		if (!(WeaponShooter == null) && Player.IsFirstPerson != null)
		{
			if (Player.IsFirstPerson.Get())
			{
				WeaponShooter.m_ProjectileSpawnPoint = FPCamera.gameObject;
			}
			else
			{
				WeaponShooter.m_ProjectileSpawnPoint = FPController.gameObject;
			}
			if (Player.CurrentWeaponName.Get() != base.name)
			{
				WeaponShooter.m_ProjectileSpawnPoint = FPCamera.gameObject;
			}
		}
	}
}
