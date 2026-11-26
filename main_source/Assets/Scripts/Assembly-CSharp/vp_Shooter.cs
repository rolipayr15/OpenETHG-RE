using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class vp_Shooter : vp_Component
{
	public delegate void NetworkFunc();

	public delegate Vector3 FirePositionFunc();

	public delegate Quaternion FireRotationFunc();

	public delegate int FireSeedFunc();

	protected CharacterController m_CharacterController;

	public GameObject m_ProjectileSpawnPoint;

	protected GameObject m_ProjectileDefaultSpawnpoint;

	public GameObject ProjectilePrefab;

	public float ProjectileScale = 1f;

	public float ProjectileFiringRate = 0.3f;

	public float ProjectileSpawnDelay;

	public int ProjectileCount = 1;

	public float ProjectileSpread;

	public bool ProjectileSourceIsRoot = true;

	public string FireMessage = "";

	protected bool HaveFireMessage;

	protected float m_NextAllowedFireTime;

	public Vector3 MuzzleFlashPosition = Vector3.zero;

	public Vector3 MuzzleFlashScale = Vector3.one;

	public float MuzzleFlashFadeSpeed = 0.075f;

	public GameObject MuzzleFlashPrefab;

	public float MuzzleFlashDelay;

	protected GameObject m_MuzzleFlash;

	public Transform m_MuzzleFlashSpawnPoint;

	public GameObject ShellPrefab;

	public float ShellScale = 1f;

	protected Vector3 m_ActualShellScale = Vector3.one;

	public Vector3 ShellEjectDirection = new Vector3(1f, 1f, 1f);

	public Vector3 ShellEjectPosition = new Vector3(1f, 0f, 1f);

	public float ShellEjectVelocity = 0.2f;

	public float ShellEjectDelay;

	public float ShellEjectSpin;

	protected Rigidbody m_ShellRigidbody;

	public Transform m_ShellEjectSpawnPoint;

	public AudioClip SoundFire;

	public float SoundFireDelay;

	public Vector2 SoundFirePitch = new Vector2(1f, 1f);

	public NetworkFunc m_SendFireEventToNetworkFunc;

	public FirePositionFunc GetFirePosition;

	public FireRotationFunc GetFireRotation;

	public FireSeedFunc GetFireSeed;

	protected Vector3 m_CurrentFirePosition = Vector3.zero;

	protected Quaternion m_CurrentFireRotation = Quaternion.identity;

	protected int m_CurrentFireSeed;

	public Vector3 FirePosition = Vector3.zero;

	public GameObject ProjectileSpawnPoint => m_ProjectileSpawnPoint;

	public GameObject MuzzleFlash
	{
		get
		{
			if (m_MuzzleFlash == null && MuzzleFlashPrefab != null && ProjectileSpawnPoint != null)
			{
				m_MuzzleFlash = (GameObject)vp_Utility.Instantiate(MuzzleFlashPrefab, ProjectileSpawnPoint.transform.position, ProjectileSpawnPoint.transform.rotation);
				m_MuzzleFlash.name = base.transform.name + "MuzzleFlash";
				m_MuzzleFlash.transform.parent = ProjectileSpawnPoint.transform;
			}
			return m_MuzzleFlash;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (m_ProjectileSpawnPoint == null)
		{
			m_ProjectileSpawnPoint = base.gameObject;
		}
		m_ProjectileDefaultSpawnpoint = m_ProjectileSpawnPoint;
		if (GetFirePosition == null)
		{
			GetFirePosition = () => FirePosition;
		}
		if (GetFireRotation == null)
		{
			GetFireRotation = () => m_ProjectileSpawnPoint.transform.rotation;
		}
		if (GetFireSeed == null)
		{
			GetFireSeed = () => Random.Range(0, 100);
		}
		m_CharacterController = m_ProjectileSpawnPoint.transform.root.GetComponentInChildren<CharacterController>();
		m_NextAllowedFireTime = Time.time;
		ProjectileSpawnDelay = Mathf.Min(ProjectileSpawnDelay, ProjectileFiringRate - 0.1f);
		if (ShellPrefab != null)
		{
			m_ActualShellScale = ShellPrefab.transform.localScale * ShellScale;
		}
		HaveFireMessage = !string.IsNullOrEmpty(FireMessage);
	}

	protected override void Start()
	{
		base.Start();
		base.Audio.playOnAwake = false;
		base.Audio.dopplerLevel = 0f;
		RefreshDefaultState();
		Refresh();
	}

	protected override void LateUpdate()
	{
		FirePosition = m_ProjectileSpawnPoint.transform.position;
	}

	public virtual bool CanFire()
	{
		if (Time.time < m_NextAllowedFireTime)
		{
			return false;
		}
		return true;
	}

	public virtual bool TryFire()
	{
		if (Time.time < m_NextAllowedFireTime)
		{
			return false;
		}
		Fire();
		return true;
	}

	protected virtual void Fire()
	{
		m_NextAllowedFireTime = Time.time + ProjectileFiringRate;
		if (SoundFireDelay == 0f)
		{
			PlayFireSound();
		}
		else
		{
			vp_Timer.In(SoundFireDelay, PlayFireSound);
		}
		if (ProjectileSpawnDelay == 0f)
		{
			SpawnProjectiles();
		}
		else
		{
			vp_Timer.In(ProjectileSpawnDelay, delegate
			{
				SpawnProjectiles();
			});
		}
		if (ShellEjectDelay == 0f)
		{
			EjectShell();
		}
		else
		{
			vp_Timer.In(ShellEjectDelay, EjectShell);
		}
		if (MuzzleFlashDelay == 0f)
		{
			ShowMuzzleFlash();
		}
		else
		{
			vp_Timer.In(MuzzleFlashDelay, ShowMuzzleFlash);
		}
		if (HaveFireMessage)
		{
			base.gameObject.transform.root.gameObject.SendMessage(FireMessage, SendMessageOptions.DontRequireReceiver);
		}
	}

	protected virtual void PlayFireSound()
	{
		if (!(base.Audio == null))
		{
			base.Audio.pitch = Random.Range(SoundFirePitch.x, SoundFirePitch.y) * Time.timeScale;
			base.Audio.clip = SoundFire;
			base.Audio.Play();
		}
	}

	protected virtual void SpawnProjectiles()
	{
		if (!(ProjectilePrefab == null))
		{
			if (m_SendFireEventToNetworkFunc != null)
			{
				m_SendFireEventToNetworkFunc();
			}
			m_CurrentFirePosition = GetFirePosition();
			m_CurrentFireRotation = GetFireRotation();
			m_CurrentFireSeed = GetFireSeed();
			for (int i = 0; i < ProjectileCount; i++)
			{
				GameObject gameObject = null;
				gameObject = (GameObject)vp_Utility.Instantiate(ProjectilePrefab, m_CurrentFirePosition, m_CurrentFireRotation);
				gameObject.SendMessage("SetSource", ProjectileSourceIsRoot ? base.Root : base.Transform, SendMessageOptions.DontRequireReceiver);
				gameObject.transform.localScale = new Vector3(ProjectileScale, ProjectileScale, ProjectileScale);
				SetSpread(m_CurrentFireSeed * (i + 1), gameObject.transform);
			}
		}
	}

	public void SetSpread(int seed, Transform target)
	{
		vp_MathUtility.SetSeed(seed);
		target.Rotate(0f, 0f, Random.Range(0, 360));
		target.Rotate(0f, Random.Range(0f - ProjectileSpread, ProjectileSpread), 0f);
	}

	protected virtual void ShowMuzzleFlash()
	{
		if (!(MuzzleFlash == null))
		{
			if (m_MuzzleFlashSpawnPoint != null && ProjectileSpawnPoint != null)
			{
				MuzzleFlash.transform.position = m_MuzzleFlashSpawnPoint.transform.position;
				MuzzleFlash.transform.rotation = m_MuzzleFlashSpawnPoint.transform.rotation;
			}
			MuzzleFlash.SendMessage("Shoot", SendMessageOptions.DontRequireReceiver);
		}
	}

	protected virtual void EjectShell()
	{
		if (this == null || ShellPrefab == null)
		{
			return;
		}
		GameObject gameObject = null;
		gameObject = (GameObject)vp_Utility.Instantiate(ShellPrefab, (m_ShellEjectSpawnPoint == null) ? (FirePosition + m_ProjectileSpawnPoint.transform.TransformDirection(ShellEjectPosition)) : m_ShellEjectSpawnPoint.transform.position, m_ProjectileSpawnPoint.transform.rotation);
		gameObject.transform.localScale = m_ActualShellScale;
		vp_Layer.Set(gameObject.gameObject, 29);
		m_ShellRigidbody = gameObject.GetComponent<Rigidbody>();
		if (m_ShellRigidbody == null)
		{
			return;
		}
		Vector3 force = ((m_ShellEjectSpawnPoint == null) ? (base.transform.TransformDirection(ShellEjectDirection).normalized * ShellEjectVelocity) : (m_ShellEjectSpawnPoint.transform.forward.normalized * ShellEjectVelocity));
		m_ShellRigidbody.AddForce(force, ForceMode.Impulse);
		if ((bool)m_CharacterController)
		{
			Vector3 velocity = m_CharacterController.velocity;
			m_ShellRigidbody.AddForce(velocity, ForceMode.VelocityChange);
		}
		if (ShellEjectSpin > 0f)
		{
			if (Random.value > 0.5f)
			{
				m_ShellRigidbody.AddRelativeTorque(-Random.rotation.eulerAngles * ShellEjectSpin);
			}
			else
			{
				m_ShellRigidbody.AddRelativeTorque(Random.rotation.eulerAngles * ShellEjectSpin);
			}
		}
	}

	public virtual void DisableFiring(float seconds = 10000000f)
	{
		m_NextAllowedFireTime = Time.time + seconds;
	}

	public virtual void EnableFiring()
	{
		m_NextAllowedFireTime = Time.time;
	}

	public override void Refresh()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (MuzzleFlash != null)
		{
			if (m_MuzzleFlashSpawnPoint == null)
			{
				if (ProjectileSpawnPoint == m_ProjectileDefaultSpawnpoint)
				{
					m_MuzzleFlashSpawnPoint = vp_Utility.GetTransformByNameInChildren(ProjectileSpawnPoint.transform, "muzzle");
				}
				else
				{
					m_MuzzleFlashSpawnPoint = vp_Utility.GetTransformByNameInChildren(base.Transform, "muzzle");
				}
			}
			if (m_MuzzleFlashSpawnPoint != null)
			{
				if (ProjectileSpawnPoint == m_ProjectileDefaultSpawnpoint)
				{
					m_MuzzleFlash.transform.parent = ProjectileSpawnPoint.transform.parent.parent.parent;
				}
				else
				{
					m_MuzzleFlash.transform.parent = ProjectileSpawnPoint.transform;
				}
			}
			else
			{
				m_MuzzleFlash.transform.parent = ProjectileSpawnPoint.transform;
				MuzzleFlash.transform.localPosition = MuzzleFlashPosition;
				MuzzleFlash.transform.rotation = ProjectileSpawnPoint.transform.rotation;
			}
			MuzzleFlash.transform.localScale = MuzzleFlashScale;
			MuzzleFlash.SendMessage("SetFadeSpeed", MuzzleFlashFadeSpeed, SendMessageOptions.DontRequireReceiver);
		}
		if (ShellPrefab != null && m_ShellEjectSpawnPoint == null && ProjectileSpawnPoint != null)
		{
			if (ProjectileSpawnPoint == m_ProjectileDefaultSpawnpoint)
			{
				m_ShellEjectSpawnPoint = vp_Utility.GetTransformByNameInChildren(ProjectileSpawnPoint.transform, "shell");
			}
			else
			{
				m_ShellEjectSpawnPoint = vp_Utility.GetTransformByNameInChildren(base.Transform, "shell");
			}
		}
	}

	public override void Activate()
	{
		base.Activate();
		if (MuzzleFlash != null)
		{
			vp_Utility.Activate(MuzzleFlash);
		}
	}

	public override void Deactivate()
	{
		base.Deactivate();
		if (MuzzleFlash != null)
		{
			vp_Utility.Activate(MuzzleFlash, activate: false);
		}
	}

	protected void DrawProjectileDebugInfo(int projectileIndex)
	{
		GameObject gameObject = vp_3DUtility.DebugPointer();
		gameObject.transform.rotation = GetFireRotation();
		gameObject.transform.position = GetFirePosition();
		GameObject gameObject2 = vp_3DUtility.DebugBall();
		if (Physics.Linecast(gameObject.transform.position, gameObject.transform.position + gameObject.transform.forward * 1000f, out var hitInfo, vp_Layer.Mask.ExternalBlockers) && !hitInfo.collider.isTrigger && base.Root.InverseTransformPoint(hitInfo.point).z > 0f)
		{
			gameObject2.transform.position = hitInfo.point;
		}
	}
}
