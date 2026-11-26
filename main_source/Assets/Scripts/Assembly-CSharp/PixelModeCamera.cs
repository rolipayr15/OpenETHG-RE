using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PixelModeCamera : MonoBehaviour
{
	private Camera cam;

	private void Awake()
	{
		cam = GetComponent<Camera>();
	}

	private void OnEnable()
	{
		PixelModeCameraRegistry.Register(cam);
	}

	private void OnDisable()
	{
		PixelModeCameraRegistry.Unregister(cam);
	}
}
