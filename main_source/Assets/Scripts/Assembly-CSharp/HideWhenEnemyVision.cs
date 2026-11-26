using Prime31.MessageKit;
using UnityEngine;
using UnityEngine.UI;

public class HideWhenEnemyVision : MonoBehaviour
{
	[SerializeField]
	private Image[] imagesToHide;

	private Button buttonToDisable;

	[SerializeField]
	private Text textToHide;

	private void Start()
	{
		buttonToDisable = GetComponent<Button>();
		MessageKit<bool>.addObserver(7, HideOrShow);
	}

	public virtual void HideOrShow(bool isEnemyVisionActive)
	{
		foreach (Image image in imagesToHide)
		{
			image.enabled = !isEnemyVisionActive;
		}
		buttonToDisable.interactable = !isEnemyVisionActive;
		if (textToHide != null)
		{
			textToHide.enabled = !isEnemyVisionActive;
		}
	}
}
