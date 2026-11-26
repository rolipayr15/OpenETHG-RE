using System;
using System.Collections.Generic;

namespace EyesAnalytics
{
	public sealed class EventDef
	{
		private static readonly ParamDef[] none;

		public string Name { get; }

		public IReadOnlyList<ParamDef> Required { get; }

		public IReadOnlyList<ParamDef> Optional { get; }

		public string Doc { get; }

		public EventDef(string name, ParamDef[] required = null, ParamDef[] optional = null, string doc = null)
		{
		}

		public bool Allows(ParamDef param)
		{
			return false;
		}

		public bool Send(Action<EventBuilder> fill = null)
		{
			return false;
		}

		public bool Send(AnalyticsContext.Snapshot context, Action<EventBuilder> fill)
		{
			return false;
		}
	}
}
