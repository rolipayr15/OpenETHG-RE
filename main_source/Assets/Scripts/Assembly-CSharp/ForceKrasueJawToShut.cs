using UnityEngine;

public class ForceKrasueJawToShut : MonoBehaviour
{
	private static readonly Quaternion ShutRotation = Quaternion.Euler(89.09388f, 259.6379f, 169.2304f);

	private void Start()
	{
		if (UserGameSettings.currentEnemy != Enemy.Custom)
		{
			gameObject.SetActive(false);
		}
	}

	private void LateUpdate()
	{
		transform.localRotation = ShutRotation;
	}
}
