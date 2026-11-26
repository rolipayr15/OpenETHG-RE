using System;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Control options (touch / keyboard & mouse / gamepad). Uses the scene-authored ControlOptionsWindow when one is
// assigned, otherwise builds an equivalent panel at runtime styled after the given text/slider/button templates.
public sealed class OptionsControlMode : MonoBehaviour
{
	[Serializable]
	public struct LocalizedLabel
	{
		public Text text;

		public string term;
	}

	[SerializeField]
	private Text textStyle;

	[SerializeField]
	private Slider sliderStyle;

	[SerializeField]
	private Button buttonStyle;

	[Tooltip("Optional scene button. Its RectTransform and navigation are kept as configured in the editor.")]
	[SerializeField]
	private Button launcherButton;

	[SerializeField]
	private Vector2 launcherPosition;

	[SerializeField]
	private bool launcherAtTopRight;

	[Header("Scene window")]
	[SerializeField]
	private ControlOptionsWindow window;

	[SerializeField]
	private GameObject panel;

	[SerializeField]
	private List<Button> modeButtons = new List<Button>();

	[SerializeField]
	private Slider mouseSensitivity;

	[SerializeField]
	private Slider touchSensitivity;

	[SerializeField]
	private Toggle invertY;

	[SerializeField]
	private Toggle movingJoystick;

	[SerializeField]
	private Toggle showKeyboardHints;

	[SerializeField]
	private Toggle showControllerHints;

	[SerializeField]
	private Text status;

	[SerializeField]
	private Text hints;

	[SerializeField]
	private Text mouseValue;

	[SerializeField]
	private Text touchValue;

	[SerializeField]
	private ScrollRect scroll;

	[SerializeField]
	private LocalizedLabel[] sceneLabels = new LocalizedLabel[0];

	private ControlModeManager manager;

	private Button launcher;

	private Button back;

	private Selectable beforeTouchSensitivity;

	private Selectable afterTouchSensitivity;

	private bool touchNavigationReady;

	private readonly Dictionary<Text, string> labels = new Dictionary<Text, string>();

	private GameplayInputContext.Lease lease;

	private GameObject lastSelection;

	private GameObject previousSelection;

	private int gamepadLabelsVersion = -1;

	private ChoiceGroup modeGroup;

	public GameplayInputContext.Lease InputLease => lease;

	private static bool ShowMobileTouchOptions => Application.isEditor || Application.isMobilePlatform;

	private void Awake()
	{
		if (window != null)
		{
			window.Bind(this);
			panel = window.gameObject;
			modeButtons = window.ModeButtons;
			mouseSensitivity = window.MouseSensitivity;
			touchSensitivity = window.TouchSensitivity;
			invertY = window.InvertY;
			movingJoystick = window.MovingJoystick;
			showKeyboardHints = window.ShowKeyboardHints;
			showControllerHints = window.ShowControllerHints;
			status = window.Status;
			hints = window.Hints;
			mouseValue = window.MouseValue;
			touchValue = window.TouchValue;
			scroll = window.Scroll;
			foreach (LocalizedLabel label in window.SceneLabels)
			{
				if (label.text != null)
				{
					labels[label.text] = label.term;
				}
			}
		}
		if (panel == null)
		{
			return;
		}
		foreach (LocalizedLabel label in sceneLabels)
		{
			if (label.text != null)
			{
				labels[label.text] = label.term;
			}
		}
		panel.SetActive(false);
	}

	private void Update()
	{
		if (panel == null || !panel.activeSelf)
		{
			return;
		}
		if (!IsCanvasVisible())
		{
			Close();
			return;
		}
		if (manager != null && manager.Mode == ControlMode.Gamepad && gamepadLabelsVersion != GamepadBindingLabels.Version)
		{
			Refresh();
		}
		GameObject selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
		if (selected == lastSelection || selected == null)
		{
			return;
		}
		if (!selected.transform.IsChildOf(scroll.content))
		{
			return;
		}
		// Keep the gamepad/keyboard selection centred in the scroll view.
		lastSelection = selected;
		Canvas.ForceUpdateCanvases();
		Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(scroll.content, selected.transform);
		float viewportHeight = scroll.viewport.rect.height;
		float scrollable = scroll.content.rect.height - viewportHeight;
		if (scrollable <= 0f)
		{
			return;
		}
		float offset = (-bounds.center.y - viewportHeight * 0.5f) / scrollable;
		scroll.verticalNormalizedPosition = 1f - Mathf.Clamp01(offset);
	}

