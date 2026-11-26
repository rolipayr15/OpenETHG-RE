using DarkTonic.MasterAudio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using VLB;

public class RandomFlicker : MonoBehaviour
{

	[SerializeField]
	private EffectFlicker effectFlicker;

	[SerializeField]
	private GameObject sparks;

	private static bool alreadyBrokenInThisGame;

	private bool canBeBroken;

	private int counter;

	private void Start()
	{
		float value = UnityEngine.Random.value;
		if (value > 0.9f)
		{
			SetHighFlicker();
		}
		else if (value > 0.6f)
		{
			SetMediumFlicker();
		}
		else
		{
			canBeBroken = true;
			effectFlicker.enabled = false;
		}
	}

	private void SetMediumFlicker()
	{
		effectFlicker.frequency = 10f;
		effectFlicker.smoothing = 0.05f;
	}

	private void SetHighFlicker()
	{
		effectFlicker.smoothing = 0f;
		effectFlicker.frequency = 20f;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player") && canBeBroken && UnityEngine.Random.value > 0.4f && !alreadyBrokenInThisGame)
		{
			if (counter < 5)
			{
				counter++;
			}
			else
			{
				StartCoroutine(BulbBreak());
				canBeBroken = false;
				alreadyBrokenInThisGame = true;
			}
		}
	}

	private IEnumerator BulbBreak()
	{
		MasterAudio.PlaySound3DAtTransform("fx", gameObject.transform, 1f, null, 0f, "bulb_break");
		VLB.VolumetricLightBeamSD volumetricLightBeamSD = GetComponent<VLB.VolumetricLightBeamSD>();
		volumetricLightBeamSD.intensityMultiplier = 0.1f;
		effectFlicker.enabled = true;
		SetHighFlicker();
		yield return Bibloteka.Utils.GetDelay(1f);
		sparks.SetActive(true);
		MasterAudio.PlaySound3DAtTransform("fx", gameObject.transform, 1f, null, 0f, "lightbulb-break");
		yield return Bibloteka.Utils.GetDelay(0.15f);
		effectFlicker.enabled = false;
		volumetricLightBeamSD.enabled = false;
		yield return Bibloteka.Utils.GetDelay(1f);
		sparks.SetActive(false);
	}

	private void OnDestroy()
	{
		alreadyBrokenInThisGame = false;
	}
}
