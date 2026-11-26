namespace EyesAnalytics
{
	public static class Lighting
	{
		public static string Bucket(float multiplier)
		{
			return null;
		}

		public static void PublishState(LightingMode mode, float brightnessMultiplier)
		{
		}

		public static bool Commit(LightingSettingsSource source, LightingMode modeFrom, LightingMode modeTo, float brightnessFrom, float brightnessTo, float screenBrightness, CalibrationFacts? calibration = null)
		{
			return false;
		}

		public static float ReadScreenBrightness()
		{
			return 0f;
		}
	}
}
