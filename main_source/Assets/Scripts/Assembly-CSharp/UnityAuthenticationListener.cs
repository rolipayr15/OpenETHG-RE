using System;
using System.Runtime.CompilerServices;
using Unity.Services.Authentication;

public class UnityAuthenticationListener : IAuthListener, IDisposable
{
	[CompilerGenerated]
	private Action<string> m_OnPlayerSignedIn;

	[CompilerGenerated]
	private Action<string> m_OnPlayerSignedOut;

	[CompilerGenerated]
	private Action<string> m_OnPlayerSessionExpired;

	private string lastKnownPlayerId;

	private bool hasRegisteredCallbacks;

	private bool isDisposed;

	private bool externalProviderLoggedIn;

	private PlayerInfo observedPlayerInfo;

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

	private void OnUnityServicesInitialized()
	{
	}

	private void RegisterCallbacks()
	{
	}

	private void OnUnitySignedIn()
	{
	}

	private void OnUnitySignedOut()
	{
	}

	private void OnUnityExpired()
	{
	}

	private void OnPlayerInfoChanged(PlayerInfo playerInfo)
	{
	}

	private void OnObservedPlayerInfoChanged(PlayerInfo _)
	{
	}

	private void RefreshLinkedProviderState()
	{
	}

	private void ObservePlayerInfo(PlayerInfo playerInfo)
	{
	}

	private static bool HasExternalProvider(PlayerInfo playerInfo)
	{
		return false;
	}

	private static void InvokeSafely(Action<string> callback, string playerId, string eventName)
	{
	}

	private static bool IsAuthServiceInitialized()
	{
		return false;
	}

	public void Dispose()
	{
	}
}
