using UnityEngine;

namespace EyesAnalytics
{
	internal static class AnalyticsBootstrap
	{
		internal enum SinkChoice
		{
			None = 0,
			Log = 1,
			Firebase = 2
		}

		internal static SinkChoice Choose(bool isEditor, bool debugBuild, bool platformSupported)
		{
			return SinkChoice.None;
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Install()
		{
		}
	}
}
