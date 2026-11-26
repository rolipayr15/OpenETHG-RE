using Prime31.MessageKit;
using UnityEngine;
using UnityEngine.UI;

public class OptionsDynamicJoystick : MonoBehaviour
{
	[SerializeField]
	private Toggle dynamicJoystickToggle;

	private void Start()
	{
		dynamicJoystickToggle.SetIsOnWithoutNotify(UserGameSettings.dynamicJoystickEnabled);
	}

	public void OnDynamicJoystickSettingChange(bool val)
	{
		SaveSetting(val);
	}

	public static void SaveSetting(bool val)
	{
		UserGameSettings.dynamicJoystickEnabled = val;
		PlayerPrefs.Save();
		MessageKit.post(38);
	}
}
