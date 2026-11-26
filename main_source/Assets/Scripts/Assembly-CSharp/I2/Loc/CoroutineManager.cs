using System.Collections;
using UnityEngine;

namespace I2.Loc
{
	public class CoroutineManager : MonoBehaviour
	{
		private static CoroutineManager mInstance;

		private static CoroutineManager pInstance => mInstance != null ? mInstance : (mInstance = new GameObject("_Coroutiner").AddComponent<CoroutineManager>());

		private void Awake()
		{
			DontDestroyOnLoad(gameObject);
			gameObject.hideFlags |= HideFlags.HideAndDontSave;
		}

		public static Coroutine Start(IEnumerator coroutine)
		{
			return pInstance.StartCoroutine(coroutine);
		}
	}
}
