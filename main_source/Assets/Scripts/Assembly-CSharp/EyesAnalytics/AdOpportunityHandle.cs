namespace EyesAnalytics
{
	public readonly struct AdOpportunityHandle
	{
		private readonly AdOpportunity opportunity;

		public string Id => null;

		internal AdOpportunityHandle(AdOpportunity opportunity)
		{
			this.opportunity = null;
		}

		public void MarkShown(AdUnitRole? unitRole = null)
		{
		}

		public void MarkRequested(string adUnitId)
		{
		}

		public void MarkClicked()
		{
		}

		public void MarkRewarded()
		{
		}

		public void MarkLegacyDisable(string trigger)
		{
		}

		public void Resolve(AdOutcome outcome)
		{
		}

		public void ResolveNotReady(AdLoadState loadState, long loadAgeMs)
		{
		}
	}
}
