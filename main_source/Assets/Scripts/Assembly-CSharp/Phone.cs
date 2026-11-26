using Bibloteka;
using DarkTonic.MasterAudio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Phone : MonoBehaviour
{

	private Coroutine callCoroutine;

	[SerializeField]
	private Collider phoneCollider;

	private bool firstTime = true;

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player") && firstTime && (int)UserGameSettings.currentEnemy == 0 && (int)UserGameSettings.currentMode == 0 && (int)UserGameSettings.currentGameMode == 0)
		{
			callCoroutine = StartCoroutine(WaitAndCall());
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other != null && other.CompareTag("Player") && callCoroutine != null)
		{
			StopCoroutine(callCoroutine);
			phoneCollider.enabled = false;
		}
	}

	private IEnumerator WaitAndCall()
	{
		yield return Utils.GetDelay(15f);
		MasterAudio.PlaySound3DAtTransform("fx", transform, 1f, null, 0f, "telephonering");
		phoneCollider.enabled = true;
	}

	public void Call()
	{
		MasterAudio.StopAllSoundsOfTransform(transform);
		MasterAudio.PlaySound3DAtTransform("fx", transform, 1f, null, 0f, "rickroll");
		firstTime = false;
	}

	public void FinishCall()
	{
		MasterAudio.StopAllSoundsOfTransform(transform);
		MasterAudio.PlaySound3DAtTransform("fx", transform, 1f, null, 0f, "phonePutDown");
		phoneCollider.enabled = false;
		firstTime = true;
	}
}
