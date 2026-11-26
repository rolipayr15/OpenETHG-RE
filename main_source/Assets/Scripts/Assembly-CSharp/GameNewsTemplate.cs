using UnityEngine;
using UnityEngine.UI;

public class GameNewsTemplate : MonoBehaviour
{
	[SerializeField]
	private Text title;

	[SerializeField]
	private Text body;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private Sprite infoIcon;

	[SerializeField]
	private Sprite updateIcon;

	private GameNewsItem _newsItem;

	public void Setup(GameNewsItem newsItem)
	{
		_newsItem = newsItem;
		if (newsItem != null)
		{
			title.text = newsItem.title;
			body.text = newsItem.body;
			DisplayAsRead(newsItem.isRead);
			icon.sprite = GetCategoryIcon(newsItem.GetCategory());
		}
	}

	private Sprite GetCategoryIcon(GameNewsCategory category)
	{
		return (int)category == 1 ? updateIcon : infoIcon;
	}

	private void DisplayAsRead(bool wasRead)
	{
		Color color = wasRead ? Color.grey : Color.white;
		title.color = color;
		body.color = color;
	}

	public void ShowDetails()
	{
		JournalEntryBox.Show(title.text, body.text, icon.sprite);
		if (_newsItem != null && GameNewsService.MarkAsRead(_newsItem.id))
		{
			DisplayAsRead(true);
		}
	}
}
