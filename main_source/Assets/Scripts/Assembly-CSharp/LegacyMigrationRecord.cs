public sealed class LegacyMigrationRecord
{
	public string sourceId;

	public LegacyMigrationSource source;

	public bool appliedLocally;

	public long providerAcceptedRevision = -1L;
}
