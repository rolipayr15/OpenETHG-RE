using UnityEngine;

[RequireComponent(typeof(vp_FPPlayerEventHandler))]
public class vp_FPPlayerDamageHandler : vp_PlayerDamageHandler
{
	public float CameraShakeFactor = 0.02f;

	protected float m_DamageAngle;

	protected float m_DamageAngleFactor = 1f;

	protected vp_FPPlayerEventHandler m_FPPlayer;

	protected vp_FPCamera m_FPCamera;

	protected CharacterController m_CharacterController;

	protected vp_FPPlayerEventHandler FPPlayer
	{
		get
		{
			if (m_FPPlayer == null)
			{
				m_FPPlayer = base.transform.GetComponent<vp_FPPlayerEventHandler>();
			}
			return m_FPPlayer;
		}
	}

	protected vp_FPCamera FPCamera
	{
		get
		{
			if (m_FPCamera == null)
			{
				m_FPCamera = base.transform.GetComponentInChildren<vp_FPCamera>();
			}
			return m_FPCamera;
		}
	}

	protected CharacterController CharacterController
	{
		get
		{
			if (m_CharacterController == null)
			{
				m_CharacterController = base.transform.root.GetComponentInChildren<CharacterController>();
			}
			return m_CharacterController;
		}
	}

	protected override void OnEnable()
	{
		if (FPPlayer != null)
		{
			FPPlayer.Register(this);
		}
		RefreshColliders();
	}

	protected override void OnDisable()
	{
		if (FPPlayer != null)
		{
			FPPlayer.Unregister(this);
		}
	}

	protected virtual void Update()
	{
		if (FPPlayer.Dead.Active && Time.timeScale < 1f)
		{
			vp_TimeUtility.FadeTimeScale(1f, 0.05f);
		}
	}

	public override void Damage(float damage)
	{
		if (base.enabled && vp_Utility.IsActive(base.gameObject))
		{
			base.Damage(damage);
			FPPlayer.HUDDamageFlash.Send(new vp_DamageInfo(damage, null));
			FPPlayer.HeadImpact.Send((Random.value < 0.5f) ? (damage * CameraShakeFactor) : (0f - damage * CameraShakeFactor));
		}
	}

	public override void Damage(vp_DamageInfo damageInfo)
	{
		if (base.enabled && vp_Utility.IsActive(base.gameObject))
		{
			base.Damage(damageInfo);
			FPPlayer.HUDDamageFlash.Send(damageInfo);
			if (damageInfo.Source != null)
			{
				m_DamageAngle = vp_3DUtility.LookAtAngleHorizontal(FPCamera.Transform.position, FPCamera.Transform.forward, damageInfo.Source.position);
				m_DamageAngleFactor = ((Mathf.Abs(m_DamageAngle) > 30f) ? 1f : Mathf.Lerp(0f, 1f, Mathf.Abs(m_DamageAngle) * 0.033f));
				FPPlayer.HeadImpact.Send(damageInfo.Damage * CameraShakeFactor * m_DamageAngleFactor * (float)((m_DamageAngle < 0f) ? 1 : (-1)));
			}
		}
	}

	public override void Die()
	{
		base.Die();
		if (base.enabled && vp_Utility.IsActive(base.gameObject))
		{
			FPPlayer.InputAllowGameplay.Set(o: false);
		}
	}

	public virtual void RefreshColliders()
	{
		if (!(CharacterController != null) || !CharacterController.enabled)
		{
			return;
		}
		foreach (Collider collider in base.Colliders)
		{
			if (collider.enabled)
			{
				Physics.IgnoreCollision(CharacterController, collider, ignore: true);
			}
		}
	}

	protected override void Reset()
	{
		base.Reset();
		if (Application.isPlaying)
		{
			FPPlayer.InputAllowGameplay.Set(o: true);
			FPPlayer.HUDDamageFlash.Send(null);
			RefreshColliders();
		}
	}

	private void OnStart_Crouch()
	{
		RefreshColliders();
	}

	private void OnStop_Crouch()
	{
		RefreshColliders();
	}
}
