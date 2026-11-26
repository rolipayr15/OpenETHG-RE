using System.Collections.Generic;

public sealed class LegacySnapshotReadResult
{
	public string account;

	public bool complete;

	public bool ownershipVerified;

	public List<LegacySnapshotPayload> snapshots = new List<LegacySnapshotPayload>();

	public string error;
}
