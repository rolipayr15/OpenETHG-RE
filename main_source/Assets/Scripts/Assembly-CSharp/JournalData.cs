using DarkTonic.MasterAudio;
using I2.Loc;
using Prime31.MessageKit;
using UnityEngine;

[CreateAssetMenu(fileName = "New JournalData", menuName = "Journal Data", order = 51)]
public class JournalData : ScriptableObject
{
	[SerializeField]
	private JournalEntryId entryId;

	[SerializeField]
	private Sprite image;

	[SerializeField]
	private string shortContent;

	[SerializeField]
	private string longContent;

	public string GetShortText()
	{
		return LocalizationManager.GetTranslation(shortContent);
	}

	public string GetLongText()
	{
		return LocalizationManager.GetTranslation(longContent);
	}

	public string GetShortTranslationKey()
	{
		return shortContent;
	}

	public string GetLongTranslationKey()
	{
		return longContent;
	}

	public Sprite GetImage()
	{
		return image;
	}

	public JournalEntryId GetJournalId()
	{
		return entryId;
	}

	public bool IsUnlocked()
	{
		return JournalProgressStore.IsUnlocked(entryId.ToString());
	}

	public void Unlock()
	{
		JournalProgressStore.Unlock(entryId.ToString());
		MessageKit<string>.post(41, entryId.ToString());
		MasterAudio.PlaySound3DAtTransform("fx", PlayerManager.Instance.Player.transform, 0.5f, null, 0f, "paper");
	}

	public void SilentUnlock()
	{
		JournalProgressStore.Unlock(entryId.ToString());
	}
}
