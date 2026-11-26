using Prime31.MessageKit;
using UnityEngine;

public class JournalMultipleObjectsInteraction : Interactable
{
	[SerializeField]
	private JournalData journalEntry;

	[SerializeField]
	private Material materialAfterInteraction;

	[SerializeField]
	private JournalObjects journalObject;

	private int MessageId => journalObject == JournalObjects.SPEAKER ? 61 : 60;

	private new void Start()
	{
		if (journalEntry.IsUnlocked())
		{
			MakeNotInteractable();
		}
		if (journalObject == JournalObjects.SPEAKER || journalObject == JournalObjects.BELL)
		{
			MessageKit.addObserver(MessageId, MakeNotInteractable);
		}
	}

	public override void Clicked(bool isClickedByOtherPlayer)
	{
		if (!isClickedByOtherPlayer)
		{
			CustomSingleton<MultiplayerSettings>.Instance.SendInteraction(transform.position.sqrMagnitude, MultiplayerProtocol.InteractableIdCodes.GetInteractableId(GetType().Name));
		}
		if (journalEntry != null)
		{
			MessageKit<string>.post(3, journalEntry.GetShortText());
			journalEntry.Unlock();
			InteractionEnd();
			MessageKit.post(MessageId);
		}
	}

	private void MakeNotInteractable()
	{
		if (materialAfterInteraction != null)
		{
			GetComponent<Renderer>().material = materialAfterInteraction;
		}
		gameObject.layer = LayerMask.NameToLayer("Default");
	}

	protected override void OnDestroy()
	{
		MessageKit.removeObserver(MessageId, MakeNotInteractable);
		base.OnDestroy();
	}
}
