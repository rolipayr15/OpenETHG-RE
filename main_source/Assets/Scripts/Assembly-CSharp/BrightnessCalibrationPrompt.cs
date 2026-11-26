using System.Collections;
using UnityEngine;

public sealed class BrightnessCalibrationPrompt : MonoBehaviour
{
	private const float QuietSeconds = 0.5f;

	public static bool ShouldShow(bool hasSeenFlag, bool enabled)
	{
		return enabled && !hasSeenFlag;
	}

	private IEnumerator Start()
	{
		bool hasSeen = PlayerPrefs.HasKey("brightness_calibration_seen");
		if (!ShouldShow(hasSeen, RemoteConfigWrapper.GetBool("CalibratorPrompt")))
		{
			yield break;
		}
		// Wait until no other popup has been on screen for QuietSeconds, then show the calibrator.
		float quietSince = -1f;
		while (true)
		{
			bool busy = ConsentRecovery.IsConsentFormVisible
				|| MessageBox.IsRequestActive
				|| (CustomSingleton<MenuContentWindow>.HasInstance && CustomSingleton<MenuContentWindow>.Instance.IsWindowOpen)
				|| BrightnessCalibration.IsActive;
			if (busy)
			{
				quietSince = -1f;
			}
			else
			{
				float now = Time.unscaledTime;
				if (quietSince < 0f)
				{
					quietSince = now;
				}
				else if (now - quietSince >= QuietSeconds)
				{
					BrightnessCalibration.Show();
					yield break;
				}
			}
			yield return null;
		}
	}
}
