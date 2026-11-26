using Prime31.MessageKit;
using UnityEngine;

[CreateAssetMenu(fileName = "New JournalEnemy", menuName = "Journal Enemy", order = 51)]
public class JournalEnemy : ScriptableObject
{
	[SerializeField]
	private Enemy enemyId;

	[SerializeField]
	private JournalData[] dialogues;

	private int numberOfGhostVisionsUsed;

	private int lastGhostVisionLines;

	private JournalData currentLines;

	public void Init()
	{
		numberOfGhostVisionsUsed = 0;
		lastGhostVisionLines = 0;
		if (!UserGameSettings.IsMultiplayer())
		{
			MessageKit<bool>.addObserver(7, CheckIfMonsterShouldSpeak);
		}
	}

	private void CheckIfMonsterShouldSpeak(bool isMonsterVisionActive)
	{
		if (!isMonsterVisionActive || UserGameSettings.currentMode != Mode.STANDARD || UserGameSettings.currentGameMode != GameMode.SINGLE)
		{
			return;
		}
		numberOfGhostVisionsUsed++;
		// Every other ghost vision the monster says its next line; the player answers when it ends.
		if (numberOfGhostVisionsUsed % 2 == 1)
		{
			currentLines = dialogues[lastGhostVisionLines];
			lastGhostVisionLines++;
			if (!currentLines.IsUnlocked())
			{
				MessageKit<string>.post(45, currentLines.GetShortText());
				MessageKit<bool>.addObserver(7, ShowPlayerLine);
			}
		}
		if (numberOfGhostVisionsUsed == 12)
		{
			MessageKit<bool>.removeObserver(7, CheckIfMonsterShouldSpeak);
		}
	}

	private void ShowPlayerLine(bool isActive)
	{
		if (!isActive)
		{
			MessageKit<string>.post(3, currentLines.GetLongText());
			currentLines.Unlock();
		}
		MessageKit<bool>.removeObserver(7, ShowPlayerLine);
	}

	public JournalData[] GetJournalDatas()
	{
		return dialogues;
	}

	public Enemy GetEnemy()
	{
		return enemyId;
	}
}
