using Bibloteka;
using I2.Loc;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class BestTime : MonoBehaviour
{
	private StringBuilder sb;

	[SerializeField]
	private Text currentTime;

	[SerializeField]
	private Text bestTimeLabel;

	private float bestTime;

	private void Start()
	{
		sb = new StringBuilder();
		sb.Append(ScriptLocalization.YouEscapedIn);
		sb.Append(" ");
		sb.Append(Utils.FormatTime(UserGameSettings.lastGameDuration));
		currentTime.text = sb.ToString();
		string map = null;
		switch ((int)UserGameSettings.currentLevel)
		{
		case 0:
			map = "MANSION";
			break;
		case 1:
			map = "HOSPITAL";
			break;
		case 3:
			map = "SCHOOL";
			break;
		}
		string difficulty = null;
		switch ((int)UserGameSettings.currentDifficulty)
		{
		case 0:
			difficulty = "CASUAL";
			break;
		case 1:
			difficulty = "EASY";
			break;
		case 2:
			difficulty = "NORMAL";
			break;
		case 3:
			difficulty = "HARD";
			break;
		case 4:
			difficulty = "NIGHTMARE";
			break;
		}
		string key = map != null && difficulty != null ? map + "_" + difficulty + "_TIME_SANDBOX" : string.Empty;
		bestTime = PlayerPrefs.GetFloat(key, 9999f);
		if (UserGameSettings.lastGameDuration < bestTime)
		{
			PlayerPrefs.SetFloat(key, UserGameSettings.lastGameDuration);
			bestTime = UserGameSettings.lastGameDuration;
		}
		sb.Length = 0;
		sb.Append(ScriptLocalization.YouEscapedInBestTime);
		sb.Append(" ");
		sb.Append(Utils.FormatTime(bestTime));
		bestTimeLabel.text = sb.ToString();
	}
}
