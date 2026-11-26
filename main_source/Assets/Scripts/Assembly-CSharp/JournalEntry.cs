using UnityEngine;

public class JournalEntry : Interactable
{
	[SerializeField]
	private JournalEntryId entryId;

	[SerializeField]
	private JournalData jorunalEntry;

	private void Awake()
	{
	}

	public override void Clicked(bool isClickedByOtherPlayer)
	{
	}

	private void UnlockJournalEntry(string id)
	{
	}
}
