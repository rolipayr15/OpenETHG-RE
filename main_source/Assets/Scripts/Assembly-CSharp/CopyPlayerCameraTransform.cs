using UnityEngine;

public class CopyPlayerCameraTransform : MonoBehaviour
{
	private Transform playerCameraT;

	private void Start()
	{
		playerCameraT = CustomSingleton<PlayerManager>.Instance.PlayerCamera.transform;
	}

	private void LateUpdate()
	{
		transform.SetPositionAndRotation(playerCameraT.position, playerCameraT.rotation);
	}
}
