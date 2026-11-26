using System.Collections.Generic;

public class InMemoryKeyValueStore : IKeyValueStore
{
	private readonly Dictionary<string, string> stringStore;

	private readonly Dictionary<string, int> intStore;

	private readonly Dictionary<string, float> floatStore;

	private readonly Dictionary<string, bool> boolStore;

	public void SetValue(string key, string value)
	{
	}

	public void SetValue(string key, int value)
	{
	}

	public void SetValue(string key, float value)
	{
	}

	public void SetValue(string key, bool value)
	{
	}

	public string GetString(string key, string defaultValue = "")
	{
		return null;
	}

	public int GetInt(string key, int defaultValue = 0)
	{
		return 0;
	}

	public float GetFloat(string key, float defaultValue = 0f)
	{
		return 0f;
	}

	public bool GetBool(string key, bool defaultValue = false)
	{
		return false;
	}

	public bool HasKey(string key)
	{
		return false;
	}

	public void DeleteKey(string key)
	{
	}

	public void DeleteAll()
	{
	}
}
