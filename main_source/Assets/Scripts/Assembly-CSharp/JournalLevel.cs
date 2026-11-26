using Prime31.MessageKit;
using UnityEngine;

[CreateAssetMenu(fileName = "New JournalLevel", menuName = "Journal Level", order = 51)]
public class JournalLevel : ScriptableObject
{
	[SerializeField]
	private Level levelId;

	[SerializeField]
	private JournalData[] items;

	private int bagsFound;

	private int playerLinesSpoken;

	public void Init()
	{
		bagsFound = 0;
		playerLinesSpoken = 1;
		MessageKit<string>.addObserver(19, BagCollected);
	}

	private void BagCollected(string bagId)
	{
		bagsFound++;
		// Every second bag the player says the next journal line.
		if (bagsFound % 2 == 0)
		{
			JournalData line = items[playerLinesSpoken];
			if (!line.IsUnlocked())
			{
				MessageKit<string>.post(3, line.GetShortText());
				line.Unlock();
			}
			playerLinesSpoken++;
		}
		if (bagsFound == 20)
		{
			MessageKit<string>.removeObserver(19, BagCollected);
		}
	}

	public JournalData[] GetJournalDatas()
	{
		return items;
	}

	public Level GetLevel()
	{
		return levelId;
	}
}
