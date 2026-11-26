using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class vp_Effect : ScriptableObject
{
	[Serializable]
	public struct ObjectSection
	{
		public List<ObjectSpawnInfo> m_Objects;
	}

	[Serializable]
	public struct SoundSection
	{
		public List<AudioClip> m_Sounds;

		[Range(-2f, 1f)]
		public float MinPitch;

		[Range(1f, 2f)]
		public float MaxPitch;

		public bool MaxOnePerFrame;
	}

	[Serializable]
	public struct ObjectSpawnInfo
	{
		public GameObject Prefab;

		[Range(0f, 1f)]
		public float Probability;

		public ObjectSpawnInfo(bool init)
		{
			Prefab = null;
			Probability = 1f;
		}
	}

	public SoundSection Sound;

	public ObjectSection Objects;

	protected AudioClip m_SoundToPlay;

	protected AudioClip m_LastPlayedSound;

	protected int m_LastPlayFrame;

	public virtual void Init()
	{
		Sound.MinPitch = 1f;
		Sound.MaxPitch = 1f;
		Objects.m_Objects = new List<ObjectSpawnInfo>();
		Objects.m_Objects.Add(new ObjectSpawnInfo(init: true));
	}

	public virtual void Spawn(RaycastHit hit, AudioSource audioSource = null)
	{
		SpawnObjects(hit);
		PlaySound(audioSource);
	}

	private void SpawnObjects(RaycastHit hit)
	{
		for (int i = 0; i < Objects.m_Objects.Count; i++)
		{
			if (!(Objects.m_Objects[i].Prefab == null) && UnityEngine.Random.value < Objects.m_Objects[i].Probability)
			{
				vp_Utility.Instantiate(Objects.m_Objects[i].Prefab, hit.point, Quaternion.LookRotation(hit.normal));
			}
		}
	}

	public virtual void PlaySound(AudioSource audioSource)
	{
		if (audioSource == null)
		{
			return;
		}
		if ((!Sound.MaxOnePerFrame || m_LastPlayFrame != Time.frameCount) && vp_Utility.IsActive(audioSource.gameObject) && Sound.m_Sounds != null && Sound.m_Sounds.Count != 0 && (!(Camera.main != null) || !(Vector3.Distance(audioSource.transform.position, Camera.main.transform.position) > audioSource.maxDistance)))
		{
			while (true)
			{
				m_SoundToPlay = Sound.m_Sounds[UnityEngine.Random.Range(0, Sound.m_Sounds.Count)];
				if (m_SoundToPlay == null)
				{
					break;
				}
				if (Sound.m_Sounds.Count <= 1 || !(m_SoundToPlay == m_LastPlayedSound))
				{
					if (Sound.MinPitch == Sound.MaxPitch)
					{
						audioSource.pitch = Sound.MinPitch * Time.timeScale;
					}
					else
					{
						audioSource.pitch = UnityEngine.Random.Range(Sound.MinPitch, Sound.MaxPitch) * Time.timeScale;
					}
					audioSource.clip = m_SoundToPlay;
					if (!audioSource.enabled)
					{
						break;
					}
					audioSource.Play();
					m_LastPlayedSound = m_SoundToPlay;
					m_LastPlayFrame = Time.frameCount;
					return;
				}
			}
		}
		audioSource.clip = null;
	}
}
