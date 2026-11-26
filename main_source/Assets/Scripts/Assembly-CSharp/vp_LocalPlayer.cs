using UnityEngine;
using UnityEngine.SceneManagement;

public static class vp_LocalPlayer
{
	private static vp_FPPlayerEventHandler m_FPEventHandler;

	private static vp_FPCamera m_FPCamera;

	private static vp_FPController m_FPController;

	private static vp_FPInput m_FPInput;

	private static vp_FPPlayerDamageHandler m_FPDamageHandler;

	private static vp_FPWeaponHandler m_FPWeaponHandler;

	private static vp_PlayerInventory m_Inventory;

	private static vp_PlayerRespawner m_Respawner;

	private static vp_FPBodyAnimator m_FPBodyAnimator;

	private static vp_RagdollHandler m_RagdollHandler;

	private static vp_PlayerFootFXHandler m_FootFXHandler;

	private static int m_LevelOfLocalPlayer = -99999;

	private static Vector2 m_MouseSensBackup = new Vector2(5f, 5f);

	private static Texture m_CrosshairBackup = null;

	private static Texture m_InvisibleTexture = null;

	public static bool Exists => m_FPEventHandler != null;

	public static vp_FPPlayerEventHandler EventHandler
	{
		get
		{
			return m_FPEventHandler;
		}
		set
		{
			m_FPEventHandler = value;
			m_LevelOfLocalPlayer = SceneManager.GetActiveScene().buildIndex;
			RefreshPlayerComponents();
		}
	}

	public static vp_FPCamera Camera => m_FPCamera;

	public static vp_FPController Controller => m_FPController;

	public static vp_FPInput InputManager => m_FPInput;

	public static vp_FPPlayerDamageHandler DamageHandler => m_FPDamageHandler;

	public static vp_FPWeaponHandler WeaponHandler => m_FPWeaponHandler;

	public static vp_PlayerInventory Inventory => m_Inventory;

	public static vp_PlayerRespawner Respawner => m_Respawner;

	public static vp_FPBodyAnimator BodyAnimator => m_FPBodyAnimator;

	public static vp_RagdollHandler RagdollHandler => m_RagdollHandler;

	public static vp_PlayerFootFXHandler FootFXHandler => m_FootFXHandler;

	public static Vector3 Position
	{
		get
		{
			return m_FPEventHandler.Position.Get();
		}
		set
		{
			m_FPEventHandler.Position.Set(value);
		}
	}

	public static Transform Ground
	{
		get
		{
			if (m_FPController == null)
			{
				return null;
			}
			return m_FPController.GroundTransform;
		}
	}

	public static Transform Platform => m_FPEventHandler.Platform.Get();

	public static Transform LookTransform => GetLookTransform(2f);

	public static Vector2 Rotation
	{
		get
		{
			return m_FPEventHandler.Rotation.Get();
		}
		set
		{
			m_FPEventHandler.Rotation.Set(value);
		}
	}

	public static Vector3 BodyHeadLookDirection => m_FPEventHandler.HeadLookDirection.Get();

	public static Vector3 AimDirection => m_FPEventHandler.AimDirection.Get();

	public static Vector3 Velocity
	{
		get
		{
			return m_FPEventHandler.Velocity.Get();
		}
		set
		{
			m_FPEventHandler.Velocity.Set(value);
		}
	}

	public static float VelocityMagnitude => m_FPEventHandler.Velocity.Get().magnitude;

	public static float Health
	{
		get
		{
			return m_FPEventHandler.Health.Get();
		}
		set
		{
			m_FPEventHandler.Health.Set(value);
		}
	}

	public static float MaxHealth
	{
		get
		{
			return m_FPEventHandler.MaxHealth.Get();
		}
		set
		{
			m_FPEventHandler.MaxHealth.Set(value);
		}
	}

	public static int CurrentWeaponIndex => m_FPEventHandler.CurrentWeaponIndex.Get();

	public static string CurrentWeaponName => m_FPEventHandler.CurrentWeaponName.Get();

	public static int CurrentAmmo => m_FPEventHandler.CurrentWeaponAmmoCount.Get();

	public static int CurrentMaxAmmo => m_FPEventHandler.CurrentWeaponMaxAmmoCount.Get();

	public static int SpareAmmoForCurrentWeapon
	{
		get
		{
			if (m_Inventory == null)
			{
				return 0;
			}
			return m_Inventory.GetExtraAmmoForCurrentWeapon();
		}
	}

