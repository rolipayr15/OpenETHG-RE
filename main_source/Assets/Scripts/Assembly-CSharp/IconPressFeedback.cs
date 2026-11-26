using System.Collections;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Menu icon "press" juice: squash, blood-red flash and an expanding echo, also triggered by gamepad shortcuts.
[RequireComponent(typeof(RectTransform))]
public class IconPressFeedback : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerExitHandler
{
	[ActionIdProperty(typeof(RewiredConsts.Action))]
	[SerializeField]
	private int rewiredAction = -1;

	[SerializeField]
	private bool delaysUse;

	[SerializeField]
	private float useDelay = 0.12f;

	[SerializeField]
	private Color bloodColor = new Color(0.6f, 0.02f, 0.02f, 1f);

	[SerializeField]
	[Range(0f, 1f)]
	private float flashAlpha = 0.55f;

	[SerializeField]
	[Range(0f, 1f)]
	private float echoAlpha = 0.3f;

	[SerializeField]
	private float flashRise = 0.25f;

	[SerializeField]
	private float pressedScale = 0.86f;

	[SerializeField]
	private float popScale = 1.12f;

	[SerializeField]
	private float echoScale = 1.45f;

	[SerializeField]
	private float pressDuration = 0.07f;

	[SerializeField]
	private float releaseDuration = 0.3f;

	[SerializeField]
	private float echoDuration = 0.38f;

	// Fraction of the release spent overshooting to popScale before settling back to 1.
	private const float PopShare = 0.4f;

	private Image icon;

	private RectTransform target;

	private Selectable selectable;

	private Canvas[] canvases;

	private Vector3 restScale;

	private Vector3 restPosition;

	private Vector4 restPadding;

	private Image flash;

	private Image echo;

	private bool pressed;

	private bool releaseAfterPress;

	private bool animating;

	private float pressTime;

	private float releaseTime;

	private float echoTime;

	private float scale = 1f;

	private float pressStartScale = 1f;

	private float releaseStartScale = 1f;

	private float flashLevel;

	private float releaseStartFlash;

	private int pointerUpFrame = -1;

	private Coroutine pendingUse;

	private static readonly List<IconPressFeedback> enabledIcons = new List<IconPressFeedback>();

	// Plays the press animation of the delayed-use icon bound to this action, then runs use; runs it at once if none matches.
	public static void PlayThen(int action, System.Action use)
	{
		foreach (IconPressFeedback feedback in enabledIcons)
		{
			if (feedback.delaysUse && feedback.rewiredAction == action && feedback.CanPlay())
			{
				feedback.Use(use, true);
				return;
			}
		}
		use();
	}

	private void Awake()
	{
		icon = GetComponent<Image>();
		selectable = GetComponent<Selectable>();
		if (icon == null && selectable != null)
		{
			icon = selectable.targetGraphic as Image;
		}
		target = icon != null ? icon.rectTransform : (RectTransform)transform;
		canvases = GetComponentsInParent<Canvas>(true);
		Button button = selectable as Button;
		if (delaysUse && button != null)
		{
			// Re-route the button so its original listeners fire only after the press animation.
			Button.ButtonClickedEvent original = button.onClick;
			button.onClick = new Button.ButtonClickedEvent();
			button.onClick.AddListener(delegate
			{
				Use(original.Invoke, pointerUpFrame != Time.frameCount);
			});
		}
	}

	private void OnEnable()
	{
		enabledIcons.Add(this);
	}

	private void OnDisable()
	{
		enabledIcons.Remove(this);
		pendingUse = null;
		ResetToRest();
	}

	private void Use(System.Action use, bool pulse)
	{
		if (pendingUse != null)
		{
			return;
		}
		if (pulse)
		{
			Press(true);
		}
		pendingUse = StartCoroutine(UseAfterDelay(use));
	}

	private IEnumerator UseAfterDelay(System.Action use)
	{
		yield return new WaitForSecondsRealtime(useDelay);
		pendingUse = null;
		use();
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			Press(false);
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		pointerUpFrame = Time.frameCount;
		Release();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Release();
	}

	public void Pulse()
	{
		Press(true);
	}

	private void Press(bool autoRelease)
	{
		if (!CanPlay())
		{
			return;
		}
		if (!animating)
		{
			restScale = target.localScale;
			restPosition = target.localPosition;
			if (icon != null)
			{
				restPadding = icon.raycastPadding;
			}
		}
		pressTime = Time.unscaledTime;
		releaseAfterPress = autoRelease;
		pressed = true;
		echoTime = pressTime;
		pressStartScale = scale;
		animating = true;
		ShowOverlays();
	}

	private void Release()
	{
		if (pressed)
		{
			pressed = false;
			releaseTime = Time.unscaledTime;
			releaseStartScale = scale;
			releaseStartFlash = flashLevel;
		}
	}

