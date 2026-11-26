using UnityEngine;
using UnityEngine.UI;

public class ImmersiveModeMenu : MonoBehaviour
{
	private Toggle immersiveToggle;

	private void Start()
	{
		immersiveToggle = GetComponent<Toggle>();
		if (UserGameSettings.isImmersiveMode && (UserGameSettings.unlockedImmersive || UserGameSettings.unlockedImmersiveOneTime))
		{
			immersiveToggle.SetIsOnWithoutNotify(true);
		}
		else
		{
			UserGameSettings.isImmersiveMode = false;
			immersiveToggle.SetIsOnWithoutNotify(false);
		}
	}

	public void SetImmersiveMode(bool isActive)
	{
		if (!UserGameSettings.unlockedImmersiveOneTime && !UserGameSettings.unlockedImmersive)
		{
			// Not unlocked yet: keep the toggle off and offer the purchase instead.
			immersiveToggle.SetIsOnWithoutNotify(false);
			ImmersiveModePurchase.Show(purchased =>
			{
				if (purchased)
				{
					immersiveToggle.SetIsOnWithoutNotify(true);
					UserGameSettings.isImmersiveMode = true;
				}
			});
			return;
		}
		UserGameSettings.isImmersiveMode = isActive;
	}
}
