using Prime31.MessageKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Portrait : Interactable
{

	[SerializeField]
	private Material interactable;

	[SerializeField]
	private Material nonInteractable;

	[SerializeField]
	private int portraitIndex;

	private MeshRenderer mesh;

	[SerializeField]
	private JournalEntryId journalId;

	[SerializeField]
	private JournalData journalEntry;

	private Quaternion startRotation;

	[SerializeField]
	private Transform endRotation;

	private bool isRotated;

	public override void Start()
	{
		base.Start();
		MessageKit.addObserver(47, Reset);
		startRotation = transform.rotation;
		mesh = GetComponent<MeshRenderer>();
	}

	public override void Clicked(bool isClickedByOtherPlayer)
	{
		base.Clicked(isClickedByOtherPlayer);
		if (!UserGameSettings.IsMultiplayer() && !journalEntry.IsUnlocked())
		{
			MessageKit<string>.post(3, journalEntry.GetShortText());
			journalEntry.Unlock();
		}
		mesh.material = nonInteractable;
		gameObject.layer = LayerMask.NameToLayer("Default");
		MessageKit<int>.post(46, portraitIndex);
		StartCoroutine(RotateOrReset());
		InteractionEnd();
	}

	private void Reset()
	{
		mesh.material = interactable;
		gameObject.layer = LayerMask.NameToLayer("Interactable");
		StartCoroutine(RotateOrReset());
	}

	private IEnumerator RotateOrReset()
	{
		Transform portraitTransform = transform;
		float t = Time.time;
		Quaternion from = isRotated ? endRotation.rotation : startRotation;
		Quaternion to = isRotated ? startRotation : endRotation.rotation;
		isRotated = !isRotated;
		while (Time.time < t + 0.5f)
		{
			portraitTransform.rotation = Quaternion.Slerp(from, to, (Time.time - t) * 2f);
			yield return null;
		}
	}
}
