using System;
using System.Collections.Generic;

public sealed class SaveGameProgressCoordinator : IDisposable
{
	private const string Unassigned = "unassigned";

	private const string Mixed = "mixed";

	private const double RetrySeconds = 30.0;

	private const double TimeoutSeconds = 30.0;

	private readonly ISaveGameProgressCloud cloud;

	private readonly ISaveGameProgressRecoveryStore recovery;

	private readonly Func<GameSaveData> capture;

	private readonly Action<GameSaveData> apply;

	private readonly Func<double> clock;

	private readonly Action<string> log;

	private readonly ILegacySnapshotReader legacyReader;

	private readonly ILegacyMigrationStore legacyStore;

	private double nextLegacyRead;

	private Action legacyTimeout;

	private int cloudDataRevision;

	private GameSaveData observed;

	private GameSaveData pending;

	private string account;

	private string localOwner;

	private string lastError;

	private long revision;

	private int generation;

	private int operation;

	private double deadline;

	private double retryAt;

	private bool started;

	private bool disposed;

	private bool busy;

	private bool needsRead;

	private bool applying;

	private bool diskBlocked;

	private bool accountChangedDuringDiskFailure;

	public SaveGameProgressCoordinator(ISaveGameProgressCloud cloud, ISaveGameProgressRecoveryStore recovery, Func<GameSaveData> capture, Action<GameSaveData> apply, Func<double> clock, Action<string> log, ILegacySnapshotReader legacyReader = null, ILegacyMigrationStore legacyStore = null)
	{
		observed = SaveGameProgressData.Empty();
		pending = SaveGameProgressData.Empty();
		needsRead = true;
		this.cloud = cloud ?? throw new ArgumentNullException("cloud");
		this.recovery = recovery ?? throw new ArgumentNullException("recovery");
		this.capture = capture ?? throw new ArgumentNullException("capture");
		this.apply = apply ?? throw new ArgumentNullException("apply");
		this.clock = clock ?? throw new ArgumentNullException("clock");
		this.log = log ?? delegate
		{
		};
		if ((legacyReader == null) != (legacyStore == null))
		{
			throw new ArgumentException("Legacy reader and store must be supplied together.");
		}
		this.legacyReader = legacyReader;
		this.legacyStore = legacyStore;
	}

	public void Start()
	{
		if (started || disposed)
		{
			return;
		}
		started = true;
		cloud.AccountChanged += OnAccountChanged;
		cloud.DataChanged += OnDataChanged;
		observed = SaveGameProgressData.Merge(capture());
		if (!recovery.TryLoadOwner(out localOwner, out string error))
		{
			BlockDisk(error);
		}
		else if (SaveRecovery(Unassigned, observed, 0L))
		{
			SwitchAccount(cloud.Account, true);
			Tick();
		}
	}

	public void CaptureChanges()
	{
		if (!started || disposed || applying || diskBlocked)
		{
			return;
		}
		if (cloud.Account != account)
		{
			SwitchAccount(cloud.Account, true);
			return;
		}
		GameSaveData current = SaveGameProgressData.Merge(capture());
		GameSaveData difference = SaveGameProgressData.Difference(current, observed);
		observed = current;
		if (account == null)
		{
			SaveRecovery(Unassigned, current, 0L);
		}
		else if (!SaveGameProgressData.ContainsAll(SaveGameProgressData.Empty(), difference))
		{
			pending = SaveGameProgressData.Merge(pending, difference);
			revision++;
			SaveRecovery(account, pending, revision);
			needsRead = true;
		}
	}

	public void RequestSync()
	{
		if (!applying && !disposed)
		{
			CaptureChanges();
			needsRead = true;
			Tick();
		}
	}

