using System;
using System.Collections.Generic;

[Serializable]
public sealed class GameSaveData
{
	public int schemaVersion = 2;

	public List<string> unlockedItems = new List<string>();

	public List<DifficultyRecord> difficultyStars = new List<DifficultyRecord>();

	public List<string> unlockedJournalEntries = new List<string>();
}
