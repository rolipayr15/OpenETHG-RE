namespace EyesAnalytics
{
	public static class Tutorial
	{
		public enum Step
		{
			Movement = 0,
			Eye = 1,
			Map = 2,
			MoneyBag = 3,
			Cupboard = 4,
			Journal = 5
		}

		public const long Version = 1L;

		private static Step[] ActiveSteps => null;

		public static bool Begin()
		{
			return false;
		}

		public static void StepCompleted(Step step, float timeSinceLevelStart)
		{
		}

		public static bool Completed(float timeSinceLevelStart, int attemptsTaken)
		{
			return false;
		}
	}
}
