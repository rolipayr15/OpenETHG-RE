using Bibloteka;
using DarkTonic.MasterAudio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MusicBox : Interactable
{

	[SerializeField]
	private AnimationClip openBox;

	[SerializeField]
	private AnimationClip playMusic;

	[SerializeField]
	private AnimationClip closeBox;

	private bool isPlaying;

	private Animation anim;

	private Coroutine playing;

	[SerializeField]
	private Renderer meshRenderer;

	[SerializeField]
	private Material materialAfterInteraction;

	private bool firstTime = true;

	public override void Start()
	{
		base.Start();
		anim = GetComponent<Animation>();
	}

	public override void Clicked(bool isClickedByOtherPlayer)
	{
		if (!isClickedByOtherPlayer)
		{
			CustomSingleton<MultiplayerSettings>.Instance.SendInteraction(transform.position.sqrMagnitude, MultiplayerProtocol.InteractableIdCodes.GetInteractableId(GetType().Name));
		}
		if (firstTime)
		{
			firstTime = false;
			meshRenderer.material = materialAfterInteraction;
		}
		if (!isPlaying)
		{
			playing = StartCoroutine(PlayMusic());
			return;
		}
		StopCoroutine(playing);
		StopMusic();
		InteractionEnd();
	}

	private IEnumerator PlayMusic()
	{
		isPlaying = true;
		anim.clip = openBox;
		anim[openBox.name].speed = 1f;
		anim.Play();
		yield return Utils.GetDelay(1f);
		MasterAudio.PlaySound3DAtTransform("fx", transform, 1f, null, 0f, "musicBox");
		anim.clip = playMusic;
		anim.Play();
		InteractionEnd();
		yield return Utils.GetDelay(10f);
		StopMusic();
	}

	private void StopMusic()
	{
		MasterAudio.StopAllSoundsOfTransform(transform);
		anim.clip = closeBox;
		anim.Play();
		isPlaying = false;
	}
}
