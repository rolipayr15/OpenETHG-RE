namespace EyesAnalytics
{
	internal sealed class PlayerPrefsAttemptStore : IAttemptStore
	{
		private const string Key = "ANALYTICS_PENDING_ATTEMPT";

		public string Load()
		{
			return null;
		}

		public void Save(string json)
		{
		}

		public void Clear()
		{
		}
	}
}
