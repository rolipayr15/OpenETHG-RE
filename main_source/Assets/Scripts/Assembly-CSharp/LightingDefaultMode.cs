public static class LightingDefaultMode
{
	public static LightingMode Parse(string value)
	{
		if (!string.IsNullOrEmpty(value) && string.Equals(value, "classic", System.StringComparison.OrdinalIgnoreCase))
		{
			return (LightingMode)1;
		}
		return LightingMode.Flashlight;
	}

	public static LightingMode ResolveStartupMode(bool hasSavedMode, LightingMode savedMode, LightingMode remoteDefault)
	{
		return hasSavedMode ? savedMode : remoteDefault;
	}

	public static string Variant(LightingMode mode)
	{
		return mode.ToString().ToLowerInvariant();
	}
}
