using UnityEngine;

public class ButtonWebsite : MonoBehaviour
{
	public void OpenWebsite()
	{
		Application.OpenURL("https://eyesthehorrorgame.com/");
	}

	public void OpenStore()
	{
		Application.OpenURL("https://shop.eyesthehorrorgame.com/");
	}

	public void OpenDiscord()
	{
		Application.OpenURL(UserGameSettings.discordUrl);
	}
}
