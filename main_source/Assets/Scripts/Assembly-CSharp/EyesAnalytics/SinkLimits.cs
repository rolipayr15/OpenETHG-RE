namespace EyesAnalytics
{
	public readonly struct SinkLimits
	{
		public readonly int MaxParams;

		public readonly int MaxNameLength;

		public readonly int MaxStringLength;

		public readonly int MaxUserPropertyNameLength;

		public readonly int MaxUserPropertyValueLength;

		public static readonly SinkLimits Firebase;

		public SinkLimits(int maxParams, int maxNameLength, int maxStringLength, int maxUserPropertyNameLength, int maxUserPropertyValueLength)
		{
			MaxParams = 0;
			MaxNameLength = 0;
			MaxStringLength = 0;
			MaxUserPropertyNameLength = 0;
			MaxUserPropertyValueLength = 0;
		}
	}
}
