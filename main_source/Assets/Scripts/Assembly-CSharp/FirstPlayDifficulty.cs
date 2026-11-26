using System;
public static class FirstPlayDifficulty
{
	public static Difficulty Parse(string value)
	{
		if (!string.IsNullOrEmpty(value) && !char.IsDigit(value[0]) && value[0] != '-' && Enum.TryParse<Difficulty>(value, true, out Difficulty parsed) && Enum.IsDefined(typeof(Difficulty), parsed))
		{
			return parsed;
		}
		return (Difficulty)2;
	}

	public static bool Applies(int playCounter)
	{
		return playCounter == 0;
	}

	public static string Variant(Difficulty difficulty)
	{
		return difficulty.ToString().ToLowerInvariant();
	}
}
