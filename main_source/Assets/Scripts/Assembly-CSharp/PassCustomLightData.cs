using UnityEngine;

[ExecuteAlways]
[DefaultExecutionOrder(10000)]
public class PassCustomLightData : MonoBehaviour
{
	private static readonly int WorldToLightXId = Shader.PropertyToID("unity_WorldToLightX");

	private static readonly int LightColor0XId = Shader.PropertyToID("_LightColor0X");

	private static readonly int LightTexture0XId = Shader.PropertyToID("_LightTexture0X");

	private static readonly int WorldSpaceLightPos0XId = Shader.PropertyToID("_WorldSpaceLightPos0X");

	[SerializeField]
	private Light _flashlight;

	[SerializeField]
	private Texture _cookieFallback;

	private void Start()
	{
		Shader.globalMaximumLOD = 200;
		SetShaderVars();
	}

	private void LateUpdate()
	{
		SetShaderVars();
	}

	public void Refresh()
	{
		SetShaderVars();
	}

	private void SetShaderVars()
	{
		if (_flashlight != null && _flashlight.range > 0f && _flashlight.spotAngle > 0f && _flashlight.spotAngle < 180f)
		{
			float depth = 1f / _flashlight.range;
			float spread = depth * 0.5f / Mathf.Tan(_flashlight.spotAngle * 0.5f * (Mathf.PI / 180f));
			Matrix4x4 projection = Matrix4x4.zero;
			projection.m00 = spread;
			projection.m11 = spread;
			projection.m22 = depth;
			projection.m32 = depth;
			Shader.SetGlobalMatrix(WorldToLightXId, projection * _flashlight.transform.worldToLocalMatrix);
			Shader.SetGlobalColor(LightColor0XId, _flashlight.color * _flashlight.intensity);
			Shader.SetGlobalVector(WorldSpaceLightPos0XId, _flashlight.transform.position);
		}
		Texture cookie = (_flashlight != null) ? _flashlight.cookie : _cookieFallback;
		if (cookie != null)
		{
			Shader.SetGlobalTexture(LightTexture0XId, cookie);
		}
	}
}
