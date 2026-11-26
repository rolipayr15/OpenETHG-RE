using System;

namespace ExternalApi.Models
{
	public struct CurrentDailyChallengeResponse
	{
		public int challengeId;

		public DateTimeOffset deadline;

		// Recovery addition: only sent by the custom server for challenges written in its panel.
		public string title;

		public string description;

		public string shortDescription;

		public int kind;

		public int parameter;

		public int goal;
	}
}
