using System.Collections.Generic;

namespace EyesAnalytics
{
	public abstract class ParamDef
	{
		public string Name { get; }

		public ParamKind Kind { get; }

		public Ga4Role Ga4 { get; }

		public string Doc { get; }

		public virtual IReadOnlyList<string> Values => null;

		protected ParamDef(string name, ParamKind kind, Ga4Role ga4, string doc)
		{
		}
	}
}
