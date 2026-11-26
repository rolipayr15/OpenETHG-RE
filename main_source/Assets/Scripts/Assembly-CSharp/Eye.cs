using Bibloteka;
using Prime31.MessageKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Eye : Interactable
{

	[SerializeField]
	private MeshRenderer meshRenderer;

	[SerializeField]
	private Collider eyeCollider;

	[SerializeField]
	private Material materialNoShine;

	public override void Start()
	{
		int difficulty = (int)UserGameSettings.currentDifficulty;
		if (difficulty == 3 || difficulty == 4)
		{
			meshRenderer.material = materialNoShine;
		}
		base.Start();
	}

	public override void Clicked(bool isClickedByOtherPlayer)
	{
		base.Clicked(isClickedByOtherPlayer);
		bool respawns = false;
		if (!isClickedByOtherPlayer)
		{
			Eyes.DailyChallenges.DailyChallengeRuntime.EyeCollected(this);
			MessageKit.post(1);
			MessageKit<Interactable>.post(58, this);
			respawns = (int)UserGameSettings.currentMode == 1;
		}
		if (respawns)
		{
			StartCoroutine(Cooldown());
		}
		else
		{
			gameObject.SetActive(false);
		}
		InteractionEnd();
	}

	private IEnumerator Cooldown()
	{
		meshRenderer.enabled = false;
		eyeCollider.enabled = false;
		// Reappears only while the player is on another floor.
		do
		{
			yield return Utils.GetDelay(60f);
		}
		while (CustomSingleton<FloorManager>.Instance.GetFloor(CustomSingleton<PlayerManager>.Instance.Heart.transform.position) == CustomSingleton<FloorManager>.Instance.GetFloor(gameObject.transform.position));
		meshRenderer.enabled = true;
		eyeCollider.enabled = true;
	}
}
