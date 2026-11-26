using System;
using System.Collections.Generic;
using UnityEngine;

public class vp_SurfaceManager : MonoBehaviour
{
	[Serializable]
	public class UVTexture
	{
		public Texture Texture;

		public Rect UV;

		public bool ShowUV;

		public UVTexture(bool init)
		{
			Texture = null;
			UV = new Rect(0f, 0f, 1f, 1f);
			ShowUV = false;
		}
	}

	[Serializable]
	public class DefaultFallbacks
	{
		public vp_ImpactEvent ImpactEvent;

		public vp_SurfaceType SurfaceType;

		public bool AllowDecals;
	}

	[Serializable]
	public class ObjectSurface
	{
		public string Name;

		public vp_SurfaceType SurfaceType;

		public List<UVTexture> UVTextures;

		public bool Foldout = true;

		public bool TexturesFoldout = true;

		public ObjectSurface()
		{
			Name = "";
			UVTextures = new List<UVTexture>();
		}
	}

	public bool m_ShowHelp = true;

	public List<ObjectSurface> ObjectSurfaces = new List<ObjectSurface>();

	public DefaultFallbacks Fallbacks;

	protected static bool m_UsingFallbackImpact = false;

	protected static bool m_UsingFallbackSurface = false;

	protected static ObjectSurface[] m_DefaultMaterial = new ObjectSurface[1]
	{
		new ObjectSurface()
	};

	public Rect DefaultUV = new Rect(0f, 0f, 1f, 1f);

	protected static bool m_HaveTerrain = false;

	protected static bool m_CachedHaveTerrain = false;

	protected static Dictionary<Texture, vp_SurfaceType> m_SurfacesByTerrainTexture = new Dictionary<Texture, vp_SurfaceType>();

	protected static Dictionary<Texture, ObjectSurface> m_NonUVSurfaces = new Dictionary<Texture, ObjectSurface>();

	protected static Dictionary<UVTexture, ObjectSurface> m_UVSurfaces = new Dictionary<UVTexture, ObjectSurface>();

	protected static Dictionary<Texture, List<UVTexture>> m_UVTexturesByTexture = new Dictionary<Texture, List<UVTexture>>();

	protected static Dictionary<vp_SurfaceType, Dictionary<vp_ImpactEvent, vp_SurfaceEffect>> m_ImpactFXBySurface = new Dictionary<vp_SurfaceType, Dictionary<vp_ImpactEvent, vp_SurfaceEffect>>();

	protected static Dictionary<Collider, bool> m_UVColliders = new Dictionary<Collider, bool>();

	protected static Dictionary<Collider, bool> m_MultiMatColliders = new Dictionary<Collider, bool>();

	protected static Dictionary<Collider, Terrain> m_TerrainsByCollider = new Dictionary<Collider, Terrain>();

	protected static Dictionary<Collider, vp_SurfaceIdentifier> m_SurfaceIdentifiersByCollider = new Dictionary<Collider, vp_SurfaceIdentifier>();

	protected static Dictionary<Collider, Texture> m_MainTexturesByCollider = new Dictionary<Collider, Texture>();

	protected static Dictionary<Texture, vp_SurfaceType> m_SurfaceTypesByTexture = new Dictionary<Texture, vp_SurfaceType>();

	protected static Dictionary<Collider, vp_SurfaceType> m_SurfacesTypesByCollider = new Dictionary<Collider, vp_SurfaceType>();

	protected static Dictionary<Collider, bool> m_DecalsAllowedByCollider = new Dictionary<Collider, bool>();

	private static Dictionary<Collider, Mesh> m_MeshesByCollider = new Dictionary<Collider, Mesh>();

	private static Dictionary<Collider, Renderer> m_RenderersByCollider = new Dictionary<Collider, Renderer>();

	protected static vp_SurfaceManager m_Instance = null;

	private static bool m_CachedInstance = false;

	protected static bool HaveTerrain
	{
		get
		{
			if (!m_CachedHaveTerrain)
			{
				m_HaveTerrain = UnityEngine.Object.FindObjectOfType<Terrain>() != null;
				m_CachedHaveTerrain = true;
			}
			return m_HaveTerrain;
		}
	}

	public static vp_SurfaceManager Instance
	{
		get
		{
			if (!m_CachedInstance && m_Instance == null)
			{
				m_Instance = UnityEngine.Object.FindObjectOfType<vp_SurfaceManager>();
				m_CachedInstance = true;
			}
			return m_Instance;
		}
	}

