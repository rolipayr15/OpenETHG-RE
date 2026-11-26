using Prime31.MessageKit;
using UnityEngine;

public class ResetJoystickPosition : MonoBehaviour
{
	private Vector3 initialPosition;

	private void Start()
	{
		initialPosition = transform.position;
		MessageKit.addObserver(38, ResetPosition);
	}

	private void OnDestroy()
	{
		MessageKit.removeObserver(38, ResetPosition);
	}

	private void ResetPosition()
	{
		if (!UserGameSettings.dynamicJoystickEnabled)
		{
			gameObject.transform.position = initialPosition;
		}
	}
}
