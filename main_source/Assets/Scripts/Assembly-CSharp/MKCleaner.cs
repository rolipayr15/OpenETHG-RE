using Prime31.MessageKit;
using UnityEngine;

public class MKCleaner : MonoBehaviour
{
	private void OnDestroy()
	{
		MessageKitManager.clearAllMessageTables();
	}
}
