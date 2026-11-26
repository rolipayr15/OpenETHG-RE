using UnityEngine;

public class Negatoscope : MonoBehaviour
{
	[SerializeField]
	private Material negatoscope;

	[SerializeField]
	private Texture lightTexture;

	[SerializeField]
	private Texture darkTexture;

	private float randomTime;

	private float lastCheckTime;

	private void Update()
	{
		if (UserGameSettings.isEnemyOnTheSameFloor)
		{
			if (lastCheckTime + randomTime < Time.time)
			{
				randomTime = Random.Range(1, 5) / 10f;
				ChangeTexture();
				lastCheckTime = Time.time;
			}
		}
		else if (negatoscope != null && negatoscope.mainTexture != darkTexture)
		{
			negatoscope.mainTexture = darkTexture;
		}
	}

	private void ChangeTexture()
	{
		if (negatoscope != null)
		{
			negatoscope.mainTexture = negatoscope.mainTexture == lightTexture ? darkTexture : lightTexture;
		}
	}
}
