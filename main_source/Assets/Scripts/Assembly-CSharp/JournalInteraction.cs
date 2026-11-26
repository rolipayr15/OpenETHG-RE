using Prime31.MessageKit;
using UnityEngine;

public class JournalInteraction : Interactable
{
	[SerializeField]
	private JournalData journalEntry;

	[SerializeField]
	private Material materialAfterInteraction;

	private new void Start()
	{
		if (journalEntry.IsUnlocked())
		{
			MakeNotInteractable();
		}
	}

	public override void Clicked(bool isClickedByOtherPlayer)
	{
		if (!isClickedByOtherPlayer)
		{
			CustomSingleton<MultiplayerSettings>.Instance.SendInteraction(transform.position.sqrMagnitude, MultiplayerProtocol.InteractableIdCodes.GetInteractableId(GetType().Name));
			if (!journalEntry.IsUnlocked())
			{
				EyesAnalytics.Game.CountJournalUnlock();
			}
		}
		MessageKit<string>.post(3, journalEntry.GetShortText());
		journalEntry.Unlock();
		MakeNotInteractable();
		InteractionEnd();
	}

	private void MakeNotInteractable()
	{
		if (materialAfterInteraction != null)
		{
			GetComponent<Renderer>().material = materialAfterInteraction;
		}
		gameObject.layer = LayerMask.NameToLayer("Default");
	}
}