	public static Texture2D CurrentAmmoIcon => m_FPEventHandler.CurrentAmmoIcon.Get();

	public static bool IsFirstPerson => m_FPEventHandler.IsFirstPerson.Get();

	public static bool IsGrounded => m_FPEventHandler.Grounded.Get();

	public static bool IsMaster => vp_Gameplay.IsMaster;

	public static Collider Collider
	{
		get
		{
			if (m_FPController == null)
			{
				return null;
			}
			return m_FPController.Collider;
		}
	}

	public static Transform Head
	{
		get
		{
			if (m_FPBodyAnimator != null)
			{
				return m_FPBodyAnimator.HeadBone.transform;
			}
			if (m_FPCamera != null)
			{
				return m_FPCamera.transform;
			}
			return null;
		}
	}

	public static Transform LeftFoot
	{
		get
		{
			if (m_FootFXHandler != null && m_FootFXHandler.FootLeft != null)
			{
				return m_FootFXHandler.FootLeft.transform;
			}
			if (m_FPController != null)
			{
				return m_FPController.transform;
			}
			return null;
		}
	}

	public static Transform RightFoot
	{
		get
		{
			if (m_FootFXHandler != null && m_FootFXHandler.FootRight != null)
			{
				return m_FootFXHandler.FootRight.transform;
			}
			if (m_FPController != null)
			{
				return m_FPController.transform;
			}
			return null;
		}
	}

	public static AudioClip DeathSound
	{
		get
		{
			if (m_FPDamageHandler == null)
			{
				return null;
			}
			return m_FPDamageHandler.DeathSound;
		}
		set
		{
			if (!(m_FPDamageHandler == null))
			{
				m_FPDamageHandler.DeathSound = value;
			}
		}
	}

	public static vp_ImpactEvent FootstepFXImpactEvent
	{
		get
		{
			if (m_FootFXHandler == null)
			{
				return null;
			}
			return m_FootFXHandler.FootstepImpactEvent;
		}
		set
		{
			if (!(m_FootFXHandler == null))
			{
				m_FootFXHandler.FootstepImpactEvent = value;
			}
		}
	}

	public static vp_ImpactEvent FallFXImpactEvent
	{
		get
		{
			if (m_FootFXHandler == null)
			{
				return null;
			}
			return m_FootFXHandler.FallImpactEvent;
		}
		set
		{
			if (!(m_FootFXHandler == null))
			{
				m_FootFXHandler.FallImpactEvent = value;
			}
		}
	}

	public static vp_ImpactEvent JumpFXImpactEvent
	{
		get
		{
			if (m_FootFXHandler == null)
			{
				return null;
			}
			return m_FootFXHandler.JumpImpactEvent;
		}
		set
		{
			if (!(m_FootFXHandler == null))
			{
				m_FootFXHandler.JumpImpactEvent = value;
			}
		}
	}

	public static Texture CrosshairTexture
	{
		get
		{
			return m_FPEventHandler.Crosshair.Get();
		}
		set
		{
			m_FPEventHandler.Crosshair.Set(value);
			if (value != m_InvisibleTexture)
			{
				m_CrosshairBackup = value;
			}
		}
	}

	public static void Refresh()
	{
		if (m_FPEventHandler == null || m_LevelOfLocalPlayer != vp_Gameplay.CurrentLevel)
		{
			m_FPEventHandler = Object.FindObjectOfType(typeof(vp_FPPlayerEventHandler)) as vp_FPPlayerEventHandler;
			m_LevelOfLocalPlayer = ((m_FPEventHandler != null) ? vp_Gameplay.CurrentLevel : (-99999));
			RefreshPlayerComponents();
		}
	}

	private static void RefreshPlayerComponents()
	{
		m_FPCamera = null;
		m_FPController = null;
		m_FPInput = null;
		m_FPDamageHandler = null;
		m_FPWeaponHandler = null;
		m_Inventory = null;
		m_Respawner = null;
		m_FPBodyAnimator = null;
		m_RagdollHandler = null;
		m_FootFXHandler = null;
		if (!(m_FPEventHandler == null))
		{
			m_FPCamera = m_FPEventHandler.GetComponentInChildren<vp_FPCamera>();
			m_FPController = m_FPEventHandler.GetComponentInChildren<vp_FPController>();
			m_FPInput = m_FPEventHandler.GetComponentInChildren<vp_FPInput>();
			m_FPDamageHandler = m_FPEventHandler.GetComponentInChildren<vp_FPPlayerDamageHandler>();
			m_FPWeaponHandler = m_FPEventHandler.GetComponentInChildren<vp_FPWeaponHandler>();
			m_Inventory = m_FPEventHandler.GetComponentInChildren<vp_PlayerInventory>();
			m_Respawner = m_FPEventHandler.GetComponentInChildren<vp_PlayerRespawner>();
			m_FPBodyAnimator = m_FPEventHandler.GetComponentInChildren<vp_FPBodyAnimator>();
			m_RagdollHandler = m_FPEventHandler.GetComponentInChildren<vp_RagdollHandler>();
			m_FootFXHandler = m_FPEventHandler.GetComponentInChildren<vp_PlayerFootFXHandler>();
		}
	}

