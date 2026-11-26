using UnityEngine;
using System;
using System.Runtime.CompilerServices;

public class PlayerContextManager : IPlayerContextManager
{
	[CompilerGenerated]
	private Action<string, string> m_OnPlayerChanged;

	private string _currentPlayerId;

	private readonly object _lock = new object();

	private readonly IAuthListener _authListener;

	public string CurrentPlayerId { get { lock (_lock) { return _currentPlayerId ?? string.Empty; } } }

	public event Action<string, string> OnPlayerChanged
	{
		add { m_OnPlayerChanged += value; }
		remove { m_OnPlayerChanged -= value; }
	}

	public PlayerContextManager(IAuthListener authListener = null)
	{
		_authListener = authListener;
		if (authListener == null)
		{
			return;
		}
		authListener.OnPlayerSignedIn += OnAuthPlayerSignedIn;
		authListener.OnPlayerSignedOut += OnAuthPlayerSignedOut;
		authListener.OnPlayerSessionExpired += OnAuthPlayerSessionExpired;
		if (authListener.IsPlayerAuthenticated && !string.IsNullOrEmpty(authListener.CurrentPlayerId))
		{
			_currentPlayerId = authListener.CurrentPlayerId;
		}
	}

	public void SetCurrentPlayerId(string playerId)
	{
		SetCurrentPlayerIdInternal(playerId, true);
	}

	private void SetCurrentPlayerIdInternal(string playerId, bool warnIfAuthListenerPresent)
	{
		if (string.IsNullOrEmpty(playerId))
		{
			throw new ArgumentException("Player ID cannot be null or empty", "playerId");
		}
		string previous;
		lock (_lock)
		{
			previous = _currentPlayerId;
			if (string.Equals(previous, playerId, StringComparison.Ordinal))
			{
				return;
			}
			_currentPlayerId = playerId;
		}
		if (warnIfAuthListenerPresent && _authListener != null)
		{
			Debug.LogWarning("PlayerContextManager: SetCurrentPlayerId() called while IAuthListener is provided. Be careful with manual player ID management as authentication events may override this change. Consider using authentication methods instead of manual SetCurrentPlayerId() calls.");
		}
		m_OnPlayerChanged?.Invoke(previous, playerId);
	}

	public void Logout()
	{
		LogoutInternal(true);
	}

	private void LogoutInternal(bool warnIfAuthListenerPresent)
	{
		string previous;
		lock (_lock)
		{
			previous = _currentPlayerId;
			if (string.IsNullOrEmpty(previous))
			{
				return;
			}
			_currentPlayerId = string.Empty;
		}
		if (warnIfAuthListenerPresent && _authListener != null)
		{
			Debug.LogWarning("PlayerContextManager: Logout() called while IAuthListener is provided. Be careful with manual player management as authentication events may override this change. Consider using authentication methods instead of manual Logout() calls.");
		}
		m_OnPlayerChanged?.Invoke(previous, string.Empty);
	}

	private void OnAuthPlayerSignedIn(string playerId)
	{
		if (!string.IsNullOrEmpty(playerId))
		{
			SetCurrentPlayerIdInternal(playerId, false);
		}
	}

	private void OnAuthPlayerSignedOut(string playerId)
	{
		LogoutInternal(false);
	}

	private void OnAuthPlayerSessionExpired(string playerId)
	{
		LogoutInternal(false);
	}

	public void Dispose()
	{
		if (_authListener != null)
		{
			_authListener.OnPlayerSignedIn -= OnAuthPlayerSignedIn;
			_authListener.OnPlayerSignedOut -= OnAuthPlayerSignedOut;
			_authListener.OnPlayerSessionExpired -= OnAuthPlayerSessionExpired;
		}
	}
}
