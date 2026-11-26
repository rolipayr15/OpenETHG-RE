using UnityEngine;
using UnityEngine.UI;

public class SpinnerServerConnection : MonoBehaviour
{
	[SerializeField]
	private GameObject spinner;

	[SerializeField]
	private GameObject waitingOther;

	private bool waitingForOtherPlayer;

	[SerializeField]
	private Text labelWithUsername;

	private void Start()
	{
		UserGameSettings.IsMultiplayer();
	}

	private void ToggleSpinner(bool disconnected)
	{
		waitingOther.SetActive(!disconnected && waitingForOtherPlayer);
		spinner.SetActive(disconnected);
	}

	private void ToggleWaitingOther(bool isWaiting)
	{
		waitingForOtherPlayer = isWaiting;
		waitingOther.SetActive(isWaiting);
	}

	private void MultiplayerGameFinished()
	{
		spinner.SetActive(false);
		waitingOther.SetActive(false);
	}
}
