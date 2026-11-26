using EyesAnalytics;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ExternalApi.Models;

public class ExperienceManager : IExperienceManager
{
	private readonly IKeyValueStore keyValueStore;

	private readonly IExpFormulaProvider formulaProvider;

	private Dictionary<int, int> levelToExpCache;

	[CompilerGenerated]
	private Action<int, int> m_OnLevelUp;

	private const string TOTAL_EXP_KEY = "TOTAL_EXP";

	[CompilerGenerated]
	private Action<GrantExpData, bool> m_OnExpChanged;

	public event Action<int, int> OnLevelUp
	{
		add { m_OnLevelUp += value; }
		remove { m_OnLevelUp -= value; }
	}

	public event Action<GrantExpData, bool> OnExpChanged
	{
		add { m_OnExpChanged += value; }
		remove { m_OnExpChanged -= value; }
	}

	public ExperienceManager(IKeyValueStore keyValueStore, IExpFormulaProvider formulaProvider)
	{
		this.keyValueStore = keyValueStore ?? throw new ArgumentNullException("keyValueStore");
		this.formulaProvider = formulaProvider ?? throw new ArgumentNullException("formulaProvider");
	}

	public int GetTotalExp()
	{
		return keyValueStore.GetInt(TOTAL_EXP_KEY, 0);
	}

	public int GetLevelForExp(int exp)
	{
		return formulaProvider.GetLevelForExp(exp);
	}

	public int GetCurrentLevel()
	{
		return GetLevelForExp(GetTotalExp());
	}

	public int GetExpNeededBetweenLevels(int fromLevel, int toLevel)
	{
		if (toLevel <= fromLevel)
		{
			return 0;
		}
		return formulaProvider.GetTotalExpNeededForLevel(toLevel) - formulaProvider.GetTotalExpNeededForLevel(fromLevel);
	}

	public void GrantExp(GrantExpData exp)
	{
		int total = GetTotalExp();
		int newTotal = exp.exp + total;
		int gained = HowManyLevelUps(total, newTotal);
		if (gained > 0)
		{
			int newLevel = GetLevelForExp(newTotal);
			m_OnLevelUp?.Invoke(GetLevelForExp(GetTotalExp()), newLevel);
			Experience.LevelUp(newLevel, gained);
		}
		keyValueStore.SetValue(TOTAL_EXP_KEY, newTotal);
		m_OnExpChanged?.Invoke(exp, false);
	}

	public void OverwriteExp(int exp)
	{
		GrantExpData data = ExpDataF.Exp(exp, "overwrite");
		keyValueStore.SetValue(TOTAL_EXP_KEY, exp);
		m_OnExpChanged?.Invoke(data, true);
	}

	public int HowManyLevelUps(int currentExp, int newExp)
	{
		return Math.Max(GetLevelForExp(newExp) - GetLevelForExp(currentExp), 0);
	}

	public int GetExpSinceLastLevelUp()
	{
		return GetTotalExp() - formulaProvider.GetTotalExpNeededForLevel(GetLevelForExp(GetTotalExp()));
	}

	public int GetExpNeededForLevelUp()
	{
		return formulaProvider.GetTotalExpNeededForLevel(GetLevelForExp(GetTotalExp()) + 1) - GetTotalExp();
	}
}
