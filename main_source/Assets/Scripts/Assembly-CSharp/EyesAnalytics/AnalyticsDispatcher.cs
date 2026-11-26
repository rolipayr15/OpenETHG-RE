using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace EyesAnalytics
{
	public static class AnalyticsDispatcher
	{
		public const long SchemaVersion = 2L;

		private static readonly List<IAnalyticsSink> sinks;

		private static SynchronizationContext mainThread;

		private static int mainThreadId;

		internal static Func<bool> Gate;

		internal static Func<int> Revision;

		internal static bool IsMainThread => false;

		internal static bool IsReady => false;

		private static bool DefaultGate()
		{
			return false;
		}

		private static int DefaultRevision()
		{
			return 0;
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		internal static void InitializeMainThread()
		{
		}

		internal static void Install(IAnalyticsSink sink)
		{
		}

		internal static void Reset()
		{
		}

		internal static void RunOnMainThread(Action action)
		{
		}

		internal static void RunWhenReady(Action action)
		{
		}

		internal static bool Send(EventDef def, AnalyticsContext.Snapshot context, Action<EventBuilder> fill)
		{
			return false;
		}

		internal static void SetUserProperty(string name, string value)
		{
		}

		internal static void ReportStoreTransaction(string transactionId)
		{
		}

		private static void Deliver(IAnalyticsSink sink, AnalyticsEvent analyticsEvent)
		{
		}
	}
}
