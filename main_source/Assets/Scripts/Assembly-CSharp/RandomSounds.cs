using DarkTonic.MasterAudio;
using UnityEngine;

public class RandomSounds : MonoBehaviour
{
	[SerializeField]
	private string soundGroupName;

	private int counter;

	private void OnTriggerEnter(Collider other)
	{
		if (other != null && other.CompareTag("Player") && Random.value > 0.5f && counter < 2)
		{
			counter++;
			MasterAudio.PlaySound3DAtTransform(soundGroupName, gameObject.transform);
		}
	}
}
