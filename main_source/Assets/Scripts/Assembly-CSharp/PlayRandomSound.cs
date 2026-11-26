using Bibloteka;
using DarkTonic.MasterAudio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayRandomSound : MonoBehaviour
{

	[SerializeField]
	private string variationName;

	[SerializeField]
	private int interval;

	[SerializeField]
	private int schoolIntervalLow;

	[SerializeField]
	private int schoolIntervalHigh;

	private void Start()
	{
		StartCoroutine(PlaySound());
	}

	private IEnumerator PlaySound()
	{
		while (true)
		{
			// The school map uses a randomized interval.
			if ((int)UserGameSettings.currentLevel == 3)
			{
				interval = UnityEngine.Random.Range(schoolIntervalLow, schoolIntervalHigh);
			}
			yield return Utils.GetDelay(interval);
			MasterAudio.PlaySound3DAtTransform(variationName, transform, 0.5f);
		}
	}
}
