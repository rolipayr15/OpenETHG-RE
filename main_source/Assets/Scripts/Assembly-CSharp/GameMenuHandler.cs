using System;
using AppleAuth.Interfaces;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class GameMenuHandler
{
	public GameObject Parent;

	public Text AppleUserIdLabel;

	public Text AppleUserCredentialLabel;

	public void SetVisible(bool visible)
	{
	}

	public void SetupAppleData(string appleUserId, ICredential receivedCredential)
	{
	}
}