	public static void DismountPlatform()
	{
		m_FPEventHandler.Platform.Set(null);
	}

	public static Transform GetLookTransform(float maxRange, int layerMask = -1)
	{
		if (m_FPCamera == null)
		{
			return null;
		}
		return m_FPCamera.GetLookTransform(maxRange);
	}

	public static void Damage(float damage)
	{
		if (!(m_FPDamageHandler == null))
		{
			m_FPDamageHandler.Damage(damage);
		}
	}

	public static void Damage(float damage, Transform source, vp_DamageInfo.DamageType type = vp_DamageInfo.DamageType.Unknown)
	{
		if (!(m_FPDamageHandler == null) && (!(source == m_FPDamageHandler.transform) || type != vp_DamageInfo.DamageType.Bullet))
		{
			vp_DamageInfo damageInfo = new vp_DamageInfo(damage, source, type);
			m_FPDamageHandler.Damage(damageInfo);
		}
	}

	public static void Damage(vp_DamageInfo damageInfo)
	{
		if (!(m_FPDamageHandler == null) && (!(damageInfo.Source == m_FPDamageHandler.transform) || damageInfo.Type != vp_DamageInfo.DamageType.Bullet))
		{
			m_FPDamageHandler.Damage(damageInfo);
		}
	}

	public static void Die(float painHUDIntensity = 0f)
	{
		if (!m_FPEventHandler.Dead.Active)
		{
			m_FPEventHandler.SendMessage("Die");
			if (painHUDIntensity <= 0f)
			{
				m_FPEventHandler.HUDDamageFlash.Send(null);
			}
			else
			{
				m_FPEventHandler.HUDDamageFlash.Send(new vp_DamageInfo(painHUDIntensity, null));
			}
		}
	}

	public static void AddForce(Vector3 force)
	{
		m_FPEventHandler.ForceImpact.Send(force);
	}

	public static void CameraShake(float force = 0.5f, AudioClip audioClip = null, AudioSource audioSource = null)
	{
		if (m_FPCamera == null)
		{
			return;
		}
		m_FPEventHandler.CameraBombShake.Send(force);
		if (!(audioClip == null))
		{
			if (audioSource == null)
			{
				audioSource = m_FPCamera.Audio;
			}
			if (!(audioSource == null))
			{
				audioSource.Stop();
				audioSource.PlayOneShot(audioClip);
			}
		}
	}

	public static void GroundStomp(float force = 0.5f, AudioClip audioClip = null, AudioSource audioSource = null)
	{
		if (m_FPCamera == null)
		{
			return;
		}
		m_FPEventHandler.CameraGroundStomp.Send(force);
		if (!(audioClip == null))
		{
			if (audioSource == null)
			{
				audioSource = m_FPCamera.Audio;
			}
			if (!(audioSource == null))
			{
				audioSource.Stop();
				audioSource.PlayOneShot(audioClip);
			}
		}
	}

	public static void Move(Vector2 inputVector)
	{
		m_FPEventHandler.InputMoveVector.Set(inputVector);
	}

	public static void Stop()
	{
		m_FPEventHandler.Stop.Send();
	}

	public static void Teleport(Vector3 position, Vector2 rotation)
	{
		Position = position;
		Rotation = rotation;
		Stop();
	}

	public static void Teleport(float x, float y, float z, float pitch, float yaw)
	{
		Teleport(new Vector3(x, y, z), new Vector2(pitch, yaw));
	}

	public static void Teleport(Vector3 position)
	{
		Position = position;
		Stop();
	}

	public static void Teleport(float x, float y, float z)
	{
		Teleport(new Vector3(x, y, z));
	}

