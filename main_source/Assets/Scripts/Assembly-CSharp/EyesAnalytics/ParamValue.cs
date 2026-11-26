namespace EyesAnalytics
{
	public readonly struct ParamValue
	{
		public readonly ParamDef Def;

		public readonly string Text;

		public readonly long Number;

		public readonly double Real;

		internal ParamValue(ParamDef def, string text, long number, double real)
		{
			Def = null;
			Text = null;
			Number = 0L;
			Real = 0.0;
		}
	}
}
