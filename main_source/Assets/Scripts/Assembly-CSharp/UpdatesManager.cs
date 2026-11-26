using System;
using UnityEngine;
using VoxelBusters.CoreLibrary;
using VoxelBusters.EssentialKit;

public class UpdatesManager : MonoBehaviour
{
	private string[] messages = new string[7]
	{
		"What’s scarier than ghosts? Missing the update.",
		"You can run, but you can’t skip this update.",
		"Update's out. Just sayin'.",
		"Skipping updates? Bold move. Fix it now.",
		"Old versions attract more ghosts. Probably. Update's out.",
		"Less glitchy, more spooky. Update's out.",
		"Ghosts are scary. Bugs? Annoying. Fixed some in the latest update."
	};

	private Version currentVersion;

	private Version latestVersion;

	private void Start()
	{
		CheckForUpdates();
	}

	private void CheckForUpdates()
	{
		if (!AppUpdater.IsAvailable())
		{
			Debug.LogWarning("App Updater is not available on this platform or build.");
			return;
		}
		AppUpdater.RequestUpdateInfo((info, error) =>
		{
			if (error != null)
			{
				HandleUpdateError(error);
				return;
			}
			Debug.Log(string.Format("Update status: {0}", info.Status));
			switch (info.Status)
			{
			case AppUpdaterUpdateStatus.Unknown:
				Debug.LogWarning("Could not determine update status");
				break;
			case AppUpdaterUpdateStatus.Available:
				Debug.Log("Update available!");
				ShowUpdatePrompt(false);
				break;
			case AppUpdaterUpdateStatus.NotAvailable:
				Debug.Log("App is up to date");
				break;
			case AppUpdaterUpdateStatus.InProgress:
				Debug.Log("Update is already in progress");
				break;
			case AppUpdaterUpdateStatus.Downloaded:
				Debug.Log("Update already downloaded, ready to install");
				InstallDownloadedUpdate();
				break;
			}
		});
	}

	private void HandleUpdateError(Error error)
	{
		switch (error.Code)
		{
		case 1:
			Debug.LogWarning("No internet connection - cannot check for updates.");
			Debug.LogWarning("Show an in-game network error prompt with retry.");
			break;
		case 2:
			Debug.LogError("Device cannot install the target build (architecture or version mismatch).");
			break;
		case 3:
			Debug.LogWarning("Call RequestUpdateInfo before prompting the update.");
			break;
		case 4:
			Debug.Log("User already has the latest version.");
			break;
		case 5:
			Debug.Log("Update is already running in another flow. Show progress UI.");
			break;
		case 6:
			Debug.LogWarning("User cancelled the update.");
			break;
		default:
			Debug.LogError("Update check failed: " + error.Description);
			break;
		}
	}

	private void ShowUpdatePrompt(bool isForceUpdate)
	{
		PromptUpdateOptions options = new PromptUpdateOptions.Builder()
			.SetPromptTitle("Update Available")
			.SetPromptMessage(messages[UnityEngine.Random.Range(0, messages.Length - 1)])
			.SetIsForceUpdate(isForceUpdate)
			.Build();
		AppUpdater.PromptUpdate(options, (progress, error) =>
		{
			if (error != null)
			{
				Debug.LogError("Update failed: " + error.Description);
				return;
			}
			Debug.Log(string.Format("Update progress: {0}%", progress * 100f));
			if (progress >= 1f)
			{
				Debug.Log("Update completed successfully");
			}
		});
	}

	private void InstallDownloadedUpdate()
	{
		PromptUpdateOptions options = new PromptUpdateOptions.Builder()
			.SetPromptTitle("Install Update")
			.SetPromptMessage("Update is ready to install. App will restart.")
			.SetAllowInstallationIfDownloaded(true)
			.Build();
		AppUpdater.PromptUpdate(options, (progress, error) =>
		{
			if (progress >= 1f && error == null)
			{
				Debug.Log("Installing update...");
			}
		});
	}

	private void Skip()
	{
		PlayerPrefs.SetString("VERSION_TO_SKIP", latestVersion.ToString());
	}

	private void Later()
	{
		UserGameSettings.skipThisTime = true;
	}
}
