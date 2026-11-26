using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Toggle))]
public sealed class ToggleSwitchVisual : MonoBehaviour
{
	[SerializeField]
	private Image background;

	[SerializeField]
	private RectTransform handle;

	[SerializeField]
	private Sprite backgroundOn;

	[SerializeField]
	private Sprite backgroundOff;

	[SerializeField]
	private Color backgroundOnTint = Color.white;

	[SerializeField]
	private Color backgroundOffTint = Color.white;

	[SerializeField]
	private float handleInset = 1f;

	private Toggle toggle;

	private void Awake()
	{
		toggle = GetComponent<Toggle>();
	}

	private void OnEnable()
	{
		if (toggle == null)
		{
			toggle = GetComponent<Toggle>();
		}
		toggle.onValueChanged.AddListener(UpdateVisual);
		UpdateVisual(toggle.isOn);
	}

	private void OnDisable()
	{
		if (toggle != null)
		{
			toggle.onValueChanged.RemoveListener(UpdateVisual);
		}
	}

	private void UpdateVisual(bool isOn)
	{
		if (background != null)
		{
			background.sprite = isOn ? backgroundOn : backgroundOff;
			background.color = isOn ? backgroundOnTint : backgroundOffTint;
		}
		if (handle != null)
		{
			float x = isOn ? 1f : 0f;
			handle.anchorMin = new Vector2(x, 0f);
			handle.anchorMax = new Vector2(x, 1f);
			handle.pivot = new Vector2(x, 0.5f);
			handle.anchoredPosition = new Vector2(isOn ? -handleInset : handleInset, 0f);
		}
	}
}
