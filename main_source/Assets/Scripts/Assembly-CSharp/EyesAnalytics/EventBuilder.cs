using System;
using System.Collections.Generic;

namespace EyesAnalytics
{
	public sealed class EventBuilder
	{
		private const int MaxStringLength = 100;

		private readonly EventDef def;

		private readonly List<ParamValue> values;

		internal EventBuilder(EventDef def)
		{
		}

		public EventBuilder Set(StringParam param, string value)
		{
			return null;
		}

		public EventBuilder Set(LongParam param, long value)
		{
			return null;
		}

		public EventBuilder Flag(LongParam param, bool value)
		{
			return null;
		}

		public EventBuilder Set(DoubleParam param, double value)
		{
			return null;
		}

		public EventBuilder Set<T>(EnumParam<T> param, T member) where T : struct, Enum
		{
			return null;
		}

		public EventBuilder Set<T>(StringParam param, IReadOnlyDictionary<T, string> map, T member) where T : struct, Enum
		{
			return null;
		}

		internal EventBuilder SetGlobal(StringParam param, string value)
		{
			return null;
		}

		internal EventBuilder SetGlobal(LongParam param, long value)
		{
			return null;
		}

		internal EventBuilder SetGlobal<T>(EnumParam<T> param, string value) where T : struct, Enum
		{
			return null;
		}

		internal AnalyticsEvent Build()
		{
			return null;
		}

		private EventBuilder Add(ParamDef param, string text, long number, double real, bool declared)
		{
			return null;
		}

		private int IndexOf(ParamDef param)
		{
			return 0;
		}

		private static string Cut(string value)
		{
			return null;
		}
	}
}
