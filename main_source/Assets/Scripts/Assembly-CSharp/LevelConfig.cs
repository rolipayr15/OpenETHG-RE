internal struct LevelConfig
{
	private Difficulty difficulty;

	private Level level;

	private Platform platform;

	private int[] bagsToActivateConfig;

	private int bagsToCollect;

	private string bestScorePlayerPrefsKey;

	private float[] enemySpeed;

	public LevelConfig(Difficulty d, Level l, Platform p, Enemy e)
	{
		difficulty = d;
		level = l;
		bagsToActivateConfig = GameConfig.bagsToActivateConfig[p][l][d];
		bagsToCollect = GameConfig.bagsToCollect[d];
		bestScorePlayerPrefsKey = GameConfig.GetBestScorePlayerPrefsKey(d, l);
		platform = p;
		enemySpeed = GameConfig.GetEnemySpeed(e, d);
	}

	public int GetBagsToCollect()
	{
		return bagsToCollect;
	}

	public string GetBestScorePlayerPrefsKey()
	{
		return bestScorePlayerPrefsKey;
	}

	public int[] GetBagsToActivate()
	{
		UserGameSettings.IsMultiplayer();
		return bagsToActivateConfig;
	}

	public float[] GetEnemySpeed()
	{
		return enemySpeed;
	}
}
