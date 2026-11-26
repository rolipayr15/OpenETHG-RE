using UnityEngine;

public class InteractableHighlight : MonoBehaviour
{
	private static Shader highlightShader;

	private Transform playerHeart;

	private Shader originalShader;

	private Material material;

	private bool highlightEnabled;

	private void Start()
	{
		int difficulty = (int)UserGameSettings.currentDifficulty;
		highlightEnabled = difficulty == 0 || difficulty == 1;
		if (highlightEnabled)
		{
			Renderer target = GetComponent<Renderer>();
			if (target != null)
			{
				material = target.material;
				if (highlightShader == null)
				{
					highlightShader = Shader.Find("Custom/DiffuseHighlight");
				}
				originalShader = material.shader;
				playerHeart = CustomSingleton<PlayerManager>.Instance.Heart.transform;
			}
		}
	}

	private void Update()
	{
		if (!highlightEnabled)
		{
			return;
		}
		float distance = (base.gameObject.transform.position - playerHeart.position).sqrMagnitude;
		if (distance < 70f)
		{
			if (material.shader != highlightShader)
			{
				material.shader = highlightShader;
			}
			material.SetFloat("_OutlineWidth", (70f - distance) / 10000f);
		}
		else if (material.shader != originalShader)
		{
			material.shader = originalShader;
		}
	}
}
