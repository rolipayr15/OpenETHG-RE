using UnityEngine;

public class DestroyRenderTexture : MonoBehaviour
{
	[SerializeField]
	private RenderTexture renderTexture;

	private void OnDestroy()
	{
		if (renderTexture != null)
		{
			renderTexture.Release();
		}
	}
}
