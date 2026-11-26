using UnityEngine;

public class BlackoutPc : MonoBehaviour
{
	[SerializeField]
	private Material blueScreen;

	[SerializeField]
	private Material blackScreen;

	private bool isBlueScreenOn;

	[SerializeField]
	private MeshRenderer meshRenderer;

	private float lastCheckTime;

	private float randomTime;

	private void Update()
	{
		if (!UserGameSettings.isEnemyOnTheSameFloor)
		{
			SetBlueScreen(false);
			return;
		}
		FloorManager floors = CustomSingleton<FloorManager>.Instance;
		if (floors.GetPlayerFloor() == floors.GetFloor(transform.position) && lastCheckTime + randomTime < Time.time)
		{
			SetBlueScreen(Random.Range(0, 2) == 1);
			lastCheckTime = Time.time;
			randomTime = Random.Range(0.02f, 0.03f);
		}
	}

	private void SetBlueScreen(bool enabled)
	{
		if (isBlueScreenOn != enabled)
		{
			meshRenderer.material = enabled ? blueScreen : blackScreen;
			isBlueScreenOn = enabled;
		}
	}
}
