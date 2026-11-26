namespace EyesAnalytics
{
	public sealed class DoubleParam : ParamDef
	{
		public DoubleParam(string name, Ga4Role ga4 = Ga4Role.None, string doc = null)
			: base(null, ParamKind.String, Ga4Role.None, null)
		{
		}
	}
}
