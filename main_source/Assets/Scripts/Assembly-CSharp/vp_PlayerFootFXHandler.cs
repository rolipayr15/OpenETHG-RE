using System;
using DarkTonic.MasterAudio;
using UnityEngine;

public class vp_PlayerFootFXHandler : vp_Component
{
	public enum FootstepMode
	{
		Bypass,
		DetectBodyStep,
		FixedTimeInterval,
		DetectCameraBob
	}

	public int Mode = 1;

	public vp_ImpactEvent FootstepImpactEvent;

	public GameObject FootLeft;

	public GameObject FootRight;

	public float TriggerHeight = 0.1f;

	public float Sensitivity = 1.25f;

	public float TimeInterval = 0.35f;

	public bool RequireMoveInput;

	public bool VerifyGroundContact;

	public bool ForceAlwaysAnimate = true;

	public vp_ImpactEvent JumpImpactEvent;

	public vp_ImpactEvent FallImpactEvent;

	public float FallImpactThreshold = 0.15f;

	protected AudioSource m_CurrentAudioSource;

	protected float m_CurrentLocalTriggerHeight;

	protected Transform m_CurrentFoot;

	protected Transform m_OtherFoot;

	protected bool m_LeftWasAbove = true;

	protected bool m_RightWasAbove = true;

	protected bool m_FlipFootprint;

	protected float m_NextAllowedFootstepTime;

	protected float m_PrevYaw;

	protected bool m_Wasmoving;

	protected RaycastHit m_Hit;

	protected float m_NextAllowedJumpSoundTime;

	protected Action m_UpdateFunc;

	protected bool m_Mute;

	[SerializeField]
	protected bool m_MuteLocalFootsteps;

	protected Color m_GizmoColor = new Color(1f, 1f, 1f, 0.4f);

	protected Color m_SelectedGizmoColor = new Color32(160, byte.MaxValue, 100, 100);

	protected Vector3 GizmoScale = new Vector3(1.5f, 0.01f, 1.5f);

	private vp_PlayerEventHandler m_Player;

	private Animator m_Animator;

	protected vp_FPCamera m_FPCamera;

	protected bool m_CachedCamera;

	protected vp_FPController m_FPController;

	protected bool m_CachedController;

	private AudioSource m_AudioLeft;

	private AudioSource m_AudioRight;

	private AudioSource m_JumpFallAudio;

	protected bool IsMoving => Player.InputMoveVector.Get() != Vector2.zero;

	protected bool IsRotating => Mathf.Abs(Mathf.DeltaAngle(m_PrevYaw, base.transform.eulerAngles.y)) > 45f;

	protected float CurrentTriggerHeight
	{
		get
		{
			if (!IsMoving)
			{
				m_CurrentLocalTriggerHeight = 1f;
			}
			else if (!m_Wasmoving)
			{
				m_CurrentLocalTriggerHeight = 0f;
			}
			m_CurrentLocalTriggerHeight = Mathf.Lerp(m_CurrentLocalTriggerHeight, 1f, Time.deltaTime * Mathf.Max(2f - Sensitivity, 0.1f));
			m_Wasmoving = Player.InputMoveVector.Get().magnitude > 0.1f;
			return base.transform.position.y + TriggerHeight * m_CurrentLocalTriggerHeight;
		}
	}

	public bool MuteLocalFootsteps
	{
		get
		{
			if (Application.isPlaying)
			{
				if (m_MuteLocalFootsteps)
				{
					return Player.IsLocal.Get();
				}
				return false;
			}
			return m_MuteLocalFootsteps;
		}
		set
		{
			m_MuteLocalFootsteps = value;
		}
	}

	public vp_PlayerEventHandler Player
	{
		get
		{
			if (m_Player == null)
			{
				m_Player = base.transform.root.GetComponent<vp_PlayerEventHandler>();
				if (m_Player == null && Application.isPlaying)
				{
					Debug.LogError("Error (" + this?.ToString() + ") This component requires a vp_PlayerEventHandler. Disabling self!");
					base.enabled = false;
				}
			}
			return m_Player;
		}
	}

