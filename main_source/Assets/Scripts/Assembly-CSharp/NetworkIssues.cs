using UnityEngine.SceneManagement;
using UnityEngine;

public class NetworkIssues : MonoBehaviour
{
	private static bool isQuestionBoxActive;

	public static void Show()
	{
		if (!isQuestionBoxActive)
		{
			SceneManager.LoadSceneAsync("NetworkIssues", LoadSceneMode.Additive);
			isQuestionBoxActive = true;
		}
	}

	public void Close()
	{
		isQuestionBoxActive = false;
		SceneManager.UnloadSceneAsync("NetworkIssues", UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
	}

	private void Update()
	{
		if (!TextEntryInputGuard.BlocksShortcuts && Input.GetKeyDown(KeyCode.Escape))
		{
			Close();
		}
	}
}
