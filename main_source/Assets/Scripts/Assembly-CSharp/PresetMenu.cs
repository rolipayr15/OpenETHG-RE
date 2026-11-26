using Prime31.MessageKit;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class PresetMenu : MonoBehaviour
{
	[SerializeField]
	private HorizontalScrollSnap scroll;

	private bool isInitialValueAlreadyRead;

	private void Awake()
	{
		int last = PlayerPrefs.GetInt("LAST_PRESET_CHOSEN", 0);
		scroll.StartingScreen = last;
		scroll.GoToScreen(last);
		isInitialValueAlreadyRead = true;
		MessageKit<string>.addObserver(111, ContentLocked);
	}

	private void OnDestroy()
	{
		MessageKit<string>.removeObserver(111, ContentLocked);
	}

	public void SetLastPresetChosen(int page)
	{
		if (isInitialValueAlreadyRead)
		{
			PlayerPrefs.SetInt("LAST_PRESET_CHOSEN", page);
		}
	}

	public void SelectChapter(StoreItems chapter)
	{
		int page;
		if (chapter == StoreItems.CHARLIE_HOSPITAL)
		{
			page = 1;
		}
		else if (chapter == StoreItems.GOOD_BOY_SCHOOL)
		{
			page = 2;
		}
		else
		{
			return;
		}
		if (scroll == null)
		{
			Debug.LogWarning("[Push] Cannot select the rewarded chapter: menu scroll is missing.");
			return;
		}
		scroll.StartingScreen = page;
		scroll.GoToScreen(page);
		SetLastPresetChosen(page);
	}

	private void ContentLocked(string content)
	{
		scroll.GoToScreen(content.ToLower() == "hospital" ? 1 : 2);
	}
}
