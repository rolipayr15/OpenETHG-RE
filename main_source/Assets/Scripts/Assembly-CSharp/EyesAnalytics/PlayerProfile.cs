using System.Collections.Generic;

namespace EyesAnalytics
{
	public static class PlayerProfile
	{
		private const string PayerKey = "ANALYTICS_IS_PAYER";

		internal const string PlayPassKey = "ANALYTICS_HAD_PLAY_PASS";

		public static void SetHasPlayPass(bool value)
		{
		}

		public static void SetHasRemoveAds(bool value)
		{
		}

		public static void SetLevel(int level)
		{
		}

		public static void MarkPayer()
		{
		}

		public static void PublishOwnedProducts(IEnumerable<string> productIds, int sessionCounter = 0)
		{
		}

		private static void ReportPlayPassChange(bool playPass, bool playedBefore)
		{
		}

		private static bool PlayedBefore(int sessionCounter)
		{
			return false;
		}

		internal static bool IsPaidProduct(string productId)
		{
			return false;
		}

		public static void PublishStored(int currentLevel)
		{
		}

		internal static string LevelBucket(int level)
		{
			return null;
		}
	}
}
