using System;
using System.Collections.Generic;
using UnityEngine;

public class vp_FootstepManager : MonoBehaviour
{
	[Serializable]
	public class vp_SurfaceTypes
	{
		public Vector2 RandomPitch = new Vector2(1f, 1.5f);

		public bool Foldout = true;

		public bool SoundsFoldout = true;

		public bool TexturesFoldout = true;

		public string SurfaceName = "";

		public List<AudioClip> Sounds = new List<AudioClip>();

		public List<Texture> Textures = new List<Texture>();
	}

	private static vp_FootstepManager[] m_FootstepManagers;

	public static bool mIsDirty = true;

	public List<vp_SurfaceTypes> SurfaceTypes = new List<vp_SurfaceTypes>();

	protected vp_FPPlayerEventHandler m_Player;

	protected vp_FPCamera m_Camera;

	protected vp_FPController m_Controller;

	protected AudioSource m_Audio;

	protected AudioClip m_SoundToPlay;

	protected AudioClip m_LastPlayedSound;

	public static vp_FootstepManager[] FootstepManagers
	{
		get
		{
			if (mIsDirty)
			{
				mIsDirty = false;
				m_FootstepManagers = UnityEngine.Object.FindObjectsOfType(typeof(vp_FootstepManager)) as vp_FootstepManager[];
				if (m_FootstepManagers == null)
				{
					m_FootstepManagers = Resources.FindObjectsOfTypeAll(typeof(vp_FootstepManager)) as vp_FootstepManager[];
				}
			}
			return m_FootstepManagers;
		}
	}

	public bool IsDirty => mIsDirty;

	protected virtual void Awake()
	{
		Debug.LogWarning("Warning (" + this?.ToString() + ") This component is obsolete! please use the new vp_PlayerFootFXHandler component along with a scene vp_SurfaceManager instead.");
		m_Player = base.transform.root.GetComponentInChildren<vp_FPPlayerEventHandler>();
		m_Camera = base.transform.root.GetComponentInChildren<vp_FPCamera>();
		m_Controller = base.transform.root.GetComponentInChildren<vp_FPController>();
		m_Audio = base.transform.root.GetComponentInChildren<AudioSource>();
		if (m_Audio == null)
		{
			m_Audio = base.gameObject.AddComponent<AudioSource>();
		}
	}

	public virtual void SetDirty(bool dirty)
	{
		mIsDirty = dirty;
	}

	private void Update()
	{
		if (m_Camera.BobStepCallback == null)
		{
			vp_FPCamera camera = m_Camera;
			camera.BobStepCallback = (vp_FPCamera.BobStepDelegate)Delegate.Combine(camera.BobStepCallback, new vp_FPCamera.BobStepDelegate(Footstep));
		}
	}

	protected virtual void OnEnable()
	{
		vp_FPCamera camera = m_Camera;
		camera.BobStepCallback = (vp_FPCamera.BobStepDelegate)Delegate.Combine(camera.BobStepCallback, new vp_FPCamera.BobStepDelegate(Footstep));
	}

	protected virtual void OnDisable()
	{
		vp_FPCamera camera = m_Camera;
		camera.BobStepCallback = (vp_FPCamera.BobStepDelegate)Delegate.Remove(camera.BobStepCallback, new vp_FPCamera.BobStepDelegate(Footstep));
	}

	protected virtual void Footstep()
	{
		if (m_Player.Dead.Active || !m_Controller.Grounded || (GetGroundTexture() == null && GetSurfaceIdentifier() == null))
		{
			return;
		}
		if (GetSurfaceIdentifier() != null)
		{
			PlaySound(SurfaceTypes[GetSurfaceIdentifier().SurfaceID]);
			return;
		}
		foreach (vp_SurfaceTypes surfaceType in SurfaceTypes)
		{
			foreach (Texture texture in surfaceType.Textures)
			{
				if (texture == GetGroundTexture())
				{
					PlaySound(surfaceType);
					break;
				}
			}
		}
	}

	private vp_SurfaceIdentifier GetSurfaceIdentifier()
	{
		if (m_Controller.GroundTransform == null)
		{
			return null;
		}
		return m_Controller.GroundTransform.GetComponent<vp_SurfaceIdentifier>();
	}

	public virtual void PlaySound(vp_SurfaceTypes st)
	{
		if (m_Audio == null || !m_Audio.enabled || st.Sounds == null || st.Sounds.Count == 0)
		{
			return;
		}
		do
		{
			m_SoundToPlay = st.Sounds[UnityEngine.Random.Range(0, st.Sounds.Count)];
			if (m_SoundToPlay == null)
			{
				return;
			}
		}
		while (m_SoundToPlay == m_LastPlayedSound && st.Sounds.Count > 1);
		m_Audio.pitch = UnityEngine.Random.Range(st.RandomPitch.x, st.RandomPitch.y) * Time.timeScale;
		m_Audio.clip = m_SoundToPlay;
		m_Audio.Play();
		m_LastPlayedSound = m_SoundToPlay;
	}

	public static int GetMainTerrainTexture(Vector3 worldPos, Terrain terrain)
	{
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

	public virtual Texture GetGroundTexture()
	{
		if (m_Controller.GroundTransform == null)
		{
			return null;
		}
		Terrain component = m_Controller.GroundTransform.GetComponent<Terrain>();
		Renderer component2 = m_Controller.GroundTransform.GetComponent<Renderer>();
		if (component2 == null && component == null)
		{
			return null;
		}
		int num = -1;
		if (component != null)
		{
			num = GetMainTerrainTexture(base.transform.position, component);
			if (num > component.terrainData.terrainLayers.Length - 1)
			{
				return null;
			}
			return component.terrainData.terrainLayers[num].diffuseTexture;
		}
		if (component2 != null && component2.sharedMaterial != null)
		{
			return component2.sharedMaterial.mainTexture;
		}
		return null;
	}
}
