using System;

public interface IPlayerContextManager
{
	string CurrentPlayerId { get; }

	event Action<string, string> OnPlayerChanged;

	void SetCurrentPlayerId(string playerId);

	void Logout();
}
