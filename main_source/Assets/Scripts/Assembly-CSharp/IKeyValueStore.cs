public interface IKeyValueStore
{
	void SetValue(string key, string value);

	void SetValue(string key, int value);

	void SetValue(string key, float value);

	void SetValue(string key, bool value);

	string GetString(string key, string defaultValue = "");

	int GetInt(string key, int defaultValue = 0);

	float GetFloat(string key, float defaultValue = 0f);

	bool GetBool(string key, bool defaultValue = false);

	bool HasKey(string key);

	void DeleteKey(string key);

	void DeleteAll();
}
