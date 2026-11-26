using System;
using System.Collections.Generic;
using EyesAnalytics;
using Prime31.MessageKit;
using Rewired;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class JournalWindow : CustomSingleton<JournalWindow>
{
	private Canvas canvas;

	[SerializeField]
	private Animator buttonAnimator;

	[SerializeField]
	private ScrollSnap scrollSnap;

	private bool isOpen;

	private bool initialized;

	private GameplayInputContext.Lease inputLease;

	private IDisposable worldPause;

	private GamepadSelectionScope selectionScope;

	private GameObject lastSeenSelection;

	private bool wasTopOwner;

	private int targetPage;

	private int lastUnlockedItem;

	private bool lastUnlockedKnown;

	private List<JournalData> journalData;

	[SerializeField]
	private GameObject journalEntryTemplate;

	[SerializeField]
	private JournalLevel[] journalLevelDatas;

	[SerializeField]
	private JournalEnemy[] journalEnemyDatas;

	private bool syncOnDestroy;

	private Dictionary<string, JournalEntryGUI> journalEntries;

	public GameplayInputContext.Lease InputLease => inputLease;

	private void Awake()
	{
		canvas = GetComponent<Canvas>();
		if (scrollSnap != null)
		{
			scrollSnap.onPageChange += OnPageChange;
		}
	}

	private void OnPageChange(int page)
	{
		targetPage = page;
	}

	private void OnEnable()
	{
		MessageKit<string>.addObserver(41, HighlightIcon);
		MessageKit.addObserver(64, CloseJournal);
	}

	private void OnDisable()
	{
		MessageKit<string>.removeObserver(41, HighlightIcon);
		MessageKit.removeObserver(64, CloseJournal);
		CloseJournal();
	}

	private void Start()
	{
		if (UserGameSettings.IsMultiplayer())
		{
			if (buttonAnimator != null)
			{
				buttonAnimator.gameObject.SetActive(false);
			}
		}
		else
		{
			if (canvas == null || journalEntryTemplate == null || journalLevelDatas == null || journalEnemyDatas == null)
			{
				return;
			}
			journalData = new List<JournalData>();
			foreach (JournalLevel level in journalLevelDatas)
			{
				if (level != null && level.GetLevel() == UserGameSettings.currentLevel)
				{
					journalData.AddRange(level.GetJournalDatas());
				}
			}
			foreach (JournalEnemy enemy in journalEnemyDatas)
			{
				if (enemy != null && enemy.GetEnemy() == UserGameSettings.currentEnemy)
				{
					journalData.AddRange(enemy.GetJournalDatas());
				}
			}
			journalEntries = new Dictionary<string, JournalEntryGUI>();
			foreach (JournalData data in journalData)
			{
				GameObject clone = UnityEngine.Object.Instantiate(journalEntryTemplate, journalEntryTemplate.transform.parent);
				LocalizedFontLayout.CopyBaselines(journalEntryTemplate, clone);
				JournalEntryGUI entry = clone.GetComponent<JournalEntryGUI>();
				entry.SetupFromJournalData(data);
				journalEntries.Add(data.GetJournalId().ToString(), entry);
			}
			UnityEngine.Object.Destroy(journalEntryTemplate);
			initialized = true;
		}
	}

	public bool CanOpenJournal()
	{
		if (!base.isActiveAndEnabled || UserGameSettings.IsMultiplayer() || !initialized || canvas == null || journalEntries == null || journalEntries.Count == 0)
		{
			return false;
		}
		if (!PlayerPrefz.GetBool("WAS_ANY_JOURNAL_ENTRY_UNLOCKED", false) && TutorialManager.ShouldPlayTutorial() && !PlayerPrefz.GetBool("JOURNAL_TUTORIAL_COMPLETED", false))
		{
			return false;
		}
		return GameplayInputContext.Shared.Current.StartActions;
	}

	public void ShowJournal()
	{
		if (!isOpen && CanOpenJournal())
		{
			inputLease = GameplayInputContext.Shared.Acquire(this, (GameplayInputContext.Kind)4, (GameplayInputContext.WindowInput)0, CloseJournal);
			worldPause = CustomSingleton<PauseManager>.Instance.AcquirePause(this);
			canvas.enabled = true;
			isOpen = true;
			if (scrollSnap != null)
			{
				targetPage = scrollSnap.CurrentPage();
			}
			AddPointerProbes();
			selectionScope = GamepadSelectionScope.Begin(base.gameObject, inputLease, PreferredEntryButton, IsEntryButton);
			LevelUI.SkillUsed((LevelUI.Skill)5);
			MessageKit.post(70);
		}
	}

	public void CloseJournal()
	{
		GamepadSelectionScope.End(base.gameObject);
		selectionScope = null;
		lastSeenSelection = null;
		if (canvas != null)
		{
			canvas.enabled = false;
		}
		isOpen = false;
		if (inputLease != null)
		{
			inputLease.Dispose();
		}
		inputLease = null;
		if (worldPause != null)
		{
			worldPause.Dispose();
		}
		worldPause = null;
	}

	private void HighlightIcon(string entryId)
	{
		syncOnDestroy = true;
		if (journalEntries == null || !journalEntries.TryGetValue(entryId, out JournalEntryGUI entry))
		{
			Debug.LogWarning("Journal entry '" + entryId + "' is not available for level " + string.Format("'{0}' in scene ", UserGameSettings.currentLevel) + "'" + SceneManager.GetActiveScene().name + "'.");
			return;
		}
		if (buttonAnimator != null)
		{
			buttonAnimator.SetTrigger("highlight");
		}
		if (entry != null)
		{
			entry.UnlockEntry();
		}
	}

	public bool IsOpen()
	{
		return isOpen;
	}

	public void SetLastUnlockedItem(int index)
	{
		lastUnlockedItem = index;
		lastUnlockedKnown = true;
		scrollSnap.ChangePage(index);
	}

	private static bool IsEntryButton(Selectable selectable)
	{
		JournalEntryGUI entry = selectable.GetComponentInParent<JournalEntryGUI>();
		return entry != null && entry.OpensDetails(selectable);
	}

	private void AddPointerProbes()
	{
		if (journalEntries == null)
		{
			return;
		}
		foreach (JournalEntryGUI entry in journalEntries.Values)
		{
			if (!(entry != null))
			{
				continue;
			}
			foreach (Button button in entry.GetComponentsInChildren<Button>(true))
			{
				if (entry.OpensDetails(button) && button.GetComponent<PointerSelectionProbe>() == null)
				{
					button.gameObject.AddComponent<PointerSelectionProbe>();
				}
			}
		}
	}

	private Selectable PreferredEntryButton()
	{
		if (journalEntries == null)
		{
			return null;
		}
		JournalEntryGUI best = null;
		foreach (JournalEntryGUI entry in journalEntries.Values)
		{
			if (entry == null || !entry.IsUnlocked)
			{
				continue;
			}
			if (lastUnlockedKnown && entry.transform.GetSiblingIndex() == lastUnlockedItem)
			{
				return entry.DetailsButton();
			}
			if (best == null || best.transform.GetSiblingIndex() >= entry.transform.GetSiblingIndex())
			{
				best = entry;
			}
		}
		return (best != null) ? best.DetailsButton() : null;
	}

	private void LateUpdate()
	{
		if (!isOpen)
		{
			wasTopOwner = false;
		}
		bool becameTop = false;
		bool top = false;
		if (inputLease != null && GameplayInputContext.Shared.Current.TopOwner == inputLease)
		{
			becameTop = !wasTopOwner;
			top = true;
		}
		wasTopOwner = top;
		if (!isOpen || selectionScope == null || scrollSnap == null)
		{
			return;
		}
		EventSystem eventSystem = EventSystem.current;
		GameObject selected = ((eventSystem != null) ? eventSystem.currentSelectedGameObject : null);
		if (selected == lastSeenSelection)
		{
			return;
		}
		lastSeenSelection = selected;
		JournalEntryGUI entry = ((selected != null) ? selected.GetComponentInParent<JournalEntryGUI>() : null);
		if (!(entry == null) && IsEntryButton(selected.GetComponent<Selectable>()) && !PointerSelectionProbe.WasSelectedByPointer(selected))
		{
			if (!selectionScope.SelectedThisFrame(selected) || becameTop)
			{
				ScrollIntoView(entry.transform.GetSiblingIndex());
			}
		}
	}

	private void ScrollIntoView(int index)
	{
		int visible = Mathf.Max(scrollSnap.ItemsVisibleAtOnce, 1);
		if (targetPage <= index)
		{
			if (index <= targetPage + visible - 1)
			{
				return;
			}
			index = index - visible + 1;
		}
		scrollSnap.ChangePage(index);
	}

	private void Update()
	{
		ControlModeManager manager = ControlModeManager.Instance;
		Player player = ((manager != null) ? manager.Player : null);
		if (!ReInput.isReady || player == null)
		{
			return;
		}
		if (GameplayInputContext.Shared.TryConsumeAction(50, player.GetButtonDown(50), inputLease))
		{
			if (isOpen)
			{
				CloseJournal();
			}
			else
			{
				IconPressFeedback.PlayThen(50, ShowJournal);
			}
		}
	}

	protected override void OnDestroy()
	{
		MessageKit<string>.removeObserver(41, HighlightIcon);
		MessageKit.removeObserver(64, CloseJournal);
		CloseJournal();
		if (scrollSnap != null)
		{
			scrollSnap.onPageChange -= OnPageChange;
		}
		if (syncOnDestroy)
		{
			MessageKit.post(134);
		}
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}
