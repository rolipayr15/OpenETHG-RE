using System.Collections.Generic;

public sealed class PhaseResult
{
	public Phase Phase { get; private set; }

	public List<Step> Steps { get; } = new List<Step>();

	public int SubtotalAfterPhase { get; private set; }

	public int PhaseDelta { get; private set; }

	public PhaseResult(Phase phase)
	{
		Phase = phase;
	}

	public void Seal(int subtotal, int phaseDelta)
	{
		SubtotalAfterPhase = subtotal;
		PhaseDelta = phaseDelta;
	}
}
