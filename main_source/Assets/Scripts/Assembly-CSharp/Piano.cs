using Bibloteka;
using DarkTonic.MasterAudio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public class Piano : Interactable
{

	public override void Clicked(bool isClickedByOtherPlayer)
	{
		base.Clicked(isClickedByOtherPlayer);
		StartCoroutine(PlaySound());
	}

	private IEnumerator PlaySound()
	{
		MasterAudio.PlaySound3DAtTransform("Piano", transform);
		yield return Utils.GetDelay(0.2f);
		InteractionEnd();
	}
}
