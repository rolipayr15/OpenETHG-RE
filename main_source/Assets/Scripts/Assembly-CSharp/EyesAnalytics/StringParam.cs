namespace EyesAnalytics
{
	public sealed class StringParam : ParamDef
	{
		public StringParam(string name, Ga4Role ga4 = Ga4Role.None, string doc = null)
			: base(null, ParamKind.String, Ga4Role.None, null)
		{
		}
	}
}
