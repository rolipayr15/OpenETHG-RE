using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class CustomToast : MonoBehaviour
{

	private static string messageString;

	private static float time = 3f;

	[SerializeField]
	private Text messageLabel;

	private DateTime startOfDisplaying;

	public static void Show(string message, float t)
	{
		messageString = message;
		time = t;
		SceneManager.LoadSceneAsync("CustomToast", LoadSceneMode.Additive);
	}

	private void Start()
	{
		messageLabel.text = messageString;
		StartCoroutine(CloseAfterChosenTime(time));
	}

	private static IEnumerator CloseAfterChosenTime(float t)
	{
		yield return new WaitForSecondsRealtime(t);
		SceneManager.UnloadSceneAsync("CustomToast", UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
	}
}
