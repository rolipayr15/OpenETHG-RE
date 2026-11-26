using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BestHTTP;

public static class NetworkProbe
{
	public sealed class Spec
	{
		public string Name;

		public bool PlatformTls;

		public bool NoTls;

		public string[] Alpn;

		public string Path;

		public bool Upgrade;

		public bool SplitClientHello;

		public bool Ipv6;

		public int Port;
	}

	private sealed class Run
	{
		private const int MaxHeaderLines = 40;

		private const int MaxHeaderValue = 120;

		private readonly Spec spec;

		private readonly string host;

		private readonly Stopwatch clock;

		private readonly NetworkProbeResult result;

		private readonly object gate;

		private TcpClient client;

		private bool abandoned;

		private long Elapsed => 0L;

		public Run(Spec spec, string host)
		{
		}

		private void Update(Action<NetworkProbeResult> change)
		{
		}

		public void Execute(HTTPRequest request)
		{
		}

		private TcpClient Attach(TcpClient tcp)
		{
			return null;
		}

		public void Abandon()
		{
		}

		public NetworkProbeResult Snapshot(bool timedOut)
		{
			return null;
		}

		private Stream BestHttpHandshake(Stream stream, HTTPRequest request)
		{
			return null;
		}

		private Stream PlatformHandshake(Stream stream)
		{
			return null;
		}

		private string BuildRequest()
		{
			return null;
		}

		private static void ReadHeaders(Stream stream, out string server, out string location)
		{
			server = null;
			location = null;
		}

		private static string ReadLine(Stream stream)
		{
			return null;
		}
	}

	private sealed class SplitHelloStream : Stream
	{
		private const int PauseBetweenPartsMs = 30;

		private readonly Stream inner;

		private readonly byte[] hostName;

		private readonly Action<int> splitAt;

		private bool split;

		public override bool CanRead => false;

		public override bool CanWrite => false;

		public override bool CanSeek => false;

		public override long Length => 0L;

		public override long Position
		{
			get
			{
				return 0L;
			}
			set
			{
			}
		}

		public SplitHelloStream(Stream inner, string host, Action<int> splitAt)
		{
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
		}

		private static int IndexOf(byte[] buffer, int offset, int count, byte[] pattern)
		{
			return 0;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return 0;
		}

		public override void Flush()
		{
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return 0L;
		}

		public override void SetLength(long value)
		{
		}

		protected override void Dispose(bool disposing)
		{
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass6_0
	{
		public Run run;

		public HTTPRequest request;

		internal void _003CRunAsync_003Eb__0()
		{
		}
	}

	[StructLayout((LayoutKind)3)]
	[CompilerGenerated]
	private struct _003CResolveAsync_003Ed__7 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncTaskMethodBuilder<string> _003C_003Et__builder;

		public string host;

		public int timeoutMs;

		public CancellationToken ct;

		private Stopwatch _003Cclock_003E5__2;

		private Task<IPAddress[]> _003Clookup_003E5__3;

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
	private struct _003CRunAsync_003Ed__6 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncTaskMethodBuilder<NetworkProbeResult> _003C_003Et__builder;

		public Spec spec;

		public string host;

		public int timeoutMs;

		public CancellationToken ct;

		private _003C_003Ec__DisplayClass6_0 _003C_003E8__1;

		private Task _003Cworker_003E5__2;

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

	private static readonly string[] RequestAlpn;

	private static readonly string[] WebSocketAlpn;

	public static readonly Spec[] RouteProbes;

	public static readonly Spec[] BackendProbes;

	public static readonly (string host, Spec spec)[] ReferenceProbes;

	[AsyncStateMachine(typeof(_003CRunAsync_003Ed__6))]
	public static Task<NetworkProbeResult> RunAsync(Spec spec, string host, int timeoutMs, CancellationToken ct)
	{
		return null;
	}

	[AsyncStateMachine(typeof(_003CResolveAsync_003Ed__7))]
	public static Task<string> ResolveAsync(string host, int timeoutMs, CancellationToken ct)
	{
		return null;
	}
}
