using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

public sealed class GameNewsRepository
{
	private const string FileName = "game_news.json";

	private readonly string filePath;

	private GameNewsConfig archive;

	public GameNewsRepository()
	{
		filePath = Path.Combine(Application.persistentDataPath, OnlineConfig.NewsArchivePrefix + FileName);
		archive = Load();
		if (string.IsNullOrEmpty(OnlineConfig.NewsArchivePrefix))
		{
			// Early alpha builds saved custom server news in the official archive.
			List<GameNewsItem> official = new List<GameNewsItem>();
			foreach (GameNewsItem item in archive.items)
			{
				if (item == null || item.id == null || !System.Text.RegularExpressions.Regex.IsMatch(item.id, "^news-[0-9a-f]{12}$"))
				{
					official.Add(item);
				}
			}
			if (official.Count != archive.items.Length)
			{
				archive.items = official.ToArray();
				Save();
			}
		}
		Sort();
	}

	public IReadOnlyList<GameNewsItem> GetAll()
	{
		return archive.items;
	}

	public IReadOnlyList<GameNewsItem> GetUnread()
	{
		List<GameNewsItem> unread = new List<GameNewsItem>();
		foreach (GameNewsItem item in archive.items)
		{
			if (item != null && !item.isRead)
			{
				unread.Add(item);
			}
		}
		return unread;
	}

	public bool HasUnread()
	{
		foreach (GameNewsItem item in archive.items)
		{
			if (item != null && !item.isRead)
			{
				return true;
			}
		}
		return false;
	}

	public GameNewsItem GetById(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return null;
		}
		foreach (GameNewsItem item in archive.items)
		{
			if (item != null && item.id == id)
			{
				return item;
			}
		}
		return null;
	}

	public void Merge(IEnumerable<GameNewsItem> remoteItems)
	{
		if (remoteItems == null)
		{
			return;
		}
		List<GameNewsItem> merged = new List<GameNewsItem>(archive.items);
		foreach (GameNewsItem remote in remoteItems)
		{
			if (!IsValid(remote))
			{
				continue;
			}
			GameNewsItem known = FindById(merged, remote.id);
			if (known == null)
			{
				merged.Add(new GameNewsItem
				{
					id = remote.id,
					publishAt = remote.publishAt,
					title = remote.title,
					body = remote.body,
					category = remote.category,
					showAsPopup = remote.showAsPopup,
					isRead = false
				});
			}
			else
			{
				known.publishAt = remote.publishAt;
				known.title = remote.title;
				known.body = remote.body;
				known.category = remote.category;
			}
		}
		archive.items = merged.ToArray();
		Sort();
		Save();
	}

	public bool MarkAsRead(string id)
	{
		GameNewsItem item = GetById(id);
		if (item == null || item.isRead)
		{
			return false;
		}
		item.isRead = true;
		Save();
		return true;
	}

	private GameNewsConfig Load()
	{
		if (!File.Exists(filePath))
		{
			return new GameNewsConfig();
		}
		string json = File.ReadAllText(filePath);
		if (string.IsNullOrEmpty(json))
		{
			return new GameNewsConfig();
		}
		GameNewsConfig config = JsonUtility.FromJson<GameNewsConfig>(json);
		if (config == null)
		{
			return new GameNewsConfig();
		}
		if (config.items == null)
		{
			config.items = Array.Empty<GameNewsItem>();
		}
		return config;
	}

	private void Save()
	{
		string json = JsonUtility.ToJson(archive, false);
		string temporary = filePath + ".tmp";
		File.WriteAllText(temporary, json);
		if (File.Exists(filePath))
		{
			File.Delete(filePath);
		}
		File.Move(temporary, filePath);
	}

	private void Sort()
	{
		Array.Sort(archive.items, (GameNewsItem a, GameNewsItem b) => GetDate(b).CompareTo(GetDate(a)));
	}

	private static GameNewsItem FindById(List<GameNewsItem> items, string id)
	{
		foreach (GameNewsItem item in items)
		{
			if (item != null && item.id == id)
			{
				return item;
			}
		}
		return null;
	}

	private static bool IsValid(GameNewsItem item)
	{
		if (item == null || string.IsNullOrEmpty(item.id) || string.IsNullOrEmpty(item.publishAt) || string.IsNullOrEmpty(item.title) || string.IsNullOrEmpty(item.body))
		{
			return false;
		}
		return GameNewsRemoteConfigReader.TryParseDate(item.publishAt, out DateTimeOffset _);
	}

	private static DateTimeOffset GetDate(GameNewsItem item)
	{
		if (item != null && GameNewsRemoteConfigReader.TryParseDate(item.publishAt, out DateTimeOffset date))
		{
			return date;
		}
		return DateTimeOffset.MinValue;
	}
}