	private void Awake()
	{
		if (m_Instance != null)
		{
			base.enabled = false;
			UnityEngine.Object.Destroy(this);
			return;
		}
		if (vp_DecalManager.Instance == null)
		{
			vp_DecalManager.AutoAddTo(base.gameObject);
		}
		Reset();
	}

	public static bool SpawnEffect(RaycastHit hit, vp_ImpactEvent impact, vp_SurfaceType surface = null, AudioSource audioSource = null, bool allowDecal = true)
	{
		if (surface == null)
		{
			surface = GetSurfaceType(hit);
		}
		if (allowDecal)
		{
			allowDecal = AllowsDecals(hit.collider);
			if (allowDecal && vp_DecalManager.Instance != null && !vp_DecalManager.Instance.AllowStretchedDecals && !hit.transform.gameObject.isStatic)
			{
				allowDecal = vp_MathUtility.IsUniform(hit.transform.localScale, 1E-05f);
			}
		}
		vp_SurfaceEffect resultingEffect = GetResultingEffect(impact, surface, ref allowDecal);
		if (resultingEffect == null)
		{
			return false;
		}
		if (allowDecal)
		{
			resultingEffect.SpawnWithDecal(hit, audioSource);
		}
		else
		{
			resultingEffect.Spawn(hit, audioSource);
		}
		return true;
	}

	public static bool SpawnEffect(RaycastHit hit, vp_ImpactEvent impactEvent, AudioSource audioSource, bool allowDecal = true)
	{
		return SpawnEffect(hit, impactEvent, null, audioSource, allowDecal);
	}

	public static void SpawnFootprintEffect(RaycastHit hit, vp_SurfaceType surface, vp_ImpactEvent impactEvent, Vector3 footprintDirection, bool footprintFlip, bool footprintVerifyGroundContact = false, AudioSource audioSource = null, bool allowDecal = true)
	{
		vp_SurfaceEffect.FootprintDirection = footprintDirection;
		vp_SurfaceEffect.FootprintFlip = footprintFlip;
		vp_SurfaceEffect.FootprintVerifyGroundContact = footprintVerifyGroundContact;
		SpawnEffect(hit, impactEvent, surface, audioSource, allowDecal);
		vp_SurfaceEffect.FootprintDirection = vp_SurfaceEffect.NO_DIRECTION;
		vp_SurfaceEffect.FootprintVerifyGroundContact = false;
	}

	public static void SpawnFootprintEffect(RaycastHit hit, vp_ImpactEvent impactEvent, Vector3 footprintDirection, bool footprintFlip, bool footprintVerifyGroundContact = false, AudioSource audioSource = null, bool allowDecal = true)
	{
		SpawnFootprintEffect(hit, null, impactEvent, footprintDirection, footprintFlip, footprintVerifyGroundContact, audioSource, allowDecal);
	}

	protected static vp_SurfaceEffect GetPrimaryEffect(vp_SurfaceType surfaceType, vp_ImpactEvent impact)
	{
		if (impact == null)
		{
			return null;
		}
		Dictionary<vp_ImpactEvent, vp_SurfaceEffect> impactFXDictionary = GetImpactFXDictionary(surfaceType);
		if (impactFXDictionary == null)
		{
			return null;
		}
		if (impactFXDictionary.Count == 0)
		{
			return null;
		}
		impactFXDictionary.TryGetValue(impact, out var value);
		return value;
	}

	protected static vp_SurfaceEffect GetResultingEffect(vp_ImpactEvent impact, vp_SurfaceType surface, ref bool allowDecals)
	{
		m_UsingFallbackImpact = false;
		m_UsingFallbackSurface = false;
		if (impact == null && Instance != null)
		{
			impact = Instance.Fallbacks.ImpactEvent;
			m_UsingFallbackImpact = true;
		}
		if (impact == null)
		{
			return null;
		}
		if (surface == null && Instance != null)
		{
			surface = Instance.Fallbacks.SurfaceType;
			m_UsingFallbackSurface = true;
		}
		if (surface == null)
		{
			return null;
		}
		if (surface.ImpactFX == null)
		{
			return null;
		}
		if (surface.ImpactFX.Count == 0)
		{
			return null;
		}
		vp_SurfaceEffect primaryEffect = GetPrimaryEffect(surface, impact);
		if (primaryEffect == null && !m_UsingFallbackSurface && Instance != null)
		{
			surface = Instance.Fallbacks.SurfaceType;
			primaryEffect = GetPrimaryEffect(surface, impact);
		}
		if (primaryEffect == null)
		{
			primaryEffect = GetPrimaryEffect(surface, Instance.Fallbacks.ImpactEvent);
		}
		if (primaryEffect == null)
		{
			return null;
		}
		if (allowDecals && primaryEffect.Decal.m_Prefabs.Count > 0)
		{
			if (m_UsingFallbackSurface || m_UsingFallbackImpact)
			{
				allowDecals = Instance.Fallbacks.AllowDecals;
			}
		}
		else
		{
			allowDecals = false;
		}
		return primaryEffect;
	}

