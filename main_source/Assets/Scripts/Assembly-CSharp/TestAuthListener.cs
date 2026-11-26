using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TestAuthListener : MonoBehaviour, IAuthListener
{
	[CompilerGenerated]
	private Action<string> m_OnPlayerSignedIn;

	[CompilerGenerated]
	private Action<string> m_OnPlayerSignedOut;

	[CompilerGenerated]
	private Action<string> m_OnPlayerSessionExpired;

	private string _currentPlayerId;

	private bool _isPlayerAuthenticated;

	public string CurrentPlayerId => null;

	public bool IsPlayerAuthenticated => false;

	public bool ExternalProviderLoggedIn => false;

	public event Action<string> OnPlayerSignedIn
	{
		[CompilerGenerated]
		add
		{
		}
		[CompilerGenerated]
		remove
		{
		}
	}

	public event Action<string> OnPlayerSignedOut
	{
		[CompilerGenerated]
		add
		{
		}
		[CompilerGenerated]
		remove
		{
		}
	}

	public event Action<string> OnPlayerSessionExpired
	{
		[CompilerGenerated]
		add
		{
		}
		[CompilerGenerated]
		remove
		{
		}
	}

	private void Awake()
	{
	}

	private void Update()
	{
	}

	private void SwitchToUser(string userId)
	{
	}

	private void SignOut()
	{
	}

	private void OnGUI()
	{
	}
}
