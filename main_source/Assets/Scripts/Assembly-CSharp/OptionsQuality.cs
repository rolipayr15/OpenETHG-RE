using UnityEngine;
using UnityEngine.UI;

public class OptionsQuality : MonoBehaviour
{
	public Dropdown dropdown;

	private bool isInitializing;

	private void Start()
	{
		isInitializing = true;
		int current = (int)GraphicsQualityManager.CurrentQuality;
		if (dropdown == null)
		{
			Debug.LogError("OptionsQuality dropdown is not assigned.", this);
		}
		else
		{
			dropdown.SetValueWithoutNotify(current);
			dropdown.RefreshShownValue();
		}
		isInitializing = false;
	}

	public void OnValueChanged(int value)
	{
		if (isInitializing)
		{
			return;
		}
		int clamped = Mathf.Clamp(value, 0, 2);
		if (clamped != value)
		{
			Debug.LogWarning(string.Format("OptionsQuality dropdown value '{0}' is outside the supported range. ", value) + string.Format("Using '{0}'.", clamped), this);
		}
		if (!GraphicsQualityManager.SetQuality((GraphicsQuality)clamped))
		{
			Debug.LogError(string.Format("OptionsQuality could not apply '{0}'.", (GraphicsQuality)clamped), this);
			RestoreCurrentValue();
		}
		else if (dropdown != null)
		{
			dropdown.SetValueWithoutNotify(clamped);
			dropdown.RefreshShownValue();
		}
	}

	private void RestoreCurrentValue()
	{
		if (dropdown != null)
		{
			dropdown.SetValueWithoutNotify((int)GraphicsQualityManager.CurrentQuality);
			dropdown.RefreshShownValue();
		}
	}
}
