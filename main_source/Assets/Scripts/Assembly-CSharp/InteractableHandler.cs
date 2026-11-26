using Bibloteka;
using Prime31.MessageKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class InteractableHandler : MonoBehaviour
{

	[SerializeField]
	private Animator sackAnimator;

	[SerializeField]
	private Animator meatAnimator;

	[SerializeField]
	private Animator eyeAnimator;

	[SerializeField]
	private Animator sawAnimator;

	[SerializeField]
	private Animator safeCodeAnimator;

	private void Start()
	{
		MessageKit<string>.addObserver(19, HighlightSack);
		MessageKit.addObserver(1, HighlightEye);
		if ((int)UserGameSettings.currentEnemy == 2)
		{
			MessageKit.addObserver(28, HighlightMeat);
		}
		if ((int)UserGameSettings.currentLevel == 0)
		{
			MessageKit<bool>.addObserver(20, HighlightSaw);
			MessageKit<bool>.addObserver(18, HighlightSafeCode);
		}
	}

	private IEnumerator HighlightDelayed(Animator anim)
	{
		anim.gameObject.SetActive(true);
		anim.SetTrigger("highlight");
		yield return Utils.GetDelay(0.8f);
		anim.gameObject.SetActive(false);
	}

	private void HighlightEye()
	{
		StartCoroutine(HighlightDelayed(eyeAnimator));
	}

	private void HighlightSack(string bagId)
	{
		StartCoroutine(HighlightDelayed(sackAnimator));
	}

	private void HighlightMeat()
	{
		StartCoroutine(HighlightDelayed(meatAnimator));
	}

	private void HighlightSaw(bool isClickedByOther)
	{
		StartCoroutine(HighlightDelayed(sawAnimator));
	}

	private void HighlightSafeCode(bool isClickedByOther)
	{
		StartCoroutine(HighlightDelayed(safeCodeAnimator));
	}
}
