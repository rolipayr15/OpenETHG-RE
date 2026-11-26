using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class GameNewsWindow : MonoBehaviour
{
	private static bool isMessageBoxActive;

	[SerializeField]
	private GameObject gameNewsTemplate;

	[SerializeField]
	private ScrollRect newsScroll;

	public static void Show()
	{
		if (!isMessageBoxActive)
		{
			SceneManager.LoadSceneAsync("GameNews", LoadSceneMode.Additive);
			isMessageBoxActive = true;
		}
	}

	private void Start()
	{
		foreach (GameNewsItem item in GameNewsService.GetAll())
		{
			GameObject clone = Instantiate(gameNewsTemplate, gameNewsTemplate.transform.parent);
			LocalizedFontLayout.CopyBaselines(gameNewsTemplate, clone);
			clone.GetComponent<GameNewsTemplate>().Setup(item);
		}
		gameNewsTemplate.SetActive(false);
		Destroy(gameNewsTemplate);
		Canvas.ForceUpdateCanvases();
		LayoutRebuilder.ForceRebuildLayoutImmediate(newsScroll.content);
		newsScroll.StopMovement();
		newsScroll.verticalNormalizedPosition = 1f;
	}

	public void Close()
	{
		isMessageBoxActive = false;
		SceneManager.UnloadSceneAsync("GameNews");
	}
}
