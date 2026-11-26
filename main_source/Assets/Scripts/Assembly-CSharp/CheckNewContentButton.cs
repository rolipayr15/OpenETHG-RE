using UnityEngine;
using UnityEngine.UI.Extensions;

public class CheckNewContentButton : MonoBehaviour
{
	[SerializeField]
	private HorizontalScrollSnap chaptersScroll;

	private void Start()
	{
	}

	public void SetChapterToNewContent()
	{
		chaptersScroll.GoToScreen(PresetPageTable.IndexOf(PresetPage.Multiplayer));
	}
}
