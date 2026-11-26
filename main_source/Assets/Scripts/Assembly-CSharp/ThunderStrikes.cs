using Bibloteka;
using DarkTonic.MasterAudio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ThunderStrikes : MonoBehaviour
{

	private bool firstTime = true;

	private PlayerCameraShake shaker;

	private void Start()
	{
		shaker = CustomSingleton<PlayerManager>.Instance.PlayerCamera.GetComponent<PlayerCameraShake>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player") && firstTime)
		{
			StartCoroutine(Lightning());
			firstTime = false;
		}
	}

	private IEnumerator Lightning()
	{
		Flash(true);
		MasterAudio.PlaySound3DAtTransform("fx", transform, 1f, null, 0f, "lightning");
		shaker.ShakeLightningHit();
		yield return Utils.GetDelay(0.2f);
		Flash(false);
		yield return Utils.GetDelay(0.2f);
		Flash(true);
		yield return Utils.GetDelay(0.1f);
		Flash(false);
		yield return Utils.GetDelay(0.15f);
		Flash(true);
		yield return Utils.GetDelay(0.1f);
		Flash(false);
		gameObject.SetActive(false);
		static void Flash(bool on)
		{
			EyesLightingComposer.SetFlashMultiplier(on ? 1.25f : (float?)null);
		}
	}
}