	public void Tick()
	{
		if (!started || disposed)
		{
			return;
		}
		cloud.Tick();
		legacyReader?.Tick();
		if (diskBlocked)
		{
			if (cloud.Account != account)
			{
				accountChangedDuringDiskFailure = true;
			}
			if (clock() < retryAt || !TryResumeRecovery())
			{
				return;
			}
		}
		if (cloud.Account != account)
		{
			SwitchAccount(cloud.Account, true);
		}
		if (diskBlocked)
		{
			return;
		}
		if (busy)
		{
			if (deadline <= clock())
			{
				Action timeout = legacyTimeout;
				busy = false;
				legacyTimeout = null;
				operation++;
				if (timeout == null)
				{
					Retry("Synchronization timed out; local progress is retained.");
				}
				else
				{
					timeout();
				}
			}
			if (busy)
			{
				return;
			}
		}
		if (clock() < retryAt || !cloud.IsAvailable)
		{
			return;
		}
		if (account == null)
		{
			BeginSync(delegate
			{
				retryAt = clock() + RetrySeconds;
			});
			return;
		}
		if (legacyReader != null && nextLegacyRead <= clock())
		{
			needsRead = true;
		}
		if (needsRead)
		{
			ReadThenMerge();
		}
	}

	private void SwitchAccount(string nextAccount, bool captureBoundary)
	{
		busy = false;
		needsRead = true;
		retryAt = 0.0;
		generation++;
		operation++;
		if (captureBoundary)
		{
			observed = SaveGameProgressData.Merge(capture());
			if (!SaveRecovery(Unassigned, observed, 0L))
			{
				return;
			}
		}
		account = nextAccount;
		nextLegacyRead = 0.0;
		legacyTimeout = null;
		pending = SaveGameProgressData.Empty();
		revision = 0L;
		if (account == null)
		{
			return;
		}
		string error;
		if (!recovery.TryLoad(account, out RecoveryRecord record, out error))
		{
			BlockDisk(error);
			return;
		}
		if (record != null)
		{
			pending = record.data;
			revision = record.revision;
		}
		if (localOwner != account)
		{
			if ((string.IsNullOrEmpty(localOwner) || localOwner == Unassigned) && SaveGameProgressData.ContainsAll(SaveGameProgressData.Empty(), observed))
			{
				// Nothing has been played locally yet, so this device's cache now belongs to the account.
				if (!recovery.TrySaveOwner(account, out error))
				{
					BlockDisk(error);
					return;
				}
				localOwner = account;
			}
			else if (localOwner != Mixed && localOwner != Unassigned)
			{
				localOwner = string.IsNullOrEmpty(localOwner) ? Unassigned : Mixed;
				if (!recovery.TrySaveOwner(localOwner, out error))
				{
					BlockDisk(error);
					return;
				}
			}
		}
		if (localOwner == account)
		{
			pending = SaveGameProgressData.Merge(pending, observed);
		}
		SaveRecovery(account, pending, revision);
	}

	private void OnAccountChanged()
	{
		if (disposed || !started)
		{
			return;
		}
		if (diskBlocked)
		{
			if (cloud.Account != account)
			{
				accountChangedDuringDiskFailure = true;
			}
			return;
		}
		if (cloud.Account != account)
		{
			SwitchAccount(cloud.Account, true);
		}
		needsRead = true;
	}

	private void OnDataChanged()
	{
		if (!disposed)
		{
			needsRead = true;
			cloudDataRevision++;
		}
	}

	private void ReadThenMerge()
	{
		needsRead = false;
		BeginSync(delegate(bool success)
		{
			if (!success)
			{
				Retry("Cloud read failed; local progress is retained.");
				return;
			}
			int readDataRevision = cloudDataRevision;
			string error;
			GameSaveData remote = null;
			GameSaveData cache = null;
			if (!cloud.TryRead(out string cloudJson, out string cachedJson, out bool cacheIsAccountOwned, out error) || !SaveGameProgressData.TryParse(cloudJson, out remote, out error) || !SaveGameProgressData.TryParse(cachedJson, out cache, out error))
			{
				Retry(error);
				return;
			}
			if (!cacheIsAccountOwned)
			{
				// A cache we cannot prove is this account's goes to the unassigned record, not into the cloud.
				if (!SaveRecovery(Unassigned, cache, 0L))
				{
					return;
				}
				cache = SaveGameProgressData.Empty();
			}
			if (legacyReader != null && nextLegacyRead <= clock())
			{
				ReadLegacyThenMerge(remote, cache, cloudJson, readDataRevision);
			}
			else
			{
				MergeAndApply(remote, cache, cloudJson, readDataRevision);
			}
		});
	}

