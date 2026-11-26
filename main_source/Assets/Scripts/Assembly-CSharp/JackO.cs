using Bibloteka;
using DarkTonic.MasterAudio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class JackO : Interactable
{

	[SerializeField]
	private JournalData journalEntry;

	[SerializeField]
	private Material materialAfterInteraction;

	private bool isPlaying;

	public override void Start()
	{
		base.Start();
		if ((int)UserGameSettings.currentLevel != 3 || (journalEntry != null && journalEntry.IsUnlocked()))
		{
			SetMaterialToNotInteractable();
		}
	}

	private void SetMaterialToNotInteractable()
	{
		if (materialAfterInteraction != null)
		{
			GetComponent<Renderer>().material = materialAfterInteraction;
		}
	}

	public override void Clicked(bool isClickedByOtherPlayer)
	{
		if ((int)UserGameSettings.currentLevel == 3 && journalEntry != null && !journalEntry.IsUnlocked())
		{
			journalEntry.Unlock();
			SetMaterialToNotInteractable();
		}
		if (!isPlaying)
		{
			StartCoroutine(PlaySound());
		}
		InteractionEnd();
	}

	private IEnumerator PlaySound()
	{
		isPlaying = true;
		MasterAudio.PlaySound3DAtTransform("JackO", transform);
		yield return Utils.GetDelay(5f);
		isPlaying = false;
	}
}
