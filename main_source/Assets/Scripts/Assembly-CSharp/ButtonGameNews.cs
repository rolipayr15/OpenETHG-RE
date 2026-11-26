using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ButtonGameNews : MonoBehaviour
{
	[SerializeField]
	private Image newsIcon;

	[Header("Unread state")]
	[SerializeField]
	private Color unreadColor = new Color(0.65f, 0.05f, 0.05f, 1f);

	[Header("Heartbeat animation")]
	[SerializeField]
	private float firstBeatScale = 1.14f;

	[SerializeField]
	private float secondBeatScale = 1.08f;

	[SerializeField]
	private float beatUpDuration = 0.09f;

	[SerializeField]
	private float beatDownDuration = 0.11f;

	[SerializeField]
	private float delayBetweenBeats = 0.1f;

	[SerializeField]
	private float delayBetweenCycles = 1.15f;

	private Color defaultColor;

	private Vector3 defaultScale;

	private Coroutine animationCoroutine;

	private void Awake()
	{
		if (newsIcon == null)
		{
			newsIcon = GetComponent<Image>();
		}
		if (newsIcon == null)
		{
			Debug.LogError("[GameNews] GameNewsButtonIndicator requires an Image.", this);
			enabled = false;
			return;
		}
		defaultColor = newsIcon.color;
		defaultScale = newsIcon.rectTransform.localScale;
	}

	private void OnEnable()
	{
		GameNewsService.NewsChanged += Refresh;
		Refresh();
	}

	private void OnDisable()
	{
		GameNewsService.NewsChanged -= Refresh;
		StopAnimation();
	}

	private void OnDestroy()
	{
		GameNewsService.NewsChanged -= Refresh;
	}

	private void Refresh()
	{
		bool hasUnread = GameNewsService.HasUnread;
		if (hasUnread)
		{
			ShowUnreadState();
		}
		else
		{
			ShowReadState();
		}
		Debug.Log("zrobilem refresh " + hasUnread);
	}

	private void ShowUnreadState()
	{
		newsIcon.color = unreadColor;
		if (animationCoroutine == null)
		{
			animationCoroutine = StartCoroutine(AnimateUnreadIcon());
		}
	}

	private void ShowReadState()
	{
		StopAnimation();
		newsIcon.color = defaultColor;
		newsIcon.rectTransform.localScale = defaultScale;
	}

	// Heartbeat: a strong beat, a short pause, a weaker beat, then a longer rest.
	private IEnumerator AnimateUnreadIcon()
	{
		RectTransform iconTransform = newsIcon.rectTransform;
		Vector3 firstScale = defaultScale * firstBeatScale;
		Vector3 secondScale = defaultScale * secondBeatScale;
		while (true)
		{
			yield return AnimateScale(iconTransform, defaultScale, firstScale, beatUpDuration);
			yield return AnimateScale(iconTransform, firstScale, defaultScale, beatDownDuration);
			yield return new WaitForSecondsRealtime(delayBetweenBeats);
			yield return AnimateScale(iconTransform, defaultScale, secondScale, beatUpDuration);
			yield return AnimateScale(iconTransform, secondScale, defaultScale, beatDownDuration);
			yield return new WaitForSecondsRealtime(delayBetweenCycles);
		}
	}

	private static IEnumerator AnimateScale(RectTransform target, Vector3 from, Vector3 to, float duration)
	{
		if (duration > 0f)
		{
			float elapsed = 0f;
			while (elapsed < duration)
			{
				elapsed += Time.unscaledDeltaTime;
				float t = Mathf.Clamp01(elapsed / duration);
				t = t * t * (3f - 2f * t);
				target.localScale = Vector3.LerpUnclamped(from, to, t);
				yield return null;
			}
		}
		target.localScale = to;
	}

	private void StopAnimation()
	{
		if (animationCoroutine != null)
		{
			StopCoroutine(animationCoroutine);
			animationCoroutine = null;
		}
		if (newsIcon != null)
		{
			newsIcon.rectTransform.localScale = defaultScale;
		}
	}

	public void ShowGameNews()
	{
		GameNewsWindow.Show();
	}
}
