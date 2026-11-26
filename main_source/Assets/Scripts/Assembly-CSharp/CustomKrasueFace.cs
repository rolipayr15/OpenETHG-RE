using UnityEngine;

public class CustomKrasueFace : MonoBehaviour
{
	[SerializeField]
	private Material mask;

	[SerializeField]
	private Texture krasueFaceWithoutDetails;

	[SerializeField]
	private Texture krasueFaceWithDetails;

	[SerializeField]
	private Material krasueMat;

	[SerializeField]
	private GameObject customMask;

	private void Start()
	{
		if ((int)UserGameSettings.currentEnemy != 3)
		{
			krasueMat.mainTexture = krasueFaceWithDetails;
			return;
		}
		krasueMat.mainTexture = krasueFaceWithoutDetails;
		customMask.SetActive(true);
		if (string.IsNullOrEmpty(CustomKrasueSettings.Path))
		{
			return;
		}
		Texture2D texture = NativeGallery.LoadImageAtPath(CustomKrasueSettings.Path, 512, true, true, false);
		if (texture != null)
		{
			mask.mainTexture = texture;
			CustomKrasueSettings.UVConfig uv = CustomKrasueSettings.TextureUVConfig;
			mask.SetTextureOffset("_MainTex", new Vector2(uv.xTrans - uv.xScale * 0.5f + 0.5f, uv.yTrans - uv.yScale * 0.5f + 0.5f));
			mask.SetTextureScale("_MainTex", new Vector2(uv.xScale, uv.yScale));
		}
	}
}
