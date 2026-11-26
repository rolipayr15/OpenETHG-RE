using System;

[Serializable]
public class NetworkProbeResult
{
	public string name;

	public string address;

	public string stage;

	public bool ok;

	public bool timedOut;

	public bool skipped;

	public long dnsMs;

	public long tcpMs;

	public long tlsMs;

	public long responseMs;

	public long totalMs;

	public string alpn;

	public string status;

	public string server;

	public string location;

	public string certIssuer;

	public string certErrors;

	public string note;

	public string error;

	public NetworkProbeResult Copy()
	{
		return null;
	}
}