	private void OnEnable()
	{
		if (panel == null)
		{
			Build();
		}
		EnsureModeGroup();
		if (!touchNavigationReady && touchSensitivity != null)
		{
			beforeTouchSensitivity = touchSensitivity.navigation.selectOnUp;
			afterTouchSensitivity = touchSensitivity.navigation.selectOnDown;
			touchNavigationReady = true;
		}
		EnsureMovingJoystickOption();
		EnsureKeyboardHintsOption();
		EnsureControllerHintsOption();
		if (mouseSensitivity != null)
		{
			mouseSensitivity.SetValueWithoutNotify(UserGameSettings.mouseSensitivity);
			mouseSensitivity.minValue = 0.1f;
			mouseSensitivity.maxValue = 8f;
		}
		manager = ControlModeManager.Instance;
		if (manager != null)
		{
			manager.ModeChanged += OnModeChanged;
			manager.MouseSettingsChanged += Refresh;
			manager.AvailabilityChanged += OnAvailabilityChanged;
		}
		LocalizationManager.OnLocalizeEvent += Refresh;
		Refresh();
	}

	private void OnDisable()
	{
		Close();
		if (manager != null)
		{
			manager.ModeChanged -= OnModeChanged;
			manager.MouseSettingsChanged -= Refresh;
			manager.AvailabilityChanged -= OnAvailabilityChanged;
		}
		LocalizationManager.OnLocalizeEvent -= Refresh;
		manager = null;
	}

	private void EnsureModeGroup()
	{
		if (modeGroup != null || modeButtons.Count == 0 || modeButtons[0] == null)
		{
			return;
		}
		GameObject row = modeButtons[0].transform.parent.gameObject;
		if (!row.TryGetComponent(out modeGroup))
		{
			modeGroup = row.AddComponent<ChoiceGroup>();
		}
		// ChoiceGroup tints the graphics itself, so start them from white.
		foreach (Button button in modeButtons)
		{
			if (button != null && button.targetGraphic != null)
			{
				button.targetGraphic.color = Color.white;
			}
		}
		modeGroup.Wrap = true;
		modeGroup.SetButtons(modeButtons);
	}

	private void OnModeChanged(ControlMode mode)
	{
		Refresh();
	}

	private void OnAvailabilityChanged()
	{
		GamepadBindingLabels.Invalidate();
		Refresh();
	}

	private bool IsCanvasVisible()
	{
		if (!isActiveAndEnabled)
		{
			return false;
		}
		Canvas canvas = GetComponentInParent<Canvas>();
		return canvas != null && canvas.isActiveAndEnabled;
	}

	public void Open()
	{
		if (panel == null || panel.activeSelf || !IsCanvasVisible())
		{
			return;
		}
		previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
		lease = GameplayInputContext.Shared.Acquire(this, GameplayInputContext.Kind.Pause, GameplayInputContext.WindowInput.PointerOnly, Close);
		panel.SetActive(true);
		panel.transform.SetAsLastSibling();
		lastSelection = null;
		if (EventSystem.current != null)
		{
			EventSystem current = EventSystem.current;
			ControlMode mode = manager != null ? manager.Mode : UserGameSettings.controlMode;
			current.SetSelectedGameObject(modeButtons[(int)mode].gameObject);
		}
		Refresh();
	}

	public void Close()
	{
		bool wasOpen = panel != null && panel.activeSelf;
		if (panel != null)
		{
			panel.SetActive(false);
		}
		lease?.Dispose();
		lease = null;
		if (wasOpen)
		{
			// Sliders elsewhere in the options show the saved values again.
			OptionsSensitivity[] sensitivities = GetComponentsInChildren<OptionsSensitivity>(true);
			for (int i = 0; i < sensitivities.Length; i++)
			{
				sensitivities[i].RefreshSavedValue();
			}
			if (EventSystem.current != null)
			{
				EventSystem current = EventSystem.current;
				GameObject restore = null;
				if (IsCanvasVisible() && previousSelection != null && previousSelection.activeInHierarchy)
				{
					restore = previousSelection;
				}
				current.SetSelectedGameObject(restore);
			}
		}
		previousSelection = null;
	}

	public void SelectMode(int value)
	{
		if (manager != null && ControlModeManager.IsValidMode((ControlMode)value))
		{
			manager.SetMode((ControlMode)value);
		}
		Refresh();
	}

