using EyesAnalytics;
using UnityEngine.SceneManagement;
using System.Reflection;
using UnityEngine;

[Obfuscation(Exclude = false)]
public class GameOverButtons : MonoBehaviour
{
	[SerializeField]
	private GameObject playAgainButton;

	private void Start()
	{
		Screens.View((ScreenId)18);
		if (UserGameSettings.isOneTimeUnlocked)
		{
			playAgainButton.SetActive(false);
			UserGameSettings.isOneTimeUnlocked = false;
		}
	}

	public void PlayAgain()
	{
		Ads.SetNextInterstitialTrigger((AdTrigger)2);
		SceneManager.LoadScene("PreGameAd");
	}

	public void BackToMenu()
	{
		Ads.SetNextInterstitialTrigger((AdTrigger)4);
		SceneManager.LoadScene("PostGameAd");
	}
}
