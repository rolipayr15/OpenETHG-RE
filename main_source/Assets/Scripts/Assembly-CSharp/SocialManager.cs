using GameAuthentication;
using Prime31.MessageKit;
using UnityEngine;
using UnityEngine.UI;
using VoxelBusters.CoreLibrary;
using VoxelBusters.EssentialKit;

public class SocialManager : CustomSingleton<SocialManager>
{
	[SerializeField]
	private Text error;

	private void OnEnable()
	{
		GameServices.OnAuthStatusChange += OnAuthStatusChange;
	}

	private void Start()
	{
		if (GameServices.IsAuthenticated)
		{
			LoadAchievementDescriptions();
		}
		GooglePlayGamesAuthClient.TrySignInOnStartup();
	}

	public void SignInPlayer()
	{
		if (!GameServices.IsAuthenticated)
		{
			Debug.Log("Starting authentication...");
			GooglePlayGamesAuthClient.AuthenticateShared(true);
		}
	}

	private void OnAuthStatusChange(GameServicesAuthStatusChangeResult result, Error _error)
	{
		if (_error != null)
		{
			Debug.Log("Authentication failed: " + _error.Description);
			MessageKit<bool>.post(36, false);
			if (error != null)
			{
				error.text = _error.Description;
			}
			return;
		}
		if (result == null || result.AuthStatus == LocalPlayerAuthStatus.Authenticating)
		{
			return;
		}
		Debug.Log(string.Format("Auth status: {0}", result.AuthStatus));
		if (result.AuthStatus == LocalPlayerAuthStatus.Authenticated)
		{
			Debug.Log("Signed in as " + result.LocalPlayer.DisplayName);
			Debug.Log("Player Identifier: " + result.LocalPlayer.Identifier);
			MessageKit<bool>.post(36, true);
			LoadAchievementDescriptions();
		}
		else
		{
			MessageKit<bool>.post(36, false);
		}
	}

	private void LoadAchievementDescriptions()
	{
		GameServices.LoadAchievementDescriptions((result, error) => Debug.Log(error == null ? "Loaded achievement descriptions." : "Failed to load achievement descriptions: " + error.Description));
		GameServices.LoadLeaderboards((result, error) => Debug.Log(error == null ? "Loaded leaderboards." : "Failed to load leaderboards: " + error.Description));
	}

	private void OnDisable()
	{
		GameServices.OnAuthStatusChange -= OnAuthStatusChange;
	}

	public bool IsAuthenticated()
	{
		return GameServices.IsAuthenticated;
	}

	protected override void OnDestroy()
	{
		GameServices.OnAuthStatusChange -= OnAuthStatusChange;
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}