	public void SetMouseSensitivity(float value)
	{
		if (manager != null)
		{
			manager.SetMouseSettings(value, manager.MouseInvertY);
		}
	}

	public void SetMouseInvertY(bool value)
	{
		if (manager != null)
		{
			manager.SetMouseSettings(manager.MouseSensitivity, value);
		}
	}

	public void SetTouchSensitivity(float value)
	{
		OptionsSensitivity.SaveTouchSensitivity(value);
		Refresh();
	}

	public void SetMovingJoystick(bool value)
	{
		OptionsDynamicJoystick.SaveSetting(value);
		Refresh();
	}

	public void SetShowKeyboardHints(bool value)
	{
		UserGameSettings.showKeyboardHints = value;
		Refresh();
	}

	public void SetShowControllerHints(bool value)
	{
		UserGameSettings.showControllerHints = value;
		Refresh();
	}

	private void EnsureMovingJoystickOption()
	{
		if (!ShowMobileTouchOptions || movingJoystick != null || scroll == null || touchSensitivity == null)
		{
			return;
		}
		movingJoystick = MakeToggle("MovingJoystick", scroll.content, "DynamicJoystick", SetMovingJoystick);
		movingJoystick.transform.SetSiblingIndex(touchSensitivity.transform.parent.GetSiblingIndex() + 1);
		movingJoystick.SetIsOnWithoutNotify(UserGameSettings.dynamicJoystickEnabled);
		movingJoystick.gameObject.SetActive(false);
	}

	private void EnsureKeyboardHintsOption()
	{
		if (showKeyboardHints != null || scroll == null || invertY == null)
		{
			return;
		}
		showKeyboardHints = MakeToggle("ShowKeyboardHints", scroll.content, "Input/ShowKeyboardHints", SetShowKeyboardHints);
		showKeyboardHints.transform.SetSiblingIndex(invertY.transform.GetSiblingIndex() + 1);
		showKeyboardHints.gameObject.SetActive(false);
	}

	// Clones the keyboard-hints row so scene-authored windows get a matching controller-hints row.
	private void EnsureControllerHintsOption()
	{
		if (showControllerHints != null || showKeyboardHints == null)
		{
			return;
		}
		Transform source = showKeyboardHints.transform;
		GameObject copy = Instantiate(showKeyboardHints.gameObject, source.parent);
		copy.name = "ShowControllerHints";
		copy.transform.SetSiblingIndex(source.GetSiblingIndex() + 1);
		showControllerHints = copy.GetComponent<Toggle>();
		Toggle.ToggleEvent onValueChanged = showControllerHints.onValueChanged;
		for (int i = 0; i < onValueChanged.GetPersistentEventCount(); i++)
		{
			onValueChanged.SetPersistentListenerState(i, UnityEventCallState.Off);
		}
		onValueChanged.RemoveAllListeners();
		onValueChanged.AddListener(SetShowControllerHints);
		foreach (Text text in new List<Text>(labels.Keys))
		{
			if (text == null || !text.transform.IsChildOf(source))
			{
				continue;
			}
			Transform copiedTransform = FindCopy(source, copy.transform, text.transform);
			Text copiedText = copiedTransform != null ? copiedTransform.GetComponent<Text>() : null;
			if (copiedText != null)
			{
				labels[copiedText] = "Input/ShowControllerHints";
			}
		}
		copy.SetActive(false);
	}

	private static Transform FindCopy(Transform source, Transform copy, Transform descendant)
	{
		if (descendant == source)
		{
			return copy;
		}
		Transform parentCopy = FindCopy(source, copy, descendant.parent);
		int index = descendant.GetSiblingIndex();
		if (parentCopy != null && index < parentCopy.childCount)
		{
			return parentCopy.GetChild(index);
		}
		return null;
	}

	// The hints toggles take the touch-sensitivity slot in the navigation chain when their mode is active.
	private void RefreshHintsOption(Toggle toggle, ControlMode mode, bool value)
	{
		if (toggle == null)
		{
			return;
		}
		ControlMode current = manager.Mode;
		toggle.SetIsOnWithoutNotify(value);
		toggle.gameObject.SetActive(current == mode);
		if (current == mode)
		{
			LinkVertical(toggle, beforeTouchSensitivity, afterTouchSensitivity);
			if (beforeTouchSensitivity != null)
			{
				Navigation navigation = beforeTouchSensitivity.navigation;
				navigation.selectOnDown = toggle;
				beforeTouchSensitivity.navigation = navigation;
			}
			if (afterTouchSensitivity != null)
			{
				Navigation navigation = afterTouchSensitivity.navigation;
				navigation.selectOnUp = toggle;
				afterTouchSensitivity.navigation = navigation;
			}
		}
		EventSystem eventSystem = EventSystem.current;
		GameObject selected = eventSystem != null ? eventSystem.currentSelectedGameObject : null;
		if (current != mode && selected != null && selected.transform.IsChildOf(toggle.transform))
		{
			Selectable fallback = beforeTouchSensitivity ?? afterTouchSensitivity;
			eventSystem.SetSelectedGameObject(fallback != null ? fallback.gameObject : null);
		}
	}

