using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public sealed class SafeAreaPanel : MonoBehaviour
{
	[SerializeField]
	[Tooltip("On iPhone in landscape, apply the horizontal inset only on the notch side.")]
	private bool landscapeNotchSideOnly;

	private RectTransform panel;

	private void OnEnable()
	{
		panel = GetComponent<RectTransform>();
		Canvas.willRenderCanvases += Refresh;
		Refresh();
	}

	private void OnDisable()
	{
		Canvas.willRenderCanvases -= Refresh;
	}

	private void LateUpdate()
	{
		Refresh();
	}

	private void Refresh()
	{
		if (!TryGetSafeAreaAnchors(out Vector2 anchorMin, out Vector2 anchorMax))
		{
			return;
		}
		if (panel.anchorMin == anchorMin && panel.anchorMax == anchorMax && panel.offsetMin == Vector2.zero && panel.offsetMax == Vector2.zero)
		{
			return;
		}
		panel.anchorMin = anchorMin;
		panel.anchorMax = anchorMax;
		panel.offsetMin = Vector2.zero;
		panel.offsetMax = Vector2.zero;
	}

	private bool TryGetSafeAreaAnchors(out Vector2 anchorMin, out Vector2 anchorMax)
	{
		int width = Screen.width;
		int height = Screen.height;
		Rect safeArea = Screen.safeArea;
		if (width > 0 && height > 0 && safeArea.width > 0f && safeArea.height > 0f)
		{
			anchorMin = new Vector2(safeArea.x / width, safeArea.y / height);
			anchorMax = new Vector2((safeArea.x + safeArea.width) / width, (safeArea.y + safeArea.height) / height);
			return true;
		}
		anchorMin = Vector2.zero;
		anchorMax = Vector2.zero;
		return false;
	}
}
