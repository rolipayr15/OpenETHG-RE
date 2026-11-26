using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EyesAnalytics;
using UnityEngine.Purchasing;

public abstract class PurchaserMobiles : PurchaserBase
{
	private class PendingPurchaseContext
	{
		public Action<Product, Order> OnSuccess;

		public Action<string, string> OnFailure;

		public IAP.PurchaseContext AnalyticsContext;

		public object OrderKey;
	}

	[StructLayout((LayoutKind)3)]
	[CompilerGenerated]
	private struct _003CInitializeIAP_003Ed__17 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncVoidMethodBuilder _003C_003Et__builder;

		public PurchaserMobiles _003C_003E4__this;

		private TaskAwaiter _003C_003Eu__1;

		private void MoveNext()
		{
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[CompilerGenerated]
	private sealed class _003CTryInitializePurchasing_003Ed__16 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PurchaserMobiles _003C_003E4__this;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		[DebuggerHidden]
		public _003CTryInitializePurchasing_003Ed__16(int _003C_003E1__state)
		{
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
		}
	}

	private CatalogProvider catalog;

	private StoreController m_StoreController;

	private readonly Dictionary<string, Queue<PendingPurchaseContext>> pendingPurchaseCallbacks;

	private static readonly HashSet<object> reportedAnalyticsConfirmations;

	private readonly HashSet<object> recoveredPurchases;

	private bool wasRestoreButtonUsed;

	private static object PurchaseKey(Order order)
	{
		return null;
	}

	public override void Init()
	{
	}

	private PendingPurchaseContext RegisterPendingPurchaseCallback(string productId, Action<Product, Order> onSuccess, Action<string, string> onFailure, IAP.PurchaseContext analyticsContext)
	{
		return null;
	}

	private void BindPendingPurchaseContext(string productId, Order order)
	{
	}

	private PendingPurchaseContext DequeuePendingPurchaseCallback(string productId, Order order = null)
	{
		return null;
	}

	private bool IsLivePurchase(Order order)
	{
		return false;
	}

	private void MarkRecoveredPurchase(PendingOrder order)
	{
	}

	private void RemovePendingPurchaseCallback(string productId, PendingPurchaseContext context)
	{
	}

	private void InvokeSuccessCallback(PendingPurchaseContext context, Product product, Order order)
	{
	}

	private void InvokeFailureCallback(string productId, string reason, Order order = null)
	{
	}

	[IteratorStateMachine(typeof(_003CTryInitializePurchasing_003Ed__16))]
	private IEnumerator TryInitializePurchasing()
	{
		return null;
	}

	[AsyncStateMachine(typeof(_003CInitializeIAP_003Ed__17))]
	private void InitializeIAP()
	{
	}

	private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription description)
	{
	}

	private void OnProductsFetchFailed(ProductFetchFailed failed)
	{
	}

	private void OnStoreDisconnected(StoreConnectionFailureDescription description)
	{
	}

	private void OnPurchaseFailed(FailedOrder order)
	{
	}

	private void OnProductsFetched(List<Product> products)
	{
	}

	private void OnPurchasesFetched(Orders orders)
	{
	}

	public override bool IsInitialized()
	{
		return false;
	}

	public override void BuyProductID(string productId, Action<Product, Order> onSuccess = null, Action<string, string> onFailure = null)
	{
	}

	public override void BuyProductID(string productId, IAP.PurchaseContext analyticsContext, Action<Product, Order> onSuccess = null, Action<string, string> onFailure = null)
	{
	}

	private void OnPurchasePending(PendingOrder order)
	{
	}

	private void OnCheckEntitlement(Entitlement entitlement)
	{
	}

	private void OnPurchaseConfirmed(Order order)
	{
	}

	public override void RestorePurchasesButton()
	{
	}

	private void RestorePurchases()
	{
	}

	private void OnTransactionsRestored(bool success, string error)
	{
	}

	public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
	{
	}

	private void OnApplicationFocus(bool focusStatus)
	{
	}

	private void OnApplicationPause(bool pauseStatus)
	{
	}

	private void Unlock(string productId, bool status)
	{
	}

	public override string GetProductPrice(string productId)
	{
		return null;
	}

	public float GetProductPriceFloat(string productId)
	{
		return 0f;
	}

	public string GetUserCurrency(string productId)
	{
		return null;
	}
}
