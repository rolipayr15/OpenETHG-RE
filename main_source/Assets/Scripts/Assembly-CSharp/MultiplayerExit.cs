using UnityEngine;

public class MultiplayerExit : MonoBehaviour
{
	private bool isAnotherPlayerReady;

	private bool isPlayerReady;

	private void Start()
	{
		if ((int)UserGameSettings.currentGameMode != 1)
		{
			gameObject.SetActive(false);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Avatar")
		{
			isAnotherPlayerReady = true;
		}
		if (other.tag == "Player")
		{
			isPlayerReady = true;
		}
		Debug.Log("is another player ready " + isAnotherPlayerReady);
		Debug.Log("is player ready " + isPlayerReady);
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.tag == "Avatar")
		{
			isAnotherPlayerReady = false;
		}
		if (other.tag == "Player")
		{
			isPlayerReady = false;
		}
		Debug.Log("is another player ready " + isAnotherPlayerReady);
		Debug.Log("is player ready " + isPlayerReady);
	}

	public bool ArePlayersReady()
	{
		return isAnotherPlayerReady && isPlayerReady;
	}
}
