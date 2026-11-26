using GameAuthentication;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UnityConsent;

public class InitializeUnityServices : MonoBehaviour
{
	[Inject]
	private IAuthCoordinator authCoordinator;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void DisableUnityAnalyticsConsent()
	{
		EndUserConsent.SetConsentState(new ConsentState
		{
			AdsIntent = ConsentStatus.Denied,
			AnalyticsIntent = ConsentStatus.Denied
		});
	}

	public async void Initialize()
	{
		if (authCoordinator == null)
		{
			Debug.LogError("Authentication bootstrap skipped: IAuthCoordinator was not injected. Check ProjectInstaller registration and Unity Services / Player Accounts setup.");
			return;
		}
		AuthOperationResult result = await authCoordinator.BootstrapAsync();
		if (!result.IsSuccess)
		{
			Debug.LogWarning(string.Format("Authentication bootstrap finished with {0}: {1}", result.Code, result.Error));
		}
	}
}
