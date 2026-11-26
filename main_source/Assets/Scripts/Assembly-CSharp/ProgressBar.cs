using Bibloteka;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : CustomSingleton<ProgressBar>
{

	[SerializeField]
	private Text progressBarLabel;

	public void UpdateProgressBar(int c)
	{
		if (c > 0)
		{
			StartCoroutine(CoinsCounter(c));
			return;
		}
		foreach (Transform child in transform.parent)
		{
			if (child != transform)
			{
				Image image = child.GetComponent<Image>();
				if (image != null)
				{
					image.enabled = false;
				}
			}
		}
		progressBarLabel.text = "0";
	}

	private IEnumerator CoinsCounter(int c)
	{
		progressBarLabel.text = "0";
		int numberOfSteps = Mathf.RoundToInt(Mathf.Sqrt((float)c + 1f)) + 5;
		for (int i = 0; i <= numberOfSteps; i++)
		{
			progressBarLabel.text = Mathf.RoundToInt(Mathf.Lerp(0f, c, (float)i / (float)numberOfSteps)).ToString();
			yield return Utils.GetDelay(0.05f);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}
