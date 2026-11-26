using Bibloteka;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ScreenFader : CustomSingleton<ScreenFader>
{



	public bool fadeIn = true;

	public float fadeTime = 2f;

	public Color fadeColor = new Color(0f, 0f, 0f, 1f);

	public Material fadeMaterial;

	private bool faded;

	private bool lastFadeIn;

	private List<ScreenFadeControl> fadeControls = new List<ScreenFadeControl>();

	private string scene;

	private bool fixedDelta = true;

	private Camera playerCamera;

	private void Start()
	{
		FadeOutFast();
		GameObject camera = CustomSingleton<PlayerManager>.Instance.PlayerCamera;
		if (camera != null)
		{
			playerCamera = CustomSingleton<PlayerManager>.Instance.PlayerCamera.GetComponent<Camera>();
		}
	}

	private void SetFadersEnabled(bool value)
	{
		foreach (ScreenFadeControl control in fadeControls)
		{
			control.enabled = value;
		}
	}

	public IEnumerator FadeOut()
	{
		if (faded)
		{
			yield break;
		}
		float elapsedTime = 0f;
		Color color = fadeColor;
		color.a = 0f;
		fadeMaterial.color = color;
		while (elapsedTime < fadeTime)
		{
			yield return Utils.WaitForEndOfFrame;
			elapsedTime += fixedDelta ? Time.fixedDeltaTime : Time.deltaTime;
			color.a = Mathf.Clamp01(elapsedTime / fadeTime);
			fadeMaterial.color = color;
		}
		faded = true;
	}

	public void FadeOutFast()
	{
		if (faded)
		{
			return;
		}
		faded = true;
		Color color = fadeColor;
		if (fadeMaterial != null)
		{
			color.a = 0f;
			fadeMaterial.color = color;
			color.a = 1f;
			fadeMaterial.color = color;
		}
	}

	public IEnumerator FadeIn()
	{
		if (!faded)
		{
			yield break;
		}
		float elapsedTime = 0f;
		Color color = fadeColor;
		fadeMaterial.color = color;
		while (elapsedTime < fadeTime)
		{
			yield return Utils.WaitForEndOfFrame;
			elapsedTime += fixedDelta ? Time.fixedDeltaTime : Time.deltaTime;
			color.a = 1f - Mathf.Clamp01(elapsedTime / fadeTime);
			fadeMaterial.color = color;
		}
		faded = false;
		SetFadersEnabled(false);
	}

	public void Update()
	{
		if (lastFadeIn != fadeIn)
		{
			lastFadeIn = fadeIn;
			StartCoroutine(DoFade());
		}
	}

	public IEnumerator DoFade()
	{
		foreach (Camera camera in Camera.allCameras)
		{
			ScreenFadeControl control = camera.gameObject.AddComponent<ScreenFadeControl>();
			control.fadeMaterial = fadeMaterial;
			fadeControls.Add(control);
		}
		if (fadeIn)
		{
			yield return StartCoroutine(FadeIn());
			foreach (ScreenFadeControl control in fadeControls)
			{
				Destroy(control);
			}
			fadeControls.Clear();
		}
		else
		{
			yield return StartCoroutine(FadeOut());
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}
