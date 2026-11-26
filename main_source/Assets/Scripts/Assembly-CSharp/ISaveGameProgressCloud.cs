using System;

public interface ISaveGameProgressCloud : IDisposable
{
	string Account { get; }

	bool IsAvailable { get; }

	event Action AccountChanged;

	event Action DataChanged;

	void Synchronize(Action<bool> completed);

	void Tick();

	bool TryRead(out string cloudJson, out string cachedJson, out bool cacheIsAccountOwned, out string error);

	bool TryWrite(string json, out string error);
}
