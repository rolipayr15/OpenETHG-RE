using System;
using UnityEngine;

public abstract class vp_Controller : vp_Component
{
	public enum PushForceMode
	{
		Simplified,
		Kinetic
	}

	protected bool m_Grounded;

	protected RaycastHit m_GroundHit;

	protected Transform m_LastGroundHitTransform;

	protected Transform m_GroundHitTransform;

	protected float m_FallStartHeight = -99999f;

	protected float m_FallImpact;

	protected bool m_OnNewGround;

	protected bool m_WasFalling;

	protected vp_SurfaceIdentifier m_CurrentGroundSurface;

	public Texture m_CurrentGroundTexture;

	public Texture m_CurrentTerrainTexture;

	public float PhysicsGravityModifier = 0.2f;

	protected float m_FallSpeed;

	protected const float PHYSICS_GRAVITY_MODIFIER_INTERNAL = 0.002f;

	public bool MotorFreeFly;

	public float PhysicsPushForce = 5f;

	public PushForceMode PhysicsPushMode;

	public float PhysicsPushInterval = 0.1f;

	public float PhysicsCrouchHeightModifier = 0.5f;

	protected Vector3 m_Velocity = Vector3.zero;

	protected Vector3 m_PrevPosition = Vector3.zero;

	protected Vector3 m_PrevVelocity = Vector3.zero;

	protected float m_NextAllowedPushTime;

	[HideInInspector]
	public Vector3 PositionOnPlatform = Vector3.zero;

	protected Transform m_Platform;

	protected float m_LastPlatformAngle;

	protected Vector3 m_LastPlatformPos = Vector3.zero;

	protected float m_MovingPlatformBodyYawDif;

	protected float m_NormalHeight;

	protected Vector3 m_NormalCenter = Vector3.zero;

	protected float m_CrouchHeight;

	protected Vector3 m_CrouchCenter = Vector3.zero;

	protected const float KINETIC_PUSHFORCE_MULTIPLIER = 15f;

	protected const float CHARACTER_CONTROLLER_SKINWIDTH = 0.08f;

	protected const float DEFAULT_RADIUS_MULTIPLIER = 0.25f;

	protected const float FALL_IMPACT_MULTIPLIER = 0.075f;

	protected const float NOFALL = -99999f;

	private vp_PlayerEventHandler m_Player;

	public bool Grounded => m_Grounded;

	public Transform GroundTransform => m_GroundHitTransform;

	public float SkinWidth => 0.08f;

	[Obsolete("Please use 'PositionOnPlatform' instead.")]
	public Vector3 m_PositionOnPlatform
	{
		get
		{
			return PositionOnPlatform;
		}
		set
		{
			PositionOnPlatform = value;
		}
	}

	protected vp_PlayerEventHandler Player
	{
		get
		{
			if (m_Player == null)
			{
				if (base.EventHandler != null)
				{
					m_Player = (vp_PlayerEventHandler)base.EventHandler;
				}
				if (m_Player == null)
				{
					Debug.LogError("Error (" + this?.ToString() + ") This component requires a " + ((this is vp_FPController) ? "vp_FPPlayerEventHandler" : "vp_PlayerEventHandler") + " component!");
				}
			}
			return m_Player;
		}
	}

	private float FallDistance
	{
		get
		{
			if (m_FallStartHeight == -99999f)
			{
				return 0f;
			}
			return Mathf.Max(0f, m_FallStartHeight - base.Transform.position.y);
		}
	}

	protected virtual Transform OnValue_Platform
	{
		get
		{
			return m_Platform;
		}
		set
		{
			m_Platform = value;
		}
	}

	protected virtual Vector3 OnValue_PositionOnPlatform
	{
		get
		{
			if (!(m_Platform == null))
			{
				return PositionOnPlatform;
			}
			return Vector3.zero;
		}
		set
		{
			PositionOnPlatform = ((m_Platform == null) ? Vector3.zero : value);
		}
	}

