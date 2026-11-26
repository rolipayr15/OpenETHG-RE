using System.Collections.Generic;

namespace EyesAnalytics
{
	public sealed class AnalyticsEvent
	{
		public string Name { get; }

		public IReadOnlyList<ParamValue> Values { get; }

		internal AnalyticsEvent(string name, List<ParamValue> values)
		{
		}

		public bool Has(string param)
		{
			return false;
		}

		public string Text(string param)
		{
			return null;
		}

		public long Number(string param)
		{
			return 0L;
		}

		public double Real(string param)
		{
			return 0.0;
		}

		private ParamValue Find(string param)
		{
			return default;
		}
	}
}
