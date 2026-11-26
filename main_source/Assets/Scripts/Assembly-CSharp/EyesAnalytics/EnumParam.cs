using System;
using System.Collections.Generic;

namespace EyesAnalytics
{
	public sealed class EnumParam<T> : ParamDef where T : struct, Enum
	{
		private readonly IReadOnlyDictionary<T, string> map;

		public override IReadOnlyList<string> Values => null;

		public EnumParam(string name, Ga4Role ga4, IReadOnlyDictionary<T, string> map, string doc = null)
			: base(null, ParamKind.String, Ga4Role.None, null)
		{
		}

		public string ValueOf(T member)
		{
			return null;
		}
	}
}
