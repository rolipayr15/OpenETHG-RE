using System;
using System.Collections.Generic;
using UnityEngine;

public class vp_PoolManager : MonoBehaviour
{
	[Serializable]
	public class vp_PreloadedPrefab
	{
		public GameObject Prefab;

		public int Amount = 15;
	}

	public List<vp_PreloadedPrefab> PreloadedPrefabs = new List<vp_PreloadedPrefab>();

	public List<GameObject> IgnoredPrefabs = new List<GameObject>();

	protected Dictionary<GameObject, bool> m_IgnoredPrefabsInternal = new Dictionary<GameObject, bool>();

	protected Transform m_Transform;

	protected Dictionary<string, List<GameObject>> m_AvailableObjects = new Dictionary<string, List<GameObject>>();

	protected Dictionary<string, int> m_UniquePrefabNames = new Dictionary<string, int>();

	protected static vp_PoolManager m_Instance;

	public static vp_PoolManager Instance => m_Instance;

	protected virtual void Awake()
	{
		m_Instance = this;
		m_Transform = base.transform;
	}

	protected virtual void Start()
	{
		foreach (GameObject ignoredPrefab in IgnoredPrefabs)
		{
			if (!(ignoredPrefab == null) && !m_IgnoredPrefabsInternal.ContainsKey(ignoredPrefab))
			{
				m_IgnoredPrefabsInternal.Add(ignoredPrefab, value: false);
			}
		}
		foreach (vp_PreloadedPrefab preloadedPrefab in PreloadedPrefabs)
		{
			if (preloadedPrefab != null && !(preloadedPrefab.Prefab == null) && preloadedPrefab.Amount >= 1)
			{
				AddToPool(preloadedPrefab.Prefab, Vector3.zero, Quaternion.identity, preloadedPrefab.Amount);
			}
		}
	}

	public virtual void AddToPool(GameObject prefab, Vector3 position, Quaternion rotation, int amount = 1)
	{
		if (!(prefab == null) && !m_IgnoredPrefabsInternal.ContainsKey(prefab))
		{
			string uniqueNameOf = GetUniqueNameOf(prefab);
			if (!m_AvailableObjects.ContainsKey(uniqueNameOf))
			{
				m_AvailableObjects.Add(uniqueNameOf, new List<GameObject>());
			}
			for (int i = 0; i < amount; i++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(prefab, position, rotation);
				gameObject.name = uniqueNameOf;
				gameObject.transform.parent = m_Transform;
				vp_Utility.Activate(gameObject, activate: false);
				m_AvailableObjects[uniqueNameOf].Add(gameObject);
			}
		}
	}

	public static GameObject Spawn(GameObject original, Vector3 position, Quaternion rotation)
	{
		if (Instance == null)
		{
			return null;
		}
		return Instance.SpawnInternal(original, position, rotation);
	}

	public virtual GameObject SpawnInternal(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		if (prefab == null)
		{
			return null;
		}
		if (m_IgnoredPrefabsInternal.ContainsKey(prefab))
		{
			return UnityEngine.Object.Instantiate(prefab, position, rotation);
		}
		GameObject gameObject = null;
		List<GameObject> value = null;
		string uniqueNameOf = GetUniqueNameOf(prefab);
		if (m_AvailableObjects.TryGetValue(uniqueNameOf, out value))
		{
			while (value.Count >= 1)
			{
				gameObject = value[0];
				if (gameObject == null)
				{
					value.Remove(gameObject);
					continue;
				}
				gameObject.transform.position = position;
				gameObject.transform.rotation = rotation;
				value.Remove(gameObject);
				vp_Utility.Activate(gameObject);
				for (int i = 0; i < gameObject.transform.childCount; i++)
				{
					Transform child = gameObject.transform.GetChild(i);
					if (child != null)
					{
						vp_Utility.Activate(child.gameObject);
					}
				}
				if (gameObject.transform.parent == m_Transform)
				{
					gameObject.transform.parent = null;
				}
				return gameObject;
			}
		}
		AddToPool(prefab, position, rotation);
		return SpawnInternal(prefab, position, rotation);
	}

	public static void Despawn(GameObject obj, float delay)
	{
		if (Instance == null)
		{
			return;
		}
		if (delay > 0f)
		{
			vp_Timer.In(delay, delegate
			{
				Instance.DespawnInternal(obj);
			});
		}
		else
		{
			Instance.DespawnInternal(obj);
		}
	}

	public static void Despawn(GameObject obj)
	{
		Instance.DespawnInternal(obj);
	}

	protected virtual void DespawnInternal(GameObject obj)
	{
		if (obj == null)
		{
			return;
		}
		if (m_IgnoredPrefabsInternal.ContainsKey(obj))
		{
			UnityEngine.Object.Destroy(obj);
			return;
		}
		List<GameObject> value = null;
		string key = obj.name;
		bool flag = false;
		while (true)
		{
			m_AvailableObjects.TryGetValue(key, out value);
			if (value != null)
			{
				break;
			}
			if (obj.transform.parent != null)
			{
				flag = true;
				key = obj.transform.parent.name;
				continue;
			}
			UnityEngine.Object.Destroy(obj);
			return;
		}
		vp_Utility.Activate(obj, activate: false);
		if (!flag)
		{
			if (obj.transform.parent == null)
			{
				obj.transform.parent = m_Transform;
			}
			value.Add(obj);
		}
	}

	public void AddIgnoredPrefab(GameObject prefab)
	{
		if (!Instance.IgnoredPrefabs.Contains(prefab))
		{
			IgnoredPrefabs.Add(prefab);
		}
		if (!Instance.m_IgnoredPrefabsInternal.ContainsKey(prefab))
		{
			m_IgnoredPrefabsInternal.Add(prefab, value: false);
		}
	}

	protected string GetUniqueNameOf(GameObject prefab)
	{
		if (m_UniquePrefabNames.TryGetValue(prefab.name, out var value))
		{
			if (prefab.GetInstanceID() == value)
			{
				return prefab.name;
			}
			string text = string.Join(" ", prefab.name, prefab.GetInstanceID().ToString());
			if (!m_UniquePrefabNames.ContainsKey(text))
			{
				m_UniquePrefabNames.Add(text, prefab.GetInstanceID());
			}
			return text;
		}
		m_UniquePrefabNames.Add(prefab.name, prefab.GetInstanceID());
		return prefab.name;
	}
}
