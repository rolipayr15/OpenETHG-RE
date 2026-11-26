using UnityEngine;

public class JournalManager : CustomSingleton<JournalManager>
{
	[SerializeField]
	private JournalData[] journalData;

	[SerializeField]
	private JournalLevel journalLevelMansion;

	[SerializeField]
	private JournalLevel journalLevelHospital;

	[SerializeField]
	private JournalLevel journalLevelSchool;

	[SerializeField]
	private JournalEnemy journalEnemeyKrasue;

	[SerializeField]
	private JournalEnemy journalEnemeyCharlie;

	[SerializeField]
	private JournalEnemy journalEnemyDog;

	private void Start()
	{
		if (!UserGameSettings.IsMultiplayer())
		{
			int level = (int)UserGameSettings.currentLevel;
			(level == 0 ? journalLevelMansion : (level == 1 ? journalLevelHospital : journalLevelSchool)).Init();
		}
		switch ((int)UserGameSettings.currentEnemy)
		{
		case 0:
			journalEnemeyKrasue.Init();
			break;
		case 1:
			journalEnemeyCharlie.Init();
			break;
		case 2:
			journalEnemyDog.Init();
			break;
		}
	}

	public void ShowJournal()
	{
		if (!CustomSingleton<PauseManager>.Instance.IsPaused() && !UserGameSettings.IsMultiplayer())
		{
			CustomSingleton<JournalWindow>.Instance.ShowJournal();
		}
	}

	public void CloseJournal()
	{
		CustomSingleton<JournalWindow>.Instance.CloseJournal();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}
