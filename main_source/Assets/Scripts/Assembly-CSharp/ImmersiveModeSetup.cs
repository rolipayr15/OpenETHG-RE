using Bibloteka;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ImmersiveModeSetup : MonoBehaviour
{

	private CanvasGroup canvasGroup;

	private IEnumerator Start()
	{
		if (UserGameSettings.isImmersiveMode)
		{
			canvasGroup = GetComponent<CanvasGroup>();
			yield return Utils.ChangeOverTime(3f, delegate(float t)
			{
				canvasGroup.alpha = 1f - t;
			});
		}
	}
}
