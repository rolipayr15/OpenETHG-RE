using Prime31.MessageKit;
using UnityEngine;

public class EnemyCameraSettings : MonoBehaviour
{
	[SerializeField]
	private Camera cam;

	[SerializeField]
	private float shift;

	[SerializeField]
	private Shader replacementShader;

	private void Start()
	{
		if (cam.GetComponent<PixelModeCamera>() == null)
		{
			cam.gameObject.AddComponent<PixelModeCamera>();
		}
		cam.SetReplacementShader(replacementShader, "RenderType");
		cam.rect = new Rect(UserGameSettings.currentEnemy == Enemy.Double ? shift : 0f, 0f, 1f, 1f);
		MessageKit<bool>.addObserver(7, EnemyVision);
	}

	private void EnemyVision(bool isActive)
	{
		cam.enabled = isActive;
	}

	private void OnDestroy()
	{
		MessageKit<bool>.removeObserver(7, EnemyVision);
	}
}
