using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class JournalEntryGUI : MonoBehaviour
{
	[SerializeField]
	private Text title;

	[SerializeField]
	private Text description;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private GameObject ShowWhenLocked;

	[SerializeField]
	private GameObject ShowWhenUnlocked;

	private JournalData journalData;

	private bool isUnlocked;

	public bool IsUnlocked => isUnlocked;

	public bool OpensDetails(Selectable selectable)
	{
		Button button = selectable as Button;
		if (button == null || !button.transform.IsChildOf(transform))
		{
			return false;
		}
		for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
		{
			if (button.onClick.GetPersistentTarget(i) == this && button.onClick.GetPersistentMethodName(i) == "ShowDetails")
			{
				return true;
			}
		}
		return false;
	}

	public Selectable DetailsButton()
	{
		return GamepadSelectionScope.FindPersistentButton(gameObject, this, "ShowDetails");
	}

	public void SetupFromJournalData(JournalData jd)
	{
		journalData = jd;
		title.gameObject.GetComponent<Localize>().SetTerm(journalData.GetShortTranslationKey());
		description.gameObject.GetComponent<Localize>().SetTerm(journalData.GetLongTranslationKey());
		icon.sprite = journalData.GetImage();
		if (journalData.IsUnlocked())
		{
			isUnlocked = true;
			DisplayAsUnlocked();
		}
		else
		{
			DisplayAsLocked();
		}
	}

	public void UnlockEntry()
	{
		isUnlocked = true;
		DisplayAsUnlocked();
		JournalWindow.Instance.SetLastUnlockedItem(transform.GetSiblingIndex());
	}

	private void DisplayAsLocked()
	{
		ShowWhenUnlocked.SetActive(false);
		ShowWhenLocked.SetActive(true);
	}

	private void DisplayAsUnlocked()
	{
		ShowWhenUnlocked.SetActive(true);
		ShowWhenLocked.SetActive(false);
	}

	public void ShowDetails()
	{
		if (isUnlocked)
		{
			JournalEntryBox.Show(title.text, description.text, icon.sprite);
		}
	}
}