	private void RefreshTouchControls()
	{
		ControlMode mode = manager.Mode;
		bool touch = mode == ControlMode.Touch;
		bool showMoving = touch && ShowMobileTouchOptions && movingJoystick != null;
		touchSensitivity.transform.parent.gameObject.SetActive(touch);
		if (movingJoystick != null)
		{
			movingJoystick.SetIsOnWithoutNotify(UserGameSettings.dynamicJoystickEnabled);
			movingJoystick.gameObject.SetActive(showMoving);
			LinkVertical(movingJoystick, touchSensitivity, afterTouchSensitivity);
		}
		Navigation touchNavigation = touchSensitivity.navigation;
		touchNavigation.selectOnDown = showMoving ? movingJoystick : afterTouchSensitivity;
		touchSensitivity.navigation = touchNavigation;
		if (beforeTouchSensitivity != null)
		{
			Navigation navigation = beforeTouchSensitivity.navigation;
			navigation.selectOnDown = touch ? touchSensitivity : afterTouchSensitivity;
			beforeTouchSensitivity.navigation = navigation;
		}
		if (afterTouchSensitivity != null)
		{
			Navigation navigation = afterTouchSensitivity.navigation;
			navigation.selectOnUp = !touch ? beforeTouchSensitivity : (showMoving ? (Selectable)movingJoystick : touchSensitivity);
			afterTouchSensitivity.navigation = navigation;
		}
		EventSystem eventSystem = EventSystem.current;
		GameObject selected = eventSystem != null ? eventSystem.currentSelectedGameObject : null;
		if (selected == null)
		{
			return;
		}
		// Move the selection off rows that just got hidden.
		Selectable target;
		if (!touch && selected.transform.IsChildOf(touchSensitivity.transform.parent))
		{
			target = beforeTouchSensitivity ?? afterTouchSensitivity;
		}
		else
		{
			if (showMoving || movingJoystick == null || selected != movingJoystick.gameObject)
			{
				return;
			}
			target = touch ? touchSensitivity : (beforeTouchSensitivity ?? afterTouchSensitivity);
		}
		eventSystem.SetSelectedGameObject(target != null ? target.gameObject : null);
	}

	private void Refresh()
	{
		Font font = LocalizationManager.GetTranslatedObjectByTermName<Font>("GeneralUseFont");
		if (font == null && textStyle != null)
		{
			font = textStyle.font;
		}
		foreach (KeyValuePair<Text, string> label in labels)
		{
			label.Key.text = Translate(label.Value);
			LocalizedFontLayout.SetFont(label.Key, font);
		}
		LocalizedFontLayout.SetFont(status, font);
		if (manager == null || mouseSensitivity == null)
		{
			return;
		}
		RefreshTouchControls();
		RefreshHintsOption(showKeyboardHints, ControlMode.KeyboardMouse, UserGameSettings.showKeyboardHints);
		RefreshHintsOption(showControllerHints, ControlMode.Gamepad, UserGameSettings.showControllerHints);
		for (int i = 0; i < modeButtons.Count; i++)
		{
			Text text = modeButtons[i].GetComponentInChildren<Text>(true);
			text.text = Translate("Input/" + (ControlMode)i);
			LocalizedFontLayout.SetFont(text, font);
		}
		if (modeGroup != null)
		{
			modeGroup.Select((int)manager.Mode, false);
		}
		mouseSensitivity.SetValueWithoutNotify(manager.MouseSensitivity);
		invertY.SetIsOnWithoutNotify(manager.MouseInvertY);
		touchSensitivity.SetValueWithoutNotify(OptionsSensitivity.LoadTouchSensitivity());
		mouseValue.text = manager.MouseSensitivity.ToString("F1");
		touchValue.text = touchSensitivity.value.ToString("F1");
		ControlMode mode = manager.Mode;
		hints.gameObject.SetActive(mode == ControlMode.KeyboardMouse || mode == ControlMode.Gamepad);
		if (mode == ControlMode.Gamepad)
		{
			hints.text = GamepadBindingLabels.FormatHints(Translate("Input/ControllerHints"));
		}
		gamepadLabelsVersion = GamepadBindingLabels.Version;
		LayoutElement hintsLayout = hints.GetComponent<LayoutElement>();
		if (hintsLayout != null)
		{
			hintsLayout.preferredHeight = mode == ControlMode.Gamepad ? 340f : 200f;
		}
		ControlModeManager.RecoveryStatus recovery = manager.Status;
		string statusText = "";
		if (recovery == ControlModeManager.RecoveryStatus.Unavailable)
		{
			statusText = Translate("Input/Unavailable");
		}
		else if (recovery == ControlModeManager.RecoveryStatus.ResumeRequired)
		{
			statusText = Translate("Input/ResumeRequired");
		}
		status.text = statusText;
	}

