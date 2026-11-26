using System.Collections.Generic;
using UnityEngine;

public class Share : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> gameObjectsToHide;

	[SerializeField]
	private List<GameObject> gameObjectsToShow;

	private List<GameObject> gameObjectsToActivate;

	[SerializeField]
	private RectTransform scorePanel;

	private Texture2D screenshot;

	private bool screenshotAlreadyTaken;

	public void ShareResult()
	{
	}

	private void TakeScreenshot()
	{
	}

	private void ShowShareWindow()
	{
	}

	private bool IsStoragePermissionGranted()
	{
		return false;
	}

	private void AskStoragePermission()
	{
	}
}
