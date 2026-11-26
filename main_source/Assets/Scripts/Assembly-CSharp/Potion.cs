using Bibloteka;
using DarkTonic.MasterAudio;
using Prime31.MessageKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Potion : Interactable
{

	private int id;

	private MeshRenderer meshRenderer;

	private Collider potionCollider;

	private bool isEndlessModeActive;

	public override void Start()
	{
		base.Start();
		if ((int)UserGameSettings.currentMode == 1)
		{
			isEndlessModeActive = true;
			meshRenderer = GetComponent<MeshRenderer>();
			potionCollider = GetComponent<Collider>();
		}
	}

	public void SetId(int i)
	{
		id = i;
	}

	public override void Clicked(bool isClickedByOtherPlayer)
	{
		base.Clicked(isClickedByOtherPlayer);
		if (!isClickedByOtherPlayer)
		{
			MessageKit<int>.post(15, id);
		}
		MasterAudio.PlaySound3DAtTransform("fx", transform, 1f, null, 0f, "gulp");
		InteractionEnd();
		if (isEndlessModeActive)
		{
			StartCoroutine(Cooldown());
		}
		else
		{
			gameObject.SetActive(false);
		}
	}

	private IEnumerator Cooldown()
	{
		meshRenderer.enabled = false;
		potionCollider.enabled = false;
		while (CustomSingleton<FloorManager>.Instance.GetFloor(CustomSingleton<PlayerManager>.Instance.Heart.transform.position) == CustomSingleton<FloorManager>.Instance.GetFloor(gameObject.transform.position))
		{
			yield return Utils.GetDelay(60f);
		}
		meshRenderer.enabled = true;
		potionCollider.enabled = true;
	}
}
