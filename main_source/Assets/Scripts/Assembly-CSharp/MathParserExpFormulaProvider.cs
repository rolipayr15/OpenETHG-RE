using System.Collections.Generic;

public class MathParserExpFormulaProvider : IExpFormulaProvider
{
	private readonly string formula;

	private readonly Dictionary<string, float> baseParameters;

	public MathParserExpFormulaProvider(string formula, Dictionary<string, float> parameters = null)
	{
	}

	public int GetTotalExpNeededForLevel(int level)
	{
		return 0;
	}

	private bool IsValidFormula(string formulaToTest)
	{
		return false;
	}

	public string GetFormula()
	{
		return null;
	}

	public Dictionary<string, float> GetBaseParameters()
	{
		return null;
	}

	public int GetLevelForExp(int exp)
	{
		return 0;
	}
}
