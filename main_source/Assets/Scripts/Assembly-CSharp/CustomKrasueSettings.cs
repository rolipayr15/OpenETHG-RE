using UnityEngine;

public class CustomKrasueSettings
{
	public struct UVConfig
	{
		internal float xTrans;

		internal float yTrans;

		internal float xScale;

		internal float yScale;

		public void Set(float _xTrans, float _yTrans, float _xScale, float _yScale)
		{
			xTrans = _xTrans;
			yTrans = _yTrans;
			xScale = _xScale;
			yScale = _yScale;
		}

		// Texture offset that keeps the scaled face centred on (xTrans, yTrans).
		public Vector2 GetOffset()
		{
			return new Vector2(xTrans - xScale * 0.5f + 0.5f, yTrans - yScale * 0.5f + 0.5f);
		}

		public Vector2 GetScale()
		{
			return new Vector2(xScale, yScale);
		}
	}

	private const string filename = "eyes_custom_mask.png";

	public static string Path;

	public const int maxSize = 512;

	public static UVConfig TextureUVConfig;
}
