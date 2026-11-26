using UnityEngine;

public class ReplacementShader : MonoBehaviour
{
	public bool bright;

	public float brightness;

	public Shader replacementShader;

	public Texture rampTexture;

	public void OnEnable()
	{
	}

	public void OnPreCull()
	{
	}
}
