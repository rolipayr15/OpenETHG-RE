using Prime31.MessageKit;
using UnityEngine;

public class SprintAnimatorHandler : MonoBehaviour
{
	[SerializeField]
	private Animator animator;

	private void Start()
	{
		MessageKit<bool>.addObserver(13, HighlightSprint);
	}

	private void HighlightSprint(bool effectStarting)
	{
		if (effectStarting)
		{
			animator.SetTrigger("highlight");
		}
	}

	private void OnDestroy()
	{
		MessageKit<bool>.removeObserver(13, HighlightSprint);
	}
}
