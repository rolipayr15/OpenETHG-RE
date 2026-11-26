using DarkTonic.MasterAudio;
using UnityEngine;

public class Clock : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (other != null && other.CompareTag("Player"))
		{
			MasterAudio.PlaySound3DAtTransform("fx", gameObject.transform, 1f, null, 0f, "clockTicking");
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other != null && other.CompareTag("Player"))
		{
			MasterAudio.StopAllSoundsOfTransform(gameObject.transform);
		}
	}
}
