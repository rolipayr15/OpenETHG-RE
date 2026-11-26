using System.Collections.Generic;

public interface ILegacyMigrationStore
{
	bool TryLoad(string account, out List<LegacyMigrationRecord> records, out string error);

	bool TryRetain(string account, IEnumerable<LegacyMigrationSource> sources, out string error);

	bool TryMarkApplied(string account, IEnumerable<string> sourceIds, out string error);

	bool TryMarkProviderAccepted(string account, GameSaveData payload, long revision, out string error);
}
