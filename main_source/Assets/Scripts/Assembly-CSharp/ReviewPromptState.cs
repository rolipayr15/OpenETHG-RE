using System;
using System.Globalization;
using UnityEngine;

public static class ReviewPromptState
{
	public static long Now => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

	public static bool AllowsRequests
	{
		get
		{
			if (!PlayerPrefz.GetBool("SHOULD_ASK_RATE_ME", true))
			{
				return false;
			}
			return !PlayerPrefz.GetBool("REVIEW_NEVER_ASK", false);
		}
	}

	private static long LastAttempt
	{
		get
		{
			int legacy = PlayerPrefs.GetInt("LAST_TIME_RATE_ME_ASKED", 0);
			string stored = PlayerPrefs.GetString("REVIEW_LAST_ATTEMPT_UTC", "");
			if (stored.Length == 0)
			{
				return Math.Max(0L, legacy);
			}
			if (long.TryParse(stored, NumberStyles.Integer, CultureInfo.InvariantCulture, out long parsed) && parsed >= 0)
			{
				return Math.Max(legacy, parsed);
			}
			return long.MaxValue;
		}
	}

	public static bool IsEligible(string version)
	{
		return ReviewPromptPolicy.IsEligible(PlayerPrefs.GetInt("REVIEW_WIN_COUNT", 0), PlayerPrefs.GetInt("GAME_SESSION_COUNTER", 0), PlayerPrefz.GetBool("SHOULD_ASK_RATE_ME", true), PlayerPrefz.GetBool("REVIEW_NEVER_ASK", false), Now, LastAttempt, version, PlayerPrefs.GetString("REVIEW_LAST_ATTEMPT_VERSION", ""));
	}

	public static void RecordWin()
	{
		int wins = PlayerPrefs.GetInt("REVIEW_WIN_COUNT", 0);
		PlayerPrefs.SetInt("REVIEW_WIN_COUNT", (wins == int.MaxValue) ? int.MaxValue : (Math.Max(0, wins) + 1));
		PlayerPrefs.Save();
	}

	public static bool TryRecordAttempt(string version, out long timestamp)
	{
		timestamp = 0L;
		if (!IsEligible(version))
		{
			return false;
		}
		timestamp = Now;
		PlayerPrefs.SetString("REVIEW_LAST_ATTEMPT_UTC", timestamp.ToString(CultureInfo.InvariantCulture));
		PlayerPrefs.SetString("REVIEW_LAST_ATTEMPT_VERSION", version);
		PlayerPrefs.Save();
		return true;
	}

	public static bool OwnsAttempt(string version, long timestamp)
	{
		if (!AllowsRequests || PlayerPrefs.GetInt("REVIEW_WIN_COUNT", 0) < 2)
		{
			return false;
		}
		int sessions = PlayerPrefs.GetInt("GAME_SESSION_COUNTER", 0);
		if (timestamp < 1 || sessions < 2 || LastAttempt != timestamp)
		{
			return false;
		}
		return PlayerPrefs.GetString("REVIEW_LAST_ATTEMPT_VERSION", "") == version;
	}

	public static void RecordResponse(ReviewPromptResponse response)
	{
		PlayerPrefs.SetInt("REVIEW_LAST_RESPONSE", (int)response);
		if ((int)response == 2)
		{
			PlayerPrefz.SetBool("REVIEW_NEVER_ASK", true);
		}
		PlayerPrefs.Save();
	}
}
