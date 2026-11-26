using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class FalloffSettings : MonoBehaviour
{
	public const string FlashlightBoostInterferenceMulProperty = "_FlashlightBoostInterferenceMul";

	[Tooltip("Authored lighting values for this scene. The asset is shared - edit it in the Project window, not per scene.")]
	public EyesLightingProfile profile;

	private bool isMenuScene;

	private void Awake()
	{
		if (Application.isPlaying)
		{
			isMenuScene = SceneManager.GetActiveScene().name == "Menu";
		}
		if (Application.isPlaying)
		{
			EyesLightingComposer.ClearRuntimeOverrides();
		}
		ApplyShaderVars();
		Shader.DisableKeyword("TURN_OFF_FALLOFF");
		Shader.DisableKeyword("TURN_OFF_LIGHTMAP");
		EyesLightingComposer.Applied += OnApplied;
		OnApplied(EyesLightingComposer.Current);
	}

	private void OnDestroy()
	{
		EyesLightingComposer.Applied -= OnApplied;
	}

	private void OnApplied(ResolvedLighting r)
	{
		if (Application.isPlaying && !isMenuScene)
		{
			Camera main = Camera.main;
			if (main != null)
			{
				main.backgroundColor = r.falloffColor;
			}
		}
	}

	public void ApplyShaderVars()
	{
		if (profile == null)
		{
			Debug.LogWarning("FalloffSettings: no EyesLightingProfile assigned - authored shader globals left untouched.", this);
		}
		EyesLightingComposer.SetProfile(EyesLightingLevelProfiles.Resolve(SceneManager.GetActiveScene().name, profile));
		if (Application.isPlaying && SceneManager.GetActiveScene().name != "Menu")
		{
			OptionsDesaturation.ApplySavedDesaturation();
		}
	}
}
