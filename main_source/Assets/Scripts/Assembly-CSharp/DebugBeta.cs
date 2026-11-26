using Prime31.MessageKit;
using UnityEngine;
using UnityEngine.UI;

public class DebugBeta : MonoBehaviour
{
	private Text debugtxt;

	private string debugInfo;

	private void Start()
	{
		if (!UserGameSettings.IsMultiplayer())
		{
			return;
		}
		debugtxt = GetComponent<Text>();
		debugInfo = UserGameSettings.multiplayerGameId;
		if (UserGameSettings.isMultiplayerHost)
		{
			debugInfo += " H";
		}
		debugtxt.text = DecodeUrlString(debugInfo);
		MessageKit<string>.addObserver(100, ShowDebugInfo);
	}

	private void ShowDebugInfo(string content)
	{
		debugtxt.text = debugInfo + content;
	}

	private void OnDestroy()
	{
		MessageKit<string>.removeObserver(100, ShowDebugInfo);
	}

	private string DecodeUrlString(string url)
	{
		string decoded;
		do
		{
			decoded = url;
			url = System.Uri.UnescapeDataString(decoded);
		}
		while (url != decoded);
		return url;
	}
}
