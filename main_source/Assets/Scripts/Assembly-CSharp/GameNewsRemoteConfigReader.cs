using UnityEngine;
using System.Linq;
using System.Globalization;
using System;
using System.Collections.Generic;

public static class GameNewsRemoteConfigReader
{
	public static GameNewsItem[] Read()
	{
		if (!RemoteConfigWrapper.IsReady)
		{
			Debug.LogWarning("[GameNews] Remote Config is not ready.");
			return Array.Empty<GameNewsItem>();
		}
		string json = RemoteConfigWrapper.GetString("gameNewsRich");
		if (string.IsNullOrWhiteSpace(json))
		{
			Debug.Log("[GameNews] Remote news configuration is empty.");
			return Array.Empty<GameNewsItem>();
		}
		GameNewsConfig config = JsonUtility.FromJson<GameNewsConfig>(json);
		if (config != null && config.items != null)
		{
			return ValidateAndSort(config.items);
		}
		Debug.LogWarning("[GameNews] Remote Config 'gameNewsRich' must contain a JSON object with an 'items' array.");
		return Array.Empty<GameNewsItem>();
	}

	private static GameNewsItem[] ValidateAndSort(IEnumerable<GameNewsItem> items)
	{
		DateTimeOffset now = DateTimeOffset.UtcNow;
		DateTimeOffset cutoff = now.AddMonths(-3);
		List<GameNewsItem> valid = new List<GameNewsItem>();
		HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
		foreach (GameNewsItem item in items)
		{
			if (!TryValidate(item, cutoff, out DateTimeOffset publishAt, out string error))
			{
				Debug.LogWarning("[GameNews] Ignoring remote item: " + error);
			}
			else if (!(publishAt > now))
			{
				if (ids.Add(item.id))
				{
					valid.Add(item);
				}
				else
				{
					Debug.LogWarning("[GameNews] Duplicate news ID: " + item.id);
				}
			}
		}
		return valid.OrderByDescending(GetPublishDate).ToArray();
	}

	private static bool TryValidate(GameNewsItem item, DateTimeOffset cutoffDate, out DateTimeOffset publishAt, out string error)
	{
		publishAt = default;
		error = null;
		if (item == null)
		{
			error = "Item is null.";
			return false;
		}
		item.id = item.id?.Trim();
		item.publishAt = item.publishAt?.Trim();
		item.title = item.title?.Trim();
		item.body = item.body?.Trim();
		item.category = string.IsNullOrWhiteSpace(item.category) ? "info" : item.category.Trim();
		if (!Enum.TryParse<GameNewsCategory>(item.category, true, out GameNewsCategory _))
		{
			error = "News '" + item.id + "' has invalid category: '" + item.category + "'.";
			return false;
		}
		if (string.IsNullOrWhiteSpace(item.id))
		{
			error = "Missing id.";
			return false;
		}
		if (string.IsNullOrWhiteSpace(item.title))
		{
			error = "News '" + item.id + "' has no titleKey.";
			return false;
		}
		if (string.IsNullOrWhiteSpace(item.body))
		{
			error = "News '" + item.id + "' has no bodyKey.";
			return false;
		}
		if (!TryParseDate(item.publishAt, out publishAt))
		{
			error = "News '" + item.id + "' has invalid publishAt: '" + item.publishAt + "'.";
			return false;
		}
		if (publishAt < cutoffDate)
		{
			error = "News '" + item.id + "' is older than 3 months.";
			return false;
		}
		return true;
	}

	public static bool TryParseDate(string value, out DateTimeOffset result)
	{
		return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out result);
	}

	private static DateTimeOffset GetPublishDate(GameNewsItem item)
	{
		return TryParseDate(item.publishAt, out DateTimeOffset date) ? date : DateTimeOffset.MinValue;
	}
}
