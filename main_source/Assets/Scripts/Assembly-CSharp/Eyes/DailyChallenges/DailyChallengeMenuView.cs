using System;
using System.Globalization;
using I2.Loc;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Eyes.DailyChallenges
{
	// Main-menu daily challenge button (countdown + status icon) and its details window.
	[DefaultExecutionOrder(-100)]
	public sealed class DailyChallengeMenuView : MonoBehaviour
	{
		[SerializeField]
		private MenuContentWindow windows;

		[SerializeField]
		private Button menuButton;

		[SerializeField]
		private Text menuCountdown;

		[SerializeField]
		private Image statusIcon;

		[SerializeField]
		private Sprite incompleteSprite;

		[SerializeField]
		private Sprite completedSprite;

		[SerializeField]
		private GameObject windowRoot;

		[SerializeField]
		private RectTransform windowPanel;

		[SerializeField]
		private Text heading;

		[SerializeField]
		private Text description;

		[SerializeField]
		private Text deadline;

		[SerializeField]
		private Text reward;

		[SerializeField]
		private Text status;

		[SerializeField]
		private Button closeButton;

		[SerializeField]
		private CanvasGroup mainMenu;

		private bool isOpen;

		private bool restorePending;

		private bool previousInteractable;

		private bool previousBlocksRaycasts;

		private GameObject previousSelection;

		private int openedFrame = -1;

		private float nextRefresh;

		private Vector2 lastPanelSize;

		private GameplayInputContext.Lease inputLease;

		private bool fontsInitialized;

		private Font authoredFont;

		private Font authoredDeadlineFont;

		public bool IsOpen => isOpen;

		private void Awake()
		{
			windowRoot.SetActive(false);
			menuButton.onClick.AddListener(windows.ShowDailyChallenge);
			closeButton.onClick.AddListener(Close);
			closeButton.navigation = default(Navigation);
		}

		private void Start()
		{
			Refresh();
		}

		private void Update()
		{
			if (nextRefresh <= Time.unscaledTime)
			{
				Refresh();
			}
			// Ignore the frame that opened the window so the same press doesn't close it again.
			if (!isOpen || Time.frameCount == openedFrame)
			{
				return;
			}
			if (InteractionManager.GetButtonDown(RewiredConsts.Action.UICancel))
			{
				Close();
				return;
			}
			if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != closeButton.gameObject)
			{
				EventSystem.current.SetSelectedGameObject(closeButton.gameObject);
			}
		}

		private void LateUpdate()
		{
			if (restorePending)
			{
				RestoreMenuInput();
			}
		}

		public void Show()
		{
			if (isOpen)
			{
				return;
			}
			if (restorePending)
			{
				RestoreMenuInput();
			}
			previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
			previousInteractable = mainMenu.interactable;
			previousBlocksRaycasts = mainMenu.blocksRaycasts;
			mainMenu.interactable = false;
			mainMenu.blocksRaycasts = false;
			inputLease = GameplayInputContext.Shared.Acquire(this, GameplayInputContext.Kind.Pause);
			isOpen = true;
			openedFrame = Time.frameCount;
			windowRoot.SetActive(true);
			Refresh();
			if (EventSystem.current != null)
			{
				EventSystem.current.SetSelectedGameObject(closeButton.gameObject);
			}
		}

		// Menu input is given back in LateUpdate so the closing click can't reach the menu this frame.
		public void Hide()
		{
			if (!isOpen)
			{
				return;
			}
			isOpen = false;
			if (windowRoot != null)
			{
				windowRoot.SetActive(false);
			}
			if (EventSystem.current != null)
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
			restorePending = true;
		}

		private void Close()
		{
			if (isOpen && Time.frameCount != openedFrame)
			{
				windows.CloseAllWindows();
			}
		}

		private void RestoreMenuInput()
		{
			restorePending = false;
			if (mainMenu != null)
			{
				mainMenu.interactable = previousInteractable;
				mainMenu.blocksRaycasts = previousBlocksRaycasts;
			}
			inputLease?.Dispose();
			inputLease = null;
			if (EventSystem.current != null)
			{
				GameObject selection;
				if (previousSelection != null && previousSelection.activeInHierarchy)
				{
					selection = previousSelection;
				}
				else
				{
					selection = menuButton != null ? menuButton.gameObject : null;
				}
				EventSystem.current.SetSelectedGameObject(selection);
			}
			previousSelection = null;
		}

		private void Refresh()
		{
			nextRefresh = Time.unscaledTime + 1f;
			Render(DailyChallengeRuntime.Refresh());
		}

		public void Render(DailyChallengeView view)
		{
			RefreshLocalizedFonts();
			bool available = view != null && view.Definition != null;
			string remaining = available ? FormatRemainingTime(view.ExpiresAtUtc, DateTime.UtcNow) : string.Empty;
			menuCountdown.text = remaining;
			menuCountdown.enabled = available;
			menuButton.interactable = available;
			if (statusIcon != null)
			{
				statusIcon.sprite = available && view.IsCompleted ? completedSprite : incompleteSprite;
				statusIcon.color = new Color(1f, 1f, 1f, available ? 1f : 0.4f);
			}
			if (!available)
			{
				heading.text = Localize("DailyChallenges/Menu", "Daily challenge");
				description.text = Localize("DailyChallenges/Unavailable", "No daily challenge is available right now.");
				deadline.text = string.Empty;
				reward.text = string.Empty;
				status.text = string.Empty;
				return;
			}
			DailyChallengeDefinition definition = view.Definition;
			string parameter = definition.Parameter.ToString("N0", CultureInfo.InvariantCulture);
			heading.text = Localize(definition.TitleTerm, definition.Title).Replace("{0}", parameter);
			description.text = Localize(definition.DescriptionTerm, definition.Description).Replace("{0}", parameter);
			// Counting challenges show progress; pass/fail ones only show the result.
			int kind = (int)definition.Kind;
			bool counted = kind > 16 || ((9 >> kind) & 1) != 0;
			string statusText;
			if (view.IsCompleted)
			{
				statusText = Localize("DailyChallenges/Completed", "Completed");
			}
			else
			{
				statusText = Localize("DailyChallenges/Incomplete", counted ? "Incomplete" : "Incompleted");
			}
			if (counted)
			{
				int goal = view.Definition != null ? view.Definition.Goal : 0;
				statusText = "<color=#8ED6E8>" + Localize("DailyChallenges/Progress", "Progress") + ": " + view.Progress.ToString(CultureInfo.InvariantCulture) + " / " + goal.ToString(CultureInfo.InvariantCulture) + "</color>\n" + statusText;
			}
			status.text = statusText;
			reward.text = Localize("DailyChallenges/CompletionReward", "Reward: 75");
			deadline.text = "<color=#F2C078>" + Localize("DailyChallenges/TimeRemaining", "Time remaining") + ": " + remaining + "</color>\n\n" + Localize("DailyChallenges/FinishBeforeDeadline", "Finish the game before the deadline for progress to count.");
		}

		private void RefreshLocalizedFonts()
		{
			if (!fontsInitialized)
			{
				authoredFont = heading.font;
				authoredDeadlineFont = deadline.font;
				fontsInitialized = true;
			}
			Font font = LocalizationManager.GetTranslatedObjectByTermName<Font>("GeneralUseFont");
			if (font == null)
			{
				return;
			}
			LocalizedFontLayout.SetFont(heading, font);
			LocalizedFontLayout.SetFont(description, font);
			LocalizedFontLayout.SetFont(reward, font);
			LocalizedFontLayout.SetFont(status, font);
			// The deadline keeps its own (digit-friendly) font while the language uses the authored one.
			LocalizedFontLayout.SetFont(deadline, font == authoredFont ? authoredDeadlineFont : font);
		}

		internal static string FormatRemainingTime(DateTime expiresAtUtc, DateTime nowUtc)
		{
			long seconds = (long)Math.Max(0.0, (expiresAtUtc - nowUtc).TotalSeconds);
			return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}", seconds / 3600, seconds / 60 % 60, seconds % 60);
		}

		private static string Localize(string term, string fallback)
		{
			string translation = string.IsNullOrEmpty(term) ? null : LocalizationManager.GetTranslation(term);
			if (string.IsNullOrWhiteSpace(translation) || translation == term)
			{
				return fallback;
			}
			return translation;
		}

		private void OnApplicationFocus(bool focused)
		{
			if (focused)
			{
				nextRefresh = 0f;
			}
		}

		private void OnDisable()
		{
			if (isOpen)
			{
				Hide();
			}
			if (restorePending)
			{
				RestoreMenuInput();
			}
		}

		private void OnDestroy()
		{
			inputLease?.Dispose();
			inputLease = null;
			if (menuButton != null && windows != null)
			{
				menuButton.onClick.RemoveListener(windows.ShowDailyChallenge);
			}
			if (closeButton != null)
			{
				closeButton.onClick.RemoveListener(Close);
			}
		}
	}
}
