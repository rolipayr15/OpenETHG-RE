using UnityEngine;

public class HideWhenAdsDisabledViaRemoteConfig : MonoBehaviour
{
	private void Awake()
	{
		if (UserGameSettings.adsDisabledViaRemoteConfig)
		{
			gameObject.SetActive(false);
		}
	}
}
