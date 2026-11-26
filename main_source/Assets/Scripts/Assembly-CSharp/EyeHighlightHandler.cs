using Prime31.MessageKit;
using UnityEngine;

public class EyeHighlightHandler : MonoBehaviour
{
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private Animator meatAnimator;

	private void Start()
	{
		MessageKit.addObserver(16, HighlightEye);
		MessageKit.addObserver(1, HighlightEye);
		if ((int)UserGameSettings.currentEnemy == 2)
		{
			MessageKit.addObserver(28, HighlightMeat);
		}
		else
		{
			meatAnimator.gameObject.SetActive(false);
		}
	}

	private void OnDestroy()
	{
		MessageKit.removeObserver(16, HighlightEye);
		MessageKit.removeObserver(1, HighlightEye);
		MessageKit.removeObserver(28, HighlightMeat);
	}

	private void HighlightEye()
	{
		animator.SetTrigger("highlight");
	}

	private void HighlightMeat()
	{
		meatAnimator.SetTrigger("highlight");
	}
}
