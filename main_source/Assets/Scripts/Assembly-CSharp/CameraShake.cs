using Prime31.MessageKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CameraShake : MonoBehaviour
{

	[Header("Default shake params")]
	[SerializeField]
	private float duration = 0.25f;

	[SerializeField]
	private float amplitude = 0.12f;

	[SerializeField]
	private float frequency = 25f;

	[Header("Damping")]
	[SerializeField]
	private AnimationCurve damping = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

	private Vector3 initialLocalPos;

	private Coroutine shakeCoroutine;

	private void Awake()
	{
		initialLocalPos = transform.localPosition;
		MessageKit.addObserver(17, ShakeMonsterGrab);
	}

	private void OnDisable()
	{
		if (shakeCoroutine != null)
		{
			StopCoroutine(shakeCoroutine);
		}
		transform.localPosition = initialLocalPos;
		shakeCoroutine = null;
	}

	public void ShakeDefault()
	{
		Shake(duration, amplitude, frequency);
	}

	public void ShakeMonsterGrab()
	{
		Shake(duration, amplitude, frequency);
	}

	public void ShakeLightningHit()
	{
		Shake(0.18f, 0.16f, 35f);
	}

	public void Shake(float duration, float amplitude, float frequency)
	{
		if (shakeCoroutine != null)
		{
			StopCoroutine(shakeCoroutine);
		}
		shakeCoroutine = StartCoroutine(ShakeRoutine(duration, amplitude, frequency));
	}

	private IEnumerator ShakeRoutine(float duration, float amplitude, float frequency)
	{
		float elapsed = 0f;
		initialLocalPos = transform.localPosition;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float strength = damping.Evaluate(Mathf.Clamp01(elapsed / duration)) * amplitude;
			float time = Time.time * frequency;
			float x = Mathf.PerlinNoise(time, 0.1f) * 2f - 1f;
			float y = Mathf.PerlinNoise(0.2f, time) * 2f - 1f;
			float z = Mathf.PerlinNoise(time, 0.7f) * 2f - 1f;
			transform.localPosition = initialLocalPos + new Vector3(x, y, z) * strength;
			yield return null;
		}
		transform.localPosition = initialLocalPos;
		shakeCoroutine = null;
	}
}
