public static class PreLevelInterstitialPolicy
{
	public static bool ShouldSkip(int config, int playCounter)
	{
		return config >= 0 && (config == 0 || playCounter < config);
	}

	public static string Variant(int config)
	{
		if (config < 0)
		{
			return "always";
		}
		return config == 0 ? "never" : "after_" + config;
	}
}
