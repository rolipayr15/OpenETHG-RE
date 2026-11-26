using System;

[Serializable]
public class WsOpenAttempt
{
	public string name;

	public bool connected;

	public long elapsedMs;

	public string state;

	public string proxy;

	public string stages;

	public string error;
}