	protected virtual Vector3 OnValue_Position
	{
		get
		{
			return base.Transform.position;
		}
		set
		{
			SetPosition(value);
		}
	}

	protected virtual float OnValue_FallSpeed
	{
		get
		{
			return m_FallSpeed;
		}
		set
		{
			m_FallSpeed = value;
		}
	}

	protected virtual Vector3 OnValue_Velocity
	{
		get
		{
			return m_Velocity;
		}
		set
		{
			m_Velocity = value;
		}
	}

	protected abstract float OnValue_Radius { get; }

	protected abstract float OnValue_Height { get; }

	protected virtual bool OnValue_Grounded => m_Grounded;

	protected override void Awake()
	{
		base.Awake();
		InitCollider();
	}

	protected override void Start()
	{
		base.Start();
		RefreshCollider();
	}

	protected override void Update()
	{
		base.Update();
		UpdatePlatformRotation();
	}

	protected override void FixedUpdate()
	{
		if (Time.timeScale != 0f)
		{
			UpdateForces();
			FixedMove();
			UpdateCollisions();
			UpdatePlatformMove();
			UpdateVelocity();
		}
	}

	protected virtual void UpdatePlatformMove()
	{
		if (!(m_Platform == null))
		{
			PositionOnPlatform = m_Platform.InverseTransformPoint(m_Transform.position);
			m_LastPlatformPos = m_Platform.position;
		}
	}

	protected virtual void UpdatePlatformRotation()
	{
		if (!(m_Platform == null))
		{
			if (Player.IsLocal.Get())
			{
				m_MovingPlatformBodyYawDif = Mathf.Lerp(m_MovingPlatformBodyYawDif, Mathf.DeltaAngle(Player.Rotation.Get().y, Player.BodyYaw.Get()), Time.deltaTime * 1f);
			}
			Player.Rotation.Set(new Vector2(Player.Rotation.Get().x, Player.Rotation.Get().y - Mathf.DeltaAngle(m_Platform.eulerAngles.y, m_LastPlatformAngle)));
			m_LastPlatformAngle = m_Platform.eulerAngles.y;
			if (Player.IsLocal.Get())
			{
				Player.BodyYaw.Set(Player.BodyYaw.Get() - m_MovingPlatformBodyYawDif);
			}
		}
	}

	protected virtual void UpdateVelocity()
	{
		m_PrevVelocity = m_Velocity;
		m_Velocity = (base.transform.position - m_PrevPosition) / Time.deltaTime;
		m_PrevPosition = base.Transform.position;
	}

	public virtual void Stop()
	{
		Player.Move.Send(Vector3.zero);
		Player.InputMoveVector.Set(Vector2.zero);
		m_FallSpeed = 0f;
		m_FallStartHeight = -99999f;
	}

	protected virtual void InitCollider()
	{
	}

	protected virtual void RefreshCollider()
	{
	}

	public virtual void EnableCollider(bool enabled)
	{
	}

	protected virtual void StoreGroundInfo()
	{
		m_LastGroundHitTransform = m_GroundHitTransform;
		m_Grounded = false;
		m_GroundHitTransform = null;
		if (Physics.SphereCast(new Ray(base.Transform.position + Vector3.up * Player.Radius.Get(), Vector3.down), Player.Radius.Get(), out m_GroundHit, 0.18f, vp_Layer.Mask.ExternalBlockers))
		{
			m_GroundHitTransform = m_GroundHit.transform;
			m_Grounded = true;
		}
		if (m_Velocity.y < 0f && m_GroundHitTransform == null && m_LastGroundHitTransform != null && !Player.Jump.Active)
		{
			SetFallHeight(base.Transform.position.y);
		}
	}

	protected void SetFallHeight(float height)
	{
		if (m_FallStartHeight == -99999f && !m_Grounded && !(m_GroundHitTransform != null))
		{
			m_FallStartHeight = height;
		}
	}

