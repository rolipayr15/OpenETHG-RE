using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class vp_Bullet : MonoBehaviour
{
	public enum HitScanType
	{
		Raycast,
		Linecast
	}

	public float Range = 100f;

	public float Force = 100f;

	public float Damage = 1f;

	public vp_DamageInfo.DamageMode DamageMode = vp_DamageInfo.DamageMode.DamageHandler;

	public string DamageMethodName = "Damage";

	public HitScanType ScanType;

	protected Transform m_Transform;

	protected Renderer m_Renderer;

	protected AudioSource m_Audio;

	protected bool m_Initialized;

	protected Transform m_Source;

	protected static vp_DamageHandler m_TargetDHandler;

	protected Ray m_Ray;

	protected RaycastHit m_Hit;

	protected int LayerMask = vp_Layer.Mask.IgnoreWalkThru;

	[Obsolete("Please use 'DamageMode instead.")]
	public bool RequireDamageHandler
	{
		get
		{
			if (DamageMode != vp_DamageInfo.DamageMode.DamageHandler)
			{
				return DamageMode == vp_DamageInfo.DamageMode.Both;
			}
			return true;
		}
		set
		{
			DamageMode = (value ? vp_DamageInfo.DamageMode.DamageHandler : vp_DamageInfo.DamageMode.UnityMessage);
		}
	}

	protected virtual void Awake()
	{
		m_Transform = base.transform;
		m_Renderer = GetComponent<Renderer>();
		m_Audio = GetComponent<AudioSource>();
	}

	protected virtual void Start()
	{
		m_Initialized = true;
		StartCoroutine(TryHitOnEndOfFrame());
	}

	protected virtual void OnEnable()
	{
		if (m_Initialized)
		{
			StartCoroutine(TryHitOnEndOfFrame());
		}
	}

	protected virtual bool TryHit()
	{
		m_Ray = new Ray(m_Transform.position, m_Transform.forward);
		if (m_Source != null && m_Source.gameObject.layer == 30)
		{
			LayerMask = vp_Layer.Mask.BulletBlockers;
		}
		else
		{
			LayerMask = vp_Layer.Mask.IgnoreWalkThru;
		}
		switch (ScanType)
		{
		case HitScanType.Raycast:
			if (!Physics.Raycast(m_Ray, out m_Hit, Range, LayerMask))
			{
				return false;
			}
			break;
		case HitScanType.Linecast:
			if (!Physics.Linecast(m_Transform.position, m_Transform.position + m_Transform.forward * Range, out m_Hit, LayerMask))
			{
				return false;
			}
			break;
		}
		DoHit();
		return true;
	}

	protected virtual void DoHit()
	{
		TrySpawnFX();
		TryPlaySound();
		TryAddForce();
		TryDamage();
		TryDestroy();
	}

	protected virtual void TrySpawnFX()
	{
		m_Transform.position = m_Hit.point;
		m_Transform.rotation = Quaternion.LookRotation(m_Hit.normal);
	}

	protected virtual void TryPlaySound()
	{
		if (!(m_Audio == null) && !(m_Audio.clip == null))
		{
			m_Audio.pitch = Time.timeScale;
			m_Audio.Stop();
			m_Audio.Play();
		}
	}

	protected virtual void TryAddForce()
	{
		Rigidbody attachedRigidbody = m_Hit.collider.attachedRigidbody;
		if (!(attachedRigidbody == null) && !attachedRigidbody.isKinematic)
		{
			attachedRigidbody.AddForceAtPosition(m_Ray.direction * Force / Time.timeScale / vp_TimeUtility.AdjustedTimeScale, m_Hit.point);
		}
	}

	protected virtual void TryDamage()
	{
		if (DamageMode == vp_DamageInfo.DamageMode.UnityMessage || DamageMode == vp_DamageInfo.DamageMode.Both)
		{
			m_Hit.collider.SendMessage(DamageMethodName, Damage, SendMessageOptions.DontRequireReceiver);
		}
		if (DamageMode == vp_DamageInfo.DamageMode.DamageHandler || DamageMode == vp_DamageInfo.DamageMode.Both)
		{
			m_TargetDHandler = vp_DamageHandler.GetDamageHandlerOfCollider(m_Hit.collider);
			if (m_TargetDHandler != null)
			{
				DoUFPSDamage();
			}
		}
	}

	protected virtual void DoUFPSDamage()
	{
		m_TargetDHandler.Damage(new vp_DamageInfo(Damage, m_Source));
	}

	protected virtual void TryDestroy()
	{
		if (this == null || (m_Renderer != null && m_Renderer.enabled))
		{
			return;
		}
		if (m_Audio != null && m_Audio.isPlaying)
		{
			vp_Timer.In(1f, TryDestroy);
			return;
		}
		if (m_Renderer != null)
		{
			m_Renderer.enabled = true;
		}
		vp_Utility.Destroy(base.gameObject);
	}

	protected IEnumerator TryHitOnEndOfFrame()
	{
		yield return new WaitForEndOfFrame();
		if (!TryHit())
		{
			StartCoroutine(DestroyOnNextFrame());
		}
	}

	protected IEnumerator DestroyOnNextFrame()
	{
		yield return 0;
		vp_Utility.Destroy(base.gameObject);
	}

	public virtual void SetSource(Transform source)
	{
		m_Source = source;
	}
}