	protected static Dictionary<vp_ImpactEvent, vp_SurfaceEffect> GetImpactFXDictionary(vp_SurfaceType surface)
	{
		if (surface == null)
		{
			return null;
		}
		if (!m_ImpactFXBySurface.TryGetValue(surface, out var value))
		{
			Dictionary<vp_ImpactEvent, vp_SurfaceEffect> dictionary = new Dictionary<vp_ImpactEvent, vp_SurfaceEffect>();
			for (int i = 0; i < surface.ImpactFX.Count; i++)
			{
				if (!(surface.ImpactFX[i].ImpactEvent == null) && !dictionary.ContainsKey(surface.ImpactFX[i].ImpactEvent))
				{
					dictionary.Add(surface.ImpactFX[i].ImpactEvent, surface.ImpactFX[i].SurfaceEffect);
				}
			}
			m_ImpactFXBySurface.Add(surface, dictionary);
			return dictionary;
		}
		return value;
	}

	protected static Mesh GetMesh(Collider col)
	{
		if (col == null)
		{
			return null;
		}
		if (col.isTrigger)
		{
			return null;
		}
		if (!m_MeshesByCollider.TryGetValue(col, out var value))
		{
			MeshFilter meshFilter = col.GetComponent<MeshFilter>();
			if (meshFilter == null)
			{
				meshFilter = col.transform.GetComponentInChildren<MeshFilter>();
			}
			if (meshFilter == null && col.transform.parent != null)
			{
				meshFilter = col.transform.parent.GetComponent<MeshFilter>();
			}
			if (meshFilter == null && col.transform.root != col.transform)
			{
				meshFilter = col.transform.root.GetComponentInChildren<MeshFilter>();
			}
			value = ((!(meshFilter == null)) ? ((meshFilter.sharedMesh != null) ? meshFilter.sharedMesh : meshFilter.mesh) : null);
			m_MeshesByCollider.Add(col, value);
		}
		return value;
	}

	protected static Renderer GetRenderer(Collider col)
	{
		if (col == null)
		{
			return null;
		}
		if (col.isTrigger)
		{
			return null;
		}
		if (!m_RenderersByCollider.TryGetValue(col, out var value))
		{
			value = col.GetComponent<Renderer>();
			if (value == null)
			{
				value = col.transform.GetComponentInChildren<Renderer>();
			}
			if (value == null && col.transform.parent != null)
			{
				value = col.transform.parent.GetComponent<Renderer>();
			}
			if (value == null && col.transform.root != col.transform)
			{
				value = col.transform.root.GetComponentInChildren<Renderer>();
			}
			if (value != null && value is SkinnedMeshRenderer)
			{
				value = null;
			}
			m_RenderersByCollider.Add(col, value);
		}
		return value;
	}

	protected static Vector2 AdjustTextureCoord(Vector2 textureCoord, Material mat)
	{
		textureCoord.x *= mat.mainTextureScale.x;
		textureCoord.y *= mat.mainTextureScale.y;
		textureCoord.x += mat.mainTextureOffset.x;
		textureCoord.y -= mat.mainTextureOffset.y;
		textureCoord.x %= 1f;
		textureCoord.y %= 1f;
		if (textureCoord.x < 0f)
		{
			textureCoord.x = 1f - Mathf.Abs(textureCoord.x);
		}
		if (textureCoord.y < 0f)
		{
			textureCoord.y = 1f - Mathf.Abs(textureCoord.y);
		}
		textureCoord.y = 1f - textureCoord.y;
		return textureCoord;
	}

