using System;

public sealed class AndroidPointerBackend : BufferedPointerBackend, IDisposable
{
	public interface ITransport : IDisposable
	{
		double[] Poll();

		void SetActive(bool active);

		void RequestCapture(int generation);

		void ReleaseCapture(int generation);

		void ResetInput();
	}

	private readonly ITransport transport;

	private int polledFrame;

	private int nativeLossVersion;

	private int lossVersion;

	private bool active;

	private bool failed;

	private bool disposed;

	public override string Id => null;

	public override bool ReplacesKeyboard => false;

	public int NativeLossVersion => 0;

	public int SdkVersion { get; private set; }

	public bool NativeFocused { get; private set; }

	public bool Failed => false;

	public AndroidPointerBackend(ITransport transport)
	{
	}

	public void SetActive(bool value)
	{
	}

	public void Pump(int unityFrame, int screenWidth, int screenHeight)
	{
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
