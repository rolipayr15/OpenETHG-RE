using DarkTonic.MasterAudio;
using UnityEngine;
using UnityEngine.UI;

public class OptionsSounds : MonoBehaviour
{
	[SerializeField]
	private Slider slider;

	private void Start()
	{
		if (!UserGameSettings.volume.HasValue)
		{
			UserGameSettings.volume = PlayerPrefs.GetFloat("musicVolume", 1f);
		}
		slider.value = UserGameSettings.volume.Value;
		PersistentAudioSettings.MixerVolume = slider.value;
		PersistentAudioSettings.MusicVolume = slider.value;
	}

	public void OnValueChanged(float value)
	{
		UserGameSettings.volume = value;
		PlayerPrefs.SetFloat("musicVolume", value);
		PersistentAudioSettings.MixerVolume = value;
		PersistentAudioSettings.MusicVolume = value;
	}
}
