using UnityEngine;

public sealed class FlashlightModelVisibility : MonoBehaviour
{
	[SerializeField]
	private Renderer model;

	private void OnEnable()
	{
		EyesLightingComposer.Applied += OnApplied;
		Refresh();
	}

	private void OnDisable()
	{
		EyesLightingComposer.Applied -= OnApplied;
	}

	private void OnApplied(ResolvedLighting r)
	{
		if (model != null)
		{
			model.enabled = !r.hideFlashlightModel;
		}
	}

	private void Refresh()
	{
		if (model != null)
		{
			model.enabled = !EyesLightingComposer.Current.hideFlashlightModel;
		}
	}
}
