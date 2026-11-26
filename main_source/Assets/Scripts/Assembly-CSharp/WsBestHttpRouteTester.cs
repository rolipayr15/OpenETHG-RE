using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BestHTTP;
using BestHTTP.WebSocket;
using UnityEngine;

public class WsBestHttpRouteTester : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass17_0
	{
		public Action<float> progress;

		public float baselineShare;

		internal void _003CCheckAndRankServerConnection_003Eb__1(float p)
		{
		}

		internal void _003CCheckAndRankServerConnection_003Eb__0(float p)
		{
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass26_0
	{
		public string[] backendHosts;

		public Action<float> progress;

		public int done;

		public int steps;

		public CancellationToken ct;

		public WsBestHttpRouteTester _003C_003E4__this;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass27_0
	{
		public bool bypassProxy;

		public TaskCompletionSource<bool> tcsOpen;

		public bool finished;

		public WsOpenAttempt attempt;

		internal void _003COpenOnceAsync_003Eb__0(WebSocket w, HTTPRequest req)
		{
		}

		internal void _003COpenOnceAsync_003Eb__1(WebSocket w)
		{
		}

		internal void _003COpenOnceAsync_003Eb__2(WebSocket w, string err)
		{
		}

		internal void _003COpenOnceAsync_003Eb__3(WebSocket w, ushort code, string message)
		{
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass28_0
	{
		public bool bypassProxy;

		public WsBulkResult res;

		public TaskCompletionSource<bool> tcsOpen;

		public bool finished;

		public PingLog frameLog;

		public WebSocket ws;

		internal void _003CRunBulkAsync_003Eb__0(WebSocket w, HTTPRequest req)
		{
		}

		internal void _003CRunBulkAsync_003Eb__1(WebSocket w)
		{
		}

		internal void _003CRunBulkAsync_003Eb__2(WebSocket w, string err)
		{
		}

		internal void _003CRunBulkAsync_003Eb__3(WebSocket w, ushort code, string message)
		{
		}

		internal void _003CRunBulkAsync_003Eb__4(WebSocket w, byte[] msg)
		{
		}

		internal bool _003CRunBulkAsync_003Eb__5()
		{
			return false;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass30_0
	{
		public bool logProgress;

		public string name;

		public WsTestResult res;

		public PingLog textLog;

		public PingLog binaryLog;

		public long connectStart;

		public TaskCompletionSource<bool> tcsOpen;

		public TaskCompletionSource<bool> tcsClosed;

		public Action<int> tasksCompleted;

		public int probesPerRoute;

		public Dictionary<int, long> pendingText;

		public int pingCount;

		public int probesReceivedOrTimedOut;

		public Dictionary<int, long> pendingBinary;

		public WebSocket ws;

		internal void _003CRunSingleAsync_003Eb__3(WebSocket w)
		{
		}

		internal void _003CRunSingleAsync_003Eb__4(WebSocket w, string err)
		{
		}

		internal void _003CRunSingleAsync_003Eb__5(WebSocket w, ushort code, string message)
		{
		}

		internal void _003CRunSingleAsync_003Eb__6(WebSocket w, string msg)
		{
		}

		internal void _003CRunSingleAsync_003Eb__7(WebSocket w, byte[] msg)
		{
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass30_1
	{
		public int pingId;

		public _003C_003Ec__DisplayClass30_0 CS_0024_003C_003E8__locals1;

		internal bool _003CRunSingleAsync_003Eb__8()
		{
			return false;
		}

		internal bool _003CRunSingleAsync_003Eb__9()
		{
			return false;
		}
	}

	[StructLayout((LayoutKind)3)]
	[CompilerGenerated]
	private struct _003CCheckAndRankServerConnection_003Ed__17 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncTaskMethodBuilder<List<WsTestResult>> _003C_003Et__builder;

		public Action<float> progress;

		public WsBestHttpRouteTester _003C_003E4__this;

		public bool extended;

		public int pingCount;

		public int pingEveryMs;

		public CancellationToken ct;

		private _003C_003Ec__DisplayClass17_0 _003C_003E8__1;

		private string[] _003CchatUrls_003E5__2;

		private List<WsTestResult> _003Cresults_003E5__3;

		private TaskAwaiter<List<WsTestResult>> _003C_003Eu__1;

		private TaskAwaiter _003C_003Eu__2;

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

	[StructLayout((LayoutKind)3)]
	[CompilerGenerated]
	private struct _003COpenOnceAsync_003Ed__27 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncTaskMethodBuilder<WsOpenAttempt> _003C_003Et__builder;

		public bool bypassProxy;

		public string name;

		public string url;

		public CancellationToken ct;

		private _003C_003Ec__DisplayClass27_0 _003C_003E8__1;

		private WebSocket _003Cws_003E5__2;

		private Stopwatch _003Cclock_003E5__3;

		private WsOpenAttempt _003C_003E7__wrap3;

		private TaskAwaiter<bool> _003C_003Eu__1;

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

	[StructLayout((LayoutKind)3)]
	[CompilerGenerated]
	private struct _003CRunAllParallelAsync_003Ed__29 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncTaskMethodBuilder<List<WsTestResult>> _003C_003Et__builder;

		public (string name, string url)[] routes;

		public Action<float> progress;

		public bool? logProgressOverride;

		public WsBestHttpRouteTester _003C_003E4__this;

		public int pingCount;

		public int pingEveryMs;

		public int pongTimeoutMs;

		public CancellationToken ct;

		private TaskAwaiter<WsTestResult[]> _003C_003Eu__1;

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

	[StructLayout((LayoutKind)3)]
	[CompilerGenerated]
	private struct _003CRunBulkAsync_003Ed__28 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncTaskMethodBuilder<WsBulkResult> _003C_003Et__builder;

		public bool bypassProxy;

		public string url;

		public CancellationToken ct;

		private _003C_003Ec__DisplayClass28_0 _003C_003E8__1;

		private Stopwatch _003Cclock_003E5__2;

		private TaskAwaiter<bool> _003C_003Eu__1;

		private TaskAwaiter<Task> _003C_003Eu__2;

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

	[StructLayout((LayoutKind)3)]
	[CompilerGenerated]
	private struct _003CRunExtendedAsync_003Ed__26 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncTaskMethodBuilder _003C_003Et__builder;

		public string[] backendHosts;

		public Action<float> progress;

		public CancellationToken ct;

		public WsBestHttpRouteTester _003C_003E4__this;

		public string[] hosts;

		public List<WsTestResult> results;

		private _003C_003Ec__DisplayClass26_0 _003C_003E8__1;

		private Task<NetworkProbeResult>[] _003Creferences_003E5__2;

		private TaskAwaiter _003C_003Eu__1;

		private TaskAwaiter<NetworkProbeResult[]> _003C_003Eu__2;

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

	[StructLayout((LayoutKind)3)]
	[CompilerGenerated]
	private struct _003CRunSingleAsync_003Ed__30 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncTaskMethodBuilder<WsTestResult> _003C_003Et__builder;

		public bool logProgress;

		public string name;

		public Action<int> tasksCompleted;

		public int pingCount;

		public string url;

		public int serverIdx;

		public int pingEveryMs;

		public int pongTimeoutMs;

		public CancellationToken ct;

		private _003C_003Ec__DisplayClass30_0 _003C_003E8__1;

		private _003C_003Ec__DisplayClass30_1 _003C_003E8__2;

		private TaskAwaiter<bool> _003C_003Eu__1;

		private int _003Ci_003E5__2;

		private TaskAwaiter<Task[]> _003C_003Eu__2;

		private TaskAwaiter _003C_003Eu__3;

		private TaskAwaiter<Task> _003C_003Eu__4;

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

	[StructLayout((LayoutKind)3)]
	[CompilerGenerated]
	private struct _003CWaitBoolWithTimeout_003Ed__33 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncTaskMethodBuilder<bool> _003C_003Et__builder;

		public Task<bool> task;

		public int timeoutMs;

		public CancellationToken ct;

		private TaskAwaiter<Task> _003C_003Eu__1;

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

	[StructLayout((LayoutKind)3)]
	[CompilerGenerated]
	private struct _003CWaitUntil_003Ed__34 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncTaskMethodBuilder _003C_003Et__builder;

		public CancellationToken ct;

		public Func<bool> predicate;

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

	private static readonly byte[] BinaryEchoPrefixByPingId;

	private const byte BinaryEchoMagic1 = 83;

	private const byte BinaryEchoMagic2 = 82;

	private const byte BinaryEchoMagic3 = 84;

	private const int BinaryEchoPayloadLength = 16;

	private const int OpenTimeoutMs = 8000;

	private const int ProbeTimeoutMs = 8000;

	private const int BulkFrameCount = 16;

	private const int BulkFrameBytes = 4096;

	private const int BulkEchoTimeoutMs = 10000;

	private const int BulkPingIdOffset = 1000;

	private const float BaselineProgressShare = 0.6f;

	[SerializeField]
	private bool logRouteTestProgress;

	public string RunInfo { get; private set; }

	[AsyncStateMachine(typeof(_003CCheckAndRankServerConnection_003Ed__17))]
	public Task<List<WsTestResult>> CheckAndRankServerConnection(int pingCount, int pingEveryMs, Action<float> progress = null, CancellationToken ct = default(CancellationToken), bool extended = false)
	{
		return null;
	}

	private static string RouteUrl(string host)
	{
		return null;
	}

	private static string DescribeRun()
	{
		return null;
	}

	private static string DescribeProxyDetection(string host)
	{
		return null;
	}

	private static void BypassProxy(HTTPRequest request)
	{
	}

	private static string DescribeRequestProxy(HTTPRequest request)
	{
		return null;
	}

	private static string DescribeStages(HTTPRequest request)
	{
		return null;
	}

	[AsyncStateMachine(typeof(_003CRunExtendedAsync_003Ed__26))]
	private Task RunExtendedAsync(string[] hosts, string[] backendHosts, List<WsTestResult> results, Action<float> progress, CancellationToken ct)
	{
		return null;
	}

	[AsyncStateMachine(typeof(_003COpenOnceAsync_003Ed__27))]
	private Task<WsOpenAttempt> OpenOnceAsync(string name, string url, bool bypassProxy, CancellationToken ct)
	{
		return null;
	}

	[AsyncStateMachine(typeof(_003CRunBulkAsync_003Ed__28))]
	private Task<WsBulkResult> RunBulkAsync(string url, bool bypassProxy, CancellationToken ct)
	{
		return null;
	}

	[AsyncStateMachine(typeof(_003CRunAllParallelAsync_003Ed__29))]
	public Task<List<WsTestResult>> RunAllParallelAsync((string name, string url)[] routes, int pingCount = 10, int pingEveryMs = 500, int pongTimeoutMs = 1500, CancellationToken ct = default(CancellationToken), Action<float> progress = null, bool? logProgressOverride = null)
	{
		return null;
	}

	[AsyncStateMachine(typeof(_003CRunSingleAsync_003Ed__30))]
	public Task<WsTestResult> RunSingleAsync(string name, string url, int serverIdx, int pingCount = 10, int pingEveryMs = 500, int pongTimeoutMs = 1500, CancellationToken ct = default(CancellationToken), Action<int> tasksCompleted = null, bool logProgress = false)
	{
		return null;
	}

	private static long NowMs()
	{
		return 0L;
	}

	private static void RecordOpenFailure(WsTestResult res, WebSocket ws, long waitedMs)
	{
	}

	[AsyncStateMachine(typeof(_003CWaitBoolWithTimeout_003Ed__33))]
	private static Task<bool> WaitBoolWithTimeout(Task<bool> task, int timeoutMs, CancellationToken ct)
	{
		return null;
	}

	[AsyncStateMachine(typeof(_003CWaitUntil_003Ed__34))]
	private static Task WaitUntil(Func<bool> predicate, CancellationToken ct)
	{
		return null;
	}

	private static void SafeClose(WebSocket ws)
	{
	}

	private static int ExtractInt(string s, string key)
	{
		return 0;
	}

	private static long ExtractLong(string s, string key)
	{
		return 0L;
	}

	private static byte[] BuildBinaryPingPayload(int pingId, long ts, int length = 16)
	{
		return null;
	}

	private static bool TryExtractBinaryEcho(byte[] payload, out int pingId, out long ts)
	{
		pingId = default;
		ts = default;
		return false;
	}

	private static byte GetBinaryEchoPrefixForPingId(int pingId)
	{
		return 0;
	}
}
