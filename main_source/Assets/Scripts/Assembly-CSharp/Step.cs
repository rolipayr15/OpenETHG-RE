public sealed class Step
{
	public string Label { get; private set; }

	public int Occurrences { get; private set; }

	public CalcMode Mode { get; private set; }

	public int Value { get; private set; }

	public int Delta { get; private set; }

	public int SubtotalAfter { get; private set; }

	public Step(string label, int occurrences, CalcMode mode, int value, int delta, int subtotalAfter)
	{
		Label = label;
		Occurrences = occurrences;
		Mode = mode;
		Value = value;
		Delta = delta;
		SubtotalAfter = subtotalAfter;
	}
}
