namespace EyesAnalytics
{
	public static class Currency
	{
		public static void SendCurrencyEvent(CurrencyFlowType flowType, CurrencyType currencyType, float amount, CurrencyItemType itemType, CurrencyItemId itemId)
		{
		}

		public static void SendCoinTransaction(int balanceBefore, int balanceAfter, CurrencyItemType itemType, CurrencyItemId itemId)
		{
		}

		private static void Send(bool isSpend, CurrencyType currencyType, double amount, CurrencyItemType itemType, CurrencyItemId itemId, int? balanceBefore, int? balanceAfter)
		{
		}

		public static void ReportCoinBalanceAdjustment(int balanceBefore, int balanceAfter, bool fromSnapshot)
		{
		}

		public static void ReportCoinBalanceSnapshot(int balance, bool isStored, bool playPassDefault)
		{
		}

		public static CurrencyItemId ItemIdFromGameMode(Mode gameMode)
		{
			return CurrencyItemId.WeeklySubscription;
		}

		public static CurrencyItemId ItemIdFromInAppName(string name)
		{
			return CurrencyItemId.WeeklySubscription;
		}
	}
}
