using System;
using Prime31.MessageKit;
using UnityEngine;

public class MultiplayerUpdateRequired : MonoBehaviour
{
	[SerializeField]
	private GameObject openMultiplayerButtons;

	[SerializeField]
	private GameObject updateRequiredButton;

	// The recovery build carries a suffix such as 8.3.0_RE, which System.Version cannot read.
	private static Version ParseVersion(string value)
	{
		string digits = System.Text.RegularExpressions.Regex.Match(value ?? "", "^[0-9]+([.][0-9]+)*").Value;
		return digits.Contains(".") ? new Version(digits) : new Version(0, 0);
	}

	private void Start()
	{
		MessageKit<string>.addObserver(87, OnError);
		bool compatible = ParseVersion(Application.version) >= ParseVersion(UserGameSettings.multiplayerLastCompatibleVersion);
		openMultiplayerButtons.SetActive(compatible);
		updateRequiredButton.SetActive(!compatible);
	}

	public void OkButtonClicked()
	{
		Application.OpenURL("market://details?id=com.eyesthegame.eyes");
	}

	private void OnError(string error)
	{
		if (error == "please update the game")
		{
			openMultiplayerButtons.SetActive(false);
			updateRequiredButton.SetActive(true);
		}
	}

	private void OnDestroy()
	{
		MessageKit<string>.removeObserver(87, OnError);
	}
}