	private void ReadLegacyThenMerge(GameSaveData remote, GameSaveData cache, string cloudJson, int readDataRevision)
	{
		string requestAccount = account;
		int request = ++operation;
		int requestGeneration = generation;
		bool consumed = false;
		busy = true;
		deadline = clock() + 125.0 + 5.0;
		nextLegacyRead = clock() + RetrySeconds;
		legacyTimeout = delegate
		{
			if (IsCurrent(requestGeneration, requestAccount))
			{
				nextLegacyRead = clock() + RetrySeconds;
				Report("Legacy read timed out; current progress can still synchronize.");
				MergeAndApply(remote, cache, cloudJson, readDataRevision);
			}
		};
		legacyReader.Read(requestAccount, delegate(LegacySnapshotReadResult result)
		{
			if (consumed || !IsCurrent(requestGeneration, requestAccount) || request != operation)
			{
				return;
			}
			consumed = true;
			legacyTimeout = null;
			busy = false;
			bool sameAccount = result != null && result.account == requestAccount;
			bool complete = sameAccount && result.ownershipVerified && result.complete;
			List<LegacyMigrationSource> sources = new List<LegacyMigrationSource>();
			if (sameAccount && result.ownershipVerified && result.snapshots != null)
			{
				foreach (LegacySnapshotPayload snapshot in result.snapshots)
				{
					if (snapshot != null && LegacyGameProgressMigration.TryConvert(snapshot.name, snapshot.base64, out LegacyMigrationSource source, out string _))
					{
						sources.Add(source);
					}
					else
					{
						complete = false;
					}
				}
			}
			if (sources.Count > 0 && !legacyStore.TryRetain(requestAccount, sources, out string error))
			{
				Retry(error);
				return;
			}
			nextLegacyRead = clock() + (complete ? 300.0 : RetrySeconds);
			if (!complete)
			{
				Report("Legacy save read is incomplete; retry remains enabled.");
			}
			MergeAndApply(remote, cache, cloudJson, readDataRevision);
		});
	}

	private bool IsCurrent(int requestGeneration, string requestAccount)
	{
		return !disposed && generation == requestGeneration && account == requestAccount && cloud.Account == requestAccount;
	}

	private void MergeAndApply(GameSaveData remote, GameSaveData cache, string cloudJson, int readDataRevision)
	{
		int requestGeneration = generation;
		string requestAccount = account;
		CaptureChanges();
		if (diskBlocked || !IsCurrent(requestGeneration, requestAccount))
		{
			return;
		}
		if (cloudDataRevision == readDataRevision)
		{
			List<GameSaveData> sources = new List<GameSaveData> { pending, remote, cache };
			List<LegacyMigrationRecord> records = new List<LegacyMigrationRecord>();
			if (legacyStore != null)
			{
				if (!legacyStore.TryLoad(requestAccount, out records, out string loadError))
				{
					Retry(loadError);
					return;
				}
				foreach (LegacyMigrationRecord record in records)
				{
					sources.Add(record.source.data);
				}
			}
			GameSaveData merged = SaveGameProgressData.Merge(sources.ToArray());
			string mergedJson = SaveGameProgressData.Serialize(merged);
			if (mergedJson != SaveGameProgressData.Serialize(pending))
			{
				revision++;
			}
			pending = merged;
			if (!SaveRecovery(requestAccount, merged, revision))
			{
				return;
			}
			applying = true;
			apply(merged);
			if (!IsCurrent(requestGeneration, requestAccount))
			{
				applying = false;
				return;
			}
			observed = SaveGameProgressData.Merge(capture());
			applying = false;
			if (!IsCurrent(requestGeneration, requestAccount))
			{
				return;
			}
			if (legacyStore != null && records.Count > 0)
			{
				List<string> applied = new List<string>();
				foreach (LegacyMigrationRecord record in records)
				{
					if (!SaveGameProgressData.ContainsAll(observed, record.source.data))
					{
						Retry("Legacy local restore verification failed; import remains pending.");
						return;
					}
					applied.Add(record.sourceId);
				}
				if (!legacyStore.TryMarkApplied(requestAccount, applied, out string markError))
				{
					Retry(markError);
					return;
				}
			}
			lastError = null;
			if (cloudDataRevision == readDataRevision)
			{
				needsRead = false;
				if (mergedJson != cloudJson)
				{
					WriteMerged(mergedJson);
				}
				return;
			}
		}
		else
		{
			// The cloud moved while we were reading: keep what we saw and read again.
			pending = SaveGameProgressData.Merge(pending, remote, cache);
			revision++;
			if (!SaveRecovery(requestAccount, pending, revision))
			{
				return;
			}
		}
		retryAt = 0.0;
		needsRead = true;
	}

