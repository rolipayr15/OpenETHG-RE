using UnityEngine;

public class ChangeMaterialForHardNightmare : MonoBehaviour
{
	[SerializeField]
	private Material materialNoShine;

	[SerializeField]
	private Renderer _renderer;

	private void Start()
	{
		int difficulty = (int)UserGameSettings.currentDifficulty;
		if (difficulty == 3 || difficulty == 4)
		{
			_renderer.material = materialNoShine;
		}
	}
}
