using UnityEngine.SceneManagement;
using System;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

public class UpdateAvailable : MonoBehaviour
{
	private static string messageString;

	private static bool isMessageBoxActive;

	[SerializeField]
	private Text messageLabel;

	private static Action callAfterDismiss;

	private static Action callAfterSkip;

	private bool resolved;

	private static Action<string> openUrl = Application.OpenURL;

	private static Action<string> unload = delegate(string scene) { SceneManager.UnloadSceneAsync(scene, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects); };

	public static void Show(string message, Action _callAfterDismiss, Action _callAfterSkip)
	{
		if (!isMessageBoxActive)
		{
			messageString = message;
			callAfterDismiss = _callAfterDismiss;
			callAfterSkip = _callAfterSkip;
			SceneManager.LoadSceneAsync("UpdateAvailable", LoadSceneMode.Additive);
			isMessageBoxActive = true;
		}
	}

	private void Start()
	{
		messageLabel.text = messageString;
	}

	private void Awake()
	{
		InteractionManager.SetRewiredMapping(DismissMessageBoxRewired, 33);
		InteractionManager.SetRewiredMapping(DismissMessageBoxRewired, 34);
	}

	private void OnDestroy()
	{
		InteractionManager.RemoveRewiredMapping(DismissMessageBoxRewired, 33);
		InteractionManager.RemoveRewiredMapping(DismissMessageBoxRewired, 34);
	}

	private void DismissMessageBoxRewired(InputActionEventData _not_used)
	{
		Later();
	}

	private bool Resolve()
	{
		if (resolved)
		{
			return false;
		}
		resolved = true;
		isMessageBoxActive = false;
		return true;
	}

	public void Download()
	{
		if (Resolve())
		{
			openUrl?.Invoke("market://details?id=com.eyesthegame.eyes");
			UserGameSettings.skipThisTime = true;
			unload?.Invoke("UpdateAvailable");
		}
	}

	public void Skip()
	{
		if (Resolve())
		{
			callAfterSkip?.Invoke();
			unload?.Invoke("UpdateAvailable");
		}
	}

	public void Later()
	{
		if (Resolve())
		{
			callAfterDismiss?.Invoke();
			unload?.Invoke("UpdateAvailable");
		}
	}

	private void Update()
	{
		if (!TextEntryInputGuard.BlocksShortcuts && Input.GetKeyDown(KeyCode.Escape))
		{
			Later();
		}
	}
}
