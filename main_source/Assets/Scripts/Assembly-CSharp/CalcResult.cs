using System.Collections.Generic;

public sealed class CalcResult
{
	public int FinalTotal { get; private set; }

	public List<PhaseResult> Phases { get; }

	public CalcResult(int finalTotal, List<PhaseResult> phases)
	{
		FinalTotal = finalTotal;
		Phases = phases;
	}

	public PhaseResult GetPhaseResult(Phase p)
	{
		foreach (PhaseResult phase in Phases)
		{
			if (phase.Phase == p)
			{
				return phase;
			}
		}
		return null;
	}
}