	// I2 translation with an English fallback, and built-in English for terms missing from the sheet.
	internal static string Translate(string term)
	{
		string translation = LocalizationManager.GetTranslation(term);
		if (!string.IsNullOrEmpty(translation) && translation != term)
		{
			return translation;
		}
		translation = LocalizationManager.GetTranslation(term, true, 0, true, false, null, "English");
		if (!string.IsNullOrEmpty(translation) && translation != term)
		{
			return translation;
		}
		switch (term)
		{
		case "DynamicJoystick":
			return "Moving joystick";
		case "Input/ShowKeyboardHints":
			return "Show keyboard shortcuts";
		case "Input/ControllerHints":
			return "Left stick: Move\nRight stick: Look\n{Run}: Run (hold)\n{Interact}: Interact\n{EnemyVision}: Enemy vision\n{ThrowMeat}: Throw meat\n{Journal}: Journal\n{Map}: Map\n{Menu}: Pause";
		case "Input/InvertY":
			return "Invert mouse Y axis";
		case "Input/ClickToContinue":
			return "Click to continue";
		case "Input/Back":
			return "Back";
		case "Input/ShowControllerHints":
			return "Show controller buttons";
		case "Input/KeyboardHints":
			return "WASD: Move\nMouse: Look\nEsc: Pause";
		case "Input/MouseCaptureWithheldLegacy":
			return "The system isn't giving the game control of the mouse. Keyboard still works; the mouse returns as soon as the system allows it.";
		case "Input/Gamepad":
			return "Gamepad";
		case "Input/Unavailable":
			return "Selected input device is not available.";
		case "Input/ResumeRequired":
			return "Resume the game to regain control.";
		case "Input/Touch":
			return "Touch";
		case "Input/MouseCaptureWithheldPhone":
			return "On iPhone the pointer stays visible. You can look with the mouse; a click uses the pointer on the HUD.";
		case "Input/MouseCaptureWithheld":
			return "iPadOS isn't giving the game control of the mouse. Keyboard still works. To get the mouse back, use the window button at the top left to make the game full screen, or choose Full Screen Apps in Settings → Multitasking & Gestures.";
		case "Input/TouchSensitivity":
			return "Touch sensitivity";
		case "Input/KeyboardMouse":
			return "Keyboard & mouse";
		case "Input/MouseSensitivity":
			return "Mouse sensitivity";
		default:
			return term;
		}
	}

