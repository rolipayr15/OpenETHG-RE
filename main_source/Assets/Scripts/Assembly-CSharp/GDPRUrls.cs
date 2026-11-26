using EyesAnalytics;
using GoogleMobileAds.Ump.Api;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class GDPRUrls : MonoBehaviour
{
	[SerializeField]
	private Toggle doNotSellMyPersonalInformationToggle;

	[SerializeField]
	private Button privacySettingsButton;

	private void Start()
	{
		RefreshPrivacyOptionsButton();
		doNotSellMyPersonalInformationToggle.SetIsOnWithoutNotify(PlayerPrefz.GetBool("DO_NOT_SELL_MY_PERSONAL_INFORMATION", false));
	}

	private void RefreshPrivacyOptionsButton()
	{
		privacySettingsButton.enabled = ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;
	}

	public static void RefreshPrivacyOptionsEntryPoints()
	{
		foreach (GDPRUrls urls in FindObjectsByType<GDPRUrls>(FindObjectsInactive.Include, FindObjectsSortMode.None))
		{
			urls.RefreshPrivacyOptionsButton();
		}
	}

	public void OpenPrivacyPolicy()
	{
		Application.OpenURL(UrlStrings.PrivacyPolicy);
	}

	public void DoNotSellMyPersonalInformationToggleChanged(bool doNotSell)
	{
		PlayerPrefz.SetBool("DO_NOT_SELL_MY_PERSONAL_INFORMATION", doNotSell);
		MessageBox.Show(ScriptLocalization.GDPR_RestartToApplyChanges);
	}

	public void ShowPrivacyOptions()
	{
		FirebaseState.SetAnalyticsConsent(false);
		ConsentForm.ShowPrivacyOptionsForm(showError =>
		{
			AnalyticsDispatcher.RunOnMainThread(() =>
			{
				FirebaseState.RefreshAnalyticsConsent(showError != null);
				AdsManager adsManager = AdsManager.Instance;
				if (adsManager != null)
				{
					adsManager.OnConsentMayHaveChanged("privacy_options");
				}
			});
			if (showError != null)
			{
				Debug.LogError(ScriptLocalization.GDPR_Error + " " + showError.Message);
			}
		});
	}
}
