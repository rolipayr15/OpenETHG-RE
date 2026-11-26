using EyesAnalytics;
using Reflex.Attributes;
using UnityEngine;

public class EyesAnalytics_MenuEntered : MonoBehaviour
{
	[Inject]
	private IExperienceManager experienceManager;

	private void Start()
	{
		AnalyticsContext.SetMenu();
		PlayerProfile.PublishStored(CurrentLevel());
		App.Ready(Landing.Menu, RemoteConfigWrapper.StartupConfigSource);
		Screens.View(ScreenId.MainMenu);
		ReportBalance();
	}

	private void ReportBalance()
	{
		CoinsManager.ReportBalanceSnapshot();
	}

	private int CurrentLevel()
	{
		if (experienceManager == null)
		{
			return 0;
		}
		return experienceManager.GetCurrentLevel();
	}
}
