using UnityEngine;

public class LevelLoadedCustomEvent : MonoBehaviour
{
	private void Start()
	{
		CustomEvents.LevelLoaded();
	}
}