	public Animator Animator
	{
		get
		{
			if (m_Animator == null)
			{
				m_Animator = GetComponent<Animator>();
			}
			return m_Animator;
		}
	}

	public vp_FPCamera FPCamera
	{
		get
		{
			if (m_FPCamera == null && !m_CachedCamera)
			{
				m_FPCamera = base.transform.root.GetComponentInChildren<vp_FPCamera>();
				m_CachedCamera = true;
			}
			return m_FPCamera;
		}
	}

	public vp_FPController FPController
	{
		get
		{
			if (m_FPController == null && !m_CachedController)
			{
				m_FPController = base.transform.root.GetComponentInChildren<vp_FPController>();
				m_CachedController = true;
			}
			return m_FPController;
		}
	}

	private AudioSource AudioLeft
	{
		get
		{
			if (m_AudioLeft == null)
			{
				Transform transform = ((FootLeft == null) ? base.transform : FootLeft.transform);
				while (transform != null && m_AudioLeft == null)
				{
					m_AudioLeft = transform.GetComponent<AudioSource>();
					transform = transform.parent;
				}
			}
			return m_AudioLeft;
		}
	}

	private AudioSource AudioRight
	{
		get
		{
			if (m_AudioRight == null)
			{
				Transform transform = ((FootRight == null) ? base.transform : FootRight.transform);
				while (transform != null && m_AudioRight == null)
				{
					m_AudioRight = transform.GetComponent<AudioSource>();
					transform = transform.parent;
				}
			}
			return m_AudioRight;
		}
	}

	protected AudioSource JumpFallAudio
	{
		get
		{
			if (m_JumpFallAudio == null)
			{
				m_JumpFallAudio = base.transform.root.gameObject.AddComponent<AudioSource>();
				m_JumpFallAudio.rolloffMode = AudioLeft.rolloffMode;
				m_JumpFallAudio.minDistance = AudioLeft.minDistance;
				m_JumpFallAudio.maxDistance = AudioLeft.maxDistance;
				m_JumpFallAudio.spatialBlend = AudioLeft.spatialBlend;
				m_JumpFallAudio.volume = AudioLeft.volume;
				m_JumpFallAudio.spread = AudioLeft.spread;
			}
			return m_JumpFallAudio;
		}
	}

	protected override void Awake()
	{
		base.StateManager.SetState("Default", base.enabled);
	}

	protected override void Start()
	{
		base.Start();
		Refresh();
	}

