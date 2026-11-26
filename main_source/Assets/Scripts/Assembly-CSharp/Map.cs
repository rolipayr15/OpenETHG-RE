using Prime31.MessageKit;
using UnityEngine;

public class Map : MonoBehaviour
{
	[SerializeField]
	private int mapFragmentId;

	private void OnTriggerEnter(Collider other)
	{
		if (other != null && other.CompareTag("Player"))
		{
			Eyes.DailyChallenges.DailyChallengeRuntime.MapFragmentDiscovered(this, mapFragmentId);
			MessageKit<int>.post(35, mapFragmentId);
			gameObject.SetActive(false);
		}
	}
}
