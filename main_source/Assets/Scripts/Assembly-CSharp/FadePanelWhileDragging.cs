using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public sealed class FadePanelWhileDragging : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
{
	private const float FadedAlpha = 0.004f;

	private const float FadeSpeed = 8f;

	private const float NavigationHold = 0.5f;

	[SerializeField]
	private CanvasGroup panelGroup;

	private Slider slider;

	private bool pointerHeld;

	private float lastValueChange = -10f;

	public CanvasGroup PanelGroup
	{
		get => panelGroup;
		set => panelGroup = value;
	}

	private void Awake()
	{
		slider = GetComponent<Slider>();
		slider.onValueChanged.AddListener(delegate
		{
			lastValueChange = Time.unscaledTime;
		});
	}

	private void OnDisable()
	{
		pointerHeld = false;
		if (panelGroup != null)
		{
			panelGroup.alpha = 1f;
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		pointerHeld = true;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		pointerHeld = false;
	}

	private void Update()
	{
		if (panelGroup == null)
		{
			return;
		}
		EventSystem current = EventSystem.current;
		GameObject selected = current != null ? current.currentSelectedGameObject : null;
		bool navigating = selected == gameObject && Time.unscaledTime - lastValueChange < NavigationHold;
		float target = navigating || pointerHeld ? FadedAlpha : 1f;
		panelGroup.alpha = Mathf.MoveTowards(panelGroup.alpha, target, Time.unscaledDeltaTime * FadeSpeed);
	}
}
