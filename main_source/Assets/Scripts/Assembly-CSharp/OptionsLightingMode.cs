using EyesAnalytics;
using UnityEngine;
using UnityEngine.UI;

public sealed class OptionsLightingMode : MonoBehaviour
{
	public enum PanelKind
	{
		Hud = 0,
		Menu = 1
	}

	[SerializeField]
	private PanelKind panel;

	[SerializeField]
	private ChoiceGroup modes;

	[SerializeField]
	private Button calibrate;

	private Canvas canvas;

	private bool wasVisible;

	private LightingMode modeAtOpen;

	private float brightnessAtOpen;

	private void Awake()
	{
		canvas = GetComponentInParent<Canvas>(true);
		if (modes == null || calibrate == null)
		{
			Debug.LogError("OptionsLightingMode: modes/calibrate not wired in the asset; the Lighting row is dead.", this);
		}
		if (modes != null)
		{
			modes.onChanged.AddListener(index => LightingSettings.SetMode((LightingMode)index));
		}
		if (calibrate != null)
		{
			calibrate.onClick.AddListener(BrightnessCalibration.Show);
		}
	}

	private void OnEnable()
	{
		LightingSettings.Changed += Render;
		BrightnessCalibration.Closed += OnCalibratorClosed;
		Render();
		wasVisible = false;
	}

	private void OnDisable()
	{
		LightingSettings.Changed -= Render;
		BrightnessCalibration.Closed -= OnCalibratorClosed;
		if (wasVisible)
		{
			Commit();
		}
		wasVisible = false;
	}

	private void OnCalibratorClosed()
	{
		if (wasVisible)
		{
			Snapshot();
		}
	}

	// Reports lighting changes once per visit to the options panel: snapshot on open, commit on close.
	private void Update()
	{
		bool visible = canvas != null && canvas.isActiveAndEnabled;
		if (visible)
		{
			if (!wasVisible)
			{
				Snapshot();
			}
		}
		else if (wasVisible)
		{
			Commit();
		}
		wasVisible = visible;
	}

	private void Snapshot()
	{
		modeAtOpen = LightingSettings.Mode;
		brightnessAtOpen = LightingSettings.BrightnessMultiplier;
	}

	private void Commit()
	{
		LightingSettingsSource source = panel == PanelKind.Menu ? LightingSettingsSource.Menu : LightingSettingsSource.Hud;
		Lighting.Commit(source, modeAtOpen, LightingSettings.Mode, brightnessAtOpen, LightingSettings.BrightnessMultiplier, Lighting.ReadScreenBrightness());
		Snapshot();
	}

	private void Render()
	{
		if (modes != null)
		{
			modes.Select((int)LightingSettings.Mode, false);
		}
	}
}