	private void WriteMerged(string json)
	{
		if (!cloud.TryWrite(json, out string error))
		{
			Retry(error);
			return;
		}
		long requestRevision = revision;
		BeginSync(delegate(bool success)
		{
			if (!success)
			{
				Retry("Cloud write deferred; the recovery copy is retained.");
				return;
			}
			if (legacyStore != null && revision == requestRevision)
			{
				if (!SaveGameProgressData.TryParse(json, out GameSaveData payload, out string parseError))
				{
					Retry(parseError);
					return;
				}
				if (!legacyStore.TryMarkProviderAccepted(account, payload, requestRevision, out string markError))
				{
					Retry(markError);
					return;
				}
			}
			needsRead = true;
			retryAt = revision == requestRevision ? clock() + RetrySeconds : 0.0;
		});
	}

	private void BeginSync(Action<bool> completed)
	{
		bool consumed = false;
		int request = ++operation;
		int requestGeneration = generation;
		string requestAccount = account;
		busy = true;
		deadline = clock() + TimeoutSeconds;
		cloud.Synchronize(delegate(bool success)
		{
			if (!consumed && !disposed && request == operation && requestGeneration == generation && requestAccount == account && requestAccount == cloud.Account)
			{
				consumed = true;
				busy = false;
				completed(success);
			}
		});
	}

	private bool SaveRecovery(string owner, GameSaveData data, long dataRevision)
	{
		if (recovery.TrySave(owner, data, dataRevision, out string error))
		{
			return true;
		}
		BlockDisk(error);
		return false;
	}

	private void BlockDisk(string error)
	{
		diskBlocked = true;
		busy = false;
		operation++;
		retryAt = clock() + RetrySeconds;
		Report(error ?? "Recovery storage unavailable; cloud writes suspended.");
	}

	private bool TryResumeRecovery()
	{
		if (!recovery.TryLoadOwner(out string owner, out string error))
		{
			BlockDisk(error);
			return false;
		}
		if (!SaveRecovery(Unassigned, SaveGameProgressData.Merge(capture()), 0L))
		{
			return false;
		}
		if (account != null && !SaveRecovery(account, pending, revision))
		{
			return false;
		}
		if (!accountChangedDuringDiskFailure && cloud.Account == account)
		{
			diskBlocked = false;
			localOwner = owner;
			CaptureChanges();
		}
		else
		{
			if (!recovery.TrySaveOwner(Mixed, out error))
			{
				BlockDisk(error);
				return false;
			}
			diskBlocked = false;
			localOwner = Mixed;
		}
		if (diskBlocked)
		{
			return false;
		}
		SwitchAccount(cloud.Account, true);
		if (diskBlocked)
		{
			return false;
		}
		accountChangedDuringDiskFailure = false;
		return true;
	}

	private void Retry(string error)
	{
		needsRead = true;
		retryAt = clock() + RetrySeconds;
		Report(error);
	}

	private void Report(string error)
	{
		if (error != lastError)
		{
			lastError = error;
			log(error);
		}
	}

	public void Dispose()
	{
		if (disposed)
		{
			return;
		}
		disposed = true;
		generation++;
		cloud.AccountChanged -= OnAccountChanged;
		cloud.DataChanged -= OnDataChanged;
		legacyReader?.Dispose();
	}
}
