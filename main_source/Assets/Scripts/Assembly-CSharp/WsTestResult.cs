using System;
using System.Collections.Generic;

[Serializable]
public class WsTestResult
{
	public string name;

	public string url;

	public int serverIdx;

	public bool connected;

	public long connectMs;

	public int sentPings;

	public int receivedPongs;

	public List<long> rtts;

	public int sentBinaryPings;

	public int receivedBinaryEchos;

	public List<long> binaryRtts;

	public string textPingMap;

	public List<long> textPingRtts;

	public string binaryPingMap;

	public List<long> binaryPingRtts;

	public ushort? closeCode;

	public string closeReason;

	public string error;

	public long openWaitMs;

	public string openState;

	public string openException;

	public string openProxy;

	public string openStages;

	public List<WsOpenAttempt> openAttempts;

	public List<string> dns;

	public List<NetworkProbeResult> probes;

	public WsBulkResult bulk;

	public double score;

	public float LossPercent => 0f;

	public float TextLossPercent => 0f;

	public float BinaryLossPercent => 0f;

	public int TextDroppedCount => 0;

	public int BinaryDroppedCount => 0;

	public long AvgRtt => 0L;

	public long MaxRtt => 0L;

	public long TextAvgRtt => 0L;

	public long TextMaxRtt => 0L;

	public long BinaryAvgRtt => 0L;

	public long BinaryMaxRtt => 0L;

	private static long Avg(List<long> values)
	{
		return 0L;
	}

	private static long Max(List<long> values)
	{
		return 0L;
	}
}
