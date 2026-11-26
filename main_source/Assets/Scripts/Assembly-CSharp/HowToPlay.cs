using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using Rewired;
using UnityEngine;

public class HowToPlay : MonoBehaviour
{
	private static bool isMessageBoxActive;

	[SerializeField]
	private GameObject krasueMansion;

	[SerializeField]
	private GameObject charlieHospital;

	[SerializeField]
	private GameObject goodBoySchool;

	[SerializeField]
	private GameObject witch;

	private Dictionary<HowToPlayEntry, GameObject> entryToDescription;

	private static HowToPlayEntry _entry;

	private static Action callAfterDismiss;

	private bool closing;

	private static Action<string> unload = delegate(string scene) { SceneManager.UnloadSceneAsync(scene, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects); };

	public static void Show(HowToPlayEntry entry)
	{
		if (!isMessageBoxActive)
		{
			_entry = entry;
			SceneManager.LoadSceneAsync("HowToPlay", LoadSceneMode.Additive);
			isMessageBoxActive = true;
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
		unload?.Invoke("HowToPlay");
	}

	private void Start()
	{
		InteractionManager.SetRewiredMapping(DismissMessageBoxRewired, 33);
		InteractionManager.SetRewiredMapping(DismissMessageBoxRewired, 34);
		entryToDescription = new Dictionary<HowToPlayEntry, GameObject>
		{
			{ HowToPlayEntry.KRASUE, krasueMansion },
			{ (HowToPlayEntry)3, charlieHospital },
			{ (HowToPlayEntry)4, goodBoySchool },
			{ (HowToPlayEntry)5, witch }
		};
		entryToDescription[_entry].SetActive(true);
	}

	private void OnDestroy()
	{
		InteractionManager.RemoveRewiredMapping(DismissMessageBoxRewired, 33);
		InteractionManager.RemoveRewiredMapping(DismissMessageBoxRewired, 34);
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
