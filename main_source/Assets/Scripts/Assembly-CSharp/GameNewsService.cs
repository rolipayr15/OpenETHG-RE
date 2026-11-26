using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class GameNewsService
{
	[CompilerGenerated]
	private static Action m_NewsChanged;

	private static GameNewsRepository repository;

	private static GameNewsRepository Repository => repository ?? (repository = new GameNewsRepository());

	public static bool HasUnread => Repository.HasUnread();

	public static event Action NewsChanged
	{
		add { m_NewsChanged += value; }
		remove { m_NewsChanged -= value; }
	}

	// Recovery addition: every server has its own news archive and read marks.
	public static void ReloadForCurrentServer()
	{
		repository = null;
		m_NewsChanged?.Invoke();
	}

	public static IReadOnlyList<GameNewsItem> GetAll()
	{
		return Repository.GetAll();
	}

	public static IReadOnlyList<GameNewsItem> GetUnread()
	{
		return Repository.GetUnread();
	}

	public static GameNewsItem GetById(string newsId)
	{
		return Repository.GetById(newsId);
	}

	public static void Synchronize(IEnumerable<GameNewsItem> remoteItems)
	{
		Repository.Merge(remoteItems);
		m_NewsChanged?.Invoke();
	}

	public static bool MarkAsRead(string newsId)
	{
		bool changed = Repository.MarkAsRead(newsId);
		if (changed)
		{
			m_NewsChanged?.Invoke();
		}
		return changed;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void ResetStaticState()
	{
		repository = null;
		m_NewsChanged = null;
	}
}
