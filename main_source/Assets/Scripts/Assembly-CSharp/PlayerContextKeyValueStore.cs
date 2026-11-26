using System;

public class PlayerContextKeyValueStore : IPlayerAwareKeyValueStore, IKeyValueStore
{
	private readonly IKeyValueStore baseStore;

	private readonly IPlayerContextManager playerContextManager;

	public string CurrentPlayerContext => string.IsNullOrEmpty(playerContextManager.CurrentPlayerId) ? "anonymous" : playerContextManager.CurrentPlayerId;

	public PlayerContextKeyValueStore(IKeyValueStore baseStore, IPlayerContextManager playerContextManager)
	{
		this.baseStore = baseStore ?? throw new ArgumentNullException("baseStore");
		this.playerContextManager = playerContextManager ?? throw new ArgumentNullException("playerContextManager");
	}

	private string GetPlayerKey(string key)
	{
		return CurrentPlayerContext + "_" + key;
	}

	public void SetValue(string key, string value)
	{
		baseStore.SetValue(GetPlayerKey(key), value);
	}

	public string GetString(string key, string defaultValue = "")
	{
		return baseStore.GetString(GetPlayerKey(key), defaultValue);
	}

	public void SetValue(string key, int value)
	{
		baseStore.SetValue(GetPlayerKey(key), value);
	}

	public int GetInt(string key, int defaultValue = 0)
	{
		return baseStore.GetInt(GetPlayerKey(key), defaultValue);
	}

	public void SetValue(string key, float value)
	{
		baseStore.SetValue(GetPlayerKey(key), value);
	}

	public float GetFloat(string key, float defaultValue = 0f)
	{
		return baseStore.GetFloat(GetPlayerKey(key), defaultValue);
	}

	public void SetValue(string key, bool value)
	{
		baseStore.SetValue(GetPlayerKey(key), value);
	}

	public bool GetBool(string key, bool defaultValue = false)
	{
		return baseStore.GetBool(GetPlayerKey(key), defaultValue);
	}

	public bool HasKey(string key)
	{
		return baseStore.HasKey(GetPlayerKey(key));
	}

	public void DeleteKey(string key)
	{
		baseStore.DeleteKey(GetPlayerKey(key));
	}

	public void DeleteAll()
	{
		throw new NotSupportedException("DeleteAll is not supported in PlayerContextKeyValueStore to prevent accidental deletion of other players' data. Delete specific keys instead.");
	}

	public string GetStorageKey(string key)
	{
		return GetPlayerKey(key);
	}

	public T WithPlayerContext<T>(string playerId, Func<IKeyValueStore, T> operation)
	{
		if (string.IsNullOrEmpty(playerId))
		{
			throw new ArgumentException("Player ID cannot be null or empty", "playerId");
		}
		if (operation == null)
		{
			throw new ArgumentNullException("operation");
		}
		string previous = playerContextManager.CurrentPlayerId;
		playerContextManager.SetCurrentPlayerId(playerId);
		try
		{
			return operation(this);
		}
		finally
		{
			if (string.IsNullOrEmpty(previous))
			{
				playerContextManager.Logout();
			}
			else
			{
				playerContextManager.SetCurrentPlayerId(previous);
			}
		}
	}

	public void WithPlayerContext(string playerId, Action<IKeyValueStore> operation)
	{
		if (operation == null)
		{
			throw new ArgumentNullException("operation");
		}
		WithPlayerContext<object>(playerId, delegate(IKeyValueStore store)
		{
			operation(store);
			return null;
		});
	}
}
