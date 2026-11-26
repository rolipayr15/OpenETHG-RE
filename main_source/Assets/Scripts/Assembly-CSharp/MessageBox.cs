using System;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// One-button message dialog loaded as the additive "MessageBox" scene. Messages raised while the consent form is up
// are queued and shown one after another once it closes.
public class MessageBox : MonoBehaviour
{
	private const string SceneName = "MessageBox";

	private static string messageString;

	internal static bool isMessageBoxActive = false;

	[SerializeField]
	private Text messageLabel;

	private static Action callAfterDismiss = null;

	private static readonly Queue<KeyValuePair<string, Action>> deferredWhileConsentFormVisible = new Queue<KeyValuePair<string, Action>>();

	private bool closing;

	private static Action<string> unload = Unload;

	public static bool IsRequestActive => isMessageBoxActive;

	public static void Show(string message, Action _callAfterDismiss = null)
	{
		if (ConsentRecovery.IsConsentFormVisible)
		{
			deferredWhileConsentFormVisible.Enqueue(new KeyValuePair<string, Action>(message, _callAfterDismiss));
			return;
		}
		if (isMessageBoxActive)
		{
			return;
		}
		messageString = message;
		callAfterDismiss = _callAfterDismiss;
		SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
		isMessageBoxActive = true;
	}

	public static void ShowDeferredMessages()
	{
		if (ConsentRecovery.IsConsentFormVisible || isMessageBoxActive || SceneManager.GetSceneByName(SceneName).isLoaded)
		{
			return;
		}
		if (deferredWhileConsentFormVisible.Count != 0)
		{
			KeyValuePair<string, Action> next = deferredWhileConsentFormVisible.Dequeue();
			Show(next.Key, next.Value);
		}
	}

	public void OkButtonClicked()
	{
		if (closing)
		{
			return;
		}
		closing = true;
		isMessageBoxActive = false;
		callAfterDismiss?.Invoke();
		unload?.Invoke(SceneName);
	}

	// The next queued message waits until this scene has actually unloaded.
	private static void Unload(string scene)
	{
		AsyncOperation operation = SceneManager.UnloadSceneAsync(scene, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
		if (operation != null && deferredWhileConsentFormVisible.Count > 0)
		{
			operation.completed += delegate
			{
				ShowDeferredMessages();
			};
		}
	}

	private void Start()
	{
		messageLabel.text = messageString;
	}

	private void Awake()
	{
		InteractionManager.SetRewiredMapping(DismissMessageBoxRewired, RewiredConsts.Action.Accept);
		InteractionManager.SetRewiredMapping(DismissMessageBoxRewired, RewiredConsts.Action.Decline);
	}

	private void OnDestroy()
	{
		InteractionManager.RemoveRewiredMapping(DismissMessageBoxRewired, RewiredConsts.Action.Accept);
		InteractionManager.RemoveRewiredMapping(DismissMessageBoxRewired, RewiredConsts.Action.Decline);
	}

	private void DismissMessageBoxRewired(InputActionEventData _not_used)
	{
		OkButtonClicked();
	}

	private void Update()
	{
		if (!TextEntryInputGuard.BlocksShortcuts && Input.GetKeyDown(KeyCode.Escape))
		{
			OkButtonClicked();
		}
	}
}
