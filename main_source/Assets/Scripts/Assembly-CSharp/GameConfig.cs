using System.Collections.Generic;

internal class GameConfig
{
	public static Dictionary<Platform, Dictionary<Level, Dictionary<Difficulty, int[]>>> bagsToActivateConfig = new Dictionary<Platform, Dictionary<Level, Dictionary<Difficulty, int[]>>>
	{
		{ Platform.Singleplayer, new Dictionary<Level, Dictionary<Difficulty, int[]>>
		{
			{ Level.Mansion, new Dictionary<Difficulty, int[]>
			{
				{ Difficulty.Casual, new int[] { 12, 6, 12 } },
				{ Difficulty.Easy, new int[] { 12, 6, 12 } },
				{ Difficulty.Normal, new int[] { 12, 6, 12 } },
				{ Difficulty.Hard, new int[] { 12, 6, 12 } },
				{ Difficulty.Nightmare, new int[] { 12, 6, 12 } },
			} },
			{ Level.Hospital, new Dictionary<Difficulty, int[]>
			{
				{ Difficulty.Casual, new int[] { 8, 6, 8 } },
				{ Difficulty.Easy, new int[] { 7, 5, 7 } },
				{ Difficulty.Normal, new int[] { 10, 6, 9 } },
				{ Difficulty.Hard, new int[] { 12, 6, 12 } },
				{ Difficulty.Nightmare, new int[] { 12, 6, 12 } },
			} },
			{ Level.School, new Dictionary<Difficulty, int[]>
			{
				{ Difficulty.Casual, new int[] { 9, 12, 9 } },
				{ Difficulty.Easy, new int[] { 9, 12, 9 } },
				{ Difficulty.Normal, new int[] { 9, 12, 9 } },
				{ Difficulty.Hard, new int[] { 9, 12, 9 } },
				{ Difficulty.Nightmare, new int[] { 9, 12, 9 } },
			} },
		} },
		{ Platform.Multiplayer, new Dictionary<Level, Dictionary<Difficulty, int[]>>
		{
			{ Level.Mansion, new Dictionary<Difficulty, int[]>
			{
				{ Difficulty.Casual, new int[] { 12, 6, 12 } },
				{ Difficulty.Easy, new int[] { 12, 6, 12 } },
				{ Difficulty.Normal, new int[] { 16, 8, 16 } },
				{ Difficulty.Hard, new int[] { 16, 8, 16 } },
				{ Difficulty.Nightmare, new int[] { 16, 8, 16 } },
			} },
			{ Level.Hospital, new Dictionary<Difficulty, int[]>
			{
				{ Difficulty.Casual, new int[] { 8, 6, 8 } },
				{ Difficulty.Easy, new int[] { 7, 5, 7 } },
				{ Difficulty.Normal, new int[] { 16, 8, 16 } },
				{ Difficulty.Hard, new int[] { 16, 8, 16 } },
				{ Difficulty.Nightmare, new int[] { 16, 8, 16 } },
			} },
			{ Level.School, new Dictionary<Difficulty, int[]>
			{
				{ Difficulty.Casual, new int[] { 9, 12, 9 } },
				{ Difficulty.Easy, new int[] { 9, 12, 9 } },
				{ Difficulty.Normal, new int[] { 16, 12, 16 } },
				{ Difficulty.Hard, new int[] { 16, 12, 16 } },
				{ Difficulty.Nightmare, new int[] { 16, 12, 16 } },
			} },
		} },
	};

	public static Dictionary<Level, Dictionary<Difficulty, string>> bestScorePlayerPrefsKey = new Dictionary<Level, Dictionary<Difficulty, string>>
	{
		{ Level.Mansion, new Dictionary<Difficulty, string>
		{
			{ Difficulty.Casual, "MANSION_CASUAL_TIME" },
			{ Difficulty.Easy, "MANSION_EASY_TIME" },
			{ Difficulty.Normal, "MANSION_NORMAL_TIME" },
			{ Difficulty.Hard, "MANSION_HARD_TIME" },
			{ Difficulty.Nightmare, "MANSION_NIGHTMARE_TIME" },
		} },
		{ Level.Hospital, new Dictionary<Difficulty, string>
		{
			{ Difficulty.Casual, "HOSPITAL_CASUAL_TIME" },
			{ Difficulty.Easy, "HOSPITAL_EASY_TIME" },
			{ Difficulty.Normal, "HOSPITAL_NORMAL_TIME" },
			{ Difficulty.Hard, "HOSPITAL_HARD_TIME" },
			{ Difficulty.Nightmare, "HOSPITAL_NIGHTMARE_TIME" },
		} },
		{ Level.School, new Dictionary<Difficulty, string>
		{
			{ Difficulty.Casual, "SCHOOL_CASUAL_TIME" },
			{ Difficulty.Easy, "SCHOOL_EASY_TIME" },
			{ Difficulty.Normal, "SCHOOL_NORMAL_TIME" },
			{ Difficulty.Hard, "SCHOOL_HARD_TIME" },
			{ Difficulty.Nightmare, "SCHOOL_NIGHTMARE_TIME" },
		} },
	};

