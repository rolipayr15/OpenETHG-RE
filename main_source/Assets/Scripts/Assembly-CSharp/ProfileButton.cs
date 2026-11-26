using System.Collections;
using ExternalApi.Models;
using GameAuthentication;
using Prime31.MessageKit;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

public class ProfileButton : MonoBehaviour
{
	[SerializeField]
	private Text _username;

	[Inject]
	private IExperienceManager experienceManager;

	[Inject]
	private IPlayerContextManager playerContext;

	[Inject]
	private IAuthCoordinator authCoordinator;

	[SerializeField]
	private Text playerLevel;

	[SerializeField]
	private Image levelProgressBar;

	[SerializeField]
	private GameObject loginLabel;

	private Coroutine expLevelAnimationCoroutine;

	private int currentlyDisplayedLevel;

	private void Start()
	{
		_username.text = "";
		playerLevel.text = experienceManager.GetCurrentLevel().ToString();
		MessageKit.addObserver(128, UpdateUsername);
		currentlyDisplayedLevel = experienceManager.GetCurrentLevel();
		RefreshExpAndLevelWithoutAnimating();
		RestartExpLevelAnimationCoroutine();
		RenderAuthState(authCoordinator.State);
	}

	public void ShowProfile()
	{
		if (UserGameSettings.IsSignedInWithBasicAuth || OnlineConfig.IsCustomServer || AuthAccountStatusPresenter.IsSignedInGameAccount(authCoordinator.State))
		{
			Profile.Show();
		}
		else
		{
			MyAuthentication.Show();
		}
	}

	private void OnEnable()
	{
		experienceManager.OnExpChanged += OnExpChanged;
		playerContext.OnPlayerChanged += OnPlayerChanged;
		authCoordinator.StateChanged += OnAuthStateChanged;
		OnlineConfig.ServerChanged += OnServerChanged;
		RenderAuthState(authCoordinator.State);
	}

	private void OnServerChanged()
	{
		if (!OnlineConfig.IsCustomServer)
		{
			_ = authCoordinator.BootstrapAsync();
		}
		RenderAuthState(authCoordinator.State);
	}

	private void OnAuthStateChanged(AuthState state)
	{
		if (state.Operation == AuthOperation.Idle)
		{
			RenderAuthState(state);
		}
	}

	private void RenderAuthState(AuthState state)
	{
		AuthAccountStatusView view = AuthAccountStatusPresenter.Build(state);
		Debug.Log("Game Account status " + view.Kind);
		if (OnlineConfig.IsCustomServer)
		{
			loginLabel.SetActive(false);
			_username.text = UserGameSettings.multiplayerUsername;
			RefreshExpAndLevelWithoutAnimating();
			return;
		}
		loginLabel.SetActive(!view.IsSignedInAccount);
		switch (view.Kind)
		{
		case AuthAccountStatusKind.LinkedKnownProvider:
		case AuthAccountStatusKind.LinkedUnknownProvider:
			_username.text = UserGameSettings.multiplayerUsername;
			break;
		case AuthAccountStatusKind.SessionExpired:
			_username.text = state.AccountType == GameAccountType.Guest ? "Guest" : "Session expired";
			break;
		case AuthAccountStatusKind.Guest:
			_username.text = "Guest";
			break;
		default:
			_username.text = string.Empty;
			break;
		}
		RefreshExpAndLevelWithoutAnimating();
	}

	private void OnPlayerChanged(string prevPlayer, string currentPlayer)
	{
		RefreshExpAndLevelWithoutAnimating();
		UpdateUsername();
	}

	private void OnDisable()
	{
		OnlineConfig.ServerChanged -= OnServerChanged;
		if (experienceManager != null)
		{
			experienceManager.OnExpChanged -= OnExpChanged;
		}
		if (playerContext != null)
		{
			playerContext.OnPlayerChanged -= OnPlayerChanged;
		}
		if (authCoordinator != null)
		{
			authCoordinator.StateChanged -= OnAuthStateChanged;
		}
	}

	private void OnExpChanged(GrantExpData data, bool isOverwrite)
	{
		if (isOverwrite)
		{
			RefreshExpAndLevelWithoutAnimating();
			RestartExpLevelAnimationCoroutine();
		}
	}

	private void RestartExpLevelAnimationCoroutine()
	{
		if (expLevelAnimationCoroutine != null)
		{
			StopCoroutine(expLevelAnimationCoroutine);
		}
		expLevelAnimationCoroutine = StartCoroutine(AnimateExpAndLevel());
	}

	// Runs for the button's lifetime: rolls the bar through gained levels, then eases it to the current exp.
	private IEnumerator AnimateExpAndLevel()
	{
		while (true)
		{
			while (currentlyDisplayedLevel < experienceManager.GetCurrentLevel())
			{
				int levelsBehind = experienceManager.GetCurrentLevel() - currentlyDisplayedLevel;
				float levelSpeedMultiplier = levelsBehind > 2 ? Mathf.Min(levelsBehind, 100f) : 3f;
				while (levelProgressBar.fillAmount < 0.99f)
				{
					levelProgressBar.fillAmount = Mathf.MoveTowards(levelProgressBar.fillAmount, 1f, Time.deltaTime * levelSpeedMultiplier);
					yield return null;
				}
				levelProgressBar.fillAmount = 0f;
				currentlyDisplayedLevel++;
				playerLevel.text = currentlyDisplayedLevel.ToString();
			}
			float correctFillAmount = CalculateFillAmountForExpBar();
			while (levelProgressBar.fillAmount < correctFillAmount * 0.99f)
			{
				correctFillAmount = CalculateFillAmountForExpBar();
				float speed = Mathf.Max((correctFillAmount - levelProgressBar.fillAmount) * 3f, 0.1f);
				levelProgressBar.fillAmount = Mathf.MoveTowards(levelProgressBar.fillAmount, correctFillAmount, Time.deltaTime * speed);
				yield return null;
			}
			levelProgressBar.fillAmount = correctFillAmount;
			yield return null;
		}
	}

	private void RefreshExpAndLevelWithoutAnimating()
	{
		currentlyDisplayedLevel = experienceManager.GetCurrentLevel();
		levelProgressBar.fillAmount = CalculateFillAmountForExpBar();
		playerLevel.text = currentlyDisplayedLevel.ToString();
	}

	private float CalculateFillAmountForExpBar()
	{
		int expSinceLevelUp = experienceManager.GetExpSinceLastLevelUp();
		int level = experienceManager.GetCurrentLevel();
		return (float)expSinceLevelUp / experienceManager.GetExpNeededBetweenLevels(level, level + 1);
	}

	private void UpdateUsername()
	{
		_username.text = UserGameSettings.multiplayerUsername;
	}

	private void OnDestroy()
	{
		MessageKit.removeObserver(128, UpdateUsername);
	}
}
