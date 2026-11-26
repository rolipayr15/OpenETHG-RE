using UnityEngine;
using UnityEngine.UI;

public class WinScreenBackground : MonoBehaviour
{
	[SerializeField]
	private Image mansionBackground;

	[SerializeField]
	private Image hospitalBackground;

	[SerializeField]
	private Image schoolBackground;

	private void Start()
	{
		mansionBackground.enabled = false;
		hospitalBackground.enabled = false;
		schoolBackground.enabled = false;
		switch ((int)UserGameSettings.currentLevel)
		{
		case 0:
			mansionBackground.enabled = true;
			break;
		case 1:
			hospitalBackground.enabled = true;
			break;
		case 3:
			schoolBackground.enabled = true;
			break;
		}
	}
}
