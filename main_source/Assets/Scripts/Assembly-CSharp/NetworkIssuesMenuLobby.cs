using System.Collections;
using UnityEngine;

// The ARM build listens to VoxelBusters NetworkServices; this polls the same reachability state.
public class NetworkIssuesMenuLobby : MonoBehaviour
{
	[SerializeField]
	private GameObject networkIssuesButton;

	private void OnEnable()
	{
		StartCoroutine(Watch());
	}

	private IEnumerator Watch()
	{
		WaitForSecondsRealtime delay = new WaitForSecondsRealtime(2f);
		while (true)
		{
			if (networkIssuesButton != null)
			{
				networkIssuesButton.SetActive(Application.internetReachability == NetworkReachability.NotReachable);
			}
			yield return delay;
		}
	}

	public void ShowWindow()
	{
		NetworkIssues.Show();
	}
}
