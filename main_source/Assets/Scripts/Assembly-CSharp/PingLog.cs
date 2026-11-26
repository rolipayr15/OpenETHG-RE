using System.Collections.Generic;

public sealed class PingLog
{
	public const string Legend = "Maps: + reply, L late reply (after timeout), x no reply, - not sent";

	private readonly char[] marks;

	private readonly long[] sentAt;

	private readonly long[] rtts;

	public string Marks => null;

	public List<long> Rtts => null;

	public PingLog(int count)
	{
	}

	public void Sent(int id, long sentAtMs)
	{
	}

	public void Replied(int id, long nowMs, long echoedSentAtMs, bool late)
	{
	}
}
