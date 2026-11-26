using UnityEngine;

public class HalloweenManager : MonoBehaviour
{
	private long startTime;

	private void Start()
	{
		UserGameSettings.halloweenMultiplier = 1;
		if ((int)UserGameSettings.currentMode != 2 && (int)UserGameSettings.currentEnemy != 7)
		{
			foreach (GameObject item in GameObject.FindGameObjectsWithTag("Halloween"))
			{
				item.SetActive(false);
			}
		}
	}
}
