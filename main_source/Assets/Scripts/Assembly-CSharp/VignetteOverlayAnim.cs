using Prime31.MessageKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class VignetteOverlayAnim : MonoBehaviour
{


	[SerializeField]
	private CanvasGroup group;

	[SerializeField]
	private float fadeInTime = 0.25f;

	[SerializeField]
	private float fadeOutTime = 0.25f;

	private Coroutine routine;

	private void Start()
	{
		MessageKit.addObserver(17, Death);
		MessageKit.addObserver(53, Revive);
	}

	private void Reset()
	{
		group = GetComponent<CanvasGroup>();
	}

	private void Death()
	{
		StartFade(0.7f, fadeOutTime);
	}

	private void Revive()
	{
		StartFade(0f, fadeInTime);
	}

	public void FadeIn(float targetAlpha = 1f)
	{
		StartFade(targetAlpha, fadeInTime);
	}

	public void FadeOut(float targetAlpha = 0f)
	{
		StartFade(targetAlpha, fadeOutTime);
	}

	public void Pulse(float baseAlpha = 0.35f, float pulseAlpha = 0.55f, float period = 0.8f)
	{
		Stop();
		routine = StartCoroutine(PulseRoutine(baseAlpha, pulseAlpha, period));
	}

	public void Stop()
	{
		if (routine != null)
		{
			StopCoroutine(routine);
		}
		routine = null;
	}

	private void StartFade(float target, float time)
	{
		Stop();
		routine = StartCoroutine(FadeRoutine(target, time));
	}

	private IEnumerator FadeRoutine(float target, float time)
	{
		float start = group.alpha;
		float t = 0f;
		float inverseTime = 1f / time;
		while (t < time)
		{
			t += Time.unscaledDeltaTime;
			group.alpha = Mathf.Lerp(start, target, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t * inverseTime)));
			yield return null;
		}
		group.alpha = target;
		routine = null;
	}

	private IEnumerator PulseRoutine(float a, float b, float period)
	{
		group.alpha = a;
		float t = 0f;
		while (true)
		{
			t += Time.unscaledDeltaTime;
			float wave = (Mathf.Sin(t / period * 3.1415927f * 2f) + 1f) * 0.5f;
			group.alpha = Mathf.Lerp(a, b, Mathf.SmoothStep(0f, 1f, wave));
			yield return null;
		}
	}
}
