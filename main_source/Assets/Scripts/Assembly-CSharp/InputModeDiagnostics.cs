using System.Diagnostics;

public static class InputModeDiagnostics
{
	[Conditional("UNITY_EDITOR")]
	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("EYES_INPUT_DIAGNOSTICS")]
	public static void Record(string phase, ControlModeManager controls, ControlMode? requested = null, bool includeCaller = false, string detail = null)
	{
	}

	private static string CallerMethods()
	{
		return null;
	}
}
