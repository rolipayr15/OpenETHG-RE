public interface ISaveGameProgressRecoveryStore
{
	bool TryLoad(string account, out RecoveryRecord record, out string error);

	bool TrySave(string account, GameSaveData data, long revision, out string error);

	bool TryLoadOwner(out string owner, out string error);

	bool TrySaveOwner(string owner, out string error);
}
