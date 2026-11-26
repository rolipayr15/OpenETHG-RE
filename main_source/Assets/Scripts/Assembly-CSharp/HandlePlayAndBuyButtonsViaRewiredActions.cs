using RewiredConsts;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

// Gamepad "Play" on the chapter carousel presses the current chapter's play button.
public class HandlePlayAndBuyButtonsViaRewiredActions : MonoBehaviour
{
	[SerializeField]
	private ScrollSnapBase chaptersScroll;

	private StoreEntry[] allChapters;

	private void Start()
	{
		allChapters = GetComponentsInChildren<StoreEntry>();
	}

	private void Update()
	{
		bool play = InteractionManager.GetButtonDown(Action.Play);
		bool buy = InteractionManager.GetButtonDown(Action.BuyWithCoins);
		if (!play && !buy)
		{
			return;
		}
		StoreEntry chapter = chaptersScroll.ChildObjects[chaptersScroll._currentPage].GetComponent<StoreEntry>();
		if (chapter == null)
		{
			return;
		}
		Button playButton = chapter.GetButtonPlay().GetComponent<Button>();
		if (play && playButton != null)
		{
			playButton.onClick.Invoke();
		}
	}
}
