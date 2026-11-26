using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class vp_DecalManager : MonoBehaviour
{
	public struct DecalToCheckLater
	{
		public GameObject Decal;

		public float CornerOverlap;

		public float TimeOfBirth;

		public DecalToCheckLater(GameObject decal, float cornerOverlap, float timeOfBirth)
		{
			Decal = decal;
			CornerOverlap = cornerOverlap;
			TimeOfBirth = timeOfBirth;
		}
	}

	[SerializeField]
	protected float m_DecalLimit = 100f;

	[SerializeField]
	protected float m_WeatheredDecals = 20f;

	public bool CleanupOverTime = true;

	public float VertexRaycastInterval = 0.5f;

	public int DecalsPerCleanupBatch = 4;

	public bool InstantQuadCornerTest = true;

	public int QuadRaycastRange = 5;

	public int MaxQuadRaycastsPerSecond = 3;

	public bool AllowStretchedDecals = true;

	public float RemoveDelay = 10f;

	public float RemoveFadeoutSpeed = 10f;

	public bool AllowInstaRemoveIfOffscreen = true;

	private const int INSTANT_INVISIBLE_SPEED = 25;

	private const float FOV_PADDING = 0.9f;

	private const float ROUGH_REMOVE_INTERVAL = 0.25f;

	public static vp_DecalManager Instance = null;

	protected static bool m_HaveInstance = false;

	protected bool m_WasAutoAdded;

	protected static float m_NonFadedDecals = 0f;

	protected static float m_FadeAmount = 0f;

	protected static bool m_Inited = false;

	protected static float m_QuadTestRate = 0f;

	protected static float m_NextAllowedVertexTestTime = 0f;

	protected static float m_NextAllowedRemoveTime = 0f;

	protected static float m_NextAllowedQuadTestTime = 0f;

	protected static Dictionary<GameObject, Renderer> m_DecalRenderers = new Dictionary<GameObject, Renderer>();

	protected static Dictionary<GameObject, Mesh> m_DecalMeshFilters = new Dictionary<GameObject, Mesh>();

	public static List<DecalToCheckLater>[] m_Queue = new List<DecalToCheckLater>[4];

	public static List<DecalToCheckLater> m_DecalsToRemoveWhenOffscreen = new List<DecalToCheckLater>();

	public static List<Renderer> m_RenderersToQuickFade = new List<Renderer>();

	protected static Renderer m_TargetRenderer;

	protected static Mesh m_TargetMesh;

	private static int[] m_List = new int[20]
	{
		0, 0, 0, 0, 0, 0, 0, 0, 2, 2,
		2, 2, 2, 2, 1, 1, 1, 1, 3, 3
	};

	private static int m_Index = 0;

	protected static List<GameObject> m_Decals = new List<GameObject>();

	protected bool m_DebugMode;

	public bool m_PlacementFoldout;

	public bool m_RemovalFoldout;

	public bool m_LimitsFoldout;

	public bool m_DebugFoldout;

	public bool m_ShowHelp = true;

	public float DecalLimit
	{
		get
		{
			return m_DecalLimit;
		}
		set
		{
			m_DecalLimit = value;
			Refresh();
		}
	}

	public float WeatheredLimit
	{
		get
		{
			return m_WeatheredDecals;
		}
		set
		{
			if (value > m_DecalLimit)
			{
				value = m_DecalLimit;
				return;
			}
			m_WeatheredDecals = value;
			Refresh();
		}
	}

	public List<GameObject> Decals => m_Decals;

	public bool DebugMode
	{
		get
		{
			return m_DebugMode;
		}
		set
		{
			m_DebugMode = value;
		}
	}

	private void Awake()
	{
		m_Queue[0] = new List<DecalToCheckLater>();
		m_Queue[1] = new List<DecalToCheckLater>();
		m_Queue[2] = new List<DecalToCheckLater>();
		m_Queue[3] = new List<DecalToCheckLater>();
		if (Instance != null)
		{
			if (!Instance.m_WasAutoAdded)
			{
				Debug.LogWarning("Warning (" + this?.ToString() + ") There can only be one vp_DecalManager in the scene! (destroying self)");
			}
			base.enabled = false;
			Object.Destroy(this);
		}
		else
		{
			Instance = this;
			m_HaveInstance = true;
		}
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnLevelLoad;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnLevelLoad;
	}

	private void Update()
	{
		if (Instance == null)
		{
			Instance = this;
			m_HaveInstance = true;
		}
		UpdateQuadTest();
		UpdateVertexTest();
		UpdateRemoval();
		UpdateQuickFading();
	}

	private void UpdateQuadTest()
	{
		if (!(Time.time < m_NextAllowedQuadTestTime))
		{
			m_NextAllowedQuadTestTime = Time.time + 1f;
			m_QuadTestRate = 0f;
		}
	}

	public static void AutoAddTo(GameObject target)
	{
		if (Instance != null)
		{
			Debug.LogError("Error (vp_DecalManager) Can't auto-add because there is already a vp_DecalManager in the scene.");
			return;
		}
		(Instance = target.AddComponent<vp_DecalManager>()).m_WasAutoAdded = true;
		m_HaveInstance = true;
	}

	protected void UpdateVertexTest()
	{
		if (!CleanupOverTime || Time.time < m_NextAllowedVertexTestTime || (m_Queue[0].Count == 0 && m_Queue[1].Count == 0 && m_Queue[2].Count == 0 && m_Queue[3].Count == 0))
		{
			return;
		}
		for (int i = 0; i < DecalsPerCleanupBatch; i++)
		{
			DoVertexTest(m_List[m_Index], RemoveFadeoutSpeed == 25f);
			Step();
			while (m_Queue[m_List[m_Index]].Count == 0)
			{
				Step();
				if (m_Queue[0].Count == 0 && m_Queue[1].Count == 0 && m_Queue[2].Count == 0 && m_Queue[3].Count == 0)
				{
					break;
				}
			}
		}
		m_NextAllowedVertexTestTime = Time.time + VertexRaycastInterval;
	}

	protected void Step()
	{
		m_Index++;
		if (m_Index > m_List.Length - 1)
		{
			m_Index = 0;
		}
	}

	protected static bool DoVertexTest(int list, bool instaRemoveOnFail)
	{
		if (m_Queue[list].Count == 0)
		{
			return false;
		}
		GameObject decal = m_Queue[list][0].Decal;
		float cornerOverlap = m_Queue[list][0].CornerOverlap;
		if (decal == null)
		{
			m_Queue[list].RemoveAt(0);
			return false;
		}
		m_DecalMeshFilters.TryGetValue(decal, out m_TargetMesh);
		if (m_TargetMesh == null)
		{
			return false;
		}
		if (!Physics.Raycast(new Ray(decal.transform.TransformPoint(m_TargetMesh.vertices[list] * (1f - cornerOverlap * 2f)) + decal.transform.forward * 0.1f, -decal.transform.forward), out var hitInfo, 0.2f, vp_Layer.Mask.ExternalBlockers) || (decal.transform.parent != null && hitInfo.transform != decal.transform.parent))
		{
			if (instaRemoveOnFail)
			{
				vp_Utility.Destroy(decal);
			}
			else
			{
				m_DecalsToRemoveWhenOffscreen.Add(m_Queue[list][0]);
			}
			m_Queue[list].RemoveAt(0);
		}
		else if (list == 3)
		{
			m_Queue[list].RemoveAt(0);
		}
		else
		{
			m_Queue[list + 1].Add(m_Queue[list][0]);
			m_Queue[list].RemoveAt(0);
		}
		return true;
	}

	protected static bool TryQuadTest(GameObject decal, float cornerOverlap)
	{
		if (!m_HaveInstance)
		{
			return false;
		}
		if (!Instance.InstantQuadCornerTest)
		{
			return false;
		}
		if (m_QuadTestRate >= (float)Instance.MaxQuadRaycastsPerSecond)
		{
			return false;
		}
		if (Vector3.Distance(Camera.main.transform.position, decal.transform.position) > (float)Instance.QuadRaycastRange)
		{
			return false;
		}
		DoQuadTest(decal, cornerOverlap);
		return true;
	}

	protected static void DoQuadTest(GameObject decal, float cornerOverlap)
	{
		if (decal == null)
		{
			return;
		}
		m_QuadTestRate += 1f;
		m_DecalMeshFilters.TryGetValue(decal, out m_TargetMesh);
		if (m_TargetMesh == null)
		{
			return;
		}
		for (int i = 0; i < 4; i++)
		{
			if (!Physics.Raycast(new Ray(decal.transform.TransformPoint(m_TargetMesh.vertices[i] * (1f - cornerOverlap * 2f)) + decal.transform.forward * 0.1f, -decal.transform.forward), out var hitInfo, 0.2f, vp_Layer.Mask.ExternalBlockers) || (decal.transform.parent != null && hitInfo.transform != decal.transform.parent))
			{
				vp_Utility.Destroy(decal);
				break;
			}
		}
	}

	private static bool IsOffScreen(GameObject decal)
	{
		if (decal == null)
		{
			return true;
		}
		return Mathf.Abs(vp_3DUtility.LookAtAngle(Camera.main.transform.position, Camera.main.transform.forward, decal.transform.position)) > Camera.main.fieldOfView * 0.9f;
	}

	private static void UpdateRemoval()
	{
		if (Time.time < m_NextAllowedRemoveTime)
		{
			return;
		}
		m_NextAllowedRemoveTime = Time.time + (0.25f + Random.value * 0.25f);
		for (int num = m_DecalsToRemoveWhenOffscreen.Count - 1; num > -1; num--)
		{
			if (m_HaveInstance && Instance.AllowInstaRemoveIfOffscreen && IsOffScreen(m_DecalsToRemoveWhenOffscreen[num].Decal))
			{
				vp_Utility.Destroy(m_DecalsToRemoveWhenOffscreen[num].Decal);
				m_DecalsToRemoveWhenOffscreen.RemoveAt(num);
			}
			else if (Vector3.Distance(Camera.main.transform.position, m_DecalsToRemoveWhenOffscreen[num].Decal.transform.position) > (float)(m_HaveInstance ? Instance.QuadRaycastRange : 5) || m_DecalsToRemoveWhenOffscreen[num].TimeOfBirth < Time.time - (m_HaveInstance ? Instance.RemoveDelay : 10f))
			{
				SetTargetRenderer(m_DecalsToRemoveWhenOffscreen[num].Decal);
				if (m_TargetRenderer != null)
				{
					m_RenderersToQuickFade.Add(m_TargetRenderer);
					m_DecalsToRemoveWhenOffscreen.RemoveAt(num);
				}
			}
		}
	}

	private static void UpdateQuickFading()
	{
		for (int num = m_RenderersToQuickFade.Count - 1; num > -1; num--)
		{
			if (m_RenderersToQuickFade[num] == null)
			{
				m_RenderersToQuickFade.Remove(m_RenderersToQuickFade[num]);
			}
			else if (m_RenderersToQuickFade[num].material.color.a < 0.01f)
			{
				vp_Utility.Destroy(m_RenderersToQuickFade[num].gameObject);
				m_RenderersToQuickFade.Remove(m_RenderersToQuickFade[num]);
			}
			else
			{
				Color color = m_RenderersToQuickFade[num].material.color;
				color.a = Mathf.Lerp(color.a, 0f, Time.deltaTime * (m_HaveInstance ? Instance.RemoveFadeoutSpeed : 10f));
				m_RenderersToQuickFade[num].material.color = color;
			}
		}
	}

	private static GameObject InstantiateDecal(GameObject original, RaycastHit hit)
	{
		GameObject gameObject = vp_Utility.Instantiate(original, hit.point, Quaternion.Euler(hit.normal)) as GameObject;
		if (gameObject == null)
		{
			return null;
		}
		gameObject.layer = 29;
		if ((m_HaveInstance && Instance.AllowStretchedDecals && !hit.transform.gameObject.isStatic) || vp_MathUtility.IsUniform(hit.transform.localScale, 1E-05f))
		{
			gameObject.transform.parent = hit.transform;
		}
		Add(gameObject);
		return gameObject;
	}

	public static void Spawn(GameObject prefab, RaycastHit hit, float cornerOverlap, float scale)
	{
		Spawn(prefab, hit, 0f, 360f, cornerOverlap, scale);
	}

	public static void Spawn(GameObject prefab, RaycastHit hit, float angle, float cornerOverlap, float scale)
	{
		Spawn(prefab, hit, angle, angle, cornerOverlap, scale);
	}

	public static void Spawn(GameObject prefab, RaycastHit hit, float minAngle, float maxAngle, float cornerOverlap, float scale)
	{
		if (prefab == null || (!vp_MathUtility.IsUniform(hit.transform.localScale, 1E-05f) && !hit.transform.gameObject.isStatic && m_HaveInstance && !Instance.AllowStretchedDecals))
		{
			return;
		}
		GameObject gameObject = InstantiateDecal(prefab, hit);
		if (!(gameObject == null))
		{
			gameObject.transform.localScale = Vector3.Scale(gameObject.transform.localScale, new Vector3(scale, scale, 1f));
			gameObject.transform.rotation = Quaternion.LookRotation(hit.normal);
			gameObject.transform.Rotate(Vector3.forward, Random.Range(minAngle, maxAngle), Space.Self);
			if (cornerOverlap < 0.5f && !TryQuadTest(gameObject, cornerOverlap))
			{
				CheckDecalLater(gameObject, cornerOverlap);
			}
		}
	}

	protected static void CheckDecalLater(GameObject decal, float cornerOverlap)
	{
		if (m_Queue[0] != null)
		{
			m_Queue[0].Add(new DecalToCheckLater(decal, cornerOverlap, Time.time));
		}
	}

	public static void SpawnFootprint(GameObject prefab, RaycastHit hit, Vector3 footprintDirection, bool flip, bool verifyGroundContact, float scale)
	{
		if (prefab == null)
		{
			return;
		}
		GameObject gameObject = InstantiateDecal(prefab, hit);
		if (!(gameObject == null))
		{
			gameObject.transform.localScale = Vector3.Scale(gameObject.transform.localScale, new Vector3(scale, scale, 1f));
			gameObject.transform.LookAt(gameObject.transform.position - footprintDirection - gameObject.transform.up * Vector3.Dot(hit.normal, -footprintDirection), hit.normal);
			gameObject.transform.Rotate(Vector3.left * 90f);
			if (flip)
			{
				gameObject.transform.localScale = Vector3.Scale(gameObject.transform.localScale, Vector3.left + Vector3.up + Vector3.forward);
			}
			if (verifyGroundContact && !TryQuadTest(gameObject, 0f))
			{
				CheckDecalLater(gameObject, 0f);
			}
		}
	}

	public static void Add(GameObject decal)
	{
		if (!m_Inited)
		{
			m_Inited = true;
			Refresh();
		}
		if (!(decal == null))
		{
			if (m_Decals.Contains(decal))
			{
				m_Decals.Remove(decal);
			}
			if (CacheMeshAndRenderer(decal))
			{
				m_Decals.Add(decal);
				WeatherAndRemove();
			}
		}
	}

	protected static bool CacheMeshAndRenderer(GameObject decal)
	{
		MeshFilter component = decal.GetComponent<MeshFilter>();
		if (component == null)
		{
			return false;
		}
		if (component.mesh == null)
		{
			return false;
		}
		m_TargetMesh = component.mesh;
		m_TargetRenderer = decal.GetComponent<Renderer>();
		if (m_TargetRenderer == null)
		{
			return false;
		}
		if (m_TargetRenderer.material == null)
		{
			return false;
		}
		Color color = m_TargetRenderer.material.color;
		color.a = 1f;
		m_TargetRenderer.material.color = color;
		if (!m_DecalRenderers.ContainsKey(decal))
		{
			m_DecalRenderers.Add(decal, m_TargetRenderer);
		}
		if (!m_DecalMeshFilters.ContainsKey(decal))
		{
			m_DecalMeshFilters.Add(decal, m_TargetMesh);
		}
		return true;
	}

	private static void SetTargetRenderer(GameObject decal)
	{
		m_DecalRenderers.TryGetValue(decal, out m_TargetRenderer);
	}

	private static void WeatherAndRemove()
	{
		if ((float)m_Decals.Count > m_NonFadedDecals)
		{
			for (int i = 0; (float)i < (float)m_Decals.Count - m_NonFadedDecals; i++)
			{
				if (m_Decals[i] != null)
				{
					SetTargetRenderer(m_Decals[i]);
					if (m_TargetRenderer != null)
					{
						Color color = m_TargetRenderer.material.color;
						color.a -= m_FadeAmount;
						m_TargetRenderer.material.color = color;
					}
				}
			}
		}
		if (m_Decals[0] != null)
		{
			SetTargetRenderer(m_Decals[0]);
			if (m_TargetRenderer != null && m_TargetRenderer.material.color.a <= 0f)
			{
				vp_Utility.Destroy(m_Decals[0]);
				m_Decals.Remove(m_Decals[0]);
			}
		}
		else
		{
			m_Decals.RemoveAt(0);
		}
	}

	protected static void Refresh()
	{
		if (!m_HaveInstance)
		{
			m_FadeAmount = 0.05f;
			m_NonFadedDecals = 80f;
			return;
		}
		if (Instance.m_DecalLimit < Instance.m_WeatheredDecals)
		{
			Instance.m_DecalLimit = Instance.m_WeatheredDecals;
		}
		m_FadeAmount = Instance.m_DecalLimit / Instance.m_WeatheredDecals / Instance.m_DecalLimit;
		m_NonFadedDecals = Instance.m_DecalLimit - Instance.m_WeatheredDecals;
	}

	protected static void SpawnDebugPoint(Vector3 pos, Transform parent)
	{
		GameObject obj = vp_3DUtility.DebugBall();
		obj.transform.localScale = Vector3.one * 0.05f;
		obj.transform.position = pos;
		Color color = Color.white;
		if (m_List[m_Index] == 0)
		{
			color = Color.blue;
		}
		if (m_List[m_Index] == 1)
		{
			color = Color.green;
		}
		if (m_List[m_Index] == 2)
		{
			color = Color.yellow;
		}
		if (m_List[m_Index] == 3)
		{
			color = Color.red;
		}
		obj.GetComponent<Renderer>().material.color = new Color(color.r, color.g, color.b, 0.5f);
	}

	protected virtual void OnLevelLoad(Scene scene, LoadSceneMode mode)
	{
		m_RenderersToQuickFade.Clear();
		m_DecalsToRemoveWhenOffscreen.Clear();
		m_DecalRenderers.Clear();
		m_DecalMeshFilters.Clear();
		m_Queue[0].Clear();
		m_Queue[1].Clear();
		m_Queue[2].Clear();
		m_Queue[3].Clear();
		Instance = null;
		m_HaveInstance = false;
		m_Inited = false;
	}
}
