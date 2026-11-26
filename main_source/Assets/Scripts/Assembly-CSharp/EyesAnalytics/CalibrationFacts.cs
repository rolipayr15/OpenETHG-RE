namespace EyesAnalytics
{
	public readonly struct CalibrationFacts
	{
		public readonly int SymbolsVisible;

		public readonly bool SliderMoved;

		public readonly long OpenMs;

		public readonly int Version;

		public CalibrationFacts(int symbolsVisible, bool sliderMoved, long openMs, int version)
		{
			SymbolsVisible = 0;
			SliderMoved = false;
			OpenMs = 0L;
			Version = 0;
		}
	}
}
