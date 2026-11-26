using UnityEngine;
using VLB;

public class FlickerWhenBlackout : MonoBehaviour
{
	[SerializeField]
	private EffectFlicker effectFlicker;

	private bool isFlickerEnabled;

	private bool isFloorDataInitialized;

	private FloorManager floorManager;

	private int lampFloor;

	private float lastCheckTime;

	private void Awake()
	{
		isFlickerEnabled = effectFlicker.enabled;
	}

	private bool TryInitializeFloorData()
	{
		if (isFloorDataInitialized)
		{
			return true;
		}
		if (!FloorManager.TryGetInitializedSceneInstance(out floorManager))
		{
			return false;
		}
		lampFloor = floorManager.GetFloor(transform.position);
		isFloorDataInitialized = true;
		return true;
	}

	private void SetMediumFlicker()
	{
		effectFlicker.frequency = 10f;
		effectFlicker.smoothing = 0.05f;
	}

	private void SetHighFlicker()
	{
		effectFlicker.smoothing = 0f;
		effectFlicker.frequency = 20f;
	}

	private void Update()
	{
		if (TryInitializeFloorData() && UserGameSettings.isEnemyOnTheSameFloor && floorManager.GetPlayerFloor() == lampFloor)
		{
			SetFlickerEnabled(true);
			if (lastCheckTime + 1f < Time.time)
			{
				if (floorManager.IsEnemyClose())
				{
					SetHighFlicker();
				}
				else
				{
					SetMediumFlicker();
				}
				lastCheckTime = Time.time;
			}
			return;
		}
		SetFlickerEnabled(false);
	}

	private void SetFlickerEnabled(bool enabled)
	{
		if (isFlickerEnabled != enabled)
		{
			effectFlicker.enabled = enabled;
			isFlickerEnabled = enabled;
		}
	}
}
