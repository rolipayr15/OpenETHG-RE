using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadMenu : MonoBehaviour
{
	private float lastProgress;

	public Text label;

	public Image image;

	private StringBuilder sb;

	private void Start()
	{
		Time.timeScale = 1f;
		sb = new StringBuilder();
		StartCoroutine(LoadLevel());
	}

	private IEnumerator LoadLevel()
	{
		AsyncOperation async = SceneManager.LoadSceneAsync("Menu");
		while (!async.isDone)
		{
			yield return UpdateLabel(async.progress);
		}
	}

	private IEnumerator UpdateLabel(float progress)
	{
		if (lastProgress < progress)
		{
			image.fillAmount = progress;
			sb.Length = 0;
			sb.AppendFormat("{0}%", (int)(progress * 100f));
			label.text = sb.ToString();
			lastProgress = progress;
		}
		yield return null;
	}
}
