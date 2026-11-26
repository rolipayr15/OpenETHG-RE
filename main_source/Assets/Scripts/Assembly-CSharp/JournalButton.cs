using Bibloteka;
using Prime31.MessageKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public class JournalButton : HideWhenGamePaused
{

	private bool wasAnyJournalEntryUnlocked;

	private void Awake()
	{
		if (UserGameSettings.IsMultiplayer())
		{
			gameObject.SetActive(false);
			return;
		}
		wasAnyJournalEntryUnlocked = PlayerPrefz.GetBool("WAS_ANY_JOURNAL_ENTRY_UNLOCKED", false);
		if (!wasAnyJournalEntryUnlocked && TutorialManager.ShouldPlayTutorial() && !PlayerPrefz.GetBool("JOURNAL_TUTORIAL_COMPLETED", false))
		{
			StartCoroutine(ObserveJournalDelayed());
			Hide(true);
			isActive = false;
		}
	}

	private IEnumerator ObserveJournalDelayed()
	{
		yield return Utils.GetDelay(3f);
		MessageKit<string>.addObserver(41, ShowButton);
		if (!wasAnyJournalEntryUnlocked)
		{
			MessageKit<string>.addObserver(41, SetPlayerPrefs);
		}
	}

	private void SetPlayerPrefs(string ignore)
	{
		MessageKit<string>.removeObserver(41, SetPlayerPrefs);
		wasAnyJournalEntryUnlocked = true;
		PlayerPrefz.SetBool("WAS_ANY_JOURNAL_ENTRY_UNLOCKED", true);
	}

	private void ShowButton(string entryId)
	{
		MessageKit<string>.removeObserver(41, ShowButton);
		isActive = true;
		RefreshVisibility();
	}

	private void OnDestroy()
	{
		MessageKit<string>.removeObserver(41, ShowButton);
		MessageKit<string>.removeObserver(41, SetPlayerPrefs);
	}
}