	public static vp_SurfaceType GetSurfaceType(RaycastHit hit)
	{
		vp_SurfaceType vp_SurfaceType2 = null;
		vp_SurfaceIdentifier surfaceIdentifier = GetSurfaceIdentifier(hit);
		if (surfaceIdentifier != null)
		{
			vp_SurfaceType2 = surfaceIdentifier.SurfaceType;
			if (vp_SurfaceType2 != null)
			{
				return vp_SurfaceType2;
			}
		}
		vp_SurfaceType2 = GetSimpleSurface(hit);
		if (vp_SurfaceType2 != null)
		{
			return vp_SurfaceType2;
		}
		vp_SurfaceType2 = GetComplexSurface(hit);
		if (vp_SurfaceType2 != null)
		{
			return vp_SurfaceType2;
		}
		vp_SurfaceType2 = GetTerrainSurface(hit);
		if (vp_SurfaceType2 != null)
		{
			return vp_SurfaceType2;
		}
		return null;
	}

	protected static vp_SurfaceType GetSimpleSurface(RaycastHit hit)
	{
		if (hit.collider == null)
		{
			return null;
		}
		vp_SurfaceType value = null;
		if (!m_SurfacesTypesByCollider.TryGetValue(hit.collider, out value))
		{
			if (!HasMultiMaterial(hit.collider))
			{
				Texture mainTexture = GetMainTexture(hit);
				if (mainTexture != null && !IsUVTexture(mainTexture))
				{
					value = GetNonUVSurface(mainTexture);
				}
			}
			m_SurfacesTypesByCollider.Add(hit.collider, value);
		}
		return value;
	}

	protected static vp_SurfaceType GetComplexSurface(RaycastHit hit)
	{
		Texture texture = null;
		Material material = null;
		if (!HasMultiMaterial(hit.collider))
		{
			texture = GetMainTexture(hit);
		}
		else
		{
			material = GetHitMaterial(hit);
			if (material != null)
			{
				texture = material.mainTexture;
			}
		}
		if (texture == null)
		{
			return null;
		}
		if (!IsUVTexture(texture))
		{
			return GetNonUVSurface(texture);
		}
		if (material == null)
		{
			return null;
		}
		return GetUVSurface(material, hit);
	}

	protected static vp_SurfaceType GetNonUVSurface(Texture texture)
	{
		if (texture == null)
		{
			return null;
		}
		if (!m_SurfaceTypesByTexture.TryGetValue(texture, out var value))
		{
			m_NonUVSurfaces.TryGetValue(texture, out var value2);
			if (value2 != null)
			{
				value = value2.SurfaceType;
				m_SurfaceTypesByTexture.Add(texture, value2.SurfaceType);
			}
			else
			{
				m_SurfaceTypesByTexture.Add(texture, null);
			}
		}
		return value;
	}

	protected static vp_SurfaceType GetUVSurface(Material mat, RaycastHit hit)
	{
		if (mat == null)
		{
			return null;
		}
		if (mat.mainTexture == null)
		{
			return null;
		}
		if (m_UVTexturesByTexture.TryGetValue(mat.mainTexture, out var value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				if (value[i].UV.Contains(AdjustTextureCoord(hit.textureCoord, mat)) && m_UVSurfaces.TryGetValue(value[i], out var value2))
				{
					return value2?.SurfaceType;
				}
			}
		}
		return null;
	}

	private static vp_SurfaceType GetTerrainSurface(RaycastHit hit)
	{
		if (!HaveTerrain)
		{
			return null;
		}
		Texture terrainTexture = GetTerrainTexture(hit.collider, hit.point);
		if (terrainTexture == null)
		{
			return null;
		}
		m_SurfacesByTerrainTexture.TryGetValue(terrainTexture, out var value);
		return value;
	}

	public static vp_SurfaceIdentifier GetSurfaceIdentifier(RaycastHit hit)
	{
		if (hit.collider == null)
		{
			return null;
		}
		if (!m_SurfaceIdentifiersByCollider.TryGetValue(hit.collider, out var value))
		{
			value = hit.collider.GetComponent<vp_SurfaceIdentifier>();
			if (value == null)
			{
				value = hit.collider.transform.GetComponentInChildren<vp_SurfaceIdentifier>();
			}
			m_SurfaceIdentifiersByCollider.Add(hit.collider, value);
			if (value != null && !m_DecalsAllowedByCollider.ContainsKey(hit.collider))
			{
				m_DecalsAllowedByCollider.Add(hit.collider, value.AllowDecals);
			}
		}
		return value;
	}