	public static void Teleport(vp_SpawnPoint spawnPoint)
	{
		if (!(spawnPoint == null))
		{
			float physicsCheckRadius = ((m_Respawner != null) ? m_Respawner.ObstructionRadius : 1f);
			vp_Placement placement = spawnPoint.GetPlacement(physicsCheckRadius);
			Position = placement.Position;
			Rotation = placement.Rotation.eulerAngles;
			Stop();
		}
	}

	public static void GoFirstPerson()
	{
		if (!IsFirstPerson)
		{
			m_FPEventHandler.CameraToggle3rdPerson.Send();
		}
	}

	public static void GoThirdPerson()
	{
		if (IsFirstPerson)
		{
			m_FPEventHandler.CameraToggle3rdPerson.Send();
		}
	}

	public static void ToggleThirdPerson()
	{
		m_FPEventHandler.CameraToggle3rdPerson.Send();
	}

	public static bool SetNextWeapon()
	{
		return m_FPEventHandler.SetNextWeapon.Try();
	}

	public static bool SetPrevWeapon()
	{
		return m_FPEventHandler.SetPrevWeapon.Try();
	}

	public static bool SetWeaponByName(string gameObjectName)
	{
		return m_FPEventHandler.SetWeaponByName.Try(gameObjectName);
	}

	public static bool SetWeaponByIndex(int index)
	{
		return m_FPEventHandler.SetWeapon.TryStart(index);
	}

	public static void UnwieldCurrentWeapon()
	{
		m_FPEventHandler.SetWeapon.TryStart(0);
	}

	public static void EnableGameplayInput()
	{
		m_FPEventHandler.InputAllowGameplay.Set(o: true);
	}

	public static void DisableGameplayInput()
	{
		Stop();
		m_FPEventHandler.InputAllowGameplay.Set(o: false);
	}

	public static void ToggleGameplayInput()
	{
		if (!m_FPEventHandler.InputAllowGameplay.Get())
		{
			EnableGameplayInput();
		}
		else
		{
			DisableGameplayInput();
		}
	}

	public static void EnableFreeLook()
	{
		if (!(m_FPInput == null) && !(m_FPInput.MouseLookSensitivity != Vector2.zero))
		{
			m_FPInput.MouseLookSensitivity = m_MouseSensBackup;
		}
	}

	public static void DisableFreeLook()
	{
		if (!(m_FPInput == null) && !(m_FPInput.MouseLookSensitivity == Vector2.zero))
		{
			m_MouseSensBackup = m_FPInput.MouseLookSensitivity;
			m_FPInput.MouseLookSensitivity = Vector2.zero;
		}
	}

	public static void ToggleFreeLook()
	{
		if (m_FPInput.MouseLookSensitivity == Vector2.zero)
		{
			EnableFreeLook();
		}
		else
		{
			DisableFreeLook();
		}
	}

	public static void ShowCrosshair()
	{
		if (!(CrosshairTexture != m_InvisibleTexture) && !(m_CrosshairBackup == null))
		{
			CrosshairTexture = m_CrosshairBackup;
		}
	}

	public static void HideCrosshair()
	{
		if (!(CrosshairTexture == m_InvisibleTexture))
		{
			m_CrosshairBackup = CrosshairTexture;
			Debug.Log(CrosshairTexture);
			CrosshairTexture = m_InvisibleTexture;
		}
	}

	public static void ToggleCrosshair()
	{
		if (CrosshairTexture == m_InvisibleTexture)
		{
			ShowCrosshair();
		}
		else
		{
			HideCrosshair();
		}
	}

	public static void ShowMouseCursor()
	{
		if (!(m_FPInput == null))
		{
			m_FPInput.MouseCursorForced = true;
			m_FPInput.MouseCursorBlocksMouseLook = true;
		}
	}

	public static void ShowMouseCursorAndAllowMouseLook()
	{
		if (!(m_FPInput == null))
		{
			m_FPInput.MouseCursorForced = true;
			m_FPInput.MouseCursorBlocksMouseLook = false;
		}
	}

	public static void HideMouseCursor()
	{
		if (!(m_FPInput == null))
		{
			m_FPInput.MouseCursorForced = false;
			m_FPInput.MouseCursorBlocksMouseLook = false;
		}
	}

	public static void ToggleMouseCursor()
	{
		if (!(m_FPInput == null))
		{
			if (m_FPInput.MouseCursorForced)
			{
				HideMouseCursor();
			}
			else
			{
				ShowMouseCursor();
			}
		}
	}
}
