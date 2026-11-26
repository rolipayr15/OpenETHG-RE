using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class LoginMenuHandler
{
	public GameObject Parent;

	public GameObject SignInWithAppleParent;

	public Button SignInWithAppleButton;

	public GameObject LoadingMessageParent;

	public Transform LoadingIconTransform;

	public Text LoadingMessageLabel;

	public void SetVisible(bool visible)
	{
	}

	public void SetLoadingMessage(bool visible, string message)
	{
	}

	public void SetSignInWithAppleButton(bool visible, bool enabled)
	{
	}

	public void UpdateLoadingMessage(float deltaTime)
	{
	}
}
