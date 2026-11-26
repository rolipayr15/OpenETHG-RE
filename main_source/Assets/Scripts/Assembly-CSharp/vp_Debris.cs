using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class vp_Debris : MonoBehaviour
{
	public float Radius = 2f;

	public float Force = 10f;

	public float UpForce = 1f;

	public List<AudioClip> Sounds = new List<AudioClip>();

	public float SoundMinPitch = 0.8f;

	public float SoundMaxPitch = 1.2f;

	private AudioSource m_Audio;

	public float LifeTime = 5f;

	protected bool m_Destroy;

	protected Collider[] m_Colliders;

	protected Dictionary<Collider, Dictionary<string, object>> m_PiecesInitial = new Dictionary<Collider, Dictionary<string, object>>();

	private AudioSource Audio
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

	private void Awake()
	{
		m_Colliders = GetComponentsInChildren<Collider>();
		Collider[] colliders = m_Colliders;
		foreach (Collider collider in colliders)
		{
			if ((bool)collider.GetComponent<Rigidbody>())
			{
				m_PiecesInitial.Add(collider, new Dictionary<string, object>
				{
					{
						"Position",
						collider.transform.localPosition
					},
					{
						"Rotation",
						collider.transform.localRotation
					}
				});
			}
		}
	}

	private void OnEnable()
	{
		m_Destroy = false;
		if (Audio != null)
		{
			Audio.playOnAwake = true;
		}
		Collider[] colliders = m_Colliders;
		foreach (Collider collider in colliders)
		{
			Rigidbody component = collider.GetComponent<Rigidbody>();
			if (!(component != null))
			{
				continue;
			}
			collider.transform.localPosition = (Vector3)m_PiecesInitial[collider]["Position"];
			collider.transform.localRotation = (Quaternion)m_PiecesInitial[collider]["Rotation"];
			component.linearVelocity = Vector3.zero;
			component.angularVelocity = Vector3.zero;
			component.AddExplosionForce(Force / Time.timeScale / vp_TimeUtility.AdjustedTimeScale, base.transform.position, Radius, UpForce);
			Collider c = collider;
			vp_Timer.In(Random.Range(LifeTime * 0.5f, LifeTime * 0.95f), delegate
			{
				if (c != null)
				{
					vp_Utility.Activate(c.gameObject, activate: false);
				}
			});
		}
		vp_Timer.In(LifeTime, delegate
		{
			m_Destroy = true;
		});
		if (Audio != null && Sounds.Count > 0)
		{
			Audio.rolloffMode = AudioRolloffMode.Linear;
			Audio.clip = Sounds[Random.Range(0, Sounds.Count)];
			Audio.pitch = Random.Range(SoundMinPitch, SoundMaxPitch) * Time.timeScale;
			Audio.Play();
		}
	}

	private void Update()
	{
		if (m_Destroy && Audio != null && !Audio.isPlaying)
		{
			Collider[] colliders = m_Colliders;
			for (int i = 0; i < colliders.Length; i++)
			{
				vp_Utility.Activate(colliders[i].gameObject);
			}
			vp_Utility.Destroy(base.gameObject);
		}
	}
}
