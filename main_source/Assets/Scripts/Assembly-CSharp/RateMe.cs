using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class RateMe : MonoBehaviour
{

	private static ReviewPromptPresenter pendingPresenter;

	private ReviewPromptPresenter owner;

	private Canvas dialogCanvas;

	private CanvasGroup loadingGroup;

	private bool canvasWasEnabled;

	private bool answered;

	public static void OpenFor(ReviewPromptPresenter presenter)
	{
		pendingPresenter = presenter;
		SceneManager.LoadSceneAsync("RateMe", LoadSceneMode.Additive);
	}

	private void Awake()
	{
		answered = true;
		owner = pendingPresenter;
		pendingPresenter = null;
		dialogCanvas = GetComponent<Canvas>();
		canvasWasEnabled = dialogCanvas != null && dialogCanvas.enabled;
		if (dialogCanvas != null)
		{
			dialogCanvas.enabled = false;
		}
		loadingGroup = gameObject.AddComponent<CanvasGroup>();
		loadingGroup.interactable = false;
		loadingGroup.blocksRaycasts = false;
	}

	private IEnumerator Start()
	{
		yield return null;
		answered = true;
		CloseWindow();
	}

	public void CloseWindow()
	{
		Scene scene = gameObject.scene;
		if (scene.IsValid() && scene.isLoaded)
		{
			SceneManager.UnloadSceneAsync(scene, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
		}
	}

	public void Agree()
	{
		if (!answered)
		{
			answered = true;
			ReviewPromptState.RecordResponse((ReviewPromptResponse)1);
			CloseWindow();
		}
	}

	public void Disagree()
	{
		if (!answered)
		{
			answered = true;
			ReviewPromptState.RecordResponse((ReviewPromptResponse)2);
			CloseWindow();
		}
	}

	public void Later()
	{
		if (!answered)
		{
			answered = true;
			ReviewPromptState.RecordResponse((ReviewPromptResponse)3);
			CloseWindow();
		}
	}
}
