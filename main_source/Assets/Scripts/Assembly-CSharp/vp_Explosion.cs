using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class vp_Explosion : MonoBehaviour
{
	public float Radius = 15f;

	public float Force = 1000f;

	public float UpForce = 10f;

	public float Damage = 10f;

	public bool AllowCover;

	public bool AllowMultiDamage;

	public float CameraShake = 1f;

	public vp_DamageInfo.DamageMode DamageMode = vp_DamageInfo.DamageMode.DamageHandler;

	public string DamageMessageName = "Damage";

	protected bool m_HaveExploded;

	public AudioClip Sound;

	public float SoundMinPitch = 0.8f;

	public float SoundMaxPitch = 1.2f;

	public List<GameObject> FXPrefabs = new List<GameObject>();

	protected Ray m_Ray;

	protected RaycastHit m_RaycastHit;

	protected Collider m_TargetCollider;

	protected Transform m_TargetTransform;

	protected Rigidbody m_TargetRigidbody;

	protected float m_DistanceModifier;

	protected Dictionary<Transform, object> m_RootTransformsHitByThisExplosion = new Dictionary<Transform, object>(50);

	protected static vp_DamageHandler m_TargetDHandler;

	protected Transform m_Transform;

	protected Transform m_Source;

	protected Transform m_OriginalSource;

	protected AudioSource m_Audio;

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

	protected float DistanceModifier
	{
		get
		{
			if (m_DistanceModifier == 0f)
			{
				m_DistanceModifier = 1f - Vector3.Distance(Transform.position, m_TargetTransform.position) / Radius;
			}
			return m_DistanceModifier;
		}
	}

	protected Transform Transform
	{
		get
		{
			if (m_Transform == null)
			{
				m_Transform = base.transform;
			}
			return m_Transform;
		}
	}

	protected Transform Source
	{
		get
		{
			if (m_Source == null)
			{
				m_Source = base.transform;
			}
			return m_Source;
		}
		set
		{
			m_Source = value;
		}
	}

	protected Transform OriginalSource
	{
		get
		{
			if (m_OriginalSource == null)
			{
				m_OriginalSource = base.transform;
			}
			return m_OriginalSource;
		}
		set
		{
			m_OriginalSource = value;
		}
	}

	protected AudioSource Audio
	{
		get
		{
			if (m_Audio == null)
			{
				m_Audio = GetComponent<AudioSource>();
			}
			return m_Audio;
		}
	}

	protected virtual void OnEnable()
	{
		Source = base.transform;
		OriginalSource = null;
		vp_TargetEvent<Transform>.Register(base.transform, "SetSource", SetSource);
	}

	protected virtual void OnDisable()
	{
		Source = null;
		OriginalSource = null;
		vp_TargetEvent<Transform>.Unregister(base.transform, "SetSource", SetSource);
	}

	private void Update()
	{
		if (m_HaveExploded)
		{
			if (!Audio.isPlaying)
			{
				m_HaveExploded = false;
				m_RootTransformsHitByThisExplosion.Clear();
				vp_Utility.Destroy(base.gameObject);
			}
		}
		else
		{
			DoExplode();
		}
	}

	private void DoExplode()
	{
		m_HaveExploded = true;
		foreach (GameObject fXPrefab in FXPrefabs)
		{
			if (fXPrefab != null)
			{
				vp_Utility.Instantiate(fXPrefab, Transform.position, Transform.rotation);
			}
		}
		Collider[] array = Physics.OverlapSphere(Transform.position, Radius, vp_Layer.Mask.IgnoreWalkThru);
		foreach (Collider collider in array)
		{
			if (collider.gameObject.isStatic)
			{
				continue;
			}
			m_DistanceModifier = 0f;
			if (!(collider != null) || !(collider.transform.root != base.transform.root))
			{
				continue;
			}
			m_TargetCollider = collider;
			m_TargetTransform = collider.transform;
			AddUFPSCameraShake();
			if (TargetInCover())
			{
				continue;
			}
			m_TargetRigidbody = collider.GetComponent<Rigidbody>();
			if (m_TargetRigidbody != null)
			{
				AddRigidbodyForce();
			}
			else
			{
				AddUFPSForce();
			}
			if (!AllowMultiDamage)
			{
				if (!m_RootTransformsHitByThisExplosion.ContainsKey(m_TargetCollider.transform.root))
				{
					m_RootTransformsHitByThisExplosion.Add(m_TargetCollider.transform.root, null);
					TryDamage();
				}
			}
			else
			{
				TryDamage();
			}
		}
		Audio.clip = Sound;
		Audio.pitch = UnityEngine.Random.Range(SoundMinPitch, SoundMaxPitch) * Time.timeScale;
		if (!Audio.playOnAwake)
		{
			Audio.Play();
		}
	}

	protected virtual void TryDamage()
	{
		if (DamageMode == vp_DamageInfo.DamageMode.UnityMessage || DamageMode == vp_DamageInfo.DamageMode.Both)
		{
			m_TargetCollider.gameObject.BroadcastMessage(DamageMessageName, DistanceModifier * Damage, SendMessageOptions.DontRequireReceiver);
		}
		if (DamageMode == vp_DamageInfo.DamageMode.DamageHandler || DamageMode == vp_DamageInfo.DamageMode.Both)
		{
			m_TargetDHandler = vp_DamageHandler.GetDamageHandlerOfCollider(m_TargetCollider);
			if (m_TargetDHandler != null)
			{
				m_TargetDHandler.Damage(new vp_DamageInfo(DistanceModifier * Damage, Source, OriginalSource, vp_DamageInfo.DamageType.Explosion));
			}
		}
	}

	protected virtual bool TargetInCover()
	{
		if (!AllowCover)
		{
			return false;
		}
		m_Ray.origin = Transform.position;
		m_Ray.direction = (m_TargetCollider.bounds.center - Transform.position).normalized;
		if (Physics.Raycast(m_Ray, out m_RaycastHit, Radius + 1f) && m_RaycastHit.transform.root.GetComponent<Collider>() == m_TargetCollider)
		{
			return false;
		}
		m_Ray.direction = (vp_3DUtility.HorizontalVector(m_TargetCollider.bounds.center) + Vector3.up * m_TargetCollider.bounds.max.y - Transform.position).normalized;
		if (Physics.Raycast(m_Ray, out m_RaycastHit, Radius + 1f) && m_RaycastHit.transform.root.GetComponent<Collider>() == m_TargetCollider)
		{
			return false;
		}
		return true;
	}

	protected virtual void AddRigidbodyForce()
	{
		if (!m_TargetRigidbody.isKinematic)
		{
			m_Ray.origin = m_TargetTransform.position;
			m_Ray.direction = -Vector3.up;
			if (!Physics.Raycast(m_Ray, out m_RaycastHit, 1f))
			{
				UpForce = 0f;
			}
			m_TargetRigidbody.AddExplosionForce(Force / Time.timeScale / vp_TimeUtility.AdjustedTimeScale, Transform.position, Radius, UpForce);
		}
	}

	protected virtual void AddUFPSForce()
	{
		vp_TargetEvent<Vector3>.Send(m_TargetTransform.root, "ForceImpact", (m_TargetTransform.position - Transform.position).normalized * Force * 0.001f * DistanceModifier);
	}

	protected virtual void AddUFPSCameraShake()
	{
		vp_TargetEvent<float>.Send(m_TargetTransform.root, "CameraBombShake", DistanceModifier * CameraShake);
	}

	public void SetSource(Transform source)
	{
		m_OriginalSource = source;
	}
}
