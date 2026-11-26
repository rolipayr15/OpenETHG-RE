using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JournalMenu : CustomSingleton<JournalMenu>
{
	private Canvas canvas;

	private List<JournalData> journalDataMansion;

	private List<JournalData> journalDataHospital;

	private List<JournalData> journalDataSchool;

	[SerializeField]
	private GameObject journalEntryTemplateMansion;

	[SerializeField]
	private GameObject journalEntryTemplateHospital;

	[SerializeField]
	private GameObject journalEntryTemplateSchool;

	[SerializeField]
	private JournalLevel[] journalLevelDatas;

	[SerializeField]
	private JournalEnemy[] journalEnemyDatas;

	[SerializeField]
	private Text mansionProgressLabel;

	[SerializeField]
	private Text hospitalProgressLabel;

	[SerializeField]
	private Text schoolProgressLabel;

	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private GameObject mansionJournal;

	[SerializeField]
	private GameObject hospitalJournal;

	[SerializeField]
	private GameObject schoolJournal;

	private void Start()
	{
		canvas = GetComponent<Canvas>();
		journalDataMansion = new List<JournalData>();
		journalDataHospital = new List<JournalData>();
		journalDataSchool = new List<JournalData>();
		foreach (JournalLevel level in journalLevelDatas)
		{
			switch (level.GetLevel())
			{
			case Level.School:
				journalDataSchool.AddRange(level.GetJournalDatas());
				break;
			case Level.Hospital:
				journalDataHospital.AddRange(level.GetJournalDatas());
				break;
			case Level.Mansion:
				journalDataMansion.AddRange(level.GetJournalDatas());
				break;
			}
		}
		foreach (JournalEnemy enemy in journalEnemyDatas)
		{
			switch (enemy.GetEnemy())
			{
			case Enemy.Dog:
				journalDataSchool.AddRange(enemy.GetJournalDatas());
				break;
			case Enemy.Charlie:
				journalDataHospital.AddRange(enemy.GetJournalDatas());
				break;
			case Enemy.Krasue:
				journalDataMansion.AddRange(enemy.GetJournalDatas());
				break;
			}
		}
		int mansionUnlocked = FillEntries(journalDataMansion, journalEntryTemplateMansion);
		int hospitalUnlocked = FillEntries(journalDataHospital, journalEntryTemplateHospital);
		int schoolUnlocked = FillEntries(journalDataSchool, journalEntryTemplateSchool);
		mansionProgressLabel.text = mansionUnlocked + "/" + journalDataMansion.Count;
		hospitalProgressLabel.text = hospitalUnlocked + "/" + journalDataHospital.Count;
		schoolProgressLabel.text = schoolUnlocked + "/" + journalDataSchool.Count;
	}

	// Clones the template once per entry next to it, then removes the template; returns the unlocked count.
	private static int FillEntries(List<JournalData> entries, GameObject template)
	{
		int unlocked = 0;
		foreach (JournalData data in entries)
		{
			GameObject entry = Instantiate(template, template.transform.parent);
			LocalizedFontLayout.CopyBaselines(template, entry);
			JournalEntryGUI gui = entry.GetComponent<JournalEntryGUI>();
			if (data.IsUnlocked())
			{
				unlocked++;
			}
			gui.SetupFromJournalData(data);
		}
		Destroy(template);
		return unlocked;
	}

	public void ShowJournal(int level)
	{
		canvas.enabled = true;
		GameObject journal;
		switch (level)
		{
		case 3:
			journal = schoolJournal;
			break;
		case 2:
			journal = hospitalJournal;
			break;
		case 1:
			journal = mansionJournal;
			break;
		default:
			return;
		}
		scrollRect.content = journal.GetComponent<RectTransform>();
		journal.SetActive(true);
	}

	public void CloseJournal()
	{
		canvas.enabled = false;
		mansionJournal.SetActive(false);
		hospitalJournal.SetActive(false);
		schoolJournal.SetActive(false);
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
