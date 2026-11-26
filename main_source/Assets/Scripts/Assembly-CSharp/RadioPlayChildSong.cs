using DarkTonic.MasterAudio;
using UnityEngine;

public class RadioPlayChildSong : Interactable
{
	private bool isPlaying;

	private bool isFirstTime = true;

	[SerializeField]
	private Material materialAfterInteraction;

	public override void Clicked(bool isClickedByOtherPlayer)
	{
		if (isFirstTime)
		{
			GetComponent<MeshRenderer>().material = materialAfterInteraction;
			isFirstTime = false;
		}
		if (!isPlaying)
		{
			MasterAudio.PlaySound3DAtTransform("school", transform, 1f, null, 0f, "abc");
			isPlaying = true;
		}
		else
		{
			MasterAudio.StopAllSoundsOfTransform(transform);
			isPlaying = false;
		}
		InteractionEnd();
	}
}
