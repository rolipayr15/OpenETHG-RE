using Prime31.MessageKit;
using UnityEngine;
using UnityEngine.UI;

public class MonstersVoiceLabel : MonoBehaviour
{
	[SerializeField]
	private Text textField;

	[SerializeField]
	private GameObject background;

	private bool isActive;

	private Coroutine showLabelCoroutine;

	private float startTime;

	private void Start()
	{
		MessageKit<string>.addObserver(45, ShowLabel);
	}

	private void ShowLabel(string txt)
	{
		isActive = true;
		textField.text = txt;
		startTime = Time.time;
		background.SetActive(true);
		MessageKit<bool>.addObserver(7, GhostVision);
	}

	private void Update()
	{
		if (isActive && startTime + 5f < Time.time)
		{
			isActive = false;
			textField.text = "";
			background.SetActive(false);
		}
	}

	private void GhostVision(bool _isActive)
	{
		isActive = _isActive;
		textField.text = "";
		background.SetActive(false);
		MessageKit<bool>.removeObserver(7, GhostVision);
	}
}
