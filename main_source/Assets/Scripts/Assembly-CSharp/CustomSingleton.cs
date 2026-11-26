using UnityEngine;

// Lazy scene singleton: finds the scene's instance, or creates a GameObject hosting one on first access.
public abstract class CustomSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T _instance;

	private static bool applicationIsQuitting;

	public static T Instance
	{
		get
		{
			if (applicationIsQuitting)
			{
				return null;
			}
			if (_instance == null)
			{
				_instance = Object.FindObjectOfType(typeof(T)) as T;
				if (_instance == null)
				{
					_instance = new GameObject().AddComponent<T>();
					_instance.gameObject.name = _instance.GetType().ToString();
				}
			}
			return _instance;
		}
	}

	// True while an instance exists; unlike Instance this never creates one.
	public static bool HasInstance => !IsDestroyed;

	public static bool IsDestroyed => _instance == null;

	public static void MakeDontDestroyOnLoad()
	{
		Object.DontDestroyOnLoad(Instance.gameObject);
	}

	protected virtual void OnDestroy()
	{
		_instance = null;
	}

	protected virtual void OnApplicationQuit()
	{
		_instance = null;
	}
}
