using System;

public interface IPlayerAwareKeyValueStore : IKeyValueStore
{
	string CurrentPlayerContext { get; }

	string GetStorageKey(string key);

	T WithPlayerContext<T>(string playerId, Func<IKeyValueStore, T> operation);

	void WithPlayerContext(string playerId, Action<IKeyValueStore> operation);
}
