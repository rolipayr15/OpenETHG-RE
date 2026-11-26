using Prime31.MessageKit;
using DarkTonic.MasterAudio;
using UnityEngine;

public class Globe : Interactable
{
	[SerializeField]
	private Animation anim;

	[SerializeField]
	private JournalData journalEntry;

	[SerializeField]
	private Material materialAfterInteraction;

	public override void Clicked(bool isClickedByOtherPlayer)
	{
		base.Clicked(isClickedByOtherPlayer);
		anim.Play();
		if (!isClickedByOtherPlayer && !journalEntry.IsUnlocked())
		{
			journalEntry.Unlock();
			MessageKit<string>.post(3, journalEntry.GetShortText());
			MasterAudio.PlaySound3DAtTransform("fx", transform, 1f, null, 0f, "paper");
		}
		if (materialAfterInteraction != null)
		{
			GetComponent<MeshRenderer>().material = materialAfterInteraction;
		}
		InteractionEnd();
	}
}
