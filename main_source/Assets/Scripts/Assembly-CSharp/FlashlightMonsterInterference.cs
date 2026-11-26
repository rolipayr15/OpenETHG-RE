using UnityEngine;

public class FlashlightMonsterInterference : MonoBehaviour
{
	[Header("Reference")]
	public float currentBoost;

	[Header("Floor")]
	public int playerFloor;

	public int monsterFloor;

	[Header("Effect (multipliers against FalloffSettings' base flashlightIntensity)")]
	public float normalBoost = 1f;

	public float sameFloorBaseBoost = 0.65f;

	public float minBoost = 0.15f;

	public float flickerSpeed = 18f;

	public float flickerStrength = 0.35f;

	[Header("Random blackout")]
	public float blackoutChancePerSecond = 0.35f;

	public float blackoutDurationMin = 0.05f;

	public float blackoutDurationMax = 0.18f;

	private float blackoutTimer;

	private float noiseSeed;

	[Header("Recovery")]
	public float recoverySpeed = 10f;

	public float effectLerpSpeed = 20f;

	private float intensity;

	private void Start()
	{
		noiseSeed = Random.value * 1000f;
		SetFlashlightBoost(normalBoost);
	}

	private void OnDisable()
	{
		EyesLightingComposer.SetInterferenceMultiplier(1f);
	}

	private void Update()
	{
		if (UserGameSettings.isEnemyOnTheSameFloor)
		{
			float deltaTime = Time.deltaTime;
			intensity = (CustomSingleton<FloorManager>.Instance.IsEnemyClose() ? 1f : 0.5f);
			HandleBlackout(intensity);
			float value;
			if (blackoutTimer <= 0f)
			{
				float noise = Mathf.PerlinNoise(noiseSeed, Time.time * flickerSpeed);
				float target = Mathf.Lerp(normalBoost, sameFloorBaseBoost, intensity) * Mathf.Lerp(1f, noise, flickerStrength * intensity);
				if (Random.value < deltaTime * 3f * intensity)
				{
					target *= Random.Range(0.2f, 0.6f);
				}
				value = Mathf.Lerp(currentBoost, Mathf.Clamp(target, minBoost, 1.05f), deltaTime * effectLerpSpeed);
			}
			else
			{
				blackoutTimer -= deltaTime;
				value = Mathf.Lerp(currentBoost, Mathf.Max(minBoost * 0.75f, 0.3f), deltaTime * effectLerpSpeed);
			}
			SetFlashlightBoost(value);
		}
		else
		{
			blackoutTimer = 0f;
			if (!Mathf.Approximately(currentBoost, normalBoost))
			{
				RestoreFlashlight();
			}
		}
	}

	private void HandleBlackout(float intensity)
	{
		if (blackoutTimer <= 0f && Random.value < blackoutChancePerSecond * intensity * Time.deltaTime)
		{
			blackoutTimer = Random.Range(blackoutDurationMin, blackoutDurationMax);
		}
	}

	private void RestoreFlashlight()
	{
		SetFlashlightBoost(Mathf.Lerp(currentBoost, normalBoost, Time.deltaTime * recoverySpeed));
	}

	private void SetFlashlightBoost(float value)
	{
		currentBoost = value;
		EyesLightingComposer.SetInterferenceMultiplier(value);
	}
}
