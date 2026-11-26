using UnityEngine;
using UnityEngine.UI;

public class WinScreenEnemy : MonoBehaviour
{
	[SerializeField]
	private Image krasue;

	[SerializeField]
	private Image charlie;

	[SerializeField]
	private Image goodBoy;

	[SerializeField]
	private GameObject doubleEnemy;

	[SerializeField]
	private Image witch;

	[Tooltip("Optional until the goose portrait exists; the Krasue portrait stands in while null.")]
	[SerializeField]
	private Image goose;

	private void Start()
	{
		int enemy = (int)UserGameSettings.currentEnemy;
		krasue.enabled = enemy == 0 || enemy == 3 || (enemy == 7 && goose == null);
		charlie.enabled = enemy == 1;
		goodBoy.enabled = enemy == 2;
		witch.enabled = enemy == 6;
		if (goose != null)
		{
			goose.enabled = enemy == 7;
		}
		doubleEnemy.SetActive(enemy == 5);
	}
}
