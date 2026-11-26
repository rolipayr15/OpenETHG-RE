using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class vp_Respawner : MonoBehaviour
{
	public enum SpawnMode
	{
		SamePosition,
		SpawnPoint
	}

	public enum ObstructionSolver
	{
		Wait,
		AdjustPlacement
	}

	public SpawnMode m_SpawnMode;

	public string SpawnPointTag = "";

	public ObstructionSolver m_ObstructionSolver;

	public float ObstructionRadius = 1f;

	public float MinRespawnTime = 3f;

	public float MaxRespawnTime = 3f;

	public float LastRespawnTime;

	public bool SpawnOnAwake;

	public AudioClip SpawnSound;

	public GameObject[] SpawnFXPrefabs;

	protected Vector3 m_InitialPosition = Vector3.zero;

	protected Quaternion m_InitialRotation;

	protected vp_Placement Placement = new vp_Placement();

	protected bool m_IsInitialSpawnOnAwake;

	protected vp_Timer.Handle m_RespawnTimer = new vp_Timer.Handle();

	protected static Dictionary<Collider, vp_Respawner> m_Instances;

	protected static vp_Respawner m_GetInstanceResult;

	protected Renderer m_Renderer;

	protected Transform m_Transform;

	protected AudioSource m_Audio;

	protected Collider m_Collider;

	protected Rigidbody m_Rigidbody;

	public static Dictionary<Collider, vp_Respawner> Instances
	{
		get
		{
			if (m_Instances == null)
			{
				m_Instances = new Dictionary<Collider, vp_Respawner>(100);
			}
			return m_Instances;
		}
	}

	public Renderer Renderer
	{
		get
		{
			if (m_Renderer == null)
			{
				m_Renderer = GetComponent<Renderer>();
			}
			return m_Renderer;
		}
	}

	public Transform Transform
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

	protected AudioSource Audio
	{
		get
		{
			if (m_Audio == null)
			{
				m_Audio = GetComponent<AudioSource>();
				if (m_Audio == null)
				{
					m_Audio = base.gameObject.AddComponent<AudioSource>();
				}
			}
			return m_Audio;
		}
	}

	protected Collider Collider
	{
		get
		{
			if (m_Collider == null)
			{
				m_Collider = GetComponent<Collider>();
			}
			return m_Collider;
		}
	}

	protected Rigidbody Rigidbody
	{
		get
		{
			if (m_Rigidbody == null)
			{
				m_Rigidbody = GetComponent<Rigidbody>();
			}
			return m_Rigidbody;
		}
	}

	protected virtual void Awake()
	{
		Placement.Position = (m_InitialPosition = Transform.position);
		Placement.Rotation = (m_InitialRotation = Transform.rotation);
		if (m_SpawnMode == SpawnMode.SamePosition)
		{
			SpawnPointTag = "";
		}
		if (SpawnOnAwake)
		{
			m_IsInitialSpawnOnAwake = true;
			vp_Utility.Activate(base.gameObject, activate: false);
			PickSpawnPoint();
		}
	}

	protected virtual void OnEnable()
	{
		SceneManager.sceneLoaded += OnLevelLoad;
		if (Collider != null && !Instances.ContainsValue(this))
		{
			Instances.Add(Collider, this);
		}
	}

	protected virtual void OnDisable()
	{
		SceneManager.sceneLoaded -= OnLevelLoad;
	}

	protected virtual void SpawnFX()
	{
		if (!m_IsInitialSpawnOnAwake)
		{
			if (Audio != null)
			{
				Audio.pitch = Time.timeScale;
				Audio.PlayOneShot(SpawnSound);
			}
			if (SpawnFXPrefabs != null && SpawnFXPrefabs.Length != 0)
			{
				GameObject[] spawnFXPrefabs = SpawnFXPrefabs;
				foreach (GameObject gameObject in spawnFXPrefabs)
				{
					if (gameObject != null)
					{
						vp_Utility.Instantiate(gameObject, Transform.position, Transform.rotation);
					}
				}
			}
		}
		m_IsInitialSpawnOnAwake = false;
	}

	protected virtual void Die()
	{
		vp_Timer.In(UnityEngine.Random.Range(MinRespawnTime, MaxRespawnTime), PickSpawnPoint, m_RespawnTimer);
	}

	public virtual void PickSpawnPoint()
	{
		if (this == null)
		{
			return;
		}
		if (m_SpawnMode == SpawnMode.SamePosition || vp_SpawnPoint.SpawnPoints.Count < 1)
		{
			Placement.Position = m_InitialPosition;
			Placement.Rotation = m_InitialRotation;
			if (Placement.IsObstructed(ObstructionRadius))
			{
				switch (m_ObstructionSolver)
				{
				case ObstructionSolver.Wait:
					vp_Timer.In(UnityEngine.Random.Range(MinRespawnTime, MaxRespawnTime), PickSpawnPoint, m_RespawnTimer);
					return;
				case ObstructionSolver.AdjustPlacement:
					if (!vp_Placement.AdjustPosition(Placement, ObstructionRadius))
					{
						vp_Timer.In(UnityEngine.Random.Range(MinRespawnTime, MaxRespawnTime), PickSpawnPoint, m_RespawnTimer);
						return;
					}
					break;
				}
			}
		}
		else
		{
			switch (m_ObstructionSolver)
			{
			case ObstructionSolver.Wait:
				Placement = vp_SpawnPoint.GetRandomPlacement(0f, SpawnPointTag);
				if (Placement == null)
				{
					Placement = new vp_Placement();
					m_SpawnMode = SpawnMode.SamePosition;
					PickSpawnPoint();
				}
				if (Placement.IsObstructed(ObstructionRadius))
				{
					vp_Timer.In(UnityEngine.Random.Range(MinRespawnTime, MaxRespawnTime), PickSpawnPoint, m_RespawnTimer);
					return;
				}
				break;
			case ObstructionSolver.AdjustPlacement:
				Placement = vp_SpawnPoint.GetRandomPlacement(ObstructionRadius, SpawnPointTag);
				if (Placement == null)
				{
					vp_Timer.In(UnityEngine.Random.Range(MinRespawnTime, MaxRespawnTime), PickSpawnPoint, m_RespawnTimer);
					return;
				}
				break;
			}
		}
		Respawn();
	}

	public virtual void PickSpawnPoint(Vector3 position, Quaternion rotation)
	{
		Placement.Position = position;
		Placement.Rotation = rotation;
		Respawn();
	}

	public virtual void Respawn()
	{
		LastRespawnTime = Time.time;
		vp_Utility.Activate(base.gameObject);
		SpawnFX();
		if (vp_Gameplay.IsMultiplayer && vp_Gameplay.IsMaster)
		{
			vp_GlobalEvent<Transform, vp_Placement>.Send("TransmitRespawn", base.transform.root, Placement);
		}
		SendMessage("Reset");
		Placement.Position = m_InitialPosition;
		Placement.Rotation = m_InitialRotation;
	}

	public virtual void Reset()
	{
		if (Application.isPlaying)
		{
			Transform.position = Placement.Position;
			Transform.rotation = Placement.Rotation;
			if (Rigidbody != null && !Rigidbody.isKinematic)
			{
				Rigidbody.angularVelocity = Vector3.zero;
				Rigidbody.linearVelocity = Vector3.zero;
			}
		}
	}

	public static void ResetAll(bool reInitTimers = false)
	{
		foreach (vp_Respawner value in Instances.Values)
		{
			if (!(value == null) && (!vp_Utility.IsActive(value.gameObject) || (value is vp_PlayerRespawner && (value as vp_PlayerRespawner).Player.Dead.Active)))
			{
				if (reInitTimers)
				{
					value.Die();
				}
				else
				{
					value.PickSpawnPoint();
				}
			}
		}
	}

	public static vp_Respawner GetByCollider(Collider col)
	{
		if (!Instances.TryGetValue(col, out m_GetInstanceResult))
		{
			m_GetInstanceResult = col.transform.root.GetComponentInChildren<vp_Respawner>();
			Instances.Add(col, m_GetInstanceResult);
		}
		return m_GetInstanceResult;
	}

	protected void OnLevelLoad(Scene scene, LoadSceneMode mode)
	{
		Instances.Clear();
	}
}
