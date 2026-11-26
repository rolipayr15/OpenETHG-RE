using DarkTonic.MasterAudio;
using UnityEngine;

public class FxRunning : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (other != null && other.CompareTag("Player"))
		{
			MasterAudio.PlaySound3DAtTransform("fx", gameObject.transform, 1f, null, 0f, "running");
			gameObject.SetActive(false);
		}
	}
}
