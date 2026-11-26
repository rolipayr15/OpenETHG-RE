using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PikselozaFixSettings : MonoBehaviour
{

	[SerializeField]
	private Transform renderPlane;

	private Camera mainCamera;

	private RenderTexture renderTexture;

	private Camera pixelCamera;

	private int renderTexSize = 80;

	private float baseWidth = 1920f;

	private float baseHeight = 1024f;

	private void Start()
	{
		renderTexture = null;
		pixelCamera = GetComponent<Camera>();
		if ((int)UserGameSettings.currentMode == 3)
		{
			StartCoroutine(FixSettingsDelayed());
		}
		else
		{
			gameObject.SetActive(false);
		}
	}

	private IEnumerator FixSettingsDelayed()
	{
		pixelCamera.enabled = true;
		yield return null;
		yield return null;
		mainCamera = CustomSingleton<PlayerManager>.Instance.PlayerCamera.GetComponent<Camera>();
		int weaponMask = 1 << LayerMask.NameToLayer("Weapon");
		mainCamera.cullingMask &= ~weaponMask;
		pixelCamera.cullingMask = weaponMask;
		float distance = pixelCamera.nearClipPlane + 0.01f;
		float tanHalfFov = Mathf.Tan(pixelCamera.fieldOfView * Mathf.Deg2Rad * 0.5f);
		float scale = Mathf.Min(Screen.width / baseWidth, Screen.height / baseHeight);
		float height = distance * tanHalfFov * 2f;
		renderPlane.localScale = new Vector3(height * pixelCamera.aspect / scale, height / scale, 1f / scale);
		renderPlane.position = pixelCamera.transform.position + pixelCamera.transform.forward * distance;
		int textureHeight = Mathf.RoundToInt(renderTexSize / ((float)Screen.width / Screen.height));
		renderTexture = new RenderTexture(renderTexSize, textureHeight, 16);
		renderTexture.filterMode = FilterMode.Point;
		mainCamera.targetTexture = renderTexture;
		PixelModeCameraRegistry.SetActiveTexture(renderTexture);
		renderPlane.GetComponent<Renderer>().material.mainTexture = renderTexture;
	}

	private void OnDestroy()
	{
		PixelModeCameraRegistry.SetActiveTexture(null);
		if (renderTexture != null)
		{
			renderTexture.Release();
		}
	}
}
