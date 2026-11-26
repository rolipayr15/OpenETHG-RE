using Bibloteka;
using Prime31.MessageKit;
using DarkTonic.MasterAudio;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class Chat : MonoBehaviour
{

	[SerializeField]
	private Text chatText;

	[SerializeField]
	private GameObject tapToContinue;

	private Coroutine coroutine;

	[SerializeField]
	private GameObject introMansion;

	[SerializeField]
	private GameObject introHospital;

	[SerializeField]
	private GameObject introSchool;

	[SerializeField]
	private JournalData journalMansion;

	[SerializeField]
	private JournalData journalHospital;

	[SerializeField]
	private JournalData journalSchool;

	private bool isLoading;

	private bool canBeSkipped;

	private static Action<Chat> leave = delegate(Chat chat) { chat.Leave(); };

	private void Start()
	{
		JournalData journal = null;
		switch ((int)UserGameSettings.currentLevel)
		{
		case 0:
			introMansion.SetActive(true);
			journal = journalMansion;
			break;
		case 1:
			introHospital.SetActive(true);
			journal = journalHospital;
			break;
		case 3:
			introSchool.SetActive(true);
			journal = journalSchool;
			break;
		}
		if (journal != null)
		{
			journal.SilentUnlock();
		}
		MessageKit.addObserver(42, MuteSound);
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		if (!UserGameSettings.volume.HasValue)
		{
			UserGameSettings.volume = PlayerPrefs.GetFloat("musicVolume", 1f);
		}
		MasterAudio.MasterVolumeLevel = UserGameSettings.volume.Value;
		MasterAudio.PlaylistMasterVolume = UserGameSettings.volume.Value;
		coroutine = StartCoroutine(ChatCoroutine());
	}

	private IEnumerator ChatCoroutine()
	{
		MasterAudio.PlaySound3DAtTransform("Intro", transform, 1f, null, 0f, "typing");
		yield return Utils.GetDelay(1f);
		tapToContinue.SetActive(true);
		canBeSkipped = true;
	}

	public void Loading()
	{
		if (!isLoading)
		{
			isLoading = true;
			leave(this);
		}
	}

	private void Leave()
	{
		MuteSound();
		if (coroutine != null)
		{
			StopCoroutine(coroutine);
		}
		SceneManager.LoadScene("Loading");
	}

	private void MuteSound()
	{
		MasterAudio.StopSoundGroupOfTransform(transform, "Intro");
	}

	private void OnDestroy()
	{
		MessageKit.removeObserver(42, MuteSound);
	}

	public void SkipIntro()
	{
		switch ((int)UserGameSettings.currentLevel)
		{
		case 0:
			PlayerPrefz.SetBool("SKIP_INTRO_MANSION", true);
			break;
		case 1:
			PlayerPrefz.SetBool("SKIP_INTRO_HOSPITAL", true);
			break;
		case 3:
			PlayerPrefz.SetBool("SKIP_INTRO_SCHOOL", true);
			break;
		}
	}
}
