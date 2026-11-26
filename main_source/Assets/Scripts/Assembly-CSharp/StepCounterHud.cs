using UnityEngine;
using UnityEngine.UI;

public sealed class StepCounterHud : MonoBehaviour
{
	[SerializeField]
	private GameObject counterRoot;

	[SerializeField]
	private Text counterLabel;

	[SerializeField]
	private Toggle visibilityToggle;

	[SerializeField]
	private Text toggleLabel;

	private bool visible;

	private int displayedSteps = -1;

	private string countFormat = "Steps: {0}";

	private void OnEnable()
	{
		visible = UserGameSettings.showStepCounter;
		visibilityToggle.SetIsOnWithoutNotify(visible);
		visibilityToggle.onValueChanged.AddListener(SetVisible);
		I2.Loc.LocalizationManager.OnLocalizeEvent += RefreshLocalization;
		counterRoot.SetActive(visible);
		RefreshLocalization();
	}

	private void OnDisable()
	{
		visibilityToggle.onValueChanged.RemoveListener(SetVisible);
		I2.Loc.LocalizationManager.OnLocalizeEvent -= RefreshLocalization;
	}

	public void SetVisible(bool value)
	{
		visible = value;
		UserGameSettings.showStepCounter = value;
		visibilityToggle.SetIsOnWithoutNotify(value);
		counterRoot.SetActive(value);
		displayedSteps = -1;
		RefreshCount();
	}

	private void LateUpdate()
	{
		RefreshCount();
	}

	private void RefreshCount()
	{
		if (visible && UserGameSettings.currentGameSteps != displayedSteps)
		{
			displayedSteps = UserGameSettings.currentGameSteps;
			counterLabel.text = countFormat.Replace("{0}", displayedSteps.ToString(System.Globalization.CultureInfo.InvariantCulture));
		}
	}

	private void RefreshLocalization()
	{
		toggleLabel.text = Translate("StepCounter/Show", "Step counter");
		countFormat = Translate("StepCounter/Count", "Steps: {0}");
		Font font = I2.Loc.LocalizationManager.GetTranslatedObjectByTermName<Font>("GeneralUseFont");
		if (font != null)
		{
			toggleLabel.font = font;
			counterLabel.font = font;
		}
		LocalizedFontLayout.Apply(toggleLabel);
		LocalizedFontLayout.Apply(counterLabel);
		displayedSteps = -1;
		RefreshCount();
	}

	private static string Translate(string term, string fallback)
	{
		string translation = I2.Loc.LocalizationManager.GetTranslation(term);
		return string.IsNullOrWhiteSpace(translation) || translation == term ? fallback : translation;
	}
}
