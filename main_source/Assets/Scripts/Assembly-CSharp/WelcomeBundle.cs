using System;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

public class WelcomeBundle : MonoBehaviour
{
	private static bool isMessageBoxActive;

	[SerializeField]
	private Text messageLabel;

	private static Action callAfterDismiss;

	private Dictionary<int, string> welcomeBundleWindowTitle;

	public static void Show(Action _callAfterDismiss = null)
	{
	}

	public void OkButtonClicked()
	{
	}

	private void Awake()
	{
	}

	private void OnDestroy()
	{
	}

	private void DismissMessageBoxRewired(InputActionEventData _not_used)
	{
	}

	private void Update()
	{
	}
}
