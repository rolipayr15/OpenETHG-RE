using Prime31.MessageKitLite;
using UnityEngine;

public class MessageKitLiteDemoUI : MonoBehaviour, MessageReceiver, MessageReceiver<GameObject>
{
	private void OnGUI()
	{
		if (GUILayout.Button("Add Observer (no params)"))
		{
			MessageKitLite.addObserver(0, this);
		}
		if (GUILayout.Button("Fire (no params)"))
		{
			MessageKitLite.post(0);
		}
		if (GUILayout.Button("Remove Observer (no params)"))
		{
			MessageKitLite.removeObserver(0, this);
		}
		GUILayout.Space(50f);
		if (GUILayout.Button("Add Observer (GameObject param)"))
		{
			MessageKitLite<GameObject>.addObserver(1, this);
		}
		if (GUILayout.Button("Fire (GameObject param)"))
		{
			MessageKitLite<GameObject>.post(1, base.gameObject);
		}
		if (GUILayout.Button("Remove Observer (GameObject param)"))
		{
			MessageKitLite<GameObject>.removeObserver(1, this);
		}
	}

	public void onMessageReceived(int messageType)
	{
		Debug.Log("onMessageReceived: " + messageType);
	}

	public void onMessageReceived(int messageType, GameObject param)
	{
		Debug.Log("onMessageReceived: " + param.name);
	}
}
