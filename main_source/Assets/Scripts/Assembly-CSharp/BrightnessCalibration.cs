using System;
using System.Runtime.CompilerServices;
using EyesAnalytics;
using UnityEngine;
using UnityEngine.UI;

public sealed class BrightnessCalibration : MonoBehaviour
{
	public const string SceneName = "BrightnessCalibration";

	public static readonly float[] SymbolSteps;

	public const int CalibratorVersion = 2;

	private static bool isActive;

	[CompilerGenerated]
	private static Action m_Closed;

	[SerializeField]
	private BrightnessCalibrationConfig config;

	[SerializeField]
	private Image[] symbols;

	[SerializeField]
	private Slider slider;

	[SerializeField]
	private Button doneButton;

	[SerializeField]
	private Text hint;

	[SerializeField]
	private Text ceilingHint;

	private GameplayInputContext.Lease lease;

	private LightingMode modeAtOpen;

	private float brightnessAtOpen;

	private float openedAt;

	private bool sliderMoved;

	private bool closing;

	public static bool IsActive => false;

	public static event Action Closed
	{
		[CompilerGenerated]
		add
		{
		}
		[CompilerGenerated]
		remove
		{
		}
	}

	public static void Show()
	{
	}

	public static bool AtCeiling(float stops, float maxStops)
	{
		return false;
	}

	public static string ReadoutText(float multiplier)
	{
		return null;
	}

	public static int VisibleCount(float multiplier)
	{
		return 0;
	}

	public static float StartStops(string config, float savedStops, float minStops, float maxStops)
	{
		return 0f;
	}

	public static (float, float) Bounds(float minStops, float maxStops)
	{
		return default;
	}

	public static CalibrationFacts Facts(float multiplier, bool sliderMoved, float openedAt, float now)
	{
		return default;
	}

	public static Color StripColor(float darkLuminance, float step, float multiplier)
	{
		return default;
	}

	private void Start()
	{
	}

	private void OnDestroy()
	{
	}

	private void Render()
	{
	}

	public void Done()
	{
	}
}
