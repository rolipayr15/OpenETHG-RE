using UnityEngine;

public class BrokenTVTextEffect : MonoBehaviour
{
	[Header("Shake Settings")]
	[Tooltip("Maximum horizontal shake distance.")]
	public float shakeAmountX = 5f;

	[Tooltip("Maximum vertical shake distance.")]
	public float shakeAmountY = 1f;

	[Tooltip("How frequently the position updates (in seconds). 0 = every frame.")]
	public float updateInterval = 0.05f;

	private RectTransform rectTransform;

	private Vector2 originalPosition;

	private float timer;

	private void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
	}

	private void OnEnable()
	{
		if (rectTransform != null)
		{
			originalPosition = rectTransform.anchoredPosition;
		}
	}

	private void Update()
	{
		if (rectTransform == null)
		{
			return;
		}
		timer += Time.deltaTime;
		if (updateInterval <= timer)
		{
			timer = 0f;
			float x = Random.Range(-shakeAmountX, shakeAmountX);
			float y = Random.Range(-shakeAmountY, shakeAmountY);
			rectTransform.anchoredPosition = new Vector2(x + originalPosition.x, y + originalPosition.y);
		}
	}

	private void OnDisable()
	{
		if (rectTransform != null)
		{
			rectTransform.anchoredPosition = originalPosition;
		}
	}
}