	public static Dictionary<Difficulty, int> bagsToCollect = new Dictionary<Difficulty, int>
	{
		{ Difficulty.Casual, 6 },
		{ Difficulty.Easy, 12 },
		{ Difficulty.Normal, 20 },
		{ Difficulty.Hard, 30 },
		{ Difficulty.Nightmare, 30 },
	};

	public static Dictionary<Enemy, Dictionary<Difficulty, float[]>> enemySpeed = new Dictionary<Enemy, Dictionary<Difficulty, float[]>>
	{
		{ Enemy.Krasue, new Dictionary<Difficulty, float[]>
		{
			{ Difficulty.Casual, new float[] { 0.5f, 1.0f } },
			{ Difficulty.Easy, new float[] { 1.0f, 1.7f } },
			{ Difficulty.Normal, new float[] { 1.5f, 2.5f } },
			{ Difficulty.Hard, new float[] { 3.0f, 4.5f } },
			{ Difficulty.Nightmare, new float[] { 3.5f, 5.5f } },
		} },
		{ Enemy.Charlie, new Dictionary<Difficulty, float[]>
		{
			{ Difficulty.Casual, new float[] { 0.6f, 1.1f } },
			{ Difficulty.Easy, new float[] { 1.2f, 1.9f } },
			{ Difficulty.Normal, new float[] { 1.7f, 2.7f } },
			{ Difficulty.Hard, new float[] { 3.2f, 4.7f } },
			{ Difficulty.Nightmare, new float[] { 4.2f, 6.2f } },
		} },
		{ Enemy.Dog, new Dictionary<Difficulty, float[]>
		{
			{ Difficulty.Casual, new float[] { 0.5f, 1.0f } },
			{ Difficulty.Easy, new float[] { 1.0f, 1.7f } },
			{ Difficulty.Normal, new float[] { 1.5f, 2.5f } },
			{ Difficulty.Hard, new float[] { 3.0f, 4.5f } },
			{ Difficulty.Nightmare, new float[] { 3.5f, 5.5f } },
		} },
		{ Enemy.Sandbox, new Dictionary<Difficulty, float[]>
		{
			{ Difficulty.Casual, new float[] { 0.5f, 1.0f } },
			{ Difficulty.Easy, new float[] { 1.0f, 1.7f } },
			{ Difficulty.Normal, new float[] { 1.5f, 2.5f } },
			{ Difficulty.Hard, new float[] { 3.0f, 4.5f } },
			{ Difficulty.Nightmare, new float[] { 3.5f, 5.5f } },
		} },
		{ Enemy.Custom, new Dictionary<Difficulty, float[]>
		{
			{ Difficulty.Casual, new float[] { 0.5f, 1.0f } },
			{ Difficulty.Easy, new float[] { 1.0f, 1.7f } },
			{ Difficulty.Normal, new float[] { 1.5f, 2.5f } },
			{ Difficulty.Hard, new float[] { 3.0f, 4.5f } },
			{ Difficulty.Nightmare, new float[] { 3.5f, 5.5f } },
		} },
		{ Enemy.Double, new Dictionary<Difficulty, float[]>
		{
			{ Difficulty.Casual, new float[] { 0.5f, 1.0f } },
			{ Difficulty.Easy, new float[] { 1.0f, 1.7f } },
			{ Difficulty.Normal, new float[] { 1.5f, 2.5f } },
			{ Difficulty.Hard, new float[] { 3.0f, 4.5f } },
			{ Difficulty.Nightmare, new float[] { 3.5f, 5.5f } },
		} },
		{ Enemy.Witch, new Dictionary<Difficulty, float[]>
		{
			{ Difficulty.Casual, new float[] { 0.5f, 1.0f } },
			{ Difficulty.Easy, new float[] { 1.0f, 1.7f } },
			{ Difficulty.Normal, new float[] { 1.5f, 2.5f } },
			{ Difficulty.Hard, new float[] { 3.0f, 4.5f } },
			{ Difficulty.Nightmare, new float[] { 3.5f, 5.5f } },
		} },
		{ Enemy.Goose, new Dictionary<Difficulty, float[]>
		{
			{ Difficulty.Casual, new float[] { 0.5f, 1.0f } },
			{ Difficulty.Easy, new float[] { 1.0f, 1.7f } },
			{ Difficulty.Normal, new float[] { 1.5f, 2.5f } },
			{ Difficulty.Hard, new float[] { 3.0f, 4.5f } },
			{ Difficulty.Nightmare, new float[] { 3.5f, 5.5f } },
		} },
	};

	public static string GetBestScorePlayerPrefsKey(Difficulty d, Level l)
	{
		return bestScorePlayerPrefsKey[l][d];
	}

	public static float[] GetEnemySpeed(Enemy e, Difficulty d)
	{
		return enemySpeed[e][d];
	}
}
