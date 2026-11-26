using UnityEngine;

public class MultiplayerMainMenu : MonoBehaviour
{
	public void PlayWithAFriend()
	{
		MultiplayerLobby.Show(false);
	}

	public void PlayWithRandom()
	{
		MultiplayerLobby.Show(true);
	}
}
