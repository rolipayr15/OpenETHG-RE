using Prime31.MessageKit;
using UnityEngine;
using UnityEngine.UI;
using VoxelBusters.CoreLibrary;
using VoxelBusters.EssentialKit;

public class SocialGUI : MonoBehaviour
{
	[SerializeField]
	private Text errorLabel;

	[SerializeField]
	private GameObject loginButton;

	[SerializeField]
	private Button buttonAchievements;

	[SerializeField]
	private Button buttonLeaderboards;

	[SerializeField]
	private Canvas socialCanvas;

	private void Start()
	{
		GameServices.OnAuthStatusChange += OnAuthStatusChange;
		MessageKit<bool>.addObserver(36, UpdateGUI);
		UpdateGUI(GameServices.IsAuthenticated);
	}

	private void OnAuthStatusChange(GameServicesAuthStatusChangeResult result, Error _error)
	{
		if (_error != null)
		{
			Debug.Log("Authentication failed: " + _error.Description);
			MessageKit<bool>.post(36, false);
			if (errorLabel != null)
			{
				errorLabel.text = _error.Description;
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
			UpdateGUI(true);
			LoadAchievementDescriptions();
		}
		else
		{
			UpdateGUI(false);
		}
	}

	public void Show()
	{
		UpdateGUI(GameServices.IsAuthenticated);
		socialCanvas.enabled = true;
	}

	public void Hide()
	{
		socialCanvas.enabled = false;
	}

	private void UpdateGUI(bool isConnected)
	{
		if (buttonAchievements != null)
		{
			buttonAchievements.interactable = isConnected;
		}
		if (buttonLeaderboards != null)
		{
			buttonLeaderboards.interactable = isConnected;
		}
		if (loginButton != null)
		{
			loginButton.SetActive(!isConnected);
		}
	}

	public void ShowAchievements()
	{
		if (GameServices.IsAuthenticated)
		{
			GameServices.ShowAchievements((result, error) => Debug.Log("Achievements UI closed"));
		}
	}

	public void ShowLeaderboards()
	{
		if (GameServices.IsAuthenticated)
		{
			GameServices.ShowLeaderboards(LeaderboardTimeScope.AllTime, (result, error) => Debug.Log("Leaderboard UI closed"));
		}
	}

	private void OnDestroy()
	{
		MessageKit<bool>.removeObserver(36, UpdateGUI);
		GameServices.OnAuthStatusChange -= OnAuthStatusChange;
	}

	private void LoadAchievementDescriptions()
	{
		GameServices.LoadAchievementDescriptions((result, error) => Debug.Log(error == null ? "Loaded achievement descriptions." : "Failed to load achievement descriptions: " + error.Description));
		GameServices.LoadLeaderboards((result, error) => Debug.Log(error == null ? "Loaded leaderboards." : "Failed to load leaderboards: " + error.Description));
	}
}
