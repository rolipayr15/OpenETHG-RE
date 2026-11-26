using Bibloteka;
using BestHTTP;
using ExternalApi.Models;
using Multiplayer.ExternalApi;
using Prime31.MessageKit;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UsernameChange : MonoBehaviour
{
	[SerializeField]
	private Text oldUsername;

	[SerializeField]
	private InputField input;

	[SerializeField]
	private GameObject spinner;

	[Inject]
	private IUserApi userApi;

	[SerializeField]
	private Text error;

	private bool isSaving;

	public static void Show()
	{
		SceneManager.LoadSceneAsync("ChangeUsername", LoadSceneMode.Additive);
	}

	private void Start()
	{
		if (oldUsername != null)
		{
			oldUsername.text = UserGameSettings.multiplayerUsername;
		}
		if (spinner != null)
		{
			spinner.SetActive(false);
		}
	}

	public void CloseWindow()
	{
		if (Utils.IsSceneLoaded("ChangeUsername"))
		{
			SceneManager.UnloadSceneAsync("ChangeUsername", UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
		}
	}

	public async void SaveButton()
	{
		if (isSaving)
		{
			Debug.Log("UsernameChange SaveButton ignored - request already in progress");
			return;
		}
		string username = input != null && input.text != null ? input.text.Trim() : "";
		if (username.Length < 3 || username.Length > 15)
		{
			error.text = "Username must be between 3 and 15 characters";
			return;
		}
		isSaving = true;
		error.text = "";
		spinner.SetActive(true);
		HTTPResponse response = await userApi.SetUsernameAsync(new UsernameData
		{
			name = username
		});
		isSaving = false;
		if (this == null)
		{
			return;
		}
		spinner.SetActive(false);
		if (response == null)
		{
			error.text = "Failed to set username";
			return;
		}
		if (response.StatusCode == 403)
		{
			error.text = "This username contains forbidden words and cannot be set. Please choose a different name.";
			return;
		}
		if (!response.IsSuccess)
		{
			Debug.Log(string.Format("Failed to set username. StatusCode={0}", response.StatusCode));
			error.text = string.Format("Failed to set username ({0})", response.StatusCode);
			return;
		}
		OnlineConfig.SavePlayerName(username);
		MessageKit.post(128);
		CloseWindow();
	}

	public void CancelButton()
	{
		CloseWindow();
	}
}
