using System;

[Serializable]
public sealed class GameNewsItem
{
	public string id;

	public string publishAt;

	public string title;

	public string body;

	public bool showAsPopup;

	public bool isRead;

	public string category;

	public GameNewsCategory GetCategory()
	{
		return Enum.TryParse<GameNewsCategory>(category, true, out GameNewsCategory result) ? result : GameNewsCategory.Info;
	}
}
