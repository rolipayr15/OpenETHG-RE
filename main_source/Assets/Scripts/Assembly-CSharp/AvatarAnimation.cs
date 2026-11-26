using Prime31.MessageKit;
using UnityEngine;

public class AvatarAnimation : MonoBehaviour
{
	private Animator anim;

	private void Start()
	{
		anim = GetComponent<Animator>();
		MessageKit.addObserver(81, Interact);
	}

	private void Interact()
	{
		anim.SetTrigger("Pickup");
	}

	private void OnDestroy()
	{
		MessageKit.removeObserver(81, Interact);
	}
}
