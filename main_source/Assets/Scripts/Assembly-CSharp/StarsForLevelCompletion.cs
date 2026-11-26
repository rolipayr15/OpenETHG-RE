using UnityEngine;
using UnityEngine.UI;

public class StarsForLevelCompletion : MonoBehaviour
{
	[SerializeField]
	private Image maskImage;

	public void SetHighestDifficultyCompleted(Difficulty difficulty)
	{
		float fill;
		switch (difficulty)
		{
		case Difficulty.Casual:
			fill = 0.35f;
			break;
		case Difficulty.Easy:
			fill = 0.5f;
			break;
		case Difficulty.Normal:
			fill = 0.65f;
			break;
		case Difficulty.Hard:
			fill = 0.842f;
			break;
		case Difficulty.Nightmare:
			fill = 1f;
			break;
		default:
			fill = 0f;
			break;
		}
		maskImage.fillAmount = fill;
	}

	public void SetLevelNotCompleted()
	{
		maskImage.fillAmount = 0f;
	}
}
