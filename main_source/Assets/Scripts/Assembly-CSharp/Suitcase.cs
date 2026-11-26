using Prime31.MessageKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class Suitcase : Interactable
{

	[SerializeField]
	private JournalData suitcase;

	[SerializeField]
	private Transform endTransform;

	private Rigidbody rigidb;

	private Quaternion startRotation;

	[SerializeField]
	protected float totalTime = 0.5f;

	private bool wasOpened;

	private bool isOpen;

	[SerializeField]
	private Material materialAfterInteraction;

	public override void Start()
	{
		rigidb = GetComponent<Rigidbody>();
		startRotation = rigidb.rotation;
		base.Start();
	}

	public override void Clicked(bool isClickedByOtherPlayer)
	{
		if (!isClickedByOtherPlayer)
		{
			CustomSingleton<MultiplayerSettings>.Instance.SendInteraction(transform.position.sqrMagnitude, MultiplayerProtocol.InteractableIdCodes.GetInteractableId(GetType().Name));
		}
		if (!suitcase.IsUnlocked())
		{
			MessageKit<string, bool>.post(3, suitcase.GetShortText(), isClickedByOtherPlayer);
			suitcase.Unlock();
		}
		StartCoroutine(OpenOrClose());
		isOpen = !isOpen;
		if (!wasOpened)
		{
			if (materialAfterInteraction != null)
			{
				GetComponent<Renderer>().material = materialAfterInteraction;
			}
			wasOpened = true;
		}
		InteractionEnd();
	}

	private IEnumerator OpenOrClose()
	{
		float elapsed = 0f;
		Quaternion from = isOpen ? endTransform.rotation : startRotation;
		Quaternion to = isOpen ? startRotation : endTransform.rotation;
		while (elapsed < totalTime)
		{
			elapsed += Time.deltaTime;
			transform.rotation = Quaternion.Slerp(from, to, elapsed / totalTime);
			yield return null;
		}
	}
}