	public static Texture GetMainTexture(RaycastHit hit)
	{
		if (hit.collider == null)
		{
			return null;
		}
		if (!m_MainTexturesByCollider.TryGetValue(hit.collider, out var value))
		{
			Renderer renderer = GetRenderer(hit.collider);
			if (renderer != null && renderer.sharedMaterial != null && renderer.sharedMaterial.mainTexture != null)
			{
				value = renderer.sharedMaterial.mainTexture;
				m_MainTexturesByCollider.Add(hit.collider, value);
			}
			else
			{
				m_MainTexturesByCollider.Add(hit.collider, null);
			}
		}
		return value;
	}

	protected static Material GetHitMaterial(RaycastHit hit)
	{
		if (hit.triangleIndex < 0)
		{
			return null;
		}
		Mesh mesh = GetMesh(hit.collider);
		if (mesh == null)
		{
			return null;
		}
		if (!mesh.isReadable)
		{
			return null;
		}
		if (mesh.triangles == null)
		{
			return null;
		}
		int[] array = new int[3]
		{
			mesh.triangles[hit.triangleIndex * 3],
			mesh.triangles[hit.triangleIndex * 3 + 1],
			mesh.triangles[hit.triangleIndex * 3 + 2]
		};
		for (int i = 0; i < mesh.subMeshCount; i++)
		{
			int[] triangles = mesh.GetTriangles(i);
			for (int j = 0; j < triangles.Length; j += 3)
			{
				if (triangles[j] == array[0] && triangles[j + 1] == array[1] && triangles[j + 2] == array[2])
				{
					Renderer renderer = GetRenderer(hit.collider);
					if (!(renderer == null) && renderer.sharedMaterials != null && renderer.sharedMaterials.Length >= i + 1 && !(renderer.sharedMaterials[i] == null))
					{
						return renderer.sharedMaterials[i];
					}
				}
			}
		}
		return null;
	}

	public static Texture GetTerrainTexture(Collider col, Vector3 position)
	{
		if (col == null)
		{
			return null;
		}
		if (!m_TerrainsByCollider.TryGetValue(col, out var value))
		{
			value = col.GetComponent<Terrain>();
			m_TerrainsByCollider.Add(col, value);
		}
		if (value == null)
		{
			return null;
		}
		int dominantTerrainTexture = GetDominantTerrainTexture(position, value);
		if (value.terrainData.terrainLayers == null || dominantTerrainTexture > value.terrainData.terrainLayers.Length - 1)
		{
			return null;
		}
		return value.terrainData.terrainLayers[dominantTerrainTexture].diffuseTexture;
	}

