using System.IO;
using System.Collections;
using System;
using System.Runtime.CompilerServices;
using VoxelBusters.CoreLibrary;
using VoxelBusters.EssentialKit;

public sealed class SaveGameProgressCloudAdapter : ISaveGameProgressCloud, IDisposable
{
	public const string SaveKey = "complete_save_data";

	[CompilerGenerated]
	private Action m_AccountChanged;

	[CompilerGenerated]
	private Action m_DataChanged;

	private readonly string initialCacheOwner;

	private readonly string initialCacheJson;

	private string writtenCacheOwner;

	private string writtenCacheJson;

	private string observedAccount;

	private readonly SaveGameProgressSyncQueue syncQueue;

	public string Account => CloudServices.ActiveUser != null && CloudServices.ActiveUser.AccountStatus == CloudUserAccountStatus.Available && !string.IsNullOrEmpty(CloudServices.ActiveUser.UserId) ? AccountPrefix + CloudServices.ActiveUser.UserId : null;

	public bool IsAvailable => CloudServices.IsAvailable();

	private static string AccountPrefix => "playgames:";

	public event Action AccountChanged
	{
		add { m_AccountChanged += value; }
		remove { m_AccountChanged -= value; }
	}

	public event Action DataChanged
	{
		add { m_DataChanged += value; }
		remove { m_DataChanged -= value; }
	}

	public SaveGameProgressCloudAdapter(bool inspectLegacyCache)
	{
		if (inspectLegacyCache)
		{
			ReadInitialCacheEvidence(out initialCacheOwner, out initialCacheJson);
		}
		observedAccount = Account;
		syncQueue = new SaveGameProgressSyncQueue(() => Account, delegate(Action<bool> completed)
		{
			CloudServices.Synchronize(delegate(CloudServicesSynchronizeResult result)
			{
				completed(result != null && result.Success);
			});
		});
		CloudServices.OnUserChange += OnUserChange;
		CloudServices.OnSavedDataChange += OnDataChange;
	}

	public void Synchronize(Action<bool> completed)
	{
		syncQueue.Synchronize(completed);
	}

	public void Tick()
	{
		syncQueue.Tick();
	}

	public bool TryRead(out string cloudJson, out string cachedJson, out bool cacheIsAccountOwned, out string error)
	{
		cloudJson = null;
		cachedJson = null;
		cacheIsAccountOwned = false;
		error = null;
		IDictionary snapshot = CloudServices.GetSnapshot();
		if (snapshot == null)
		{
			error = "Cloud snapshot unavailable.";
			return false;
		}
		bool hasCloudValue = snapshot.Contains(SaveKey);
		if (hasCloudValue && !(snapshot[SaveKey] is string))
		{
			error = "Cloud save has an invalid value type.";
			return false;
		}
		if (!CloudServicesUtility.TryGetCloudAndLocalCacheValues<string>(SaveKey, out cloudJson, out cachedJson))
		{
			error = "Cloud/cache values unavailable.";
			return false;
		}
		if (hasCloudValue && cloudJson == null)
		{
			error = "Cloud save could not be read.";
			return false;
		}
		string account = Account;
		if (account != null && cachedJson != null)
		{
			cacheIsAccountOwned = (account == initialCacheOwner && cachedJson == initialCacheJson) || (account == writtenCacheOwner && cachedJson == writtenCacheJson);
		}
		return true;
	}

	public bool TryWrite(string json, out string error)
	{
		error = null;
		string account = Account;
		if (account == null)
		{
			error = "Cloud account unavailable.";
			return false;
		}
		CloudServices.SetString(SaveKey, json);
		if (CloudServices.GetString(SaveKey) != json)
		{
			error = "Provider did not accept the local save.";
			return false;
		}
		if (account != Account)
		{
			error = "Cloud account changed during the local save.";
			return false;
		}
		writtenCacheOwner = account;
		writtenCacheJson = json;
		return true;
	}

	private void OnUserChange(CloudServicesUserChangeResult result, Error error)
	{
		string account = Account;
		if (account != observedAccount)
		{
			observedAccount = account;
			writtenCacheJson = null;
			writtenCacheOwner = null;
			syncQueue.InvalidateAccount();
		}
		m_AccountChanged?.Invoke();
	}

	private void OnDataChange(CloudServicesSavedDataChangeResult result)
	{
		if (result == null || result.ChangedKeys == null)
		{
			return;
		}
		foreach (string key in result.ChangedKeys)
		{
			if (key == SaveKey)
			{
				m_DataChanged?.Invoke();
				break;
			}
		}
	}

	public void Dispose()
	{
		syncQueue.Dispose();
		CloudServices.OnUserChange -= OnUserChange;
		CloudServices.OnSavedDataChange -= OnDataChange;
	}

	private static void ReadInitialCacheEvidence(out string owner, out string payload)
	{
		owner = null;
		payload = null;
		try
		{
			string directory = EssentialKitSettings.PersistentDataPath;
			if (string.IsNullOrEmpty(directory))
			{
				return;
			}
			string path = Path.Combine(directory, "CloudLocalCache.json");
			if (!File.Exists(path) || new FileInfo(path).Length > 4194304)
			{
				return;
			}
			if (SaveGameProgressData.TryReadJsonObject(File.ReadAllText(path), out IDictionary document, out string _, 4194304))
			{
				string user = document["$userId"] as string;
				string json = document[SaveKey] as string;
				if (!string.IsNullOrEmpty(user) && json != null)
				{
					owner = AccountPrefix + user;
					payload = json;
				}
			}
		}
		catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
		{
			owner = null;
			payload = null;
		}
	}
}