	private void Build()
	{
		if (launcherButton == null)
		{
			launcher = MakeButton("ControlsLauncher", transform, "Controls", Open);
			RectTransform launcherRect = (RectTransform)launcher.transform;
			Vector2 corner = launcherAtTopRight ? Vector2.one : new Vector2(0.5f, 0.5f);
			launcherRect.anchorMax = corner;
			launcherRect.anchorMin = corner;
			launcherRect.pivot = corner;
			launcherRect.anchoredPosition = launcherPosition;
			launcherRect.sizeDelta = new Vector2(380f, 90f);
			if (buttonStyle != null)
			{
				Navigation styleNavigation = buttonStyle.navigation;
				styleNavigation.selectOnUp = launcher;
				buttonStyle.navigation = styleNavigation;
				Navigation launcherNavigation = launcher.navigation;
				launcherNavigation.mode = Navigation.Mode.Explicit;
				launcherNavigation.selectOnDown = buttonStyle;
				launcher.navigation = launcherNavigation;
			}
		}
		else
		{
			launcher = launcherButton;
			Text launcherText = launcher.GetComponentInChildren<Text>(true);
			if (launcherText != null)
			{
				labels.Add(launcherText, "Controls");
			}
		}

		RectTransform panelRect = Rect("ControlOptionsPanel", transform);
		panel = panelRect.gameObject;
		Stretch((RectTransform)panel.transform, 0f, 0f, 0f, 0f);
		panel.AddComponent<Image>().color = new Color(0.035f, 0.035f, 0.035f, 1f);

		Text title = Label("Title", panel.transform, "Controls", 46);
		title.rectTransform.anchorMin = new Vector2(0f, 1f);
		title.rectTransform.anchorMax = Vector2.one;
		title.rectTransform.pivot = new Vector2(0f, 1f);
		title.rectTransform.offsetMin = new Vector2(30f, -100f);
		title.rectTransform.offsetMax = new Vector2(-260f, -20f);

		back = MakeButton("Back", panel.transform, "Input/Back", Close);
		RectTransform backRect = (RectTransform)back.transform;
		backRect.pivot = Vector2.one;
		backRect.anchorMax = Vector2.one;
		backRect.anchorMin = Vector2.one;
		backRect.anchoredPosition = new Vector2(-20f, -20f);
		backRect.sizeDelta = new Vector2(220f, 75f);

		RectTransform viewport = Rect("Viewport", panel.transform);
		Stretch(viewport, 20f, 110f, 64f, 20f);
		viewport.gameObject.AddComponent<RectMask2D>();
		viewport.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.01f);

		RectTransform content = Rect("Content", viewport);
		content.anchorMin = new Vector2(0f, 1f);
		content.anchorMax = Vector2.one;
		content.pivot = new Vector2(0.5f, 1f);
		content.sizeDelta = Vector2.zero;
		VerticalLayoutGroup contentLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
		contentLayout.spacing = 16f;
		contentLayout.padding = new RectOffset(16, 16, 8, 24);
		contentLayout.childControlHeight = true;
		contentLayout.childControlWidth = true;
		contentLayout.childForceExpandWidth = true;
		contentLayout.childForceExpandHeight = false;
		content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

		scroll = viewport.gameObject.AddComponent<ScrollRect>();
		scroll.viewport = viewport;
		scroll.content = content;
		scroll.horizontal = false;
		scroll.movementType = ScrollRect.MovementType.Clamped;
		scroll.scrollSensitivity = 40f;
		scroll.verticalScrollbar = MakeScrollbar(panel.transform);
		scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;

		RectTransform modesRow = Row("Modes", content, 96f);
		HorizontalLayoutGroup modesLayout = modesRow.gameObject.AddComponent<HorizontalLayoutGroup>();
		modesLayout.spacing = 12f;
		modesLayout.childForceExpandWidth = true;
		modesLayout.childControlHeight = true;
		modesLayout.childControlWidth = true;
		for (int i = 0; i < 3; i++)
		{
			int value = i;
			string modeName = ((ControlMode)value).ToString();
			Button modeButton = MakeButton(modeName, modesRow, "Input/" + modeName, delegate
			{
				SelectMode(value);
			});
			modeButton.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
			modeButtons.Add(modeButton);
		}
		modeButtons[0].gameObject.SetActive(ControlModeManager.OffersTouchMode());

		RectTransform availabilityRow = Row("Availability", content, 96f);
		status = Label("Status", availabilityRow, null, 30);
		Stretch(status.rectTransform, 8f, 0f, 8f, 0f);

		mouseSensitivity = MakeSlider("MouseSensitivity", content, "Input/MouseSensitivity", 0.1f, 8f, out mouseValue);
		mouseSensitivity.onValueChanged.AddListener(SetMouseSensitivity);
		invertY = MakeToggle("MouseInvertY", content, "Input/InvertY", SetMouseInvertY);
		touchSensitivity = MakeSlider("TouchSensitivity", content, "Input/TouchSensitivity", 0.3f, 14f, out touchValue);
		touchSensitivity.onValueChanged.AddListener(SetTouchSensitivity);

		hints = Label("KeyboardHints", content, "Input/KeyboardHints", 32);
		hints.alignment = TextAnchor.UpperLeft;
		hints.gameObject.AddComponent<LayoutElement>().preferredHeight = 200f;

