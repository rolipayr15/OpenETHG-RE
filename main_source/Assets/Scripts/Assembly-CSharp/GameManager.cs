using DarkTonic.MasterAudio;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	[SerializeField]
	private GameObject restorePurchasesButton;

	[SerializeField]
	private Toggle menuMusicToggle;

	[SerializeField]
	private Dropdown menuMusicDropdown;

	private const string MenuMusicSound = "Menu";

	private const string LullabyVariation = "ChloesLullaby";

	private void Start()
	{
		Application.targetFrameRate = 60;
		if (!UserGameSettings.volume.HasValue)
		{
			UserGameSettings.volume = PlayerPrefs.GetFloat("musicVolume", 1f);
		}
		MasterAudio.MasterVolumeLevel = UserGameSettings.volume.Value;
		MasterAudio.PlaylistMasterVolume = UserGameSettings.volume.Value;
		bool musicEnabled = PlayerPrefz.GetBool("menuMusicEnabled", true);
		if (menuMusicToggle != null)
		{
			menuMusicToggle.SetIsOnWithoutNotify(musicEnabled);
		}
		LocalizeMenuMusicOptions();
		if (menuMusicDropdown != null)
		{
			menuMusicDropdown.SetValueWithoutNotify(GetMenuMusicTrack());
		}
		ApplyMenuMusic(musicEnabled);
	}

	public void SetMenuMusicEnabled(bool enabled)
	{
		PlayerPrefz.SetBool("menuMusicEnabled", enabled);
		PlayerPrefs.Save();
		ApplyMenuMusic(enabled);
	}

	public void SetMenuMusicTrack(int track)
	{
		PlayerPrefs.SetInt("menuMusicTrack", track == 1 ? 1 : 0);
		PlayerPrefs.Save();
		ApplyMenuMusic(PlayerPrefz.GetBool("menuMusicEnabled", true));
	}

	// Localize callback for the menu-music label: fills in a fallback when the term has no translation.
	public void LocalizeMenuMusic()
	{
		if (string.IsNullOrEmpty(Localize.MainTranslation))
		{
			Localize.MainTranslation = LocalizationManager.CurrentLanguageCode == "pl" ? "Muzyka w menu" : "Menu music";
		}
	}

	public void LocalizeMenuMusicOptions()
	{
		if (menuMusicDropdown == null || menuMusicDropdown.options.Count < 2)
		{
			return;
		}
		string current = LocalizationManager.GetTranslation("MenuMusicCurrent");
		string lullaby = LocalizationManager.GetTranslation("MenuMusicLullaby");
		if (string.IsNullOrEmpty(current))
		{
			current = LocalizationManager.CurrentLanguageCode == "pl" ? "Obecna" : "Current";
		}
		menuMusicDropdown.options[0].text = current;
		menuMusicDropdown.options[1].text = string.IsNullOrEmpty(lullaby) ? "Chloe’s Lullaby" : lullaby;
		menuMusicDropdown.RefreshShownValue();
	}

	private static int GetMenuMusicTrack()
	{
		return PlayerPrefs.GetInt("menuMusicTrack", 0) == 1 ? 1 : 0;
	}

	private static void ApplyMenuMusic(bool enabled)
	{
		MasterAudio.StopAllOfSound(MenuMusicSound);
		if (enabled)
		{
			MasterAudio.PlaySound(MenuMusicSound, 1f, null, 0f, GetMenuMusicTrack() != 0 ? LullabyVariation : MenuMusicSound);
		}
	}
}
