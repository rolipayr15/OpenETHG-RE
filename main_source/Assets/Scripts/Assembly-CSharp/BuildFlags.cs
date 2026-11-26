public static class BuildFlags
{
	public static readonly (string symbol, string label)[] AvailableFlags;

	public static bool IsTestEndpointsEnabled => false;

	public static bool IsCheatingEnabled => false;

	public static bool IsTestAuthListenerEnabled => false;

	public static bool IsTestPlayerIdLabelEnabled => false;

	private static bool IsSymbolDefined(string symbol)
	{
		return false;
	}

	public static bool IsFlagEnabled(string flagName)
	{
		return false;
	}
}