		// Mode buttons wrap left/right among the visible ones; up goes to Back, down to the first slider.
		List<Button> visibleModes = new List<Button>();
		foreach (Button modeButton in modeButtons)
		{
			if (modeButton.gameObject.activeSelf)
			{
				visibleModes.Add(modeButton);
			}
		}
		int count = visibleModes.Count;
		for (int i = 0; i < count; i++)
		{
			Navigation navigation = new Navigation
			{
				mode = Navigation.Mode.Explicit,
				selectOnUp = back,
				selectOnDown = mouseSensitivity,
				selectOnLeft = visibleModes[(i + count - 1) % count],
				selectOnRight = visibleModes[(i + 1) % count]
			};
			visibleModes[i].navigation = navigation;
		}
		LinkVertical(mouseSensitivity, modeButtons[1], invertY);
		LinkVertical(invertY, mouseSensitivity, touchSensitivity);
		LinkVertical(touchSensitivity, invertY, back);
		LinkVertical(back, touchSensitivity, visibleModes[0]);
		panel.SetActive(false);
	}

	private static void LinkVertical(Selectable item, Selectable up, Selectable down)
	{
		item.navigation = new Navigation
		{
			mode = Navigation.Mode.Explicit,
			selectOnUp = up,
			selectOnDown = down
		};
	}

	private static Scrollbar MakeScrollbar(Transform parent)
	{
		RectTransform bar = Rect("VerticalScrollbar", parent);
		bar.anchorMin = new Vector2(1f, 0f);
		bar.anchorMax = Vector2.one;
		bar.pivot = new Vector2(1f, 0.5f);
		bar.anchoredPosition = new Vector2(-20f, -45f);
		bar.sizeDelta = new Vector2(32f, -130f);
		bar.gameObject.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.12f);
		RectTransform slidingArea = Rect("Sliding Area", bar);
		Stretch(slidingArea, 4f, 4f, 4f, 4f);
		RectTransform handle = Rect("Handle", slidingArea);
		Stretch(handle, 0f, 0f, 0f, 0f);
		Image handleImage = handle.gameObject.AddComponent<Image>();
		handleImage.color = new Color(1f, 1f, 1f, 0.75f);
		Scrollbar scrollbar = bar.gameObject.AddComponent<Scrollbar>();
		scrollbar.handleRect = handle;
		scrollbar.targetGraphic = handleImage;
		scrollbar.direction = Scrollbar.Direction.BottomToTop;
		scrollbar.value = 1f;
		scrollbar.size = 1f;
		scrollbar.navigation = new Navigation { mode = Navigation.Mode.None };
		return scrollbar;
	}

	private Toggle MakeToggle(string name, Transform parent, string term, UnityAction<bool> onChanged)
	{
		RectTransform row = Row(name, parent, 80f);
		Text label = Label("Label", row, term, 36);
		Stretch(label.rectTransform, 8f, 0f, 100f, 0f);
		Toggle toggle = row.gameObject.AddComponent<Toggle>();
		RectTransform box = Rect("Box", row);
		box.anchorMax = new Vector2(1f, 0.5f);
		box.anchorMin = new Vector2(1f, 0.5f);
		box.anchoredPosition = new Vector2(-45f, 0f);
		box.sizeDelta = new Vector2(64f, 64f);
		Image boxImage = box.gameObject.AddComponent<Image>();
		boxImage.color = new Color(0.25f, 0.25f, 0.25f, 1f);
		Text checkmark = Label("Checkmark", box, null, 44);
		checkmark.text = "✓";
		Stretch(checkmark.rectTransform, 0f, 0f, 0f, 0f);
		toggle.targetGraphic = boxImage;
		toggle.graphic = checkmark;
		toggle.onValueChanged.AddListener(onChanged);
		return toggle;
	}

	private Slider MakeSlider(string name, Transform parent, string term, float min, float max, out Text value)
	{
		RectTransform row = Row(name, parent, 130f);
		Text label = Label("Label", row, term, 36);
		Stretch(label.rectTransform, 8f, 0f, 130f, 70f);
		value = Label("Value", row, null, 32);
		value.alignment = TextAnchor.MiddleRight;
		Stretch(value.rectTransform, 0f, 0f, 8f, 70f);
		RectTransform styleRect = sliderStyle != null ? sliderStyle.transform as RectTransform : null;
		RectTransform styleHandle = sliderStyle != null ? sliderStyle.handleRect : null;
		Transform styleBackground = sliderStyle != null ? sliderStyle.transform.Find("Background") : null;

		RectTransform sliderRect = Rect("Slider", row);
		sliderRect.anchorMin = Vector2.zero;
		sliderRect.anchorMax = new Vector2(1f, 0f);
		sliderRect.anchoredPosition = new Vector2(0f, 30f);
		sliderRect.sizeDelta = new Vector2(-80f, styleRect != null ? styleRect.sizeDelta.y : 42.3f);
		sliderRect.localScale = styleRect != null ? styleRect.localScale : new Vector3(1f, 0.8862353f, 1f);
		sliderRect.gameObject.AddComponent<Image>().color = Color.clear;
		Slider slider = sliderRect.gameObject.AddComponent<Slider>();

		RectTransform background = Rect("Background", sliderRect);
		background.anchorMin = new Vector2(0f, 0.5f);
		background.anchorMax = new Vector2(1f, 0.5f);
		background.sizeDelta = new Vector2(0f, 8f);
		Image backgroundImage = background.gameObject.AddComponent<Image>();
		CopySliderImageStyle(backgroundImage, styleBackground != null ? styleBackground.GetComponent<Image>() : null);
		backgroundImage.raycastTarget = false;

		RectTransform handleArea = Rect("HandleArea", sliderRect);
		Stretch(handleArea, 10f, 0f, 10f, 0f);
		RectTransform handle = Rect("Handle", handleArea);
		handle.sizeDelta = styleHandle != null ? styleHandle.sizeDelta : new Vector2(80f, 44f);
		Image handleImage = handle.gameObject.AddComponent<Image>();
		CopySliderImageStyle(handleImage, styleHandle != null ? styleHandle.GetComponent<Image>() : null);

		slider.handleRect = handle;
		slider.targetGraphic = handleImage;
		slider.minValue = min;
		slider.maxValue = max;
		if (sliderStyle != null)
		{
			slider.colors = sliderStyle.colors;
			slider.transition = sliderStyle.transition;
			slider.spriteState = sliderStyle.spriteState;
		}
		return slider;
	}

	private static void CopySliderImageStyle(Image target, Image source)
	{
		if (source == null)
		{
			return;
		}
		target.sprite = source.sprite;
		target.type = source.type;
		target.color = source.color;
		target.material = source.material;
		target.preserveAspect = source.preserveAspect;
	}

	private Button MakeButton(string name, Transform parent, string term, UnityAction action)
	{
		RectTransform rect = Rect(name, parent);
		Image image = rect.gameObject.AddComponent<Image>();
		Image styleImage = buttonStyle != null ? buttonStyle.targetGraphic as Image : null;
		image.sprite = styleImage != null ? styleImage.sprite : null;
		image.type = Image.Type.Sliced;
		image.color = new Color(0.13f, 0.13f, 0.13f, 1f);
		Button button = rect.gameObject.AddComponent<Button>();
		button.targetGraphic = image;
		if (buttonStyle != null)
		{
			button.colors = buttonStyle.colors;
		}
		button.onClick.AddListener(action);
		Text label = Label("Label", rect, term, 34);
		Stretch(label.rectTransform, 10f, 4f, 10f, 4f);
		return button;
	}

	private Text Label(string name, Transform parent, string term, int size)
	{
		RectTransform rect = Rect(name, parent);
		Text text = rect.gameObject.AddComponent<Text>();
		text.font = textStyle != null ? textStyle.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
		text.color = textStyle != null ? textStyle.color : Color.white;
		text.fontSize = size;
		text.resizeTextForBestFit = true;
		text.resizeTextMinSize = 20;
		text.resizeTextMaxSize = size;
		text.alignment = TextAnchor.MiddleCenter;
		text.raycastTarget = false;
		LocalizedFontLayout.Apply(text);
		if (term != null)
		{
			labels.Add(text, term);
		}
		return text;
	}

	private static RectTransform Row(string name, Transform parent, float height)
	{
		RectTransform rect = Rect(name, parent);
		rect.gameObject.AddComponent<LayoutElement>().preferredHeight = height;
		return rect;
	}

	private static RectTransform Rect(string name, Transform parent)
	{
		GameObject go = new GameObject(name, typeof(RectTransform));
		RectTransform rect = go.GetComponent<RectTransform>();
		rect.SetParent(parent, false);
		rect.gameObject.layer = parent.gameObject.layer;
		return rect;
	}

	private static void Stretch(RectTransform rect, float left, float top, float right, float bottom)
	{
		rect.anchorMin = Vector2.zero;
		rect.anchorMax = Vector2.one;
		rect.offsetMin = new Vector2(left, bottom);
		rect.offsetMax = new Vector2(-right, -top);
	}
}
