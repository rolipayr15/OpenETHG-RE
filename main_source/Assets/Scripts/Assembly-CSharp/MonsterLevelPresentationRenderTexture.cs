using UnityEngine;

public class MonsterLevelPresentationRenderTexture : MonoBehaviour
{
	[SerializeField]
	private int width = 512;

	[SerializeField]
	private int height = 256;

	[SerializeField]
	private FilterMode filtering = FilterMode.Bilinear;

	private RenderTexture texture;

	public RenderTexture GetRenderTexture()
	{
		RecreateRenderTextureIfNeeded();
		return texture;
	}

	private void Awake()
	{
		RecreateRenderTextureIfNeeded();
	}

	private void OnDestroy()
	{
		if (texture != null)
		{
			texture.Release();
			texture = null;
		}
	}

	private void RecreateRenderTextureIfNeeded()
	{
		if (texture == null)
		{
			texture = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32);
			texture.filterMode = filtering;
		}
	}
}
