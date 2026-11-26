using System.Collections;
using UnityEngine;

public class EpilepsyWarning : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup canvasGroup;

	private void Start()
	{
		canvasGroup.alpha = 0f;
	}

	public IEnumerator Show()
	{
		yield return FadeCanvasGroup(canvasGroup, 1f, 0f, 1f);
		yield return new WaitForSecondsRealtime(1f);
		yield return FadeCanvasGroup(canvasGroup, 1f, 1f, 0f);
	}

	public static IEnumerator FadeCanvasGroup(CanvasGroup cg, float duration, float from, float to)
	{
		float t = 0f;
		float inverseDuration = 1f / duration;
		cg.alpha = from;
		while (t < duration)
		{
			t += Time.unscaledDeltaTime;
			cg.alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t * inverseDuration));
			yield return null;
		}
		cg.alpha = to;
	}
}
