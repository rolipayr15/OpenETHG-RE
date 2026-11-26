using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class MenuLinksDropdown : MonoBehaviour
{
	[SerializeField]
	private GameObject links;

	[SerializeField]
	private Image buttonImage;

	[SerializeField]
	private Sprite closedSprite;

	[SerializeField]
	private Sprite openSprite;

	[SerializeField]
	private GameObject[] hamburgerLines;

	[SerializeField]
	private GameObject unreadNewsBadge;

	public bool IsOpen => links != null && links.activeSelf;

	private void OnEnable()
	{
		Close();
		GameNewsService.NewsChanged += RefreshNewsBadge;
		RefreshNewsBadge();
	}

	private void OnDisable()
	{
		GameNewsService.NewsChanged -= RefreshNewsBadge;
		Close();
	}

	private void RefreshNewsBadge()
	{
		if (unreadNewsBadge != null)
		{
			unreadNewsBadge.SetActive(GameNewsService.HasUnread);
		}
	}

	public void Toggle()
	{
		if (IsOpen)
		{
			Close();
		}
		else if (links != null)
		{
			links.SetActive(true);
			UpdateIcon();
		}
	}

	public void Close()
	{
		if (links == null)
		{
			return;
		}
		EventSystem eventSystem = EventSystem.current;
		GameObject selected = eventSystem != null ? eventSystem.currentSelectedGameObject : null;
		// Keep gamepad focus on the button instead of a link that is about to disappear.
		if (isActiveAndEnabled && selected != null && selected.transform.IsChildOf(links.transform))
		{
			eventSystem.SetSelectedGameObject(gameObject);
		}
		links.SetActive(false);
		UpdateIcon();
	}

	private void UpdateIcon()
	{
		bool open = IsOpen;
		if (buttonImage != null)
		{
			buttonImage.sprite = open ? openSprite : closedSprite;
		}
		if (hamburgerLines == null)
		{
			return;
		}
		foreach (GameObject line in hamburgerLines)
		{
			if (line != null)
			{
				line.SetActive(!open);
			}
		}
	}
}