	protected virtual void UpdateForces()
	{
		m_LastGroundHitTransform = m_GroundHitTransform;
		if (m_Grounded && m_FallSpeed <= 0f)
		{
			m_FallSpeed = Physics.gravity.y * (PhysicsGravityModifier * 0.002f) * vp_TimeUtility.AdjustedTimeScale;
			return;
		}
		m_FallSpeed += Physics.gravity.y * (PhysicsGravityModifier * 0.002f) * vp_TimeUtility.AdjustedTimeScale;
		if (m_Velocity.y < 0f && m_PrevVelocity.y >= 0f)
		{
			SetFallHeight(base.Transform.position.y);
		}
	}

	protected virtual void FixedMove()
	{
		StoreGroundInfo();
	}

	protected virtual void UpdateCollisions()
	{
		if (Player.Climb.Active)
		{
			m_FallStartHeight = -99999f;
		}
		m_FallImpact = 0f;
		m_OnNewGround = false;
		m_WasFalling = false;
		if (!(m_GroundHitTransform != null) || !(m_GroundHitTransform != m_LastGroundHitTransform))
		{
			return;
		}
		m_OnNewGround = true;
		if (m_LastGroundHitTransform == null)
		{
			m_WasFalling = true;
			if (m_FallStartHeight > base.Transform.position.y && m_Grounded)
			{
				m_FallImpact = (MotorFreeFly ? 0f : (FallDistance * 0.075f));
				Player.FallImpact.Send(m_FallImpact);
			}
		}
		m_FallStartHeight = -99999f;
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
	}

	public virtual void SetPosition(Vector3 position)
	{
		base.Transform.position = position;
		m_PrevPosition = position;
		vp_Timer.In(0f, delegate
		{
			m_PrevVelocity = vp_3DUtility.HorizontalVector(m_PrevVelocity);
		});
	}

	public void PushRigidbody(Rigidbody rigidbody, Vector3 moveDirection, PushForceMode pushForcemode, Vector3 point)
	{
		if (PhysicsPushForce == 0f || (rigidbody.gameObject.layer == 28 && pushForcemode == PushForceMode.Simplified))
		{
			return;
		}
		switch (pushForcemode)
		{
		case PushForceMode.Simplified:
			rigidbody.linearVelocity = vp_3DUtility.HorizontalVector(new Vector3(moveDirection.x, 0f, moveDirection.z).normalized) * (PhysicsPushForce / rigidbody.mass);
			break;
		case PushForceMode.Kinetic:
			if (Vector3.Distance(vp_3DUtility.HorizontalVector(base.Transform.position), vp_3DUtility.HorizontalVector(point)) > Player.Radius.Get())
			{
				rigidbody.AddForceAtPosition(vp_3DUtility.HorizontalVector(moveDirection) * (PhysicsPushForce * 15f), point);
			}
			else
			{
				rigidbody.AddForceAtPosition(moveDirection * (PhysicsPushForce * 15f), point);
			}
			break;
		}
	}

	public void PushRigidbody(Rigidbody rigidbody, Vector3 moveDirection, PushForceMode pushForceMode)
	{
		PushRigidbody(rigidbody, moveDirection, pushForceMode, (pushForceMode == PushForceMode.Simplified) ? Vector3.zero : rigidbody.ClosestPointOnBounds(base.Collider.bounds.center));
	}

	public void PushRigidbody(Rigidbody rigidbody, Vector3 moveDirection)
	{
		PushRigidbody(rigidbody, moveDirection, PhysicsPushMode, (PhysicsPushMode == PushForceMode.Simplified) ? Vector3.zero : rigidbody.ClosestPointOnBounds(base.Collider.bounds.center));
	}

	public void PushRigidbody(Rigidbody rigidbody, Vector3 moveDirection, Vector3 point)
	{
		PushRigidbody(rigidbody, moveDirection, PhysicsPushMode, point);
	}

	protected virtual void OnMessage_Stop()
	{
		Stop();
	}

	protected virtual void OnStart_Crouch()
	{
		Player.Run.Stop();
		RefreshCollider();
	}

	protected virtual void OnStop_Crouch()
	{
		RefreshCollider();
	}
}
