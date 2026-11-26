using System;
public class QuadraticCappedExpFormulaProvider : IExpFormulaProvider
{
	private readonly int baseLevelUpCost;

	private readonly int levelUpCostIncrease;

	private readonly int cappingStartLevel;

	private readonly int cappedLevelUpCost;

	public QuadraticCappedExpFormulaProvider(int baseLevelUpCost = 100, int levelUpCostIncrease = 50, int cappingStartLevel = 39)
	{
		this.baseLevelUpCost = baseLevelUpCost;
		this.levelUpCostIncrease = levelUpCostIncrease;
		this.cappingStartLevel = cappingStartLevel;
		cappedLevelUpCost = baseLevelUpCost + (cappingStartLevel - 1) * levelUpCostIncrease;
	}

	public int GetTotalExpNeededForLevel(int level)
	{
		if (level < 2)
		{
			return 0;
		}
		long total;
		if (level <= cappingStartLevel)
		{
			long levels = level - 1;
			total = baseLevelUpCost * levels + levels * (level - 2) * levelUpCostIncrease / 2;
		}
		else
		{
			total = (long)GetTotalExpNeededForLevel(cappingStartLevel) + (long)cappedLevelUpCost * (level - cappingStartLevel);
		}
		return total > int.MaxValue ? int.MaxValue : (int)total;
	}

	public int GetLevelForExp(int exp)
	{
		if (exp < 1)
		{
			return 1;
		}
		if (exp == int.MaxValue)
		{
			// Totals saturate at int.MaxValue, so look for the first level that reaches it.
			for (int high = cappingStartLevel; high <= 1000000; high += 1000)
			{
				if (GetTotalExpNeededForLevel(high) == int.MaxValue)
				{
					int low = high - 1000;
					while (low < high)
					{
						int middle = low + (high - low) / 2;
						if (GetTotalExpNeededForLevel(middle) != int.MaxValue)
						{
							low = middle + 1;
						}
						else
						{
							high = middle;
						}
					}
					return low - 1;
				}
			}
		}
		int upper = EstimateUpperBound(exp);
		if (upper < 2)
		{
			return 0;
		}
		int lower = 1;
		while (lower < upper)
		{
			int middle = lower + (upper - lower) / 2;
			if (GetTotalExpNeededForLevel(middle) <= exp)
			{
				lower = middle + 1;
			}
			else
			{
				upper = middle;
			}
		}
		return lower - 1;
	}

	private int EstimateUpperBound(int exp)
	{
		int cappedStart = GetTotalExpNeededForLevel(cappingStartLevel);
		if (exp > cappedStart)
		{
			int above = cappedLevelUpCost != 0 ? (exp - cappedStart) / cappedLevelUpCost : 0;
			return cappingStartLevel + above + 10;
		}
		return Math.Max((int)Math.Sqrt(((double)exp + (double)exp) / (double)levelUpCostIncrease) + 10, cappingStartLevel + 1);
	}
}
