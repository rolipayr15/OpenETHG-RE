using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class vp_HitscanBullet : vp_Bullet
{
	public float m_SparkFactor = 0.5f;

	public GameObject m_ImpactPrefab;

	public GameObject m_DustPrefab;

	public GameObject m_SparkPrefab;

	public GameObject m_DebrisPrefab;

	public List<AudioClip> m_ImpactSounds = new List<AudioClip>();

	public Vector2 SoundImpactPitch = new Vector2(1f, 1.5f);

	public int[] NoDecalOnTheseLayers;

	protected static bool m_DidWarn;

	protected override void Start()
	{
		if (!m_DidWarn)
		{
			Debug.LogWarning("Warning (" + this?.ToString() + ") This script is obsolete. Please use 'vp_FXBullet' or 'vp_Bullet' instead!");
			m_DidWarn = true;
		}
		base.Start();
	}

	protected override void TryPlaySound()
	{
		if (m_ImpactSounds.Count >= 1 && !(m_Audio == null))
		{
			m_Audio.clip = m_ImpactSounds[UnityEngine.Random.Range(0, m_ImpactSounds.Count)];
			if (!(m_Audio.clip == null))
			{
				m_Audio.pitch = UnityEngine.Random.Range(SoundImpactPitch.x, SoundImpactPitch.y) * Time.timeScale;
				m_Audio.Stop();
				m_Audio.Play();
			}
		}
	}

	protected override void TrySpawnFX()
	{
		Vector3 localScale = m_Transform.localScale;
		m_Transform.parent = m_Hit.transform;
		m_Transform.localPosition = m_Hit.transform.InverseTransformPoint(m_Hit.point);
		m_Transform.rotation = Quaternion.LookRotation(m_Hit.normal);
		if (m_Hit.transform.lossyScale == Vector3.one)
		{
			m_Transform.Rotate(Vector3.forward, UnityEngine.Random.Range(0, 360), Space.Self);
		}
		else
		{
			m_Transform.parent = null;
			m_Transform.localScale = localScale;
			m_Transform.parent = m_Hit.transform;
		}
		if (m_Renderer != null && NoDecalOnTheseLayers.Length != 0)
		{
			int[] noDecalOnTheseLayers = NoDecalOnTheseLayers;
			foreach (int num in noDecalOnTheseLayers)
			{
				if (m_Hit.transform.gameObject.layer == num)
				{
					m_Renderer.enabled = false;
					TryDestroy();
				}
			}
		}
		if (m_ImpactPrefab != null)
		{
			vp_Utility.Instantiate(m_ImpactPrefab, m_Transform.position, m_Transform.rotation);
		}
		if (m_DustPrefab != null)
		{
			vp_Utility.Instantiate(m_DustPrefab, m_Transform.position, m_Transform.rotation);
		}
		if (m_SparkPrefab != null && UnityEngine.Random.value < m_SparkFactor)
		{
			vp_Utility.Instantiate(m_SparkPrefab, m_Transform.position, m_Transform.rotation);
		}
		if (m_DebrisPrefab != null)
		{
			vp_Utility.Instantiate(m_DebrisPrefab, m_Transform.position, m_Transform.rotation);
		}
	}

	protected override void DoUFPSDamage()
	{
		vp_Bullet.m_TargetDHandler.Damage(new vp_DamageInfo(Damage, m_Source, vp_DamageInfo.DamageType.Bullet));
	}

	protected override void TryDestroy()
	{
		if (m_Renderer != null && m_Renderer.enabled)
		{
			vp_DecalManager.Add(base.gameObject);
		}
		else
		{
			base.TryDestroy();
		}
	}

	[Obsolete("Please use 'SetSource' instead.")]
	public void SetSender(Transform sender)
	{
		SetSource(sender);
	}
}