	public static int GetDominantTerrainTexture(Vector3 worldPos, Terrain terrain)
	{
		if (terrain == null)
		{
			return 0;
		}
		TerrainData terrainData = terrain.terrainData;
		Vector3 position = terrain.transform.position;
		int x = (int)((worldPos.x - position.x) / terrainData.size.x * (float)terrainData.alphamapWidth);
		int y = (int)((worldPos.z - position.z) / terrainData.size.z * (float)terrainData.alphamapHeight);
		float[,,] alphamaps = terrainData.GetAlphamaps(x, y, 1, 1);
		float[] array = new float[alphamaps.GetUpperBound(2) + 1];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = alphamaps[0, 0, i];
		}
		float num = 0f;
		int result = 0;
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j] > num)
			{
				result = j;
				num = array[j];
			}
		}
		return result;
	}

	protected List<Texture> GetLevelTerrainTextures()
	{
		Terrain[] array = UnityEngine.Object.FindObjectsOfType<Terrain>();
		if (array.Length == 0)
		{
			return null;
		}
		List<Texture> list = new List<Texture>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == null || array[i].terrainData == null || array[i].terrainData.terrainLayers == null || array[i].terrainData.terrainLayers.Length < 1)
			{
				continue;
			}
			for (int j = 0; j < array[i].terrainData.terrainLayers.Length; j++)
			{
				if (!(array[i].terrainData.terrainLayers[j].diffuseTexture == null) && !list.Contains(array[i].terrainData.terrainLayers[j].diffuseTexture))
				{
					list.Add(array[i].terrainData.terrainLayers[j].diffuseTexture);
				}
			}
		}
		return list;
	}

	protected bool IsDuplicateObjectTexture(Texture texture)
	{
		int num = 0;
		for (int i = 0; i < ObjectSurfaces.Count; i++)
		{
			for (int j = 0; j < ObjectSurfaces[i].UVTextures.Count; j++)
			{
				if (ObjectSurfaces[i] != null && ObjectSurfaces[i].UVTextures != null && !(ObjectSurfaces[i].UVTextures[j].Texture == null) && ObjectSurfaces[i].UVTextures[j].Texture == texture)
				{
					num++;
				}
			}
		}
		return num > 1;
	}

	protected static bool AllowsDecals(Collider col)
	{
		if (col == null)
		{
			return false;
		}
		bool value = true;
		if (!m_DecalsAllowedByCollider.TryGetValue(col, out value))
		{
			return true;
		}
		return value;
	}

	protected static bool IsUVTexture(Texture tex)
	{
		return m_UVTexturesByTexture.ContainsKey(tex);
	}

	protected static bool HasMultiMaterial(Collider col)
	{
		if (col == null)
		{
			return false;
		}
		bool value = false;
		if (!m_MultiMatColliders.TryGetValue(col, out value))
		{
			Renderer renderer = GetRenderer(col);
			if (renderer != null)
			{
				value = renderer.sharedMaterials.Length > 1;
				if (value && renderer.gameObject.isStatic)
				{
					value = false;
				}
			}
			if (value && col is MeshCollider)
			{
				Mesh mesh = GetMesh(col);
				if (mesh != null && mesh != (col as MeshCollider).sharedMesh)
				{
					value = false;
				}
			}
			m_MultiMatColliders.Add(col, value);
		}
		return value;
	}

	protected void InitObjectSurfaces()
	{
		for (int i = 0; i < ObjectSurfaces.Count; i++)
		{
			for (int j = 0; j < ObjectSurfaces[i].UVTextures.Count; j++)
			{
				if (ObjectSurfaces[i] == null || ObjectSurfaces[i].UVTextures == null || ObjectSurfaces[i].UVTextures[j].Texture == null || m_UVSurfaces.ContainsKey(ObjectSurfaces[i].UVTextures[j]))
				{
					continue;
				}
				if (ObjectSurfaces[i].UVTextures[j].UV == DefaultUV && !IsDuplicateObjectTexture(ObjectSurfaces[i].UVTextures[j].Texture))
				{
					m_NonUVSurfaces.Add(ObjectSurfaces[i].UVTextures[j].Texture, ObjectSurfaces[i]);
					continue;
				}
				m_UVSurfaces.Add(ObjectSurfaces[i].UVTextures[j], ObjectSurfaces[i]);
				if (!m_UVTexturesByTexture.TryGetValue(ObjectSurfaces[i].UVTextures[j].Texture, out var value))
				{
					value = new List<UVTexture>();
					value.Add(ObjectSurfaces[i].UVTextures[j]);
					m_UVTexturesByTexture.Add(ObjectSurfaces[i].UVTextures[j].Texture, value);
				}
				else
				{
					value.Add(ObjectSurfaces[i].UVTextures[j]);
				}
			}
		}
	}

	protected void InitTerrainSurfaces()
	{
		List<Texture> levelTerrainTextures = GetLevelTerrainTextures();
		if (levelTerrainTextures == null)
		{
			return;
		}
		foreach (Texture item in levelTerrainTextures)
		{
			vp_SurfaceType nonUVSurface = GetNonUVSurface(item);
			if (nonUVSurface != null)
			{
				m_SurfacesByTerrainTexture.Add(item, nonUVSurface);
			}
		}
	}

	public virtual void Reset()
	{
		m_SurfacesByTerrainTexture.Clear();
		m_NonUVSurfaces.Clear();
		m_UVSurfaces.Clear();
		m_UVTexturesByTexture.Clear();
		m_UVColliders.Clear();
		m_MultiMatColliders.Clear();
		m_ImpactFXBySurface.Clear();
		m_TerrainsByCollider.Clear();
		m_SurfaceIdentifiersByCollider.Clear();
		m_MainTexturesByCollider.Clear();
		m_SurfaceTypesByTexture.Clear();
		m_SurfacesTypesByCollider.Clear();
		m_DecalsAllowedByCollider.Clear();
		m_MeshesByCollider.Clear();
		m_RenderersByCollider.Clear();
		InitObjectSurfaces();
		m_CachedInstance = false;
		m_CachedHaveTerrain = false;
		m_HaveTerrain = false;
		InitTerrainSurfaces();
		vp_SurfaceEffect.FootprintDirection = vp_SurfaceEffect.NO_DIRECTION;
		vp_SurfaceEffect.FootprintVerifyGroundContact = false;
	}
}
