using UnityEngine;
using UnityEngine.UI;

public class ObjectiveObjectsIcon : MonoBehaviour
{
	[SerializeField]
	private Image moneyBag;

	[SerializeField]
	private Image sweets;

	private void Start()
	{
		sweets.enabled = false;
		moneyBag.enabled = false;
		((int)UserGameSettings.currentMode == 2 ? sweets : moneyBag).enabled = true;
	}
}
