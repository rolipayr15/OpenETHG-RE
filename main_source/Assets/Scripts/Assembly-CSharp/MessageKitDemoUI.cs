using Prime31.MessageKit;
using UnityEngine;

public class MessageKitDemoUI : MonoBehaviour
{
	private void OnGUI()
	{
		if (GUILayout.Button("Add Observer (no params)"))
		{
			MessageKit.addObserver(0, stuffHappened);
		}
		if (GUILayout.Button("Fire (no params)"))
		{
			MessageKit.post(0);
		}
		if (GUILayout.Button("Remove Observer (no params)"))
		{
			MessageKit.removeObserver(0, stuffHappened);
		}
		if (GUILayout.Button("Debug.Log Observer (no params)"))
		{
			MessageKit.LogObservers(0);
		}
		GUILayout.Space(50f);
		if (GUILayout.Button("Add Observer (two params)"))
		{
			MessageKit<string, GameObject>.addObserver(2, twoParamsEvent);
		}
		if (GUILayout.Button("Fire (two params)"))
		{
			MessageKit<string, GameObject>.post(2, "string param", base.gameObject);
		}
		if (GUILayout.Button("Remove Observer (two params)"))
		{
			MessageKit<string, GameObject>.removeObserver(2, twoParamsEvent);
		}
		if (GUILayout.Button("Debug.Log Observer (two params)"))
		{
			MessageKit<string, GameObject>.logObservers(2);
		}
	}

	private void stuffHappened()
	{
		Debug.Log("stuffHappened fired");
	}

	private void twoParamsEvent(string firstParam, GameObject secondParam)
	{
		Debug.Log("twoParamsEvent: firstParam: " + firstParam + ", secondParam: " + secondParam);
	}
}
