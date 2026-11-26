using System;

public sealed class IOSPointerBackend : BufferedPointerBackend, IDisposable, IPointerDiagnostics, MouseManager.ICaptureReadiness
{
	public interface ITransport : IDisposable
	{
		double[] Poll();

		void SetActive(bool active);

		void RequestCapture(int generation);

		void ReleaseCapture(int generation);

		void ResetInput();
	}

	public interface IDiagnosticTransport
	{
		int[] Diagnostics();
	}

	public const int PacketLength = 26;

	public const int DiagnosticLength = 16;

	private static readonly string[] LossReasons;

	private readonly ITransport transport;

	private int polledFrame;

	private int nativeLossVersion;

	private int lossVersion;

	private bool active;

	private bool failed;

	private bool disposed;

	private bool lockReady;

	public override string Id => null;

	public override bool ReplacesKeyboard => false;

	public int OSVersionMajor { get; private set; }

	public bool NativeFocused { get; private set; }

	public int NativeLossVersion => 0;

	public bool Failed => false;

	public bool ReportsLockReadiness => false;

	public bool CaptureLockReady => false;

	public bool LockNeedsClick { get; }

	public bool LooksWithoutLock { get; }

	protected override bool KeepsUncapturedDelta => false;

	public IOSPointerBackend(ITransport transport, bool lockNeedsClick = false, bool looksWithoutLock = false)
	{
	}

	public void SetActive(bool value)
	{
	}

	public void Pump(int unityFrame, int screenWidth, int screenHeight)
	{
	}

	private static bool ValidPacket(double[] data)
	{
		return false;
	}

	private static bool IntegerInRange(double value, int maximum)
	{
		return false;
	}

	protected override void RequestPlatformCapture(int generation)
	{
	}

	protected override void ReleasePlatformCapture(int generation)
	{
	}

	public override void SetPointerVisible(bool visible)
	{
	}

	public string DescribeNativeState()
	{
		return null;
	}

	public override void ResetInput()
	{
	}

	private void FailClosed()
	{
	}

	public void Dispose()
	{
	}
}
