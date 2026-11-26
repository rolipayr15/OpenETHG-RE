public static class CharactersConfig
{
	public static float[] GetEnemySpeed(Difficulty d, Level l, Platform p, Enemy e)
	{
		return new LevelConfig(d, l, p, e).GetEnemySpeed();
	}
}
