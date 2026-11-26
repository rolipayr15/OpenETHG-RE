using UnityEngine.SceneManagement;
using System;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

public class JournalEntryBox : MonoBehaviour
{
	private static string _shortContent;

	private static string _longContent;

	private static Sprite _sprite;

	private static bool isMessageBoxActive;

	private static GameplayInputContext.Lease pendingInputLease;

	private GameplayInputWindow inputWindow;

	private bool usesGameplayInput;

	private bool closing;

	[SerializeField]
	private Text shortContent;

	[SerializeField]
	private Text longContent;

	[SerializeField]
	private Image image;

	private static Action callAfterDismiss;

	private static Action<string> unload = delegate(string scene) { SceneManager.UnloadSceneAsync(scene, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects); };

	private GameplayInputContext.Lease inputLease => inputWindow != null ? inputWindow.Lease : null;

	private int SubmitAction => usesGameplayInput ? 47 : 33;

	public static void Show(string shortContent, string longContent, Sprite sprite)
	{
		if (isMessageBoxActive)
		{
			return;
		}
		_shortContent = shortContent;
		_longContent = longContent;
		_sprite = sprite;
		isMessageBoxActive = true;
		if (CustomSingleton<PauseManager>.HasInstance)
		{
			pendingInputLease = GameplayInputContext.Shared.Acquire(typeof(JournalEntryBox), GameplayInputContext.Kind.Modal);
		}
		if (SceneManager.LoadSceneAsync("JournalEntryBox", LoadSceneMode.Additive) == null)
		{
			throw new InvalidOperationException("Could not load JournalEntryBox.");
		}
	}

	public void OkButtonClicked()
	{
		if (closing)
		{
			return;
		}
		closing = true;
		callAfterDismiss?.Invoke();
		unload?.Invoke("JournalEntryBox");
	}

	private void Awake()
	{
		usesGameplayInput = CustomSingleton<PauseManager>.HasInstance;
	}

	private void OnEnable()
	{
		if (usesGameplayInput && !closing)
		{
			inputWindow = GameplayInputWindow.GetOrAdd(this);
			inputWindow.Open(GameplayInputContext.Kind.Modal, GameplayInputContext.WindowInput.Cancel | GameplayInputContext.WindowInput.Submit, OkButtonClicked);
		}
		pendingInputLease?.Dispose();
		pendingInputLease = null;
		InteractionManager.SetRewiredMapping(DismissMessageBoxRewired, SubmitAction);
		if (!usesGameplayInput)
		{
			InteractionManager.SetRewiredMapping(DismissMessageBoxRewired, 34);
		}
	}

	private void Start()
	{
		shortContent.text = _shortContent;
		longContent.text = _longContent;
		image.sprite = _sprite;
	}

	private void OnDisable()
	{
		InteractionManager.RemoveRewiredMapping(DismissMessageBoxRewired, SubmitAction);
		InteractionManager.RemoveRewiredMapping(DismissMessageBoxRewired, 34);
		if (inputWindow != null)
		{
			inputWindow.Close();
		}
	}

	private void OnDestroy()
	{
		OnDisable();
		isMessageBoxActive = false;
	}

	private void DismissMessageBoxRewired(InputActionEventData _not_used)
	{
		if (!usesGameplayInput || GameplayInputContext.Shared.TryConsumeAction(SubmitAction, true, inputLease))
		{
			OkButtonClicked();
		}
	}

	private void Update()
	{
		if (!usesGameplayInput && !TextEntryInputGuard.BlocksShortcuts && Input.GetKeyDown(KeyCode.Escape))
		{
			OkButtonClicked();
		}
	}
}
