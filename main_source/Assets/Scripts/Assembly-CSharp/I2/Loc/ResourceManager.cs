using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace I2.Loc
{
	public class ResourceManager : MonoBehaviour
	{
		private static ResourceManager mInstance;

		public List<IResourceManager_Bundles> mBundleManagers;

		public Object[] Assets;

		private readonly Dictionary<string, Object> mResourcesCache = new Dictionary<string, Object>(System.StringComparer.Ordinal);

		public static ResourceManager pInstance
		{
			get
			{
				if (mInstance == null)
				{
					mInstance = FindAnyObjectByType<ResourceManager>();
					if (mInstance == null)
					{
						mInstance = new GameObject("I2ResourceManager", typeof(ResourceManager)).GetComponent<ResourceManager>();
						mInstance.gameObject.hideFlags |= HideFlags.HideAndDontSave;
					}
					if (Application.isPlaying)
					{
						DontDestroyOnLoad(mInstance.gameObject);
					}
				}
				return mInstance;
			}
		}

		public static void MyOnLevelWasLoaded(Scene scene, LoadSceneMode mode)
		{
		}

		public T GetAsset<T>(string Name) where T : Object
		{
			T asset = FindAsset(Name) as T;
			return asset != null ? asset : LoadFromResources<T>(Name);
		}

		private Object FindAsset(string Name)
		{
			if (Assets != null)
			{
				foreach (Object asset in Assets)
				{
					if (asset != null && asset.name == Name)
					{
						return asset;
					}
				}
			}
			return null;
		}

		public bool HasAsset(Object Obj)
		{
			return Assets != null && System.Array.IndexOf(Assets, Obj) >= 0;
		}

		public T LoadFromResources<T>(string Path) where T : Object
		{
			if (string.IsNullOrEmpty(Path))
			{
				return null;
			}
			if (mResourcesCache.TryGetValue(Path, out Object cached) && cached != null)
			{
				return cached as T;
			}
			T asset = Resources.Load<T>(Path);
			mResourcesCache[Path] = asset;
			return asset;
		}

		public T LoadFromBundle<T>(string path) where T : Object
		{
			return null;
		}

		public void CleanResourceCache(bool unloadResources = false)
		{
			mResourcesCache.Clear();
			if (unloadResources)
			{
				Resources.UnloadUnusedAssets();
			}
		}
	}
}
