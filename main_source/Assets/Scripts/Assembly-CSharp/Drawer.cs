using DarkTonic.MasterAudio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class Drawer : Interactable
{

	private static readonly WaitForFixedUpdate FixedUpdateYield = new WaitForFixedUpdate();

	[SerializeField]
	private Transform endTransform;

	private Vector3 startPosition;

	private Rigidbody rigidb;

	private Renderer furnitureRenderer;

	[SerializeField]
	private GameObject collectible;

	[SerializeField]
	private Material materialAfterInteraction;

	protected bool isOpen;

	[SerializeField]
	protected float totalTime = 0.5f;

	private Coroutine moveCoroutine;

	public override void Start()
	{
		base.Start();
		startPosition = transform.position;
		rigidb = GetComponent<Rigidbody>();
		rigidb.isKinematic = true;
		rigidb.interpolation = RigidbodyInterpolation.Interpolate;
		furnitureRenderer = GetComponent<Renderer>();
		int difficulty = (int)UserGameSettings.currentDifficulty;
		if (difficulty == 3 || difficulty == 4)
		{
			furnitureRenderer.material = materialAfterInteraction;
		}
	}

	public override void Clicked(bool isClickedByOtherPlayer)
	{
		base.Clicked(isClickedByOtherPlayer);
		if (collectible != null && collectible.activeInHierarchy)
		{
			Interactable interactable = collectible.GetComponent<Interactable>();
			if (interactable != null)
			{
				interactable.Clicked(isClickedByOtherPlayer);
			}
		}
		if (moveCoroutine != null)
		{
			StopCoroutine(moveCoroutine);
		}
		bool target = !isOpen;
		isOpen = target;
		moveCoroutine = StartCoroutine(OpenOrClose(false, target));
		furnitureRenderer.material = materialAfterInteraction;
		InteractionEnd();
		if (!isClickedByOtherPlayer)
		{
			MasterAudio.PlaySound3DAtTransform("drawer", transform);
		}
	}

	private IEnumerator OpenOrClose(bool isClickedByOtherPlayer, bool openTarget)
	{
		Vector3 from = rigidb.position;
		Vector3 to = openTarget ? endTransform.position : startPosition;
		float elapsed = 0f;
		while (elapsed < totalTime)
		{
			elapsed += Time.fixedDeltaTime;
			float t = Mathf.Clamp01(elapsed / totalTime);
			rigidb.MovePosition(Vector3.Lerp(from, to, t * t * (3f - 2f * t)));
			yield return FixedUpdateYield;
		}
		rigidb.MovePosition(to);
		moveCoroutine = null;
	}
}
