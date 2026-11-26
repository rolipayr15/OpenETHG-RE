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

public class ChatOutro : MonoBehaviour
{

	private Text chatText;

	[SerializeField]
	private GameObject krasue;

	[SerializeField]
	private GameObject charlie;

	[SerializeField]
	private GameObject goodBoy;

	[SerializeField]
	private GameObject tapToContinue;

	[SerializeField]
	private JournalData krasueJournal;

	[SerializeField]
	private JournalData charlieJournal;

	[SerializeField]
	private JournalData goodBoyJournal;

	private bool loading;

	private static Action<ChatOutro> leave = delegate(ChatOutro chat) { chat.Leave(); };

	private void Start()
	{
		JournalData journal = null;
		switch ((int)UserGameSettings.currentEnemy)
		{
		case 0:
			krasue.SetActive(true);
			chatText = krasue.GetComponent<Text>();
			journal = krasueJournal;
			break;
		case 1:
			charlie.SetActive(true);
			chatText = charlie.GetComponent<Text>();
			journal = charlieJournal;
			break;
		case 2:
			goodBoy.SetActive(true);
			chatText = goodBoy.GetComponent<Text>();
			journal = goodBoyJournal;
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
		StartCoroutine(ChatCoroutine());
	}

	private IEnumerator ChatCoroutine()
	{
		MasterAudio.PlaySound3DAtTransform("Chat", transform, 1f, null, 0f, "typing");
		yield return Utils.GetDelay(1f);
		tapToContinue.SetActive(true);
		yield return Utils.GetDelay(13f);
		switch ((int)UserGameSettings.currentEnemy)
		{
		case 0:
			MasterAudio.PlaySound3DAtTransform("Chat", transform, 1f, null, 0f, "mmm");
			break;
		case 1:
			MasterAudio.PlaySound3DAtTransform("Chat", transform, 1f, null, 0f, "mmmCharlie");
			break;
		}
	}

	public void LoadMenu()
	{
		if (!loading)
		{
			loading = true;
			leave(this);
		}
	}

	private void Leave()
	{
		SceneManager.LoadScene("Win");
	}

	private void MuteSound()
	{
		MasterAudio.StopSoundGroupOfTransform(transform, "Chat");
	}

	private void OnDestroy()
	{
		MessageKit.removeObserver(42, MuteSound);
	}

	public void SkipOutro()
	{
		switch ((int)UserGameSettings.currentLevel)
		{
		case 0:
			PlayerPrefz.SetBool("SKIP_OUTRO_MANSION", true);
			break;
		case 1:
			PlayerPrefz.SetBool("SKIP_OUTRO_HOSPITAL", true);
			break;
		case 3:
			PlayerPrefz.SetBool("SKIP_OUTRO_SCHOOL", true);
			break;
		}
	}
}
