using System;
using System.Collections.Generic;
using UnityEngine;

public class vp_RigidbodyFX : MonoBehaviour
{
	[Serializable]
	public struct SurfaceFXInfo
	{
		public vp_SurfaceType SurfaceType;

		public AudioClip Sound;

		public SurfaceFXInfo(bool init)
		{
			SurfaceType = null;
			Sound = null;
		}
	}

	public vp_ImpactEvent ImpactEvent;

	[Range(1f, 100f)]
	public float ImpactThreshold = 1f;

	[Range(1f, 100f)]
	public float MinCameraDistance = 20f;

	[SerializeField]
	public List<SurfaceFXInfo> CollisionSounds = new List<SurfaceFXInfo>();

	protected static float nextAllowedPlayEffectTime;

	protected bool m_HaveRigidbody;

	protected bool m_HaveObjectSounds;

	protected bool m_HaveSurfaceManager;

	protected Rigidbody m_RigidBody;

	protected const float STARTUP_MUTE_DELAY = 2f;

	protected const float SURFACE_DETECTION_RAYCAST_DISTANCE = 0.4f;

	protected const float MIN_GLOBAL_FX_INTERVAL = 0.2f;

	protected AudioSource m_Audio;

	protected AudioSource m_Audio2;

	public Rigidbody Rigidbody
	{
		get
		{
			if (m_RigidBody == null)
			{
				m_RigidBody = GetComponent<Rigidbody>();
				if (m_RigidBody != null)
				{
					m_HaveRigidbody = true;
				}
			}
			return m_RigidBody;
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
			if (m_Audio == null)
			{
				m_Audio = base.gameObject.AddComponent<AudioSource>();
			}
			return m_Audio;
		}
	}

	protected AudioSource Audio2
	{
		get
		{
			if (m_Audio2 == null)
			{
				m_Audio2 = base.gameObject.AddComponent<AudioSource>();
				m_Audio2.rolloffMode = Audio.rolloffMode;
				m_Audio2.minDistance = Audio.minDistance;
				m_Audio2.maxDistance = Audio.maxDistance;
				m_Audio2.spatialBlend = 1f;
			}
			return m_Audio2;
		}
	}

	public bool CanPlayFX
	{
		get
		{
			if (Time.realtimeSinceStartup < 2f)
			{
				return false;
			}
			if (Time.time < nextAllowedPlayEffectTime)
			{
				return false;
			}
			if (Camera.main != null && Vector3.Distance(Camera.main.transform.position, base.transform.position) > MinCameraDistance)
			{
				return false;
			}
			return true;
		}
	}

	protected virtual void Start()
	{
		_ = (bool)Rigidbody;
		if (CollisionSounds != null && CollisionSounds.Count > 0)
		{
			for (int num = CollisionSounds.Count - 1; num > -1; num--)
			{
				if (CollisionSounds[num].SurfaceType == null || CollisionSounds[num].Sound == null)
				{
					CollisionSounds.RemoveAt(num);
				}
			}
			if (CollisionSounds.Count > 0)
			{
				m_HaveObjectSounds = true;
			}
		}
		m_HaveSurfaceManager = vp_SurfaceManager.Instance != null;
	}

	protected virtual void OnCollisionEnter(Collision collision)
	{
		if (CanPlayFX && m_HaveRigidbody && !Rigidbody.IsSleeping() && !(collision.relativeVelocity.sqrMagnitude < ImpactThreshold))
		{
			RaycastHit hitInfo = default(RaycastHit);
			Vector3 normalized = (collision.contacts[0].point - base.transform.position).normalized;
			if (Physics.Raycast(new Ray(collision.contacts[0].point - normalized * 0.2f, normalized), out hitInfo, 0.4f, vp_Layer.Mask.ExternalBlockers))
			{
				TryPlayFX(hitInfo);
			}
		}
	}

	public bool TryPlayFX(RaycastHit hit)
	{
		if (!CanPlayFX)
		{
			return false;
		}
		nextAllowedPlayEffectTime = Time.time + 0.2f;
		bool isPlaying = Audio.isPlaying;
		vp_SurfaceManager.SpawnEffect(hit, ImpactEvent, (!isPlaying) ? Audio : null);
		if (!m_HaveObjectSounds)
		{
			if (m_HaveSurfaceManager && isPlaying)
			{
				if (ImpactEvent == null)
				{
					vp_SurfaceManager.SpawnEffect(hit, vp_SurfaceManager.Instance.Fallbacks.ImpactEvent, Audio2);
				}
				else
				{
					vp_SurfaceManager.SpawnEffect(hit, ImpactEvent, Audio2);
				}
			}
			return false;
		}
		TryPlaySound(vp_SurfaceManager.GetSurfaceType(hit));
		return true;
	}

	public bool TryPlaySound(vp_SurfaceType surfaceType)
	{
		if (surfaceType == null)
		{
			return false;
		}
		if (Audio2 == null)
		{
			return false;
		}
		if (Vector3.Distance(base.transform.position, Camera.main.transform.position) > Audio2.maxDistance)
		{
			return false;
		}
		if (!vp_Utility.IsActive(base.gameObject))
		{
			return false;
		}
		bool flag = false;
		int num = -1;
		for (int num2 = CollisionSounds.Count - 1; num2 > -1; num2--)
		{
			if (surfaceType == CollisionSounds[num2].SurfaceType)
			{
				DoPlaySound(CollisionSounds[num2].Sound);
				flag = true;
			}
			else if (m_HaveSurfaceManager && CollisionSounds[num2].SurfaceType == vp_SurfaceManager.Instance.Fallbacks.SurfaceType)
			{
				num = num2;
			}
		}
		if (!flag && num >= 0)
		{
			DoPlaySound(CollisionSounds[num].Sound);
		}
		return true;
	}

	private void DoPlaySound(AudioClip sound)
	{
		if (!(sound == null))
		{
			Audio2.pitch = vp_TimeUtility.AdjustedTimeScale;
			Audio2.clip = sound;
			Audio2.Stop();
			Audio2.Play();
		}
	}
}
