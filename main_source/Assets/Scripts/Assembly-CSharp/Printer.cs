using Bibloteka;
using DarkTonic.MasterAudio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Printer : Interactable
{

	[SerializeField]
	private Collider coll;

	[SerializeField]
	private GameObject page;

	[SerializeField]
	private Animation pageAnimation;

	public override void Start()
	{
		base.Start();
		page.SetActive(false);
	}

	public override void Clicked(bool isClickedByOtherPlayer)
	{
		base.Clicked(isClickedByOtherPlayer);
		coll.enabled = false;
		if (!isClickedByOtherPlayer)
		{
			MasterAudio.PlaySound3DAtTransform("fx", transform, 1f, null, 0f, "printer");
		}
		StartCoroutine(Print());
		InteractionEnd();
	}

	private IEnumerator Print()
	{
		yield return Utils.GetDelay(5.5f);
		page.SetActive(true);
		pageAnimation.Play();
	}
}
