using UnityEngine.Purchasing;

namespace EyesAnalytics
{
	public static class IAP
	{
		public sealed class PurchaseContext
		{
			public string ItemId { get; }

			public CartType CartType { get; }

			public bool IsUpsell { get; }

			public PurchaseContext(string itemId, CartType cartType, bool isUpsell = false)
			{
			}
		}

		internal static void ReportConfirmedPurchase(Product product, ConfirmedOrder order, PurchaseContext context, string purchaseOrigin)
		{
		}

		internal static void ReportPurchase(string productId, PurchaseContext context, string purchaseOrigin)
		{
		}

		public static ItemType ItemIdToType(string itemId)
		{
			return ItemType.Chapter;
		}
	}
}
