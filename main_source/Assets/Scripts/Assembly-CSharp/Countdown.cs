using Bibloteka;
using Prime31.MessageKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Countdown : MonoBehaviour
{

	private int t;

	[SerializeField]
	private Text text;

	private Coroutine countdownCoroutine;

	private StringBuilder sb;

	private bool shouldBeActive = true;

	private void Awake()
	{
		MessageKit<int>.addObserver(8, StartCountdown);
		MessageKit<bool>.addObserver(7, Vision);
		MessageKit<bool>.addObserver(6, Pause);
		MessageKit.addObserver(17, Death);
		MessageKit.addObserver(53, Revive);
	}

	private void Start()
	{
		sb = new StringBuilder();
		text.text = "";
	}

	private void StartCountdown(int i)
	{
		if (!shouldBeActive)
		{
			return;
		}
		t = i;
		if (countdownCoroutine != null)
		{
			StopCoroutine(countdownCoroutine);
			text.text = "";
		}
		if (i != 0)
		{
			countdownCoroutine = StartCoroutine(StartCountdownCoroutine());
		}
	}

	private IEnumerator StartCountdownCoroutine()
	{
		while (t >= 1)
		{
			t--;
			sb.Length = 0;
			sb.Append(t < 10 ? "0:0" : "O:");
			sb.Append(t);
			text.text = sb.ToString();
			yield return Utils.GetDelay(1f);
		}
		text.text = "";
		MessageKit.post(9);
	}

	private void Vision(bool isVisionActive)
	{
		text.enabled = !isVisionActive;
	}

	private void Pause(bool isGamePaused)
	{
		if (shouldBeActive)
		{
			text.enabled = !isGamePaused;
		}
	}

	private void Death()
	{
		text.enabled = false;
		shouldBeActive = false;
	}

	private void Revive()
	{
		text.enabled = true;
		shouldBeActive = true;
	}

	private void OnDestroy()
	{
		MessageKit<int>.removeObserver(8, StartCountdown);
		MessageKit<bool>.removeObserver(7, Vision);
		MessageKit<bool>.removeObserver(6, Pause);
		MessageKit.removeObserver(17, Death);
		MessageKit.removeObserver(53, Revive);
	}
}
