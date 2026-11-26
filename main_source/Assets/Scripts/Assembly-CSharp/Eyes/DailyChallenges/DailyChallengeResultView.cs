using Rewired;
using I2.Loc;
using UnityEngine.EventSystems;
using System.Globalization;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Eyes.DailyChallenges
{
	[DefaultExecutionOrder(-100)]
	public sealed class DailyChallengeResultView : MonoBehaviour
	{
		[SerializeField]
		private Button entryButton;

		[SerializeField]
		private Text entryLabel;

		[SerializeField]
		private GameObject tips;

		[SerializeField]
		private GameObject windowRoot;

		[SerializeField]
		private RectTransform windowPanel;

		[SerializeField]
		private Text heading;

		[SerializeField]
		private Text content;

		[SerializeField]
		private ScrollRect scroll;

		[SerializeField]
		private Text nextChallengeLabel;

		[SerializeField]
		private Text nextChallengeCountdown;

		[SerializeField]
		private Button shareButton;

		[SerializeField]
		private Text shareLabel;

		[SerializeField]
		private Button closeButton;

		[SerializeField]
		private CanvasGroup backgroundGroup;

		[SerializeField]
		private WinResultShare resultShare;

		private DailyChallengeCompletion completion;

		private Canvas windowCanvas;

		private bool isOpen;

		private bool restorePending;

		private bool previousInteractable;

		private bool previousBlocksRaycasts;

		private bool tipsWereActive;

		private GameObject previousSelection;

		private int openedFrame = -1;

		private float nextRefresh;

		private GameplayInputContext.Lease inputLease;

		public bool IsOpen => isOpen;

		private void Awake()
		{
			windowCanvas = windowRoot.GetComponent<Canvas>();
			tipsWereActive = tips != null && tips.activeSelf;
			windowRoot.SetActive(false);
			entryButton.gameObject.SetActive(false);
			entryButton.onClick.AddListener(Show);
			closeButton.onClick.AddListener(Close);
			shareButton.onClick.AddListener(Share);
			shareButton.navigation = NavigationTo(closeButton);
			closeButton.navigation = NavigationTo(shareButton);
		}

		private void Start()
		{
			completion = DailyChallengeRuntime.LatestCompletion;
			Refresh();
		}

		private void Update()
		{
			if (nextRefresh <= Time.unscaledTime)
			{
				Refresh();
			}
			if (!isOpen)
			{
				return;
			}
			FitPanel();
			shareButton.interactable = resultShare != null && !resultShare.IsSharing;
			if (Time.frameCount == openedFrame || (resultShare != null && resultShare.IsSharing) || HasHigherWindow())
			{
				return;
			}
			if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
			{
				bool canShare = shareButton.gameObject.activeInHierarchy && shareButton.IsInteractable();
				EventSystem.current.SetSelectedGameObject((canShare ? shareButton : closeButton).gameObject);
			}
			if (InteractionManager.GetButtonDown(39) || (!TextEntryInputGuard.BlocksShortcuts && Input.GetKeyDown(KeyCode.Escape)))
			{
				Close();
			}
			if (!ReInput.isReady)
			{
				return;
			}
			float overflow = scroll.content.rect.height - scroll.viewport.rect.height;
			if (overflow > 0f)
			{
				float axis = InteractionManager.RewiredPlayer.GetAxis(36);
				scroll.verticalNormalizedPosition = Mathf.Clamp01(scroll.verticalNormalizedPosition + axis * 500f * Time.unscaledDeltaTime / overflow);
			}
		}

		private void LateUpdate()
		{
			if (restorePending)
			{
				RestoreBackground();
			}
		}

		private bool HasCompletion()
		{
			return completion != null && completion.Challenge.IsCompleted && completion == DailyChallengeRuntime.LatestCompletion;
		}

		private void Refresh()
		{
			nextRefresh = Time.unscaledTime + 1f;
			RefreshLocalizedFonts();
			entryLabel.text = Localize("DailyChallenges/CompletedButton", "Daily challenge completed!");
			shareLabel.text = Localize("Share", "Share");
			bool has = HasCompletion();
			if (!has && isOpen)
			{
				Hide();
			}
			entryButton.gameObject.SetActive(has);
			if (tips != null)
			{
				tips.SetActive(!has && tipsWereActive);
			}
			if (!has)
			{
				return;
			}
			DailyChallengeDefinition definition = completion.Challenge.Definition;
			string parameter = definition.Parameter.ToString("N0", CultureInfo.InvariantCulture);
			heading.text = Localize(definition.TitleTerm, definition.Title).Replace("{0}", parameter);
			content.supportRichText = true;
			content.text = Localize(definition.DescriptionTerm, definition.Description).Replace("{0}", parameter);
			DateTime now = DateTime.UtcNow;
			nextChallengeLabel.text = now < completion.NextChallengeAtUtc ? Localize("DailyChallenges/NextChallengeIn", "Next challenge in") : Localize("DailyChallenges/NextChallengeAvailable", "Next challenge is available");
			nextChallengeCountdown.text = FormatRemainingTime(completion.NextChallengeAtUtc, now);
			shareButton.interactable = resultShare != null && !resultShare.IsSharing;
			ResizeContent();
		}

		private void RefreshLocalizedFonts()
		{
			Font font = LocalizationManager.GetTranslatedObjectByTermName<Font>("GeneralUseFont");
			if (font != null)
			{
				LocalizedFontLayout.SetFont(entryLabel, font);
				LocalizedFontLayout.SetFont(heading, font);
				LocalizedFontLayout.SetFont(content, font);
				LocalizedFontLayout.SetFont(nextChallengeLabel, font);
				LocalizedFontLayout.SetFont(shareLabel, font);
			}
		}

		public void Show()
		{
			if (isOpen || !HasCompletion())
			{
				return;
			}
			if (restorePending)
			{
				RestoreBackground();
			}
			previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
			previousInteractable = backgroundGroup.interactable;
			previousBlocksRaycasts = backgroundGroup.blocksRaycasts;
			backgroundGroup.interactable = false;
			backgroundGroup.blocksRaycasts = false;
			inputLease = GameplayInputContext.Shared.Acquire(this, GameplayInputContext.Kind.Pause);
			isOpen = true;
			openedFrame = Time.frameCount;
			windowRoot.SetActive(true);
			FitPanel();
			Refresh();
			scroll.verticalNormalizedPosition = 1f;
			if (EventSystem.current != null)
			{
				EventSystem.current.SetSelectedGameObject((shareButton.gameObject.activeInHierarchy ? shareButton : closeButton).gameObject);
			}
		}

		public void Close()
		{
			if (isOpen && Time.frameCount != openedFrame)
			{
				Hide();
			}
		}

		private void Hide()
		{
			isOpen = false;
			windowRoot.SetActive(false);
			if (EventSystem.current != null)
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
			restorePending = true;
		}

		private void Share()
		{
			if (isOpen && HasCompletion() && resultShare != null)
			{
				resultShare.ShareDailyChallengeFrom(shareButton, completion);
			}
		}

		private void RestoreBackground()
		{
			restorePending = false;
			if (backgroundGroup != null)
			{
				backgroundGroup.interactable = previousInteractable;
				backgroundGroup.blocksRaycasts = previousBlocksRaycasts;
			}
			inputLease?.Dispose();
			inputLease = null;
			if (EventSystem.current != null)
			{
				GameObject target = null;
				if (previousSelection != null && previousSelection.activeInHierarchy)
				{
					target = previousSelection;
				}
				else if (entryButton != null && entryButton.gameObject.activeInHierarchy)
				{
					target = entryButton.gameObject;
				}
				EventSystem.current.SetSelectedGameObject(target);
				previousSelection = null;
			}
		}

		private bool HasHigherWindow()
		{
			foreach (GraphicRaycaster raycaster in FindObjectsByType<GraphicRaycaster>(FindObjectsSortMode.None))
			{
				if (raycaster.isActiveAndEnabled)
				{
					Canvas canvas = raycaster.GetComponent<Canvas>();
					if (canvas != null && canvas.isActiveAndEnabled && canvas.sortingOrder > windowCanvas.sortingOrder)
					{
						return true;
					}
				}
			}
			return false;
		}

		private void FitPanel()
		{
			RectTransform parent = windowPanel.parent as RectTransform;
			Canvas canvas = parent != null ? parent.GetComponentInParent<Canvas>() : null;
			if (canvas == null)
			{
				return;
			}
			Canvas root = canvas.rootCanvas;
			Camera camera = root.renderMode == RenderMode.ScreenSpaceOverlay ? null : root.worldCamera;
			Rect safe = Screen.safeArea;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, safe.min, camera, out Vector2 min);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, safe.max, camera, out Vector2 max);
			windowPanel.anchoredPosition = (min + max) * 0.5f;
			Rect rect = windowPanel.rect;
			float scale = Mathf.Min((max.x - min.x - 120f) / rect.width, (max.y - min.y - 120f) / rect.height);
			scale = scale < 0.01f ? 0.01f : Mathf.Min(scale, 1f);
			windowPanel.localScale = new Vector3(scale, scale, 1f);
			ResizeContent();
		}

		private void ResizeContent()
		{
			if (isOpen)
			{
				content.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(scroll.viewport.rect.height, content.preferredHeight + 16f));
			}
		}

		internal static string FormatRemainingTime(DateTime expiresAtUtc, DateTime nowUtc)
		{
			long seconds = (long)Math.Max(0.0, (expiresAtUtc - nowUtc).TotalSeconds);
			return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}", seconds / 3600, seconds / 60 % 60, seconds % 60);
		}

		private static Navigation NavigationTo(Selectable other)
		{
			Navigation navigation = default(Navigation);
			navigation.mode = Navigation.Mode.Explicit;
			navigation.selectOnUp = other;
			navigation.selectOnDown = other;
			navigation.selectOnLeft = other;
			navigation.selectOnRight = other;
			return navigation;
		}

		private static string Localize(string term, string fallback)
		{
			string value = string.IsNullOrEmpty(term) ? null : LocalizationManager.GetTranslation(term);
			return string.IsNullOrWhiteSpace(value) || value == term ? fallback : value;
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
				RestoreBackground();
			}
		}

		private void OnDestroy()
		{
			inputLease?.Dispose();
			inputLease = null;
			if (entryButton != null)
			{
				entryButton.onClick.RemoveListener(Show);
			}
			if (closeButton != null)
			{
				closeButton.onClick.RemoveListener(Close);
			}
			if (shareButton != null)
			{
				shareButton.onClick.RemoveListener(Share);
			}
		}
	}
}
