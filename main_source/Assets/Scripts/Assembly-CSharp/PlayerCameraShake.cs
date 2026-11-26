using Bibloteka;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerCameraShake : MonoBehaviour
{

	[SerializeField]
	private vp_FPCamera playerCamera;

	public void ShakeLightningHit()
	{
		StartCoroutine(Shake(0.5f));
	}

	private IEnumerator Shake(float duration)
	{
		playerCamera.ShakeSpeed = 5f;
		yield return Utils.GetDelay(duration);
		playerCamera.ShakeSpeed = 0f;
	}
}
