using Prime31.MessageKit;
using UnityEngine;
using UnityEngine.UI;

public class KeysCounter : MonoBehaviour
{
	[SerializeField]
	private Text label;

	private void Start()
	{
		label.text = KeysManager.GetMultiplayerKeys().ToString();
		MessageKit.addObserver(131, UpdateKeyCounter);
	}

	private void UpdateKeyCounter()
	{
		label.text = KeysManager.GetMultiplayerKeys().ToString();
	}

	private void OnDestroy()
	{
		MessageKit.removeObserver(131, UpdateKeyCounter);
	}
}
