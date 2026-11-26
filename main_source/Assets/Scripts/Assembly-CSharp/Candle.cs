using Bibloteka;
using DarkTonic.MasterAudio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Candle : Interactable
{


	private bool isBlown;

	[SerializeField]
	private ParticleSystem fire;

	[SerializeField]
	private Light fireLight;

	[SerializeField]
	[Min(0f)]
	private float lightFadeDuration = 0.5f;

	public override void Clicked(bool isClickedByOtherPlayer)
	{
		base.Clicked(isClickedByOtherPlayer);
		if (!isBlown)
		{
			StartCoroutine(Blow());
		}
		InteractionEnd();
	}

	private IEnumerator Blow()
	{
		isBlown = true;
		fire.Stop();
		MasterAudio.PlaySound3DAtTransform("fx", transform, 1f, null, 0f, "candleBreath");
		float litIntensity = fireLight != null ? fireLight.intensity : 0f;
		yield return AnimateLightIntensity(0f, lightFadeDuration);
		if (fireLight != null)
		{
			fireLight.enabled = false;
		}
		yield return Utils.GetDelay(3f);
		fire.Play();
		if (fireLight != null)
		{
			fireLight.intensity = 0f;
			fireLight.enabled = true;
		}
		yield return AnimateLightIntensity(litIntensity, lightFadeDuration);
		isBlown = false;
	}

	private IEnumerator AnimateLightIntensity(float targetIntensity, float duration)
	{
		if (duration > 0f)
		{
			if (fireLight == null)
			{
				yield return Utils.GetDelay(duration);
				yield break;
			}
			float startIntensity = fireLight.intensity;
			float elapsed = 0f;
			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				fireLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, elapsed / duration);
				yield return null;
			}
		}
		if (fireLight != null)
		{
			fireLight.intensity = targetIntensity;
		}
	}
}
