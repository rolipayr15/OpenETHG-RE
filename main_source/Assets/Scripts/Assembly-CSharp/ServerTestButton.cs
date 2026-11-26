using UnityEngine;
using UnityEngine.UI;

public class ServerTestButton : MonoBehaviour
{
	[SerializeField]
	private GameObject spinnerPrefab;

	[SerializeField]
	private GameObject label;

	private GameObject spinner;

	private Button button;

	private void Awake()
	{
		button = GetComponent<Button>();
	}

	public void ShowServerTest()
	{
		ServerTest.Show();
	}

	private void OnEnable()
	{
		MultiplayerNetworkTest.Changed += Refresh;
		Refresh();
	}

	private void OnDisable()
	{
		MultiplayerNetworkTest.Changed -= Refresh;
	}

	private void Refresh()
	{
		bool running = MultiplayerNetworkTest.IsRunning;
		if (spinner == null && running && spinnerPrefab != null)
		{
			spinner = Instantiate(spinnerPrefab, transform);
		}
		if (spinner != null)
		{
			spinner.SetActive(running);
		}
		if (label != null)
		{
			label.SetActive(!running);
		}
	}
}
