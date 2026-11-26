using UnityEngine;
using I2.Loc;
using System.Globalization;
using System;

public sealed class WinResultData
{
	private const string DefaultShareChallenge = "I escaped in {time}. Can you beat my time?";

	private const string DefaultLostShareChallenge = "I survived for {time}. Can you last longer?";

	private const string DefaultEndlessShareChallenge = "I collected {bags} money bags in {time}. Can you beat my score?";

	public Level Level { get; }

	public Enemy Enemy { get; }

	public Mode Mode { get; }

	public Difficulty Difficulty { get; }

	public GameMode GameMode { get; }

	public float DurationSeconds { get; }

	public int BagsCollected { get; }

	public bool IsGameOver { get; }

	public string ModeName { get; }

	public string LevelText { get; }

	public string EnemyText { get; }

	public string DifficultyName { get; }

	public string DifficultyText { get; }

	public bool ShowDifficultyStars { get; }

	public string DurationText { get; }

	public string DurationCaption { get; }

	public string Heading { get; }

	public string Title { get; }

	public string DetailsText { get; }

	public string SummaryText { get; }

	public string ShareDescription { get; }

	public string ShareText { get; }

	public static WinResultData Capture(bool isGameOver = false)
	{
		return new WinResultData(UserGameSettings.currentLevel, UserGameSettings.currentEnemy, UserGameSettings.currentMode, UserGameSettings.currentDifficulty, UserGameSettings.currentGameMode, UserGameSettings.lastGameDuration, UserGameSettings.bagsCollected, null, isGameOver);
	}

	public WinResultData(Level level, Enemy enemy, Mode mode, Difficulty difficulty, GameMode gameMode, float durationSeconds, int bagsCollected, Func<string, string> translate = null, bool isGameOver = false)
	{
		Level = level;
		Enemy = enemy;
		Mode = mode;
		Difficulty = difficulty;
		GameMode = gameMode;
		DurationSeconds = durationSeconds;
		BagsCollected = bagsCollected;
		IsGameOver = isGameOver;
		Func<string, string> lookup = translate ?? GetTranslation;
		LevelText = GetLevelName(level, lookup);
		EnemyText = GetEnemyName(enemy, lookup) ?? string.Empty;
		ModeName = GetModeName(mode, lookup);
		string difficultyName = null;
		if (HasSelectableDifficulty(mode))
		{
			ShowDifficultyStars = true;
			difficultyName = GetDifficultyName(difficulty, lookup);
		}
		DifficultyName = difficultyName;
		DifficultyText = difficultyName ?? string.Empty;
		bool endless = (int)mode == 1;
		Heading = endless ? bagsCollected.ToString(CultureInfo.InvariantCulture) : (isGameOver ? "LOST" : "SURVIVED");
		DurationCaption = endless ? "money bags collected in" : (isGameOver ? "survived for" : "escaped in:");
		DurationText = FormatDuration(durationSeconds);
		DetailsText = ModeName ?? string.Empty;
		SummaryText = DetailsText;
		string fallback = endless ? DefaultEndlessShareChallenge : (isGameOver ? DefaultLostShareChallenge : DefaultShareChallenge);
		string term = endless ? "WinShareEndlessChallenge" : (isGameOver ? "WinShareLostChallenge" : "WinShareChallenge");
		string challenge = Localize(lookup, term, fallback);
		if (!challenge.Contains("{time}") || (endless && !challenge.Contains("{bags}")))
		{
			challenge = fallback;
		}
		Title = challenge.Replace("{time}", DurationText).Replace("{bags}", bagsCollected.ToString(CultureInfo.InvariantCulture));
		ShareDescription = UrlStrings.GetGameStoreUrl(Application.platform);
		ShareText = Title + "\n\n" + ShareDescription;
	}

	public static bool HasSelectableDifficulty(Mode mode)
	{
		int value = (int)mode;
		return value >= 0 && value < 4 && value != 1;
	}

	private static string GetModeName(Mode mode, Func<string, string> lookup)
	{
		switch ((int)mode)
		{
		case 0:
			return null;
		case 1:
			return Localize(lookup, "Endless", "Endless");
		case 2:
			return Localize(lookup, "trickOrTreat", "Trick or Treat");
		case 3:
			return Localize(lookup, "PixelMode", "Pixel");
		default:
			return mode.ToString();
		}
	}

	private static string GetLevelName(Level level, Func<string, string> lookup)
	{
		switch ((int)level)
		{
		case 0:
			return Localize(lookup, "Mansion", "Mansion");
		case 1:
			return Localize(lookup, "Hospital", "Hospital");
		case 3:
			return Localize(lookup, "SchoolLevelName", "School");
		default:
			return level.ToString();
		}
	}

	private static string GetDifficultyName(Difficulty difficulty, Func<string, string> lookup)
	{
		switch ((int)difficulty)
		{
		case 0:
			return Localize(lookup, "DifficultyCasual", "Newbie");
		case 1:
			return Localize(lookup, "DifficultyEasy", "Easy");
		case 2:
			return Localize(lookup, "DifficultyNormal", "Normal");
		case 3:
			return Localize(lookup, "DifficultyHard", "Hard");
		case 4:
			return Localize(lookup, "DifficultyNightmare", "Nightmare");
		default:
			return difficulty.ToString();
		}
	}

	private static string GetEnemyName(Enemy enemy, Func<string, string> lookup)
	{
		switch ((int)enemy)
		{
		case 0:
			return Localize(lookup, "Krasue", "Krasue");
		case 1:
			return Localize(lookup, "Charlie", "Charlie");
		case 2:
			return Localize(lookup, "GoodBoy", "Good Boy");
		case 3:
			return Localize(lookup, "KrasueYourself", "Your own ghost!");
		case 4:
			return null;
		case 5:
			return Localize(lookup, "Krasue", "Krasue") + " + " + Localize(lookup, "Charlie", "Charlie");
		case 6:
			return Localize(lookup, "witch", "Ursula");
		case 7:
			return Localize(lookup, "Goose", "Geese");
		default:
			return enemy.ToString();
		}
	}

	private static string FormatDuration(float durationSeconds)
	{
		if (float.IsNaN(durationSeconds) || durationSeconds < 0f || float.IsInfinity(durationSeconds))
		{
			return "00:00";
		}
		double seconds = (int)durationSeconds;
		CultureInfo culture = CultureInfo.InvariantCulture;
		return ((long)(seconds / 60.0)).ToString("00", culture) + ":" + (seconds % 60.0).ToString("00", culture);
	}

	private static string GetTranslation(string term)
	{
		return LocalizationManager.GetTranslation(term, false);
	}

	private static string Localize(Func<string, string> lookup, string term, string fallback)
	{
		string value = lookup(term);
		if (!string.IsNullOrWhiteSpace(value) && value != term)
		{
			return value.Trim();
		}
		return fallback;
	}
}
