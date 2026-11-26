using AppleAuth;
using AppleAuth.Interfaces;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
	private const string AppleUserIdKey = "AppleUserId";

	private IAppleAuthManager _appleAuthManager;

	public LoginMenuHandler LoginMenu;

	public GameMenuHandler GameMenu;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void SignInWithAppleButtonPressed()
	{
	}

	private void InitializeLoginMenu()
	{
	}

	private void SetupLoginMenuForUnsupportedPlatform()
	{
	}

	private void SetupLoginMenuForSignInWithApple()
	{
	}

	private void SetupLoginMenuForCheckingCredentials()
	{
	}

	private void SetupLoginMenuForQuickLoginAttempt()
	{
	}

	private void SetupLoginMenuForAppleSignIn()
	{
	}

	private void SetupGameMenu(string appleUserId, ICredential credential)
	{
	}

	private void CheckCredentialStatusForUserId(string appleUserId)
	{
	}

	private void AttemptQuickLogin()
	{
	}

	private void SignInWithApple()
	{
	}
}
