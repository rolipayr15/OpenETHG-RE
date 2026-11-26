using Prime31.MessageKit;
using UnityEngine;

public class ActivateEnemy : MonoBehaviour
{
	private void OnTriggerEnter(Collider coll)
	{
		if (coll.CompareTag("Player") || coll.CompareTag("Avatar"))
		{
			EyesAnalytics.Game.NoteActivationCause((EyesAnalytics.ActivationCause)2);
			MessageKit.post(25);
		}
	}
}
