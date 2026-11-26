using System;

public interface ILegacySnapshotReader : IDisposable
{
	void Read(string account, Action<LegacySnapshotReadResult> done);

	void Tick();
}
