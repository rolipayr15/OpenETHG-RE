public class LinearExpFormulaProvider : IExpFormulaProvider
{
	private readonly int expPerLevel;

	public LinearExpFormulaProvider(int expPerLevel = 100)
	{
	}

	public int GetTotalExpNeededForLevel(int level)
	{
		return 0;
	}

	public int GetLevelForExp(int exp)
	{
		return 0;
	}
}
