using System;
using System.Collections;
using System.Text;
using EyesAnalytics;
using ExternalApi.Models;
using Firebase.Crashlytics;
using I2.Loc;
using Multiplayer.ExternalApi;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
	private const float SceneReadyProgress = 0.9f;

	private const float SceneActivationFallbackSeconds = 10f;

	public Image image;

	private float lastProgress;

	private StringBuilder objective;

	private StringBuilder label;

	private bool canBeContinued;

	[SerializeField]
	private GameObject loadingPanel;

	[SerializeField]
	private Text pleaseWait;

	[SerializeField]
	private GameObject continueButton;

	private bool isLoading;

	[SerializeField]
	private Text objectivesMoneyBags;

	[SerializeField]
	private GameObject objectivesGroupHalloween;

	[SerializeField]
	private GameObject objectivesGroupStandard;

	[SerializeField]
	private GameObject objectivesGroupEndless;

	[SerializeField]
	private GameObject objectivesGroupHospital;

	[SerializeField]
	private GameObject objectivesPanel;

	[SerializeField]
	private GameObject objectivesGroupDog;

	[SerializeField]
	private GameObject objectivesGroupSchool;

	[SerializeField]
	private GameObject objectivesGroupSandbox;

	[SerializeField]
	private GameObject objectivesGroupWitch;

	[SerializeField]
	private GameObject objectivesGroupGeese;

	[SerializeField]
	private GameObject objectivesGroupMultiplayerCoop;

	[SerializeField]
	private GameObject objectivesGroupMultiplayerVs;

	[SerializeField]
	private Text usernamesCoop;

	[SerializeField]
	private Text usernamesVs;

	private AsyncOperation async;

	private bool levelLoadStarted;

	[Inject]
	private IExpApi expApi;

	[Inject]
	private IExperienceManager experienceManager;

	private IEnumerator Start()
	{
		if (FirebaseState.IsInitialized)
		{
			Crashlytics.Log("LoadingScene");
		}
		objectivesPanel.SetActive(false);
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		objective = new StringBuilder();
		objective.Append(ScriptLocalization.MoneyBagsToFind);
		objective.Append(" ");
		switch (UserGameSettings.currentDifficulty)
		{
		case Difficulty.Casual:
			objective.Append(6);
			break;
		case Difficulty.Easy:
			objective.Append(12);
			break;
		case Difficulty.Normal:
			objective.Append(20);
			break;
		case Difficulty.Hard:
		case Difficulty.Nightmare:
			objective.Append(30);
			break;
		}
		objectivesMoneyBags.text = objective.ToString();
		DeactivateAll();

		if (UserGameSettings.IsMultiplayer())
		{
			yield return expApi.GetOpponentLocalExpInfo(UserGameSettings.multiplayerGameId).ToCoroutine(this, info =>
			{
				if (!info.HasValue)
				{
					UserGameSettings.currentSecondPlayerUsername = "";
					UserGameSettings.currentSecondPlayerLevel = 0;
				}
				else
				{
					UserGameSettings.currentSecondPlayerUsername = info.Value.username;
					UserGameSettings.currentSecondPlayerLevel = experienceManager.GetLevelForExp(info.Value.localExp);
				}
			});
			if (UserGameSettings.IsMultiplayerCoop())
			{
				objectivesGroupMultiplayerCoop.SetActive(true);
				usernamesCoop.text = UserGameSettings.multiplayerUsername + " (" + experienceManager.GetCurrentLevel() + ")\nand\n" + UserGameSettings.currentSecondPlayerUsername + " (" + UserGameSettings.currentSecondPlayerLevel + ")";
			}
			else
			{
				objectivesGroupMultiplayerVs.SetActive(true);
				usernamesVs.text = UserGameSettings.multiplayerUsername + " (" + experienceManager.GetCurrentLevel() + ")\nvs\n" + UserGameSettings.currentSecondPlayerUsername + " (" + UserGameSettings.currentSecondPlayerLevel + ")";
			}
		}
		else
		{
			GameObject group;
			if (UserGameSettings.currentEnemy == Enemy.Sandbox)
			{
				group = objectivesGroupSandbox;
			}
			else if (UserGameSettings.currentEnemy == Enemy.Witch)
			{
				group = objectivesGroupWitch;
			}
			else if (UserGameSettings.currentEnemy == Enemy.Goose && objectivesGroupGeese != null)
			{
				group = objectivesGroupGeese;
			}
			else if (UserGameSettings.currentLevel == Level.School)
			{
				group = objectivesGroupSchool;
			}
			else if (UserGameSettings.currentEnemy == Enemy.Dog)
			{
				group = objectivesGroupDog;
			}
			else if (UserGameSettings.currentMode == Mode.ENDLESS)
			{
				group = objectivesGroupEndless;
			}
			else if (UserGameSettings.currentMode == Mode.HALLOWEEN)
			{
				group = objectivesGroupHalloween;
			}
			else if (UserGameSettings.currentLevel == Level.Hospital)
			{
				group = objectivesGroupHospital;
			}
			else
			{
				group = objectivesGroupStandard;
			}
			group.SetActive(true);
		}
		objectivesPanel.SetActive(true);
		SendAnalyticsEvents();
		yield return null;

		if (!AssetBundleManager.LevelInAssetBundle(UserGameSettings.currentLevel))
		{
			StartLevelLoad();
			yield break;
		}
		string[] bundleDependencyList = new string[2]
		{
			"common_hospital_school",
			UserGameSettings.currentLevel.ToString().ToLower()
		};
		if (!TryGetBundlesToDownload(bundleDependencyList, out string[] bundlesToDownload))
		{
			yield break;
		}
		Action download = delegate
		{
			AssetBundleManager.MakeSureBundlesAreDownloadedAndLoaded(bundle => StartLevelLoad(bundle), error => MessageBox.Show(error, () => SceneManager.LoadScene("MenuLoader")), bundleDependencyList);
		};
		if (bundlesToDownload.Length != 0 && Application.platform != RuntimePlatform.IPhonePlayer && Application.internetReachability != NetworkReachability.ReachableViaLocalAreaNetwork)
		{
			QuestionBox.Show(AssetBundleManager.GetDownloadSizeNoticeTextForBundles(bundlesToDownload), "Ok!", "Cancel", download, () => SceneManager.LoadScene("MenuLoader"));
			yield break;
		}
		download();
	}

	private bool TryGetBundlesToDownload(string[] bundleNames, out string[] bundlesToDownload)
	{
		bundlesToDownload = AssetBundleManager.FilterBundleNamesThatNeedDownloading(bundleNames);
		return true;
	}

	private void SendAnalyticsEvents()
	{
		if (UserGameSettings.currentGameMode == GameMode.SINGLE)
		{
			AnalyticsContext.SetSingleplayer();
		}
		else
		{
			AnalyticsContext.SetMultiplayer(UserGameSettings.currentGameMode == GameMode.COOP, !UserGameSettings.isPlayingWithRandom);
		}
	}

	private void LoadLevelFromTheBundle(AssetBundle bundle)
	{
		StartLevelLoad(bundle);
	}

	private void StartLevelLoad(AssetBundle bundle = null)
	{
		if (levelLoadStarted)
		{
			LogLoadingState("Ignored a duplicate request to load the level");
			return;
		}
		levelLoadStarted = true;
		StartCoroutine(LoadLevel(bundle));
	}

	private void DeactivateAll()
	{
		objectivesGroupStandard.SetActive(false);
		objectivesGroupHalloween.SetActive(false);
		objectivesGroupEndless.SetActive(false);
		objectivesGroupHospital.SetActive(false);
		objectivesGroupDog.SetActive(false);
		objectivesGroupSchool.SetActive(false);
		objectivesGroupSandbox.SetActive(false);
		objectivesGroupSandbox.SetActive(false);
		objectivesGroupWitch.SetActive(false);
		if (objectivesGroupGeese != null)
		{
			objectivesGroupGeese.SetActive(false);
		}
		objectivesGroupMultiplayerCoop.SetActive(false);
		objectivesGroupMultiplayerVs.SetActive(false);
	}

	public void ClickToContinue()
	{
		if (!isLoading && async != null)
		{
			RequestSceneActivation("continue button");
		}
	}

	private void RequestSceneActivation(string reason)
	{
		if (async == null || async.allowSceneActivation)
		{
			return;
		}
		canBeContinued = false;
		continueButton.SetActive(false);
		isLoading = true;
		pleaseWait.text = ScriptLocalization.PleaseWait;
		LogLoadingState("Activating the level scene: " + reason);
		async.allowSceneActivation = true;
	}

	private IEnumerator LoadLevel(AssetBundle bundle = null)
	{
		string sceneName;
		if (bundle == null)
		{
			Debug.Log("bundle to null");
			sceneName = UserGameSettings.currentLevel.ToString();
		}
		else
		{
			string[] scenePaths = bundle.GetAllScenePaths();
			if (scenePaths == null || scenePaths.Length == 0)
			{
				HandleLevelLoadError("The target AssetBundle contains no scene");
				yield break;
			}
			Debug.Log("scene paths to" + scenePaths[0]);
			sceneName = scenePaths[0];
		}
		AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
		if (loadOperation == null)
		{
			HandleLevelLoadError("LoadSceneAsync returned null");
			yield break;
		}
		async = loadOperation;
		loadOperation.allowSceneActivation = false;
		continueButton.SetActive(true);
		while (loadOperation.progress < SceneReadyProgress)
		{
			UpdateLabel(loadOperation.progress);
			yield return null;
		}
		image.fillAmount = 1f;
		canBeContinued = true;
		float activationDeadline = Time.realtimeSinceStartup + SceneActivationFallbackSeconds;
		while (!loadOperation.allowSceneActivation && Time.realtimeSinceStartup < activationDeadline)
		{
			yield return null;
		}
		if (!loadOperation.allowSceneActivation)
		{
			RequestSceneActivation("10-second safety timeout");
		}
		while (!loadOperation.isDone)
		{
			yield return null;
		}
	}

	private void HandleLevelLoadError(string reason)
	{
		LogLoadingState("Level load failed: " + reason);
		MessageBox.Show(ScriptLocalization.GPAssetDeliveryErrorCodeInternalError, () => SceneManager.LoadScene("MenuLoader"));
	}

	private void LogLoadingState(string message)
	{
		string line = "LoadingScene: " + message;
		Debug.Log(line);
		if (FirebaseState.IsInitialized)
		{
			Crashlytics.Log(line);
		}
	}

	private void UpdateLabel(float progress)
	{
		if (lastProgress < progress)
		{
			image.fillAmount = progress;
			lastProgress = progress;
		}
	}
}