	public override void Refresh()
	{
		m_UpdateFunc = null;
		if (FPCamera != null)
		{
			vp_FPCamera fPCamera = FPCamera;
			fPCamera.BobStepCallback = (vp_FPCamera.BobStepDelegate)Delegate.Remove(fPCamera.BobStepCallback, new vp_FPCamera.BobStepDelegate(UpdateFakeStepWithFoot));
			vp_FPCamera fPCamera2 = FPCamera;
			fPCamera2.BobStepCallback = (vp_FPCamera.BobStepDelegate)Delegate.Remove(fPCamera2.BobStepCallback, new vp_FPCamera.BobStepDelegate(UpdateFakeStepWithoutFoot));
		}
		switch (Mode)
		{
		case 0:
			m_UpdateFunc = null;
			break;
		case 1:
			m_UpdateFunc = UpdateRealStep;
			if (ForceAlwaysAnimate && Application.isPlaying && Animator != null)
			{
				Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
			}
			break;
		case 2:
			if (FootLeft != null && FootRight != null)
			{
				m_UpdateFunc = UpdateFakeStepWithFoot;
				break;
			}
			m_UpdateFunc = UpdateFakeStepWithoutFoot;
			m_CurrentAudioSource = ((AudioLeft != null) ? AudioLeft : AudioRight);
			m_CurrentFoot = base.transform.root;
			break;
		case 3:
			if (FPCamera != null)
			{
				if (FPCamera.BobRate.y == 0f && FPCamera.BobAmplitude.y == 0f)
				{
					Debug.LogWarning("Warning (" + this?.ToString() + ") FootstepMode is 'DetectCameraBob' but this vp_FPCamera has no Y axis bob.");
				}
				if (FootLeft != null && FootRight != null)
				{
					vp_FPCamera fPCamera3 = FPCamera;
					fPCamera3.BobStepCallback = (vp_FPCamera.BobStepDelegate)Delegate.Combine(fPCamera3.BobStepCallback, new vp_FPCamera.BobStepDelegate(UpdateFakeStepWithFoot));
					break;
				}
				vp_FPCamera fPCamera4 = FPCamera;
				fPCamera4.BobStepCallback = (vp_FPCamera.BobStepDelegate)Delegate.Combine(fPCamera4.BobStepCallback, new vp_FPCamera.BobStepDelegate(UpdateFakeStepWithoutFoot));
				m_CurrentAudioSource = ((AudioLeft != null) ? AudioLeft : AudioRight);
				m_CurrentFoot = base.transform.root;
			}
			break;
		}
		if (Application.isPlaying)
		{
			if (FootLeft != null && FootLeft.transform.root != base.Root)
			{
				Debug.LogWarning("Warning (" + this?.ToString() + ") LeftFoot is in another gameobject hierarchy (removing it). Did you copy component values from another body model?");
				FootLeft = null;
			}
			if (FootRight != null && FootRight.transform.root != base.Root)
			{
				Debug.LogWarning("Warning (" + this?.ToString() + ") RightFoot is in another gameobject hierarchy (removing it). Did you copy component values from another body model?");
				FootRight = null;
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		if (m_UpdateFunc == null || (Mode == 2 && Time.time < m_NextAllowedFootstepTime))
		{
			return;
		}
		if (!IsMoving)
		{
			Animator.SetFloat("MoveSpeed", 0f);
			if (RequireMoveInput)
			{
				return;
			}
			if (!IsRotating)
			{
				if (Mode == 2)
				{
					return;
				}
			}
			else if (Player.Platform.Get() != null)
			{
				return;
			}
		}
		m_UpdateFunc();
	}

	protected virtual void UpdateRealStep()
	{
		if (!(FootLeft == null) && !(FootRight == null))
		{
			m_CurrentFoot = FootLeft.transform;
			m_OtherFoot = FootRight.transform;
			m_FlipFootprint = false;
			m_CurrentAudioSource = AudioLeft;
			TryTriggerStep(ref m_LeftWasAbove);
			m_CurrentFoot = FootRight.transform;
			m_OtherFoot = FootLeft.transform;
			m_FlipFootprint = true;
			m_CurrentAudioSource = AudioRight;
			TryTriggerStep(ref m_RightWasAbove);
		}
	}

	protected virtual void UpdateFakeStepWithFoot()
	{
		if (!(FootLeft == null) && !(FootRight == null))
		{
			if (m_FlipFootprint)
			{
				m_CurrentAudioSource = AudioLeft;
				m_CurrentFoot = FootLeft.transform;
			}
			else
			{
				m_CurrentAudioSource = AudioRight;
				m_CurrentFoot = FootRight.transform;
			}
			m_FlipFootprint = !m_FlipFootprint;
			TryStep();
		}
	}

	protected virtual void UpdateFakeStepWithoutFoot()
	{
		if (!(m_CurrentFoot == null))
		{
			m_FlipFootprint = !m_FlipFootprint;
			TryStep();
		}
	}

	protected virtual void TryTriggerStep(ref bool wasAbove)
	{
		if (m_CurrentFoot != null && m_CurrentFoot.transform.position.y > CurrentTriggerHeight)
		{
			wasAbove = true;
		}
		else if (!(m_OtherFoot != null) || !(m_OtherFoot.transform.position.y < CurrentTriggerHeight))
		{
			if (wasAbove)
			{
				TryStep();
			}
			wasAbove = false;
		}
	}

	public virtual void TryStep()
	{
		if (!(Player == null) && !Player.Dead.Active && Player.Grounded.Get() && !Player.OutOfControl.Active && (Mode != 3 || IsMoving || !(Player.Platform.Get() != null)) && (!IsMoving || !(FPController != null) || FPController.MotorAcceleration != 0f) && Physics.Raycast(new Ray(m_CurrentFoot.transform.position + Vector3.up * 0.5f, Vector3.down), out m_Hit, 1f, vp_Layer.Mask.ExternalBlockers))
		{
			DoStep();
		}
	}

	// Added by the studio after the PC build. TODO(ARM): verify where 8.3.0 raises it.
	public event Action StepPerformed;

	protected virtual void DoStep()
	{
		if (m_Mute)
		{
			return;
		}
		StepPerformed?.Invoke();
		if (FootstepImpactEvent != null)
		{
			vp_SurfaceType surfaceType = vp_SurfaceManager.GetSurfaceType(m_Hit);
			if (!IsMoving || surfaceType == null || !surfaceType.CanHaveFootprints())
			{
				MasterAudio.PlaySound3DAtTransform("Footsteps", base.transform);
				Animator.SetFloat("MoveSpeed", 3f);
			}
			else
			{
				vp_SurfaceManager.SpawnFootprintEffect(m_Hit, surfaceType, FootstepImpactEvent, m_CurrentFoot.transform.forward, m_FlipFootprint, VerifyGroundContact, m_CurrentAudioSource);
			}
		}
		if (Mode == 2)
		{
			if (!IsMoving && IsRotating)
			{
				m_NextAllowedFootstepTime = Time.time + TimeInterval * UnityEngine.Random.Range(1f, 2f);
			}
			else
			{
				m_NextAllowedFootstepTime = Time.time + TimeInterval / Mathf.Max(1f, 1f);
			}
		}
		m_PrevYaw = base.transform.eulerAngles.y;
	}

	public virtual void OnDrawGizmosSelected()
	{
		if (base.enabled && Mode == 1)
		{
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.color = m_SelectedGizmoColor;
			Gizmos.DrawCube(Vector3.up * (TriggerHeight * (Application.isPlaying ? m_CurrentLocalTriggerHeight : 1f)), GizmoScale);
			Gizmos.color = new Color(0f, 0f, 0f, 1f);
			Gizmos.DrawLine(Vector3.zero, Vector3.forward);
		}
	}

	protected virtual void OnApplicationFocus(bool focusStatus)
	{
		if (MuteLocalFootsteps)
		{
			m_Mute = focusStatus;
		}
	}

	protected virtual void OnMessage_FallImpact(float impact)
	{
		if (!(impact < FallImpactThreshold) && Physics.Raycast(new Ray(base.transform.position + Vector3.up * 0.5f, Vector3.down), out var hitInfo, 1f, vp_Layer.Mask.ExternalBlockers))
		{
			JumpFallAudio.Stop();
			vp_SurfaceManager.SpawnEffect(hitInfo, FallImpactEvent, JumpFallAudio);
			m_NextAllowedJumpSoundTime = Time.time + 0.5f;
		}
	}

	protected virtual void OnStart_Jump()
	{
		if (!(Time.time < m_NextAllowedJumpSoundTime) && Physics.Raycast(new Ray(base.transform.position + Vector3.up * 0.5f, Vector3.down), out var hitInfo, 1f, vp_Layer.Mask.ExternalBlockers))
		{
			vp_SurfaceManager.SpawnEffect(hitInfo, JumpImpactEvent, JumpFallAudio);
		}
	}
}