	private void Update()
	{
		PollShortcut();
		if (!animating)
		{
			return;
		}
		if (!IsVisible())
		{
			ResetToRest();
			return;
		}
		float now = Time.unscaledTime;
		if (pressed && releaseAfterPress && now - pressTime >= pressDuration)
		{
			Release();
		}
		bool released;
		float newScale;
		float newFlash;
		if (pressed)
		{
			float elapsed = now - pressTime;
			float press = Mathf.Clamp01(elapsed / pressDuration);
			newScale = pressStartScale + EaseOut(press) * (pressedScale - pressStartScale);
			newFlash = flashAlpha * EaseInOut(Mathf.Clamp01(elapsed / flashRise));
			released = false;
		}
		else
		{
			float t = Mathf.Clamp01((now - releaseTime) / releaseDuration);
			if (t < PopShare)
			{
				newScale = releaseStartScale + EaseOut(t / PopShare) * (popScale - releaseStartScale);
			}
			else
			{
				newScale = popScale + EaseInOut((t - PopShare) / (1f - PopShare)) * (1f - popScale);
			}
			newFlash = (1f - t) * releaseStartFlash;
			released = t >= 1f;
		}
		scale = newScale;
		ApplyScale(scale);
		flashLevel = newFlash;
		SetAlpha(flash, flashLevel);
		float echoProgress = Mathf.Clamp01((now - echoTime) / echoDuration);
		if (echo != null)
		{
			float remaining = 1f - echoProgress;
			echo.rectTransform.localScale = Vector3.one * (1f + EaseOut(echoProgress) * (echoScale - 1f));
			SetAlpha(echo, remaining * remaining * echoAlpha);
		}
		if (released && !pressed && echoProgress >= 1f)
		{
			ResetToRest();
		}
	}

	// Scales around the visual centre (not the pivot) and keeps the raycast area unchanged.
	private void ApplyScale(float s)
	{
		target.localScale = restScale * s;
		Rect rect = target.rect;
		Vector2 pivot = target.pivot;
		Vector3 centerOffset = new Vector3(rect.width * (0.5f - pivot.x) * restScale.x, rect.height * (0.5f - pivot.y) * restScale.y, 0f);
		if (centerOffset != Vector3.zero)
		{
			target.localPosition = restPosition + (1f - s) * (target.localRotation * centerOffset);
		}
		if (icon != null)
		{
			float shrink = 1f - 1f / s;
			float padX = rect.width * 0.5f * shrink;
			float padY = rect.height * 0.5f * shrink;
			icon.raycastPadding = restPadding + new Vector4(padX, padY, padX, padY);
		}
	}

	private void PollShortcut()
	{
		if (rewiredAction >= 0 && !delaysUse && InteractionManager.GetButtonDown(rewiredAction))
		{
			Press(true);
		}
	}

	private bool CanPlay()
	{
		if (!IsVisible())
		{
			return false;
		}
		return selectable == null || selectable.IsInteractable();
	}

	private bool IsVisible()
	{
		if (icon != null && !icon.enabled)
		{
			return false;
		}
		foreach (Canvas canvas in canvases)
		{
			if (canvas != null && !canvas.enabled)
			{
				return false;
			}
		}
		return true;
	}

	private void ShowOverlays()
	{
		if (icon == null || icon.sprite == null)
		{
			return;
		}
		if (flash == null)
		{
			echo = CreateOverlay("PressEcho");
			flash = CreateOverlay("PressFlash");
		}
		Image[] overlays = new Image[2] { echo, flash };
		for (int i = 0; i < overlays.Length; i++)
		{
			Image overlay = overlays[i];
			overlay.sprite = icon.sprite;
			overlay.type = icon.type;
			overlay.preserveAspect = icon.preserveAspect;
			overlay.enabled = true;
		}
		echo.rectTransform.localScale = Vector3.one;
		SetAlpha(echo, echoAlpha);
		SetAlpha(flash, 0f);
	}

	private Image CreateOverlay(string overlayName)
	{
		GameObject go = new GameObject(overlayName, typeof(RectTransform));
		go.layer = gameObject.layer;
		RectTransform rectTransform = (RectTransform)go.transform;
		rectTransform.SetParent(target, false);
		rectTransform.SetAsFirstSibling();
		rectTransform.anchorMin = Vector2.zero;
		rectTransform.anchorMax = Vector2.one;
		rectTransform.offsetMin = Vector2.zero;
		rectTransform.offsetMax = Vector2.zero;
		Image image = go.AddComponent<Image>();
		image.raycastTarget = false;
		image.enabled = false;
		return image;
	}

	private void SetAlpha(Image overlay, float alpha)
	{
		if (overlay != null)
		{
			overlay.color = new Color(bloodColor.r, bloodColor.g, bloodColor.b, bloodColor.a * alpha);
		}
	}

	private void ResetToRest()
	{
		pressed = false;
		bool wasAnimating = animating;
		animating = false;
		scale = 1f;
		flashLevel = 0f;
		if (wasAnimating)
		{
			ApplyScale(1f);
		}
		if (flash != null)
		{
			flash.enabled = false;
		}
		if (echo != null)
		{
			echo.enabled = false;
		}
	}

	private static float EaseOut(float t)
	{
		return 1f - (1f - t) * (1f - t);
	}

	private static float EaseInOut(float t)
	{
		return t * t * (3f - (t + t));
	}
}
