using Rewired;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// While a text field is being edited: registers a TextEntry input context (so typed letters don't trigger game
// shortcuts), turns off UI navigation events, and lets Escape close the field instead of opening the pause menu.
[DefaultExecutionOrder(-30200)]
public sealed class TextEntryInputGuard : MonoBehaviour
{
	private GameplayInputContext policy;

	private GameplayInputContext.Lease lease;

	private EventSystem guardedSystem;

	private bool previousNavigation;

	private GameObject lastTextSelection;

	private int selectionFrame = -1;

	public static bool BlocksShortcuts
	{
		get
		{
			GameplayInputContext shared = GameplayInputContext.Shared;
			return shared.HasActive(GameplayInputContext.Kind.TextEntry) || shared.Current.TransitionBarrier;
		}
	}

	public static bool CancelDown => GameplayInputContext.Shared.HasActive(GameplayInputContext.Kind.TextEntry) && KeyDown(KeyCode.Escape);

	// Native pointer input (desktop builds) first, Rewired's keyboard otherwise.
	public static bool KeyHeld(KeyCode key)
	{
		if (NativePointerInput.TryReadKey(key, false, out bool value))
		{
			return value;
		}
		if (!ReInput.isReady)
		{
			return false;
		}
		Keyboard keyboard = ReInput.controllers.Keyboard;
		if (keyboard == null)
		{
			return false;
		}
		int index = keyboard.GetButtonIndexByKeyCode(key);
		return index >= 0 && keyboard.GetButton(index);
	}

	public static bool KeyDown(KeyCode key)
	{
		if (NativePointerInput.TryReadKey(key, true, out bool value))
		{
			return value;
		}
		if (!ReInput.isReady)
		{
			return false;
		}
		Keyboard keyboard = ReInput.controllers.Keyboard;
		if (keyboard == null)
		{
			return false;
		}
		int index = keyboard.GetButtonIndexByKeyCode(key);
		return index >= 0 && keyboard.GetButtonDown(index);
	}

	private void OnEnable()
	{
		policy = GameplayInputContext.Shared;
		lease = policy.Register(this, GameplayInputContext.Kind.TextEntry, HasSelectedText, GameplayInputContext.WindowInput.Cancel, CloseSelectedText);
		policy.Changed += ApplyNavigation;
	}

	private bool HasSelectedText()
	{
		if (!isActiveAndEnabled)
		{
			return false;
		}
		GameObject selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
		if (selected != lastTextSelection)
		{
			lastTextSelection = selected;
			selectionFrame = Time.frameCount;
		}
		// The chat has its own input handling.
		if (selected == null || !selected.activeInHierarchy || selected.GetComponentInParent<UltimateChatAutodisabler>() != null)
		{
			return false;
		}
		// A field counts from the frame it gets selected, before it reports focus.
		bool justSelected = selectionFrame == Time.frameCount;
		TMPro.TMP_InputField tmpField = selected.GetComponent<TMPro.TMP_InputField>();
		if (tmpField != null)
		{
			return tmpField.isActiveAndEnabled && tmpField.IsInteractable() && (justSelected || tmpField.isFocused);
		}
		InputField field = selected.GetComponent<InputField>();
		return field != null && field.isActiveAndEnabled && field.IsInteractable() && (justSelected || field.isFocused);
	}

	private void CloseSelectedText()
	{
		EventSystem eventSystem = EventSystem.current;
		GameObject selected = eventSystem != null ? eventSystem.currentSelectedGameObject : null;
		if (selected != null)
		{
			TMPro.TMP_InputField tmpField = selected.GetComponent<TMPro.TMP_InputField>();
			if (tmpField != null)
			{
				tmpField.DeactivateInputField(false);
			}
			InputField field = selected.GetComponent<InputField>();
			if (field != null)
			{
				field.DeactivateInputField();
			}
			eventSystem.SetSelectedGameObject(null);
		}
		policy?.Synchronize();
	}

	private void Update()
	{
		policy.Synchronize();
		// Over gameplay the pause manager routes Escape itself.
		if (!CustomSingleton<PauseManager>.HasInstance && CancelDown && policy.TryConsumeCancel(true, out GameplayInputContext.Lease owner) == GameplayInputContext.CancelResult.CloseOwner)
		{
			owner?.TryHandleCancel();
		}
		ApplyNavigation();
	}

	private void LateUpdate()
	{
		policy.Synchronize();
		ApplyNavigation();
	}

	// Arrow keys must move the caret, not the UI selection, while typing.
	private void ApplyNavigation()
	{
		EventSystem current = EventSystem.current;
		bool unblocked = !policy.HasActive(GameplayInputContext.Kind.TextEntry) && !policy.Current.TransitionBarrier;
		if (guardedSystem != null && (guardedSystem != current || unblocked))
		{
			RestoreNavigation();
		}
		if (!unblocked && current != null)
		{
			if (guardedSystem == null)
			{
				guardedSystem = current;
				previousNavigation = current.sendNavigationEvents;
			}
			current.sendNavigationEvents = false;
		}
	}

	private void RestoreNavigation()
	{
		if (guardedSystem != null)
		{
			guardedSystem.sendNavigationEvents = previousNavigation;
		}
		guardedSystem = null;
	}

	private void OnDisable()
	{
		if (policy != null)
		{
			policy.Changed -= ApplyNavigation;
		}
		lease?.Dispose();
		lease = null;
		RestoreNavigation();
	}

	private void OnDestroy()
	{
		OnDisable();
	}
}
