using System;

public interface ILegacySnapshotTransport : IDisposable
{
	void Read(string expectedPlayerId, Action<string> done);
}
