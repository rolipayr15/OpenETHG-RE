using System;
using System.Collections.Generic;

[Serializable]
public class WsBulkResult
{
	public bool skipped;

	public bool bypassedProxy;

	public bool connected;

	public int framesSent;

	public long bytesSent;

	public int echoesReceived;

	public long bytesEchoed;

	public long elapsedMs;

	public ushort? closeCode;

	public string closeReason;

	public string error;

	public string frameMap;

	public List<long> frameRtts;
}
