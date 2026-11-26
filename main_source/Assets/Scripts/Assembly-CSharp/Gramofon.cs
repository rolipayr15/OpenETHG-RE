using Bibloteka;
using Prime31.MessageKit;
using DarkTonic.MasterAudio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Gramofon : Interactable
{


	private bool turnOn;

	private RecordPlayer gramofon;

	private bool isLocked;

	[SerializeField]
	private JournalData journalEntryGramofon;

	public override void Start()
	{
		base.Start();
		gramofon = GetComponent<RecordPlayer>();
	}

	public override void Clicked(bool isClickedByOtherPlayer)
	{
		base.Clicked(isClickedByOtherPlayer);
		if ((int)UserGameSettings.currentLevel == 1 && !UserGameSettings.IsMultiplayer() && !journalEntryGramofon.IsUnlocked())
		{
			journalEntryGramofon.Unlock();
			MessageKit<string>.post(3, journalEntryGramofon.GetShortText());
			MasterAudio.PlaySound3DAtTransform("fx", transform, 1f, null, 0f, "paper");
		}
		if (!isLocked)
		{
			if (!turnOn)
			{
				gramofon.recordPlayerActive = true;
				turnOn = true;
				StartCoroutine(TurnOn());
			}
			else
			{
				gramofon.recordPlayerActive = false;
				turnOn = false;
				MasterAudio.StopAllSoundsOfTransform(transform);
				StartCoroutine(TurnOff());
			}
		}
		InteractionEnd();
	}

	private IEnumerator TurnOn()
	{
		isLocked = true;
		yield return Utils.GetDelay(1f);
		isLocked = false;
		MasterAudio.PlaySound3DAtTransform("fx", transform, 1f, null, 0f, "frenchSong");
	}

	private IEnumerator TurnOff()
	{
		isLocked = true;
		yield return Utils.GetDelay(1f);
		isLocked = false;
	}
}
