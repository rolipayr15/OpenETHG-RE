namespace EyesAnalytics
{
	public sealed class LongParam : ParamDef
	{
		public LongParam(string name, Ga4Role ga4 = Ga4Role.None, string doc = null)
			: base(null, ParamKind.String, Ga4Role.None, null)
		{
		}
	}
}
