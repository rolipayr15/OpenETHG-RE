using DarkTonic.MasterAudio;
using UnityEngine;

public class TotemSound : MonoBehaviour
{
	private void OnTriggerEnter()
	{
		MasterAudio.PlaySound3DAtTransform("fx", gameObject.transform, 1f, null, 0f, "totem_loop");
	}

	private void OnTriggerExit()
	{
		MasterAudio.StopAllSoundsOfTransform(transform);
	}

	public void StopSoundLoop()
	{
		MasterAudio.FadeOutAllSoundsOfTransform(transform, 1f);
	}
}
