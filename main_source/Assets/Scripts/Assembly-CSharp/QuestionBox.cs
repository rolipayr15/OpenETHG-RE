using System;
using Rewired;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Yes/No dialog loaded as the additive "QuestionBox" scene. Variants: reward (single claim button, can't be
// declined), destructive (red action button, optionally gated behind an acknowledgement checkbox).
// The request lives in statics until the scene's instance resolves it; a version number guards stale instances.
public class QuestionBox : MonoBehaviour
{
	private const string SceneName = "QuestionBox";

	private static string messageString;

	private static string yesString;

	private static string noString;

	private static Action yesCallback;

	private static Action noCallback;

	private static bool rewardRequest;

	private static bool destructiveRequest;

	private static string requiredConfirmationString;

	private static Color? primaryLabelColorRequest;

	private static bool pointerOnlyRequest;

	private static bool modalInputRequest;

	private static GameplayInputContext.Kind inputKindRequest = GameplayInputContext.Kind.Modal;

	private static bool navigableRequest;

	private static bool cancelRequested;

	private static bool isQuestionBoxActive;

	private static int activeRequestVersion;

	private static GameplayInputContext.Lease pendingInputLease;

	// Owner of the input lease held while the scene is still loading.
	private static readonly object PendingLeaseOwner = new object();

	private GameplayInputWindow inputWindow;

	private bool usesGameplayInput;

	[SerializeField]
	private Text messageLabel;

	[SerializeField]
	private Text yesLabel;

	private int instanceRequestVersion;

	private bool isReward;

	private bool isDestructive;

	private string requiredConfirmationText;

	private Color? primaryLabelColorOverride;

	private bool isPointerOnly;

	private bool usesModalInput;

	private GameplayInputContext.Kind inputKind = GameplayInputContext.Kind.Modal;

	private bool isNavigable;

	private Button noButton;

	private Toggle confirmationToggle;

	private bool confirmationChecked;

	private bool resolved;

	public static bool IsSceneLoaded
	{
		get
		{
			Scene scene = SceneManager.GetSceneByName(SceneName);
			return scene.IsValid() && scene.isLoaded;
		}
	}

	private GameplayInputContext.Lease inputLease => inputWindow != null ? inputWindow.Lease : null;

	public static bool IsRequestActive => isQuestionBoxActive || IsSceneLoaded;

	// Modal dialogs over gameplay use the dedicated modal accept/decline actions.
	private int YesAction => usesModalInput ? RewiredConsts.Action.ModalYes : RewiredConsts.Action.Accept;

	private int NoAction => usesModalInput ? RewiredConsts.Action.ModalNo : RewiredConsts.Action.Decline;

	public static void Show(string message, string yesLabel, string noLabel, Action onYes, Action onNo)
	{
		TryShow(message, yesLabel, noLabel, onYes, onNo, null, false, false, GameplayInputContext.Kind.Modal, false);
	}

	public static bool TryShow(string message, string yesLabel, string noLabel, Action onYes, Action onNo, Color? primaryLabelColor = null, bool pointerOnly = false, bool useModalInput = false, GameplayInputContext.Kind inputKind = GameplayInputContext.Kind.Modal, bool navigable = false)
	{
		return TryShowInternal(message, yesLabel, noLabel, onYes, onNo, false, false, null, primaryLabelColor, pointerOnly, useModalInput, inputKind, navigable);
	}

	public static bool TryShowReward(string message, string claimLabel, Action onClaimed, Action onClosed)
	{
		return TryShowInternal(message, claimLabel, null, onClaimed, onClosed, true, false, null, null, false, false, GameplayInputContext.Kind.Modal, false);
	}

	public static bool TryShowDestructive(string message, string actionLabel, string cancelLabel, string requiredConfirmation, Action onConfirmed, Action onCancelled)
	{
		if (string.IsNullOrEmpty(requiredConfirmation))
		{
			Debug.LogError("A destructive QuestionBox requires acknowledgement text.");
			return false;
		}
		return TryShowInternal(message, actionLabel, cancelLabel, onConfirmed, onCancelled, false, true, requiredConfirmation, null, false, false, GameplayInputContext.Kind.Modal, false);
	}

	private static bool TryShowInternal(string message, string yesLabel, string noLabel, Action onYes, Action onNo, bool isReward, bool isDestructive, string requiredConfirmation, Color? primaryLabelColor, bool pointerOnly, bool useModalInput = false, GameplayInputContext.Kind inputKind = GameplayInputContext.Kind.Modal, bool navigable = false)
	{
		// One dialog at a time, and never on top of the consent form.
		if (IsRequestActive || ConsentRecovery.IsConsentFormVisible)
		{
			return false;
		}
		messageString = message ?? string.Empty;
		yesString = string.IsNullOrEmpty(yesLabel) ? "OK" : yesLabel;
		noString = string.IsNullOrEmpty(noLabel) ? "Cancel" : noLabel;
		yesCallback = onYes;
		noCallback = onNo;
		requiredConfirmationString = requiredConfirmation;
		rewardRequest = isReward;
		destructiveRequest = isDestructive;
		primaryLabelColorRequest = primaryLabelColor;
		pointerOnlyRequest = pointerOnly;
		modalInputRequest = useModalInput;
		inputKindRequest = inputKind;
		navigableRequest = !(isReward || pointerOnly) && navigable;
		cancelRequested = false;
		isQuestionBoxActive = true;
		activeRequestVersion++;
		if (CustomSingleton<PauseManager>.HasInstance)
		{
			// Block gameplay input while the dialog scene is loading.
			pendingInputLease = GameplayInputContext.Shared.Acquire(PendingLeaseOwner, inputKind);
		}
		if (SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive) == null)
		{
			throw new InvalidOperationException("Could not load the QuestionBox scene.");
		}
		return true;
	}

	private void Awake()
	{
		instanceRequestVersion = activeRequestVersion;
		requiredConfirmationText = requiredConfirmationString;
		isReward = rewardRequest;
		isDestructive = destructiveRequest;
		primaryLabelColorOverride = primaryLabelColorRequest;
		isPointerOnly = pointerOnlyRequest;
		inputKind = inputKindRequest;
		usesGameplayInput = CustomSingleton<PauseManager>.HasInstance;
		usesModalInput = usesGameplayInput || modalInputRequest;
		isNavigable = navigableRequest && usesGameplayInput;
	}

	private void OnEnable()
	{
		if (usesGameplayInput && !resolved)
		{
			GameplayInputContext.WindowInput input;
			if (isNavigable)
			{
				input = GameplayInputContext.WindowInput.Cancel | GameplayInputContext.WindowInput.Navigate | GameplayInputContext.WindowInput.Submit;
			}
			else if (isPointerOnly)
			{
				input = GameplayInputContext.WindowInput.PointerOnly;
			}
			else
			{
				input = isReward ? GameplayInputContext.WindowInput.Submit : GameplayInputContext.WindowInput.Cancel | GameplayInputContext.WindowInput.Submit;
			}
			inputWindow = GameplayInputWindow.GetOrAdd(this);
			Action onCancel = !isPointerOnly && !isReward ? delegate
			{
				Resolve(false);
			} : null;
			inputWindow.Open(inputKind, input, onCancel);
		}
		pendingInputLease?.Dispose();
		pendingInputLease = null;
		if (isNavigable && noButton != null)
		{
			BeginSelectionScope();
		}
		if (!isPointerOnly && !isNavigable)
		{
			InteractionManager.SetRewiredMapping(YesQuestionBoxRewired, YesAction);
			if (!isReward && !usesGameplayInput)
			{
				InteractionManager.SetRewiredMapping(NoQuestionBoxRewired, NoAction);
			}
		}
	}

	private void Start()
	{
		if (cancelRequested)
		{
			Resolve(false);
			return;
		}
		DisableRuntimeTextLocalization(messageLabel);
		messageLabel.text = messageString;
		messageLabel.resizeTextForBestFit = true;
		messageLabel.resizeTextMinSize = 28;
		messageLabel.resizeTextMaxSize = 60;
		ConfigureButtons();
		if (isNavigable)
		{
			BeginSelectionScope();
		}
	}

	private void BeginSelectionScope()
	{
		if (inputLease != null && !resolved)
		{
			// Gamepad focus starts on the safe choice.
			GamepadSelectionScope.Begin(gameObject, inputLease, () => noButton);
		}
	}

	private void ConfigureButtons()
	{
		Button yesButton = yesLabel.GetComponentInParent<Button>();
		if (yesButton == null)
		{
			Debug.LogError("QuestionBox YesLabel must be a child of a Button.");
			return;
		}
		RectTransform yesRect = yesButton.GetComponent<RectTransform>();
		float y = yesRect.anchoredPosition.y;
		noButton = null;
		if (!isReward)
		{
			// The scene only has one button: clone it for "No" and lay the pair out side by side.
			noButton = Instantiate(yesButton, yesButton.transform.parent);
			LocalizedFontLayout.CopyBaselines(yesButton.gameObject, noButton.gameObject);
			noButton.gameObject.name = "NoButton";
			RectTransform noRect = noButton.GetComponent<RectTransform>();
			yesRect.anchoredPosition = new Vector2(250f, y);
			noRect.anchoredPosition = new Vector2(-250f, y);
		}
		else
		{
			yesRect.anchoredPosition = new Vector2(0f, y);
			foreach (Button button in GetComponentsInChildren<Button>(true))
			{
				if (button != yesButton)
				{
					button.gameObject.SetActive(false);
				}
			}
		}
		yesButton.onClick = new Button.ButtonClickedEvent();
		yesButton.onClick.AddListener(delegate
		{
			Resolve(true);
		});
		if (noButton != null)
		{
			noButton.onClick = new Button.ButtonClickedEvent();
			noButton.onClick.AddListener(delegate
			{
				Resolve(false);
			});
		}
		DisableRuntimeTextLocalization(yesLabel);
		yesLabel.text = yesString;
		yesLabel.resizeTextForBestFit = true;
		yesLabel.resizeTextMinSize = 24;
		yesLabel.resizeTextMaxSize = 60;
		if (primaryLabelColorOverride.HasValue)
		{
			yesLabel.color = primaryLabelColorOverride.Value;
		}
		Text noLabel = noButton != null ? noButton.GetComponentInChildren<Text>(true) : null;
		if (noLabel != null)
		{
			DisableRuntimeTextLocalization(noLabel);
			noLabel.text = noString;
			noLabel.color = Color.white;
			noLabel.resizeTextForBestFit = true;
			noLabel.resizeTextMinSize = 24;
			noLabel.resizeTextMaxSize = 60;
		}
		if (isReward)
		{
			ConfigureRewardButton(yesButton);
		}
		if (isDestructive)
		{
			ConfigureDestructiveButton(yesButton);
		}
		if (!string.IsNullOrEmpty(requiredConfirmationText))
		{
			ConfigureRequiredConfirmation(yesButton);
		}
	}

	private void ConfigureRewardButton(Button claimButton)
	{
		Color gold = new Color(0.98f, 0.8666667f, 0.5254902f, 1f);
		Color amber = new Color(1f, 0.7882353f, 0f, 1f);
		ConfigureButtonColors(claimButton, gold, amber, amber, new Color(0.2862745f, 0.2862745f, 0.2862745f, 0.5019608f));
		yesLabel.color = gold;
	}

	private static void ConfigureButtonColors(Button button, Color normal, Color highlighted, Color pressed, Color disabled)
	{
		if (button.targetGraphic != null)
		{
			button.targetGraphic.color = Color.white;
		}
		ColorBlock colors = button.colors;
		colors.normalColor = normal;
		colors.highlightedColor = highlighted;
		colors.pressedColor = pressed;
		colors.selectedColor = highlighted;
		colors.disabledColor = disabled;
		button.colors = colors;
	}

	private void ConfigureDestructiveButton(Button actionButton)
	{
		ColorBlock colors = actionButton.colors;
		colors.normalColor = new Color(0.78f, 0.12f, 0.12f, 1f);
		colors.highlightedColor = new Color(0.92f, 0.2f, 0.2f, 1f);
		colors.pressedColor = new Color(0.58f, 0.07f, 0.07f, 1f);
		colors.selectedColor = new Color(0.92f, 0.2f, 0.2f, 1f);
		colors.disabledColor = new Color(0.28f, 0.12f, 0.12f, 0.65f);
		actionButton.colors = colors;
		if (actionButton.targetGraphic != null)
		{
			actionButton.targetGraphic.color = new Color(0.78f, 0.12f, 0.12f, 1f);
		}
		yesLabel.color = Color.white;
	}

	// Destructive confirmations need an explicit "I understand" tick before the action button enables.
	private void ConfigureRequiredConfirmation(Button actionButton)
	{
		RectTransform messageRect = messageLabel.rectTransform;
		messageRect.anchoredPosition = new Vector2(messageRect.anchoredPosition.x, 95f);
		messageRect.sizeDelta = new Vector2(messageRect.sizeDelta.x, 285f);
		GameObject row = new GameObject("RequiredConfirmation", typeof(RectTransform), typeof(Toggle));
		row.layer = actionButton.gameObject.layer;
		row.transform.SetParent(actionButton.transform.parent, false);
		RectTransform rowRect = row.GetComponent<RectTransform>();
		rowRect.anchorMin = new Vector2(0.5f, 0.5f);
		rowRect.anchorMax = new Vector2(0.5f, 0.5f);
		rowRect.anchoredPosition = new Vector2(0f, -112f);
		rowRect.sizeDelta = new Vector2(1000f, 58f);
		Image frame = CreateCheckboxImage("CheckboxFrame", rowRect, new Vector2(-465f, 0f), new Vector2(54f, 54f), Color.white);
		CreateCheckboxImage("CheckboxBackground", frame.rectTransform, Vector2.zero, new Vector2(46f, 46f), new Color(0.08f, 0.08f, 0.08f, 1f));
		Image checkmark = CreateCheckboxImage("Checkmark", frame.rectTransform, Vector2.zero, new Vector2(36f, 36f), new Color(0.9f, 0.12f, 0.12f, 1f));
		checkmark.raycastTarget = false;
		GameObject labelObject = new GameObject("ConfirmationLabel", typeof(RectTransform), typeof(Text));
		labelObject.layer = row.layer;
		labelObject.transform.SetParent(rowRect, false);
		RectTransform labelRect = labelObject.GetComponent<RectTransform>();
		labelRect.anchorMin = Vector2.zero;
		labelRect.anchorMax = Vector2.one;
		labelRect.offsetMin = new Vector2(75f, 0f);
		labelRect.offsetMax = Vector2.zero;
		Text label = labelObject.GetComponent<Text>();
		label.font = messageLabel.font;
		LocalizedFontLayout.Apply(label);
		label.fontStyle = FontStyle.Normal;
		label.fontSize = 34;
		label.alignment = TextAnchor.MiddleLeft;
		label.color = Color.white;
		label.raycastTarget = true;
		label.resizeTextForBestFit = true;
		label.resizeTextMinSize = 22;
		label.resizeTextMaxSize = 34;
		label.text = requiredConfirmationText;
		confirmationToggle = row.GetComponent<Toggle>();
		confirmationToggle.targetGraphic = frame;
		confirmationToggle.graphic = checkmark;
		confirmationToggle.navigation = default(Navigation);
		confirmationToggle.SetIsOnWithoutNotify(false);
		checkmark.CrossFadeAlpha(0f, 0f, true);
		confirmationToggle.onValueChanged.AddListener(delegate(bool isOn)
		{
			confirmationChecked = isOn;
			actionButton.interactable = isOn;
		});
		confirmationChecked = false;
		actionButton.interactable = false;
	}

	private static Image CreateCheckboxImage(string objectName, Transform parent, Vector2 anchoredPosition, Vector2 size, Color color)
	{
		GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(Image));
		go.layer = parent.gameObject.layer;
		go.transform.SetParent(parent, false);
		RectTransform rect = go.GetComponent<RectTransform>();
		rect.anchorMin = new Vector2(0.5f, 0.5f);
		rect.anchorMax = new Vector2(0.5f, 0.5f);
		rect.anchoredPosition = anchoredPosition;
		rect.sizeDelta = size;
		Image image = go.GetComponent<Image>();
		image.color = color;
		return image;
	}

	// Texts are supplied already translated by the caller.
	private static void DisableRuntimeTextLocalization(Text label)
	{
		I2.Loc.Localize localize = label.GetComponent<I2.Loc.Localize>();
		if (localize != null)
		{
			localize.enabled = false;
		}
	}

	public void YesOrNoButtonClicked(bool yesOrNo)
	{
		Resolve(yesOrNo);
	}

	// Closes the dialog as "No" if it is still the one whose decline callback is expectedOnNo.
	public static void CancelRequest(Action expectedOnNo)
	{
		if (expectedOnNo == null || !isQuestionBoxActive || noCallback != expectedOnNo || rewardRequest)
		{
			return;
		}
		cancelRequested = true;
		Scene scene = SceneManager.GetSceneByName(SceneName);
		if (!scene.IsValid() || !scene.isLoaded)
		{
			return;
		}
		foreach (GameObject root in scene.GetRootGameObjects())
		{
			foreach (QuestionBox box in root.GetComponentsInChildren<QuestionBox>(true))
			{
				box.Resolve(false);
			}
		}
	}

	private void Resolve(bool accepted)
	{
		accepted = accepted && !cancelRequested;
		if (!accepted)
		{
			if (isReward)
			{
				return;
			}
		}
		else if (!string.IsNullOrEmpty(requiredConfirmationText) && !confirmationChecked)
		{
			return;
		}
		if (resolved || !isQuestionBoxActive || instanceRequestVersion != activeRequestVersion)
		{
			return;
		}
		resolved = true;
		Action callback = accepted ? yesCallback : noCallback;
		Scene scene = gameObject.scene;
		ResetRequest(instanceRequestVersion);
		if (scene.IsValid() && scene.isLoaded)
		{
			SceneManager.UnloadSceneAsync(scene.name, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
		}
		callback?.Invoke();
	}

	private void OnDisable()
	{
		if (isNavigable)
		{
			GamepadSelectionScope.End(gameObject);
		}
		if (inputWindow != null)
		{
			inputWindow.Close();
		}
		if (!isPointerOnly && !isNavigable)
		{
			InteractionManager.RemoveRewiredMapping(YesQuestionBoxRewired, YesAction);
			InteractionManager.RemoveRewiredMapping(NoQuestionBoxRewired, NoAction);
		}
	}

	// A dialog destroyed without an answer (scene change) counts as "No".
	private void OnDestroy()
	{
		OnDisable();
		if (!resolved && isQuestionBoxActive && instanceRequestVersion == activeRequestVersion)
		{
			resolved = true;
			Action callback = noCallback;
			ResetRequest(instanceRequestVersion);
			callback?.Invoke();
		}
	}

	private static void ResetRequest(int requestVersion)
	{
		if (activeRequestVersion != requestVersion)
		{
			return;
		}
		pendingInputLease?.Dispose();
		pendingInputLease = null;
		messageString = null;
		isQuestionBoxActive = false;
		yesString = null;
		noString = null;
		yesCallback = null;
		noCallback = null;
		requiredConfirmationString = null;
		rewardRequest = false;
		destructiveRequest = false;
		primaryLabelColorRequest = null;
		pointerOnlyRequest = false;
		modalInputRequest = false;
		inputKindRequest = GameplayInputContext.Kind.Modal;
		navigableRequest = false;
		cancelRequested = false;
	}

	private void YesQuestionBoxRewired(InputActionEventData _)
	{
		if (usesGameplayInput && !GameplayInputContext.Shared.TryConsumeAction(YesAction, true, inputLease))
		{
			return;
		}
		if (confirmationToggle == null || confirmationChecked)
		{
			Resolve(true);
		}
		else
		{
			// First press ticks the acknowledgement, the second one confirms.
			confirmationToggle.isOn = true;
		}
	}

	private void NoQuestionBoxRewired(InputActionEventData _)
	{
		Resolve(false);
	}

	private void Update()
	{
		if (!usesGameplayInput && !isPointerOnly && !isReward && !TextEntryInputGuard.BlocksShortcuts && Input.GetKeyDown(KeyCode.Escape))
		{
			Resolve(false);
		}
	}
}
