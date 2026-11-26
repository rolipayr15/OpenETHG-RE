using System.Collections.Generic;
using UnityEngine;

public static class PixelModeCameraRegistry
{
	private static readonly List<Camera> registeredCameras = new List<Camera>();

	private static RenderTexture activeTexture;

	public static void SetActiveTexture(RenderTexture texture)
	{
		activeTexture = texture;
		foreach (Camera cam in registeredCameras)
		{
			if (cam != null)
			{
				cam.targetTexture = activeTexture;
			}
		}
	}

	public static void Register(Camera cam)
	{
		if (!registeredCameras.Contains(cam))
		{
			registeredCameras.Add(cam);
		}
		if (activeTexture != null && cam != null)
		{
			cam.targetTexture = activeTexture;
		}
	}

	public static void Unregister(Camera cam)
	{
		registeredCameras.Remove(cam);
		if (cam != null && cam.targetTexture == activeTexture)
		{
			cam.targetTexture = null;
		}
	}
}
