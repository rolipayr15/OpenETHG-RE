using Prime31.MessageKit;
using UnityEngine;
using UnityEngine.UI;

public class FeedbackLabel : MonoBehaviour
{
	[SerializeField]
	private Text textField;

	[SerializeField]
	private GameObject background;

	private bool isActive;

	private Coroutine showLabelCoroutine;

	[SerializeField]
	private GameObject avatar;

	private float startTime;

	private void Start()
	{
		MessageKit<string, bool>.addObserver(3, ShowLabel);
		MessageKit<string>.addObserver(3, ShowLabel);
		enabled = false;
	}

	private void ShowLabel(string txt)
	{
		ShowLabel(txt, false);
	}

	private void ShowLabel(string txt, bool isRemote)
	{
		enabled = true;
		isActive = true;
		textField.text = txt;
		startTime = Time.time;
		background.SetActive(true);
		if (isRemote)
		{
			avatar.SetActive(true);
			if (UserGameSettings.multiplayerSecondPlayerUsername != "")
			{
				textField.text = UserGameSettings.multiplayerSecondPlayerUsername + ": " + txt;
			}
		}
	}

	private void Update()
	{
		if (isActive && startTime + 3f < Time.time)
		{
			isActive = false;
			textField.text = "";
			background.SetActive(false);
			avatar.SetActive(false);
			enabled = false;
		}
	}
}
