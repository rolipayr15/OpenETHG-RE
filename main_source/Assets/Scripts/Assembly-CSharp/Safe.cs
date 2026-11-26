using Prime31.MessageKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Safe : Interactable
{

	public bool isLocked = true;

	[SerializeField]
	private Collider safeCollider;

	[SerializeField]
	private Transform endTransform;

	private Transform startTransforn;

	[SerializeField]
	private JournalData safe;

	[SerializeField]
	private GameObject key;

	public override void Start()
	{
		base.Start();
		MessageKit.addObserver(91, SafeUnlocked);
	}

	private void SafeUnlocked()
	{
		MessageKit.removeObserver(91, SafeUnlocked);
		isLocked = false;
		safe.Unlock();
	}

	public override void Clicked(bool isClickedByOtherPlayer)
	{
		if (!isLocked)
		{
			base.Clicked(isClickedByOtherPlayer);
			startTransforn = transform;
			safeCollider.enabled = false;
			StartCoroutine(Open(isClickedByOtherPlayer));
		}
		else
		{
			MessageKit<string, bool>.post(3, I2.Loc.ScriptLocalization.SafeLocked, isClickedByOtherPlayer);
		}
		InteractionEnd();
	}

	private IEnumerator Open(bool isClickedByOther)
	{
		float t = Time.time;
		while (Time.time < t + 0.5f)
		{
			startTransforn.rotation = Quaternion.Slerp(startTransforn.rotation, endTransform.rotation, (Time.time - t) * 2f);
			yield return null;
		}
		if (key.activeInHierarchy)
		{
			Interactable interactable = key.GetComponent<Interactable>();
			if (interactable != null)
			{
				interactable.Clicked(isClickedByOther);
			}
		}
	}

	protected override void OnDestroy()
	{
		MessageKit.removeObserver(91, SafeUnlocked);
		base.OnDestroy();
	}
}
